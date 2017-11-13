using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    // Used to reference GameManager from other scripts
    public static GameManager instance;

    // Prefabs
    public GameObject TilePrefab;
    public GameObject UserPlayerPrefab;
    public GameObject ChargerPlayerPrefab;
    public GameObject BasicAI;
    public GameObject ShielderAI;
    public GameObject DummyAI;

    public GameObject MovementOptionsPrefab;

    // Used to check which character's turn it is
    [HideInInspector] public UserPlayer userPlayerCheck;
    [HideInInspector] public ChargerPlayer chargerPlayerCheck;
    [HideInInspector] public BasicAI basicAICheck;
    [HideInInspector] public ShielderAI shielderAICheck;
    [HideInInspector] public DummyAI dummyAICheck;

    // Map
    [HideInInspector] public List<List<Tile>> map = new List<List<Tile>>();
    [HideInInspector] public int mapNumberOfColumns = 0;
    [HideInInspector] public int mapNumberOfRows = 0;

    // Players
    [HideInInspector] public List<Player> players = new List<Player>();
    [HideInInspector] public List<Vector2> playersGridPosition = new List<Vector2>();
    [HideInInspector] public int currentPlayerIndex = 0;
    [HideInInspector] public int playersHaveGone = 0;

    // aiPlayers
    [HideInInspector] public List<Player> aiPlayers = new List<Player>();
    [HideInInspector] public List<Vector2> aiPlayersGridPosition = new List<Vector2>();
    [HideInInspector] public int currentAIPlayerIndex = 0;
    public bool aiTurn = false;
    public bool coroutine = false;

    // Some variables
    Color originalTileColor;

    private void Awake()
    {
        instance = this;
    }

    //==========================================================================
    // ---------------------------- START --------------------------------------
    //==========================================================================
    // Use this for initialization
    void Start()
    {
        //loadWorldOne1(); // World 1-1
        loadWorldOne2(); // World 1-2

        //loadTestWorld(); // testWorld

        originalTileColor = TilePrefab.GetComponent<SpriteRenderer>().color;

        userPlayerCheck = ((GameObject)(Instantiate(UserPlayerPrefab, new Vector2(0f, 15f), Quaternion.Euler(new Vector3())))).GetComponent<UserPlayer>();
        chargerPlayerCheck = ((GameObject)(Instantiate(ChargerPlayerPrefab, new Vector2(0f, 15f), Quaternion.Euler(new Vector3())))).GetComponent<ChargerPlayer>();
        basicAICheck = ((GameObject)(Instantiate(BasicAI, new Vector2(0f, 15f), Quaternion.Euler(new Vector3())))).GetComponent<BasicAI>();
        shielderAICheck = ((GameObject)(Instantiate(ShielderAI, new Vector2(0f, 15f), Quaternion.Euler(new Vector3())))).GetComponent<ShielderAI>();
        dummyAICheck = ((GameObject)(Instantiate(DummyAI, new Vector2(0f, 15f), Quaternion.Euler(new Vector3())))).GetComponent<DummyAI>();


        Color movementOptionsAlpha = MovementOptionsPrefab.GetComponent<SpriteRenderer>().color;
        movementOptionsAlpha.a = 0.2f;
        MovementOptionsPrefab.GetComponent<SpriteRenderer>().color = movementOptionsAlpha;
        
    }

    //==========================================================================
    // --------------------------- UPDATE --------------------------------------
    //==========================================================================
    // Update is called once per frame
    void Update()
    {
        //Debug.Log(aiPlayers[0].gridPosition);
        // Start of the player's turn
        if (!aiTurn)
        {
            // Update players grid position
            playersGridPosition.Clear();
            foreach (Player p in players)
            {
                playersGridPosition.Add(p.gridPosition);
            }
            if (players[currentPlayerIndex].HP > 0)
            {
                players[currentPlayerIndex].TurnUpdate();
            }
            else
            {
                nextTurn();
            }
        }
        // Start of the AI's turn
        else
        {
            // Update AI players grid position
            aiPlayersGridPosition.Clear();
            foreach (Player p in aiPlayers)
            {
                aiPlayersGridPosition.Add(p.gridPosition);
            }
            if (aiPlayers[currentAIPlayerIndex].HP > 0)
            {
                aiPlayers[currentAIPlayerIndex].TurnUpdate();
            }
            else
            {
                nextAITurn();
            }
        }
    }

    //==========================================================================
    // --------------------------- GUI -----------------------------------------
    //==========================================================================
    void OnGUI()
    {
        if (players[currentPlayerIndex].HP > 0)
        {
            players[currentPlayerIndex].TurnOnGUI();
        }
    }

    //==========================================================================
    // ------------------------- METHODS ---------------------------------------
    //==========================================================================
    
    // Triggers the next turn
    public void nextTurn()
    {
        if (playersHaveGone + 1 < players.Count)
        {
            int i = 0;
            foreach(Player p in players)
            {
                if(!p.hasGone)
                {
                    currentPlayerIndex = i;
                }
                i++;
            }
            playersHaveGone++;
        }
        else
        {
            currentPlayerIndex = 0;
            playersHaveGone = 0;
            aiTurn = true;
        }
    }

    public void nextAITurn()
    {
        if (currentAIPlayerIndex + 1 < aiPlayers.Count)
        {
            currentAIPlayerIndex++;
        }
        else
        {
            currentAIPlayerIndex = 0;
            aiTurn = false;
        }
    }

    // Highlight tiles within range of ai
    public void aiHighlightTilesAt(Vector2 originLocation, Color highlightColor, int distance)
    {
        List<Tile> highlightedTiles = aiTileHighlight.aiFindHighlight(map[(int)originLocation.x][(int)originLocation.y], distance);

        foreach (Tile t in highlightedTiles)
        {
            // Don't highlight a space with an ai
            if (!t.occupiedAI)
            {
                t.GetComponent<SpriteRenderer>().color = highlightColor;
            }
        }
    }

    // Highlight tiles within range of player
    public void highlightTilesAt(Vector2 originLocation, int moveDistance, int attackRange)
    {
        List<Tile> highlightedTiles = TileHighlight.FindHighlight(map[(int)originLocation.x][(int)originLocation.y], moveDistance, attackRange);

        foreach (Tile t in highlightedTiles)
        {
            // Don't highlight a space with an ally
            if (!t.impassible)
            {
                if (t.moveTile && !t.occupiedAlly)
                    t.GetComponent<SpriteRenderer>().color = Color.blue;
                else if(t.moveTile && t.occupiedAlly)
                    t.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.4f, 0.8f, 1f);
                else if (t.attackTile && t.occupiedAI)
                    t.GetComponent<SpriteRenderer>().color = Color.red;
                else if (t.attackTile && !t.occupiedAI)
                    t.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.4f, 0.2f, 1f);
            }
        }
    }

    // Highlight cardinal directions ( used for charger )
    public void cardinalDirectionsHighlightTilesAt(Vector2 originLocation, int moveDistance, int attackRange)
    {
        List<Tile> highlightedTiles = CardinalDirectionsTileHighlight.FindCardinalDirectionHighlight(map[(int)originLocation.x][(int)originLocation.y], moveDistance, attackRange);

        foreach (Tile t in highlightedTiles)
        {
            // Don't highlight a space with an ally
            if (!t.impassible)
            {
                if (t.moveTile && !t.occupiedAlly)
                    t.GetComponent<SpriteRenderer>().color = Color.blue;
                else if (t.moveTile && t.occupiedAlly)
                    t.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.4f, 0.8f, 1f);
                else if (t.attackTile && t.occupiedAI)
                    t.GetComponent<SpriteRenderer>().color = Color.red;
                else if (t.attackTile && !t.occupiedAI)
                    t.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.4f, 0.2f, 1f);
            }
        }
    }

    // Remove tiles highlights
    public void removeTileHighlights()
    {
        for (int i = 0; i < mapNumberOfColumns; i++)
        {
            for (int j = 0; j < mapNumberOfRows; j++)
            {
                if (!map[i][j].impassible)
                {
                    map[i][j].GetComponent<SpriteRenderer>().color = originalTileColor;
                    map[i][j].attackTile = false;
                    map[i][j].moveTile = false;
                }
            }
        }
    }

    //----------------------
    // Main Character Action
    //----------------------
    public void actionWithCurrentPlayer(Tile destTile)
    {
        // If AI is within attack range hit them
        if (destTile.GetComponent<SpriteRenderer>().color == Color.red &&
            Mathf.Abs((destTile.gridPosition.x - players[currentPlayerIndex].gridPosition.x)) +
            Mathf.Abs((destTile.gridPosition.y - players[currentPlayerIndex].gridPosition.y)) <
            players[currentPlayerIndex].attackRange + 1)
        {
            Player target = null;
            foreach (Player p in aiPlayers)
            {
                // If true then player is found
                if (p.gridPosition == destTile.gridPosition && p.HP > 0)
                {
                    target = p;
                }
            }
            // attack logic
            if (target != null)
            {
                // Drain an action point only if they successfully attack
                players[currentPlayerIndex].actionPoints--;

                removeTileHighlights();
                players[currentPlayerIndex].attacking = false;

                // DAMAGE
                float amountOfDamage = players[currentPlayerIndex].damageBase;

                // Check if the enemy is a shielder
                if (target.GetType() == shielderAICheck.GetType())
                {
                    // Reduce damage by one
                    if (players[currentPlayerIndex].GetType() == userPlayerCheck.GetType())
                    {
                        amountOfDamage = players[currentPlayerIndex].damageBase - 1;
                    }
                }
                // If not don't reduce damage
                else
                {
                    amountOfDamage = players[currentPlayerIndex].damageBase;
                }

                target.HP -= amountOfDamage;
                //Debug.Log(players[currentPlayerIndex].playerName + " successfuly hit " + target.playerName + " for " + amountOfDamage + " damage");

                // Tile is no longer occupied if target is dead
                // I tried putting this in AIPlayer and it didn't work
                if (target.HP <= 0)
                {
                    map[(int)target.gridPosition.x][(int)target.gridPosition.y].occupiedAI = false;
                }
            }
        }
        // Move towards nearest attack range then attack
        else if (destTile.GetComponent<SpriteRenderer>().color == Color.red &&
                Mathf.Abs((destTile.gridPosition.x - players[currentPlayerIndex].gridPosition.x)) +
                Mathf.Abs((destTile.gridPosition.y - players[currentPlayerIndex].gridPosition.y)) >
                players[currentPlayerIndex].attackRange && destTile.occupiedAI)
        {
            // This is so there is a delay between moving and attacking
            StartCoroutine(moveThenAttack(destTile));
            // moveThenAttack is a method listed below
        }

        // Move the player to tile
        else if (destTile.GetComponent<SpriteRenderer>().color == Color.blue && !destTile.impassible && !destTile.occupiedAlly && !destTile.occupiedAI)
        {
            // Current tile is no longer occupied
            map[(int)playersGridPosition[currentPlayerIndex].x][(int)playersGridPosition[currentPlayerIndex].y].occupiedAlly = false;
            removeTileHighlights();
            players[currentPlayerIndex].moving = false;

            foreach (Tile t in TilePathFinder.FindPath(map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y], destTile))
            {
                players[currentPlayerIndex].positionQueue.Add(map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position);
            }
            players[currentPlayerIndex].gridPosition = destTile.gridPosition;

            // Destination tile is occupied
            map[(int)destTile.gridPosition.x][(int)destTile.gridPosition.y].occupiedAlly = true;
        }

        else
        {
            //Debug.Log("Destination invalid");
        }
    }

    //-------------------------------
    // Main Character Choice Action
    //-------------------------------
    public void choiceActionWithCurrentPlayer(List<Tile> listOfTiles)
    {
        // If AI is within attack range hit them
        if (listOfTiles[listOfTiles.Count-1].GetComponent<SpriteRenderer>().color == Color.red &&
            Mathf.Abs((listOfTiles[listOfTiles.Count - 1].gridPosition.x - players[currentPlayerIndex].gridPosition.x)) +
            Mathf.Abs((listOfTiles[listOfTiles.Count - 1].gridPosition.y - players[currentPlayerIndex].gridPosition.y)) <
            players[currentPlayerIndex].attackRange + 1)
        {
            Player target = null;
            foreach (Player p in aiPlayers)
            {
                // If true then player is found
                if (p.gridPosition == listOfTiles[listOfTiles.Count - 1].gridPosition && p.HP > 0)
                {
                    target = p;
                }
            }
            // attack logic
            if (target != null)
            {
                // Drain an action point only if they successfully attack
                players[currentPlayerIndex].actionPoints--;

                removeTileHighlights();
                players[currentPlayerIndex].attacking = false;

                // DAMAGE
                float amountOfDamage = players[currentPlayerIndex].damageBase;

                // Check if the enemy is a shielder
                if (target.GetType() == shielderAICheck.GetType())
                {
                    // Reduce damage by one
                    if (players[currentPlayerIndex].GetType() == userPlayerCheck.GetType())
                    {
                        amountOfDamage = players[currentPlayerIndex].damageBase - 1;
                    }
                }
                // If not don't reduce damage
                else
                {
                    amountOfDamage = players[currentPlayerIndex].damageBase;
                }

                target.HP -= amountOfDamage;
                //Debug.Log(players[currentPlayerIndex].playerName + " successfuly hit " + target.playerName + " for " + amountOfDamage + " damage");

                // Tile is no longer occupied if target is dead
                // I tried putting this in AIPlayer and it didn't work
                if (target.HP <= 0)
                {
                    map[(int)target.gridPosition.x][(int)target.gridPosition.y].occupiedAI = false;
                }
            }
        }
        // Move towards nearest attack range then attack
        else if (listOfTiles[listOfTiles.Count - 1].GetComponent<SpriteRenderer>().color == Color.red &&
                Mathf.Abs((listOfTiles[listOfTiles.Count - 1].gridPosition.x - players[currentPlayerIndex].gridPosition.x)) +
                Mathf.Abs((listOfTiles[listOfTiles.Count - 1].gridPosition.y - players[currentPlayerIndex].gridPosition.y)) >
                players[currentPlayerIndex].attackRange && listOfTiles[listOfTiles.Count - 1].occupiedAI)
        {
            // This is so there is a delay between moving and attacking
            StartCoroutine(moveThenAttack(listOfTiles[listOfTiles.Count - 1]));
            // moveThenAttack is a method listed below
        }

        // Move the player to tile
        else if (listOfTiles[listOfTiles.Count - 1].GetComponent<SpriteRenderer>().color == Color.blue && !listOfTiles[listOfTiles.Count - 1].impassible && !listOfTiles[listOfTiles.Count - 1].occupiedAlly && !listOfTiles[listOfTiles.Count - 1].occupiedAI)
        {
            // Current tile is no longer occupied
            map[(int)playersGridPosition[currentPlayerIndex].x][(int)playersGridPosition[currentPlayerIndex].y].occupiedAlly = false;
            removeTileHighlights();
            players[currentPlayerIndex].moving = false;

            foreach (Tile t in listOfTiles)
            {
                players[currentPlayerIndex].positionQueue.Add(map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position);
            }
            players[currentPlayerIndex].gridPosition = listOfTiles[listOfTiles.Count - 1].gridPosition;

            // Destination tile is occupied
            map[(int)listOfTiles[listOfTiles.Count - 1].gridPosition.x][(int)listOfTiles[listOfTiles.Count - 1].gridPosition.y].occupiedAlly = true;
        }

        else
        {
            //Debug.Log("Destination invalid");
        }
    }

    // -----------------------
    // Charger Action
    // -----------------------
    public void actionWithCurrentCharger(Tile destTile)
    {
        
        // If AI is within attack range hit them
        if (destTile.GetComponent<SpriteRenderer>().color == Color.red &&
            Mathf.Abs((destTile.gridPosition.x - players[currentPlayerIndex].gridPosition.x)) +
            Mathf.Abs((destTile.gridPosition.y - players[currentPlayerIndex].gridPosition.y)) ==
            1)
        {
            Player target = null;
            foreach (Player p in aiPlayers)
            {
                // If true then player is found
                if (p.gridPosition == destTile.gridPosition && p.HP > 0)
                {
                    target = p;
                }
            }
            // attack logic
            if (target != null)
            {
                // Drain an action point only if they successfully attack
                players[currentPlayerIndex].actionPoints--;

                removeTileHighlights();
                players[currentPlayerIndex].attacking = false;

                // ~DAMAGE~

                //float amountOfDamage = players[currentPlayerIndex].damageBase + Random.Range(0.0f, players[currentPlayerIndex].damageRollSides);
                float amountOfDamage = players[currentPlayerIndex].damageBase;
                target.HP -= amountOfDamage;
                //Debug.Log(players[currentPlayerIndex].playerName + " successfuly hit " + target.playerName + " for " + amountOfDamage + " damage");

                // Tile is no longer occupied if target is dead
                // I tried putting this in AIPlayer and it didn't work
                if (target.HP <= 0)
                {
                    map[(int)target.gridPosition.x][(int)target.gridPosition.y].occupiedAI = false;
                }

                // Knockback mechanic
                StartCoroutine(knockBack(target));
                

            }
        }
        
        // Move towards nearest attack range then attack
        //Debug.Log("Something is happening");
        if (destTile.GetComponent<SpriteRenderer>().color == Color.red && destTile.occupiedAI)
        {
            // This is so there is a delay between moving and attacking
            StartCoroutine(chargerMoveThenAttack(destTile));
            // moveThenAttack is a method listed below
        }

        // Move the player to tile
        else if (destTile.GetComponent<SpriteRenderer>().color == Color.blue && !destTile.impassible && !destTile.occupiedAlly && !destTile.occupiedAI)
        {
            // Current tile is no longer occupied
            map[(int)playersGridPosition[currentPlayerIndex].x][(int)playersGridPosition[currentPlayerIndex].y].occupiedAlly = false;
            removeTileHighlights();
            players[currentPlayerIndex].moving = false;

            foreach (Tile t in TilePathFinder.FindPath(map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y], destTile))
            {
                players[currentPlayerIndex].positionQueue.Add(map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position);
            }
            players[currentPlayerIndex].gridPosition = destTile.gridPosition;

            // Destination tile is occupied
            map[(int)destTile.gridPosition.x][(int)destTile.gridPosition.y].occupiedAlly = true;
        }

        else
        {
            //Debug.Log("Destination invalid");
        }
    }

    // AI move method
    public void moveCurrentAIPlayer(Tile destTile)
    {
        if (!destTile.impassible && !destTile.occupiedAI)
        {
            // Current tile is no longer occupied
            map[(int)aiPlayersGridPosition[currentAIPlayerIndex].x][(int)aiPlayersGridPosition[currentAIPlayerIndex].y].occupiedAI = false;

            removeTileHighlights();
            aiPlayers[currentAIPlayerIndex].moving = false;

            foreach (Tile t in TilePathFinder.FindPath(map[(int)aiPlayers[currentAIPlayerIndex].gridPosition.x][(int)aiPlayers[currentAIPlayerIndex].gridPosition.y], destTile))
            {
                aiPlayers[currentAIPlayerIndex].positionQueueAI.Add(map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position);
            }
            aiPlayers[currentAIPlayerIndex].gridPosition = destTile.gridPosition;

            // Destination tile is occupied
            map[(int)destTile.gridPosition.x][(int)destTile.gridPosition.y].occupiedAI = true;
        }
        else
        {
            //Debug.Log("Destination invalid");
        }
    }
    // AI Attacking method
    public void attackWithCurrentAIPlayer(Tile destTile)
    {
        if (!destTile.impassible && !destTile.occupiedAI)
        {
            Player target = null;
            // Searches for the player
            foreach (Player p in players)
            {
                // If true then player is found
                if (p.gridPosition == destTile.gridPosition && p.HP > 0)
                {
                    target = p;
                }
            }

            // attack logic
            if (target != null)
            {
                // Drain an action point only if they successfully attack
                aiPlayers[currentAIPlayerIndex].actionPoints--;

                //removeTileHighlights();
                aiPlayers[currentAIPlayerIndex].moving = false;

                // DAMAGE
                //float amountOfDamage = players[currentPlayerIndex].damageBase + Random.Range(0.0f, players[currentPlayerIndex].damageRollSides);
                float amountOfDamage = aiPlayers[currentAIPlayerIndex].damageBase;
                target.HP -= amountOfDamage;

                // Tile is no longer occupied if target is dead
                // I tried putting this in UserPlayer and it didn't work
                if (target.HP <= 0)
                {
                    map[(int)target.gridPosition.x][(int)target.gridPosition.y].occupiedAlly = false;
                }
            }
        }
        else
        {
            //Debug.Log("Destination invalid");
        }
        //players[currentPlayerIndex].moveDestination = destTile.transform.position;
    }

    // Generates the play area
    void generateMap()
    {
        // Number of Rows
        map = new List<List<Tile>>();
        for (int i = 0; i < mapNumberOfColumns; i++)
        {
            List<Tile> row = new List<Tile>();
            //Debug.Log("Created: " + "row List");

            // Number of Columns
            for (int j = 0; j < mapNumberOfRows; j++)
            {
                Tile tile = ((GameObject)(Instantiate(TilePrefab, new Vector2(i - Mathf.Floor(mapNumberOfColumns / 2), -j + Mathf.Floor(mapNumberOfRows / 2)), Quaternion.Euler(new Vector3())))).GetComponent<Tile>();
                tile.gridPosition = new Vector2(i, j);

                // Add random impassible terrain
                if (i != 0 && j != 0 && j != mapNumberOfRows - 1 && i != mapNumberOfColumns - 1)
                {
                    if (Random.Range(0, 5) > 3)
                        tile.impassible = true;
                }
                //----------------------------

                row.Add(tile);
            }
            map.Add(row);
        }

    }

    
    public void restartMission()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    // --------------
    // moveThenAttack
    // --------------
    // This is so there is a delay between moving and attacking
    IEnumerator moveThenAttack(Tile destTile)
    {
        coroutine = true;
        map[(int)playersGridPosition[currentPlayerIndex].x][(int)playersGridPosition[currentPlayerIndex].y].occupiedAlly = false;
        removeTileHighlights();
        players[currentPlayerIndex].moving = false;

        List<Tile> path = TilePathFinder.FindPath(map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y], destTile);
        Tile moveTo = path[(int)Mathf.Max(0, path.Count - 1 - players[currentPlayerIndex].attackRange)];
        // Create a new path to the Tile closest to opponent in attack range
        List<Tile> newPath = TilePathFinder.FindPath(map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y], moveTo);

        // If the move to tile is occupied by ally, try to find one that is closer
        if (moveTo.occupiedAlly)
        {
            for(int i = players[currentPlayerIndex].attackRange - 1; i >= 0; i--)
            {
                moveTo = path[(int)Mathf.Max(0, path.Count - 1 - i)];
                newPath = TilePathFinder.FindPath(map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y], moveTo);
                if (moveTo.occupiedAlly)
                    continue;
                else
                    break;
            }
        }

        if (!moveTo.occupiedAlly)
        {
            foreach (Tile t in newPath)
            {
                players[currentPlayerIndex].positionQueue.Add(map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position);
            }
            players[currentPlayerIndex].gridPosition = path[(int)Mathf.Max(0, path.Count - 1 - players[currentPlayerIndex].attackRange)].gridPosition;

            // Destination tile is occupied
            map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y].occupiedAlly = true;

            yield return new WaitForSeconds(0.2f);

            Player target = null;
            foreach (Player p in aiPlayers)
            {
                // If true then player is found
                if (p.gridPosition == destTile.gridPosition && p.HP > 0)
                {
                    target = p;
                    //Debug.Log("found AI target");
                }
            }
            if (target != null)
            {
                //Debug.Log("Now attacking after waiting");
                // Drain an action point only if they successfully attack
                players[currentPlayerIndex].actionPoints--;

                removeTileHighlights();
                players[currentPlayerIndex].attacking = false;

                // DAMAGE
                float amountOfDamage = players[currentPlayerIndex].damageBase;

                // Check if the enemy is a shielder
                if (target.GetType() == shielderAICheck.GetType())
                {
                    // Reduce damage by one
                    if(players[currentPlayerIndex].GetType() == userPlayerCheck.GetType())
                    {
                        amountOfDamage = players[currentPlayerIndex].damageBase - 1;
                    }
                }
                // If not don't reduce damage
                else
                {
                    amountOfDamage = players[currentPlayerIndex].damageBase;
                }
                
                target.HP -= amountOfDamage;
                //Debug.Log(players[currentPlayerIndex].playerName + " successfuly hit " + target.playerName + " for " + amountOfDamage + " damage");

                // Tile is no longer occupied if target is dead
                // I tried putting this in AIPlayer and it didn't work
                if (target.HP <= 0)
                {
                    map[(int)target.gridPosition.x][(int)target.gridPosition.y].occupiedAI = false;
                }
            }
        }
        // Can't move and attack b/c ally is blocking the tile
        else
        {
            // Display some kind of movement error message
            //Debug.Log("Ally blocks the path");
        }

        yield return new WaitForSeconds(0.2f);
        coroutine = false;  
    }

    IEnumerator chargerMoveThenAttack(Tile destTile)
    {
        bool pathBlockedByAlly = false;
        coroutine = true;
        map[(int)playersGridPosition[currentPlayerIndex].x][(int)playersGridPosition[currentPlayerIndex].y].occupiedAlly = false;
        removeTileHighlights();
        players[currentPlayerIndex].moving = false;

        List<Tile> path = TilePathFinder.FindPath(map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y], destTile);
        Tile moveTo = path[(int)Mathf.Max(0, path.Count - 1 - players[currentPlayerIndex].movementPerActionPoint)];
        // Create a new path to the Tile closest to opponent in attack range
        List<Tile> newPath = TilePathFinder.FindPath(map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y], moveTo);

        pathBlockedByAlly = path[(int)Mathf.Max(0, path.Count - 1 - players[currentPlayerIndex].movementPerActionPoint)].occupiedAlly;

        if (!pathBlockedByAlly)
        {
            foreach (Tile t in newPath)
            {
                players[currentPlayerIndex].positionQueue.Add(map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position);
            }
            players[currentPlayerIndex].gridPosition = path[(int)Mathf.Max(0, path.Count - 1 - players[currentPlayerIndex].movementPerActionPoint)].gridPosition;

            // Destination tile is occupied
            map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y].occupiedAlly = true;

            yield return new WaitForSeconds(0.2f);

            Player target = null;
            foreach (Player p in aiPlayers)
            {
                // If true then player is found
                if (p.gridPosition == destTile.gridPosition && p.HP > 0)
                {
                    target = p;
                    //Debug.Log("found AI target");
                }
            }
            if (target != null)
            {
                //Debug.Log("Now attacking after waiting");
                // Drain an action point only if they successfully attack
                players[currentPlayerIndex].actionPoints--;

                removeTileHighlights();
                players[currentPlayerIndex].attacking = false;

                // DAMAGE
                //float amountOfDamage = players[currentPlayerIndex].damageBase + Random.Range(0.0f, players[currentPlayerIndex].damageRollSides);

                float amountOfDamage = players[currentPlayerIndex].damageBase;
                target.HP -= amountOfDamage;
                //Debug.Log(players[currentPlayerIndex].playerName + " successfuly hit " + target.playerName + " for " + amountOfDamage + " damage");

                // Tile is no longer occupied if target is dead
                // I tried putting this in AIPlayer and it didn't work
                if (target.HP <= 0)
                {
                    map[(int)target.gridPosition.x][(int)target.gridPosition.y].occupiedAI = false;
                }

                // Knockback mechanic
                yield return StartCoroutine(knockBack(target));
            }
        }
        // Can't move and attack b/c ally is blocking the tile
        else
        {
            // Display some kind of movement error message
            //Debug.Log("Ally blocks the path");
        }

        yield return new WaitForSeconds(0.5f);

        coroutine = false;
    }

    IEnumerator knockBack(Player victim)
    {
        //calculates which direction the charger came from
        // charge from north
        if (players[currentPlayerIndex].gridPosition.y < victim.gridPosition.y)
        {
            // Check if tile south of it exists
            if (!((int)victim.gridPosition.y + 1 > mapNumberOfRows - 1))
            {
                // If an enemy is in the way make them take damage
                if (map[(int)victim.gridPosition.x][(int)victim.gridPosition.y + 1].occupiedAI)
                {
                    // Find the AI in the way
                    foreach (Player p in aiPlayers)
                    {
                        // If the AI is found
                        if (p.gridPosition == map[(int)victim.gridPosition.x][(int)victim.gridPosition.y + 1].gridPosition)
                        {
                            // They also take the charge damage
                            p.HP -= players[currentPlayerIndex].damageBase;
                            // If they are dead make sure their tile is no longer occupied
                            if (p.HP <= 0)
                            {
                                map[(int)p.gridPosition.x][(int)p.gridPosition.y].occupiedAI = false;
                            }
                        }
                    }
                }
                // The tile behind them is unoccupied, knock them back
                else
                {
                    // Check if they should be knocked back 1 space
                    if (!((int)victim.gridPosition.y + 1 > mapNumberOfRows - 1))
                    {
                        // The tile they are in will no longer be occupied
                        map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;
                        // Shift the opponent over 1 tile
                        Vector3 nextToCharger = map[(int)victim.gridPosition.x][(int)victim.gridPosition.y + 1].transform.position;
                        victim.transform.position = map[(int)victim.gridPosition.x][(int)victim.gridPosition.y + 1].transform.position;
                        victim.gridPosition = map[(int)victim.gridPosition.x][(int)victim.gridPosition.y + 1].gridPosition;

                        // If they survive the knockback the tile is occupied
                        if (victim.HP > 0)
                            map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = true;
                        else
                            map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;

                        // Check if tile south of it exists
                        if (!((int)victim.gridPosition.y + 1 > mapNumberOfRows - 1))
                        {
                            // If an enemy is in the way make them take damage
                            if (map[(int)victim.gridPosition.x][(int)victim.gridPosition.y + 1].occupiedAI)
                            {
                                // Find the AI in the way
                                foreach (Player p in aiPlayers)
                                {
                                    // If the AI is found
                                    if (p.gridPosition == map[(int)victim.gridPosition.x][(int)victim.gridPosition.y + 1].gridPosition)
                                    {
                                        // They also take the charge damage
                                        p.HP -= players[currentPlayerIndex].damageBase;
                                        // If they are dead make sure their tile is no longer occupied
                                        if (p.HP <= 0)
                                        {
                                            map[(int)p.gridPosition.x][(int)p.gridPosition.y].occupiedAI = false;
                                        }
                                    }
                                }
                            }
                            // The tile behind that is unoccupied
                            else
                            {
                                // Check if they should be knocked back 2 spaces
                                if (!((int)victim.gridPosition.y + 1 > mapNumberOfRows - 1))
                                {
                                    // The tile they are in will no longer be occupied
                                    map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;
                                    // Shift the opponent over 1 tile
                                    nextToCharger = map[(int)victim.gridPosition.x][(int)victim.gridPosition.y + 1].transform.position;
                                    victim.transform.position = map[(int)victim.gridPosition.x][(int)victim.gridPosition.y + 1].transform.position;
                                    victim.gridPosition = map[(int)victim.gridPosition.x][(int)victim.gridPosition.y + 1].gridPosition;

                                    // If they survive the knockback the tile is occupied
                                    if (victim.HP > 0)
                                        map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = true;
                                    else
                                        map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;
                                }
                            }
                        }
                    }
                }
            }
        }
        // charge from south
        if (players[currentPlayerIndex].gridPosition.y > victim.gridPosition.y)
        {
            // Check if tile north of it exists
            if (!((int)victim.gridPosition.y - 1 < 0))
            {
                // If an enemy is in the way make them take damage
                if (map[(int)victim.gridPosition.x][(int)victim.gridPosition.y - 1].occupiedAI)
                {
                    // Find the AI in the way
                    foreach (Player p in aiPlayers)
                    {
                        // If the AI is found
                        if (p.gridPosition == map[(int)victim.gridPosition.x][(int)victim.gridPosition.y - 1].gridPosition)
                        {
                            // They also take the charge damage
                            p.HP -= players[currentPlayerIndex].damageBase;
                            // If they are dead make sure their tile is no longer occupied
                            if (p.HP <= 0)
                            {
                                map[(int)p.gridPosition.x][(int)p.gridPosition.y].occupiedAI = false;
                            }
                        }
                    }
                }
                // The tile behind them is unoccupied, knock them back
                else
                {
                    // Check if they should be knocked back 1 space
                    if (!((int)victim.gridPosition.y - 1 < 0))
                    {
                        // The tile they are in will no longer be occupied
                        map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;
                        // Shift the opponent over 1 tile
                        Vector3 nextToCharger = map[(int)victim.gridPosition.x][(int)victim.gridPosition.y - 1].transform.position;
                        victim.transform.position = map[(int)victim.gridPosition.x][(int)victim.gridPosition.y - 1].transform.position;
                        victim.gridPosition = map[(int)victim.gridPosition.x][(int)victim.gridPosition.y - 1].gridPosition;

                        // If they survive the knockback the tile is occupied
                        if (victim.HP > 0)
                            map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = true;
                        else
                            map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;

                        // Check if tile north of it exists
                        if (!((int)victim.gridPosition.y - 1 < 0))
                        {
                            // If an enemy is in the way make them take damage
                            if (map[(int)victim.gridPosition.x][(int)victim.gridPosition.y - 1].occupiedAI)
                            {
                                // Find the AI in the way
                                foreach (Player p in aiPlayers)
                                {
                                    // If the AI is found
                                    if (p.gridPosition == map[(int)victim.gridPosition.x][(int)victim.gridPosition.y - 1].gridPosition)
                                    {
                                        // They also take the charge damage
                                        p.HP -= players[currentPlayerIndex].damageBase;
                                        // If they are dead make sure their tile is no longer occupied
                                        if (p.HP <= 0)
                                        {
                                            map[(int)p.gridPosition.x][(int)p.gridPosition.y].occupiedAI = false;
                                        }
                                    }
                                }
                            }
                            // The tile behind that is unoccupied
                            else
                            {
                                // Check if they should be knocked back 2 spaces
                                if (!((int)victim.gridPosition.y - 1 < 0))
                                {
                                    // The tile they are in will no longer be occupied
                                    map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;
                                    // Shift the opponent over 1 tile
                                    nextToCharger = map[(int)victim.gridPosition.x][(int)victim.gridPosition.y - 1].transform.position;
                                    victim.transform.position = map[(int)victim.gridPosition.x][(int)victim.gridPosition.y - 1].transform.position;
                                    victim.gridPosition = map[(int)victim.gridPosition.x][(int)victim.gridPosition.y - 1].gridPosition;

                                    // If they survive the knockback the tile is occupied
                                    if (victim.HP > 0)
                                        map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = true;
                                    else
                                        map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;
                                }
                            }
                        }                        
                    }
                }
            }
        }
        // charge from west (left lol)
        if (players[currentPlayerIndex].gridPosition.x < victim.gridPosition.x)
        {
            // Check if tile east of it exists
            if (!((int)victim.gridPosition.x + 1 > mapNumberOfColumns - 1))
            {
                // If an enemy is in the way make them take damage
                if (map[(int)victim.gridPosition.x + 1][(int)victim.gridPosition.y].occupiedAI)
                {
                    // Find the AI in the way
                    foreach(Player p in aiPlayers)
                    {
                        // If the AI is found
                        if (p.gridPosition == map[(int)victim.gridPosition.x + 1][(int)victim.gridPosition.y].gridPosition)
                        {
                            // They also take the charge damage
                            p.HP -= players[currentPlayerIndex].damageBase;
                            // If they are dead make sure their tile is no longer occupied
                            if (p.HP <= 0)
                            {
                                map[(int)p.gridPosition.x][(int)p.gridPosition.y].occupiedAI = false;
                            }
                        }
                    }
                }
                // The tile behind them is unoccupied, knock them back
                else
                {
                    //Debug.Log("Knocking back 1 tile");
                    // Check if they should be knocked back 1 space
                    if (!((int)victim.gridPosition.x + 1 > mapNumberOfColumns - 1))
                    {
                        // The tile they are in will no longer be occupied
                        map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;
                        // Shift the opponent over 1 tile
                        Vector3 nextToCharger = map[(int)victim.gridPosition.x + 1][(int)victim.gridPosition.y].transform.position;
                        victim.transform.position = map[(int)victim.gridPosition.x + 1][(int)victim.gridPosition.y].transform.position;
                        victim.gridPosition = map[(int)victim.gridPosition.x + 1][(int)victim.gridPosition.y].gridPosition;

                        // If they survive the knockback the tile is occupied
                        if (victim.HP > 0)
                            map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = true;
                        else
                            map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;

                        // Make sure the tile behind their new position exists before checking if it's occupied
                        if (!((int)victim.gridPosition.x + 1 > mapNumberOfColumns - 1))
                        {
                            // If an enemy is in the way make them take damage
                            if (map[(int)victim.gridPosition.x + 1][(int)victim.gridPosition.y].occupiedAI)
                            {
                                // Find the AI in the way
                                foreach (Player p in aiPlayers)
                                {
                                    // If the AI is found
                                    if (p.gridPosition == map[(int)victim.gridPosition.x + 1][(int)victim.gridPosition.y].gridPosition)
                                    {
                                        // They also take the charge damage
                                        p.HP -= players[currentPlayerIndex].damageBase;
                                        // If they are dead make sure their tile is no longer occupied
                                        if (p.HP <= 0)
                                        {
                                            map[(int)p.gridPosition.x][(int)p.gridPosition.y].occupiedAI = false;
                                        }
                                    }
                                }
                            }
                            // The tile behind that is unoccupied
                            else
                            {
                                //Debug.Log("Knocking back 2 tiles");
                                //Debug.Log("Current grid position: " + victim.gridPosition.x);
                                // Check if they should be knocked back 2 spaces
                                if (!((int)victim.gridPosition.x + 1 > mapNumberOfColumns - 1))
                                {
                                    // The tile they are in will no longer be occupied
                                    map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;
                                    // Shift the opponent over 1 tile
                                    nextToCharger = map[(int)victim.gridPosition.x + 1][(int)victim.gridPosition.y].transform.position;
                                    victim.transform.position = map[(int)victim.gridPosition.x + 1][(int)victim.gridPosition.y].transform.position;
                                    victim.gridPosition = map[(int)victim.gridPosition.x + 1][(int)victim.gridPosition.y].gridPosition;

                                    if (victim.HP > 0)
                                        map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = true;
                                    else
                                        map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;
                                    // Check if the tile behind that is occupied
                                }
                            }
                        }
                    }
                }
            }
        }
        // charge from east
        if (players[currentPlayerIndex].gridPosition.x > victim.gridPosition.x)
        {
            // Check if tile west of it exists
            if (!((int)victim.gridPosition.x - 1 < 0))
            {
                // If an enemy is in the way make them take damage
                if (map[(int)victim.gridPosition.x - 1][(int)victim.gridPosition.y].occupiedAI)
                {
                    // Find the AI in the way
                    foreach (Player p in aiPlayers)
                    {
                        // If the AI is found
                        if (p.gridPosition == map[(int)victim.gridPosition.x - 1][(int)victim.gridPosition.y].gridPosition)
                        {
                            // They also take the charge damage
                            p.HP -= players[currentPlayerIndex].damageBase;
                            // If they are dead make sure their tile is no longer occupied
                            if (p.HP <= 0)
                            {
                                map[(int)p.gridPosition.x][(int)p.gridPosition.y].occupiedAI = false;
                            }
                        }
                    }
                }
                // The tile behind them is unoccupied, knock them back
                else
                {
                    // Check if they should be knocked back 1 space
                    if (!((int)victim.gridPosition.x - 1 < 0))
                    {
                        // The tile they are in will no longer be occupied
                        map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;
                        // Shift the opponent over 1 tile
                        Vector3 nextToCharger = map[(int)victim.gridPosition.x - 1][(int)victim.gridPosition.y].transform.position;
                        victim.transform.position = map[(int)victim.gridPosition.x - 1][(int)victim.gridPosition.y].transform.position;
                        victim.gridPosition = map[(int)victim.gridPosition.x - 1][(int)victim.gridPosition.y].gridPosition;

                        // If they survive the knockback then the tile is occupied
                        if (victim.HP > 0)
                            map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = true;
                        else
                            map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;

                        // Check if tile west of it exists
                        if (!((int)victim.gridPosition.x - 1 < 0))
                        {
                            // If an enemy is in the way make them take damage
                            if (map[(int)victim.gridPosition.x - 1][(int)victim.gridPosition.y].occupiedAI)
                            {
                                // Find the AI in the way
                                foreach (Player p in aiPlayers)
                                {
                                    // If the AI is found
                                    if (p.gridPosition == map[(int)victim.gridPosition.x - 1][(int)victim.gridPosition.y].gridPosition)
                                    {
                                        // They also take the charge damage
                                        p.HP -= players[currentPlayerIndex].damageBase;
                                        // If they are dead make sure their tile is no longer occupied
                                        if (p.HP <= 0)
                                        {
                                            map[(int)p.gridPosition.x][(int)p.gridPosition.y].occupiedAI = false;
                                        }
                                    }
                                }
                            }
                            // The tile behind that is unoccupied
                            else
                            {
                                // Check if they should be knocked back 2 spaces
                                if (!((int)victim.gridPosition.x - 1 < 0))
                                {
                                    // The tile they are in will no longer be occupied
                                    map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;
                                    // Shift the opponent over 1 tile
                                    nextToCharger = map[(int)victim.gridPosition.x - 1][(int)victim.gridPosition.y].transform.position;
                                    victim.transform.position = map[(int)victim.gridPosition.x - 1][(int)victim.gridPosition.y].transform.position;
                                    victim.gridPosition = map[(int)victim.gridPosition.x - 1][(int)victim.gridPosition.y].gridPosition;

                                    if (victim.HP > 0)
                                        map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = true;
                                    else
                                        map[(int)victim.gridPosition.x][(int)victim.gridPosition.y].occupiedAI = false;
                                }
                            }
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.2f);
    }

    public void gameOver()
    {
        restartMission();
    }

    //==========================================================================
    // ---------------------------- WORLD 1 ------------------------------------
    //==========================================================================
    //----------
    // World 1-1
    //----------
    public void loadWorldOne1()
    {
        // MAP
        mapNumberOfColumns = 6;
        mapNumberOfRows = 1;
        map = new List<List<Tile>>();
        for (int i = 0; i < mapNumberOfColumns; i++)
        {
            List<Tile> row = new List<Tile>();
            //Debug.Log("Created: " + "row List");

            // Number of Columns
            for (int j = 0; j < mapNumberOfRows; j++)
            {
                Tile tile = ((GameObject)(Instantiate(TilePrefab, new Vector2(i - Mathf.Floor(mapNumberOfColumns / 2), -j + Mathf.Floor(mapNumberOfRows / 2)), Quaternion.Euler(new Vector3())))).GetComponent<Tile>();
                tile.gridPosition = new Vector2(i, j);

                // Add random impassible terrain
                if (i != 0 && j != 0 && j != mapNumberOfRows - 1 && i != mapNumberOfColumns - 1)
                {
                    if (Random.Range(0, 5) > 3)
                        tile.impassible = true;
                }
                //----------------------------

                row.Add(tile);
            }
            map.Add(row);
        }

        // CHARACTERS
        // Playable characters
        UserPlayer player;

        player = ((GameObject)(Instantiate(UserPlayerPrefab, new Vector2(0 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 0), Quaternion.Euler(new Vector3())))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(0, 0);
        map[(int)player.gridPosition.x][(int)player.gridPosition.y].occupiedAlly = true;
        player.playerName = "MainCharacter";

        players.Add(player);
        playersGridPosition.Add(player.gridPosition);

        // Bots
        BasicAI aiPlayer;

        aiPlayer = ((GameObject)(Instantiate(BasicAI, new Vector2(5 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 0), Quaternion.Euler(new Vector3())))).GetComponent<BasicAI>();
        aiPlayer.gridPosition = new Vector2(5, 0);
        map[(int)aiPlayer.gridPosition.x][(int)aiPlayer.gridPosition.y].occupiedAI = true;
        aiPlayer.playerName = "Bot";

        aiPlayers.Add(aiPlayer);
        aiPlayersGridPosition.Add(aiPlayer.gridPosition);
    }
    //----------
    // World 1-2
    //----------
    public void loadWorldOne2()
    {
        mapNumberOfColumns = 6;
        mapNumberOfRows = 2;
        map = new List<List<Tile>>();
        for (int i = 0; i < mapNumberOfColumns; i++)
        {
            List<Tile> row = new List<Tile>();
            //Debug.Log("Created: " + "row List");

            // Number of Columns
            for (int j = 0; j < mapNumberOfRows; j++)
            {
                Tile tile = ((GameObject)(Instantiate(TilePrefab, new Vector2(i - Mathf.Floor(mapNumberOfColumns / 2), -j + Mathf.Floor(mapNumberOfRows / 2)), Quaternion.Euler(new Vector3())))).GetComponent<Tile>();
                tile.gridPosition = new Vector2(i, j);

                // Add impassible terrain
                if (tile.gridPosition == new Vector2(3, 1))
                    tile.impassible = true;
                //----------------------------

                row.Add(tile);
            }
            map.Add(row);
        }

        // CHARACTERS
        // Main Character
        UserPlayer player;

        player = ((GameObject)(Instantiate(UserPlayerPrefab, new Vector2(1 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 1), Quaternion.Euler(new Vector3())))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(1, 1);
        map[(int)player.gridPosition.x][(int)player.gridPosition.y].occupiedAlly = true;
        player.playerName = "MainCharacter";

        players.Add(player);
        playersGridPosition.Add(player.gridPosition);

        userPlayerCheck = player;

        // Charger
        ChargerPlayer chargerPlayer;

        chargerPlayer = ((GameObject)(Instantiate(ChargerPlayerPrefab, new Vector2(0 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 0), Quaternion.Euler(new Vector3())))).GetComponent<ChargerPlayer>();
        chargerPlayer.gridPosition = new Vector2(0, 0);
        map[(int)chargerPlayer.gridPosition.x][(int)chargerPlayer.gridPosition.y].occupiedAlly = true;
        chargerPlayer.playerName = "Charger";

        players.Add(chargerPlayer);
        playersGridPosition.Add(chargerPlayer.gridPosition);

        chargerPlayerCheck = chargerPlayer;

        // Bots
        BasicAI aiPlayer;

        aiPlayer = ((GameObject)(Instantiate(BasicAI, new Vector2(5 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 1), Quaternion.Euler(new Vector3())))).GetComponent<BasicAI>();
        aiPlayer.gridPosition = new Vector2(5, 1);
        map[(int)aiPlayer.gridPosition.x][(int)aiPlayer.gridPosition.y].occupiedAI = true;
        aiPlayer.playerName = "GenericBot";

        aiPlayers.Add(aiPlayer);
        aiPlayersGridPosition.Add(aiPlayer.gridPosition);

        basicAICheck = aiPlayer;

        // Shielder
        ShielderAI aiShielder;
            
        aiShielder = ((GameObject)(Instantiate(ShielderAI, new Vector2(4 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 1), Quaternion.Euler(new Vector3())))).GetComponent<ShielderAI>();
        aiShielder.gridPosition = new Vector2(4, 1);
        map[(int)aiShielder.gridPosition.x][(int)aiShielder.gridPosition.y].occupiedAI = true;
        aiShielder.playerName = "ShielderBot";

        aiPlayers.Add(aiShielder);
        aiPlayersGridPosition.Add(aiShielder.gridPosition);

        shielderAICheck = aiShielder;
    }
    //----------
    // World 1-3
    //----------

    //----------
    // TestWorld
    //----------
    // FOR DEBUGGING
    void loadTestWorld()
    {
        mapNumberOfColumns = 8;
        mapNumberOfRows = 8;
        map = new List<List<Tile>>();
        for (int i = 0; i < mapNumberOfColumns; i++)
        {
            List<Tile> row = new List<Tile>();
            //Debug.Log("Created: " + "row List");

            // Number of Columns
            for (int j = 0; j < mapNumberOfRows; j++)
            {
                Tile tile = ((GameObject)(Instantiate(TilePrefab, new Vector2(i - Mathf.Floor(mapNumberOfColumns / 2), -j + Mathf.Floor(mapNumberOfRows / 2)), Quaternion.Euler(new Vector3())))).GetComponent<Tile>();
                tile.gridPosition = new Vector2(i, j);

                // Add impassible terrain
                if (tile.gridPosition == new Vector2(3, 1))
                    tile.impassible = true;
                if (tile.gridPosition == new Vector2(3, 5))
                    tile.impassible = true;
                if (tile.gridPosition == new Vector2(3, 6))
                    tile.impassible = true;
                //----------------------------

                row.Add(tile);
            }
            map.Add(row);
        }

        // CHARACTERS
        // Main Character

        
        UserPlayer player;
        
        player = ((GameObject)(Instantiate(UserPlayerPrefab, new Vector2(4 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 6), Quaternion.Euler(new Vector3())))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(4, 6);
        map[(int)player.gridPosition.x][(int)player.gridPosition.y].occupiedAlly = true;
        player.playerName = "MainCharacter";

        players.Add(player);
        playersGridPosition.Add(player.gridPosition);
        
        userPlayerCheck = player;
        

        // Charger
        ChargerPlayer chargerPlayer;

        chargerPlayer = ((GameObject)(Instantiate(ChargerPlayerPrefab, new Vector2(4 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 4), Quaternion.Euler(new Vector3())))).GetComponent<ChargerPlayer>();
        chargerPlayer.gridPosition = new Vector2(4, 4);
        map[(int)chargerPlayer.gridPosition.x][(int)chargerPlayer.gridPosition.y].occupiedAlly = true;
        chargerPlayer.playerName = "Charger";

        players.Add(chargerPlayer);
        playersGridPosition.Add(chargerPlayer.gridPosition);

        chargerPlayerCheck = chargerPlayer;
        
        // Bots

        
        DummyAI aiPlayer;
        // Dummy bot 1
        aiPlayer = ((GameObject)(Instantiate(DummyAI, new Vector2(4 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 5), Quaternion.Euler(new Vector3())))).GetComponent<DummyAI>();
        aiPlayer.gridPosition = new Vector2(4, 5);
        map[(int)aiPlayer.gridPosition.x][(int)aiPlayer.gridPosition.y].occupiedAI = true;
        aiPlayer.playerName = "DummyBot1";

        aiPlayers.Add(aiPlayer);
        aiPlayersGridPosition.Add(aiPlayer.gridPosition);

        dummyAICheck = aiPlayer;

        
        // Dummy bot 2
        aiPlayer = ((GameObject)(Instantiate(DummyAI, new Vector2(4 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 3), Quaternion.Euler(new Vector3())))).GetComponent<DummyAI>();
        aiPlayer.gridPosition = new Vector2(4, 3);
        map[(int)aiPlayer.gridPosition.x][(int)aiPlayer.gridPosition.y].occupiedAI = true;
        aiPlayer.playerName = "DummyBot2";

        aiPlayers.Add(aiPlayer);
        aiPlayersGridPosition.Add(aiPlayer.gridPosition);

        dummyAICheck = aiPlayer;
        /*
        
        // Dummy bot 3
        aiPlayer = ((GameObject)(Instantiate(DummyAI, new Vector2(3 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 4), Quaternion.Euler(new Vector3())))).GetComponent<DummyAI>();
        aiPlayer.gridPosition = new Vector2(3, 4);
        map[(int)aiPlayer.gridPosition.x][(int)aiPlayer.gridPosition.y].occupiedAI = true;
        aiPlayer.playerName = "DummyBot3";

        aiPlayers.Add(aiPlayer);
        aiPlayersGridPosition.Add(aiPlayer.gridPosition);

        dummyAICheck = aiPlayer;
        */
        // Dummy bot 4
        aiPlayer = ((GameObject)(Instantiate(DummyAI, new Vector2(5 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 4), Quaternion.Euler(new Vector3())))).GetComponent<DummyAI>();
        aiPlayer.gridPosition = new Vector2(5, 4);
        map[(int)aiPlayer.gridPosition.x][(int)aiPlayer.gridPosition.y].occupiedAI = true;
        aiPlayer.playerName = "DummyBot4";

        aiPlayers.Add(aiPlayer);
        aiPlayersGridPosition.Add(aiPlayer.gridPosition);

        dummyAICheck = aiPlayer;
        
        // Dummy bot 5
        aiPlayer = ((GameObject)(Instantiate(DummyAI, new Vector2(0 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 4), Quaternion.Euler(new Vector3())))).GetComponent<DummyAI>();
        aiPlayer.gridPosition = new Vector2(0, 4);
        map[(int)aiPlayer.gridPosition.x][(int)aiPlayer.gridPosition.y].occupiedAI = true;
        aiPlayer.playerName = "DummyBot5";

        aiPlayers.Add(aiPlayer);
        aiPlayersGridPosition.Add(aiPlayer.gridPosition);

        dummyAICheck = aiPlayer;

        // Dummy bot 6
        aiPlayer = ((GameObject)(Instantiate(DummyAI, new Vector2(2 - (mapNumberOfColumns / 2), (mapNumberOfRows / 2) - 4), Quaternion.Euler(new Vector3())))).GetComponent<DummyAI>();
        aiPlayer.gridPosition = new Vector2(2, 4);
        map[(int)aiPlayer.gridPosition.x][(int)aiPlayer.gridPosition.y].occupiedAI = true;
        aiPlayer.playerName = "DummyBot6";

        aiPlayers.Add(aiPlayer);
        aiPlayersGridPosition.Add(aiPlayer.gridPosition);

        dummyAICheck = aiPlayer;
        
        


    }

    //=============================================================================
    //----------------- OLD MECHANICS FOR REFERENCE -------------------------------
    //=============================================================================
    /*
    // Moving method
    public void moveCurrentPlayer(Tile destTile)
    {
        if(destTile.GetComponent<SpriteRenderer>().color != originalTileColor && !destTile.impassible && !destTile.occupiedAlly && !destTile.occupiedAI)
        {
            // Current tile is no longer occupied
            map[(int)playersGridPosition[currentPlayerIndex].x][(int)playersGridPosition[currentPlayerIndex].y].occupiedAlly = false;
            removeTileHighlights();
            players[currentPlayerIndex].moving = false;
            
            foreach(Tile t in TilePathFinder.FindPath(map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y], destTile))
            {
                players[currentPlayerIndex].positionQueue.Add(map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position);
            }
            players[currentPlayerIndex].gridPosition = destTile.gridPosition;

            // Destination tile is occupied
            map[(int)destTile.gridPosition.x][(int)destTile.gridPosition.y].occupiedAlly  = true;
        }


        else
        {
            Debug.Log("Destination invalid");
        }
    }
    */

    /*
    // Attacking method
    public void attackWithCurrentPlayer(Tile destTile)
    {
        if(destTile.GetComponent<SpriteRenderer>().color != originalTileColor && !destTile.impassible && !destTile.occupiedAlly)
        {
            Player target = null;
            // Searches for the player
            foreach (Player p in aiPlayers)
            {
                // If true then player is found
                if (p.gridPosition == destTile.gridPosition && p.HP > 0)
                {
                    target = p;
                }
            }

            // attack logic
            if (target != null)
            {
                // If they are within attack range
                if (players[currentPlayerIndex].gridPosition.x >= target.gridPosition.x - players[currentPlayerIndex].attackRange &&
                    players[currentPlayerIndex].gridPosition.x <= target.gridPosition.x + players[currentPlayerIndex].attackRange &&
                    players[currentPlayerIndex].gridPosition.y >= target.gridPosition.y - players[currentPlayerIndex].attackRange &&
                    players[currentPlayerIndex].gridPosition.y <= target.gridPosition.y + players[currentPlayerIndex].attackRange)
                {
                    // Drain an action point only if they successfully attack
                    players[currentPlayerIndex].actionPoints--;

                    removeTileHighlights();
                    players[currentPlayerIndex].moving = false;

                    // DAMAGE
                    //float amountOfDamage = players[currentPlayerIndex].damageBase + Random.Range(0.0f, players[currentPlayerIndex].damageRollSides);
                    float amountOfDamage = players[currentPlayerIndex].damageBase;
                    target.HP -= amountOfDamage;
                    Debug.Log(players[currentPlayerIndex].playerName + " successfuly hit " + target.playerName + " for " + amountOfDamage + " damage");

                    // Tile is no longer occupied if target is dead
                    // I tried putting this in AIPlayer and it didn't work
                    if (target.HP <= 0)
                    {
                        map[(int)target.gridPosition.x][(int)target.gridPosition.y].occupiedAI = false;
                    }

                    // if you want to add RNG chance to hit
                    // roll to hit
                    bool hit = Random.Range(0.0f, 1.0f) <= players[currentPlayerIndex].attackChance;

                    if (hit)
                    {
                        // damage logic
                        float amountOfDamage = players[currentPlayerIndex].damageBase + Random.Range(0.0f, players[currentPlayerIndex].damageRollSides);
                        //Debug.Log(players[currentPlayerIndex].playerName + " successfuly hit " + target.playerName + " for " + amountOfDamage + " damage");
                        target.HP -= amountOfDamage;
                    }
                    else
                    {
                        Debug.Log(players[currentPlayerIndex].playerName + " missed " + target.playerName);
                    }
                    
                }
                else
                {
                    //Debug.Log("Target is not adjacent");
                }
            }
        }
        else
        {
            //Debug.Log("Destination invalid");
        }
        //players[currentPlayerIndex].moveDestination = destTile.transform.position;
    }
    */
    /*
    // Generates the characters
    void generatePlayers()
    {
        // Playable characters
        UserPlayer player;

        player = ((GameObject)(Instantiate(UserPlayerPrefab, new Vector2(0 - (mapNumberOfColumns / 2), -0 + (mapNumberOfRows / 2)), Quaternion.Euler(new Vector3())))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(0, 0);
        map[(int)player.gridPosition.x][(int)player.gridPosition.y].occupiedAlly = true;
        player.playerName = "STAR";

        players.Add(player);
        playersGridPosition.Add(player.gridPosition);

        // Bots
        BasicAI aiPlayer;

        aiPlayer = ((GameObject)(Instantiate(BasicAI, new Vector2((mapNumberOfColumns / 2) - 1, -(0f)), Quaternion.Euler(new Vector3())))).GetComponent<AIPlayer>();
        aiPlayer.gridPosition = new Vector2(5, 0);
        map[(int)aiPlayer.gridPosition.x][(int)aiPlayer.gridPosition.y].occupiedAI = true;
        aiPlayer.playerName = "Bot";

        aiPlayers.Add(aiPlayer);
        aiPlayersGridPosition.Add(aiPlayer.gridPosition);
    }
    */
}
