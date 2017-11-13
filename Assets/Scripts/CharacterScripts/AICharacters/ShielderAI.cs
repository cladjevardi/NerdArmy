using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShielderAI : Player
{

    //public float moveSpeed = 10.0f;
    public bool coroutine = false;
    public bool noMove = false;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    public override void Update()
    {

        if (HP <= 0)
        {
            // Player rotates on death
            transform.rotation = Quaternion.Euler(new Vector3(180f, 0, 0));
            transform.GetComponent<SpriteRenderer>().sortingOrder = 0;
            // Turns red   
            transform.GetComponent<SpriteRenderer>().color = Color.red;
        }
        base.Update();
    }

    public override void TurnUpdate()
    {
        if (positionQueueAI.Count > 0)
        {
            transform.position += (positionQueueAI[0] - transform.position).normalized * moveSpeed * Time.deltaTime;
            // Once they reach the destination
            if (Vector3.Distance(positionQueueAI[0], transform.position) <= 0.1f)
            {
                transform.position = positionQueueAI[0];
                positionQueueAI.RemoveAt(0);
                if (positionQueueAI.Count == 0)
                {
                    actionPoints--;
                }
            }
        }
        else
        {
            // Priority queue
            List<Tile> tilesInRange = aiTileHighlight.aiFindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], attackRange);
            List<Tile> movementToAttackTilesInRange = aiTileHighlight.aiFindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint + attackRange);
            List<Tile> movementTilesInRange = aiTileHighlight.aiFindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint + 100000);
            // If in range and lowest HP
            if (tilesInRange.Where(x => GameManager.instance.players.Where(y => y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0 && !coroutine && !noMove)
            {
                // Find opponents in range
                var opponentsInRange = tilesInRange.Select(x => GameManager.instance.players.Where(y => y != this && y.gridPosition == x.gridPosition).Count() > 0 ? GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).First() : null).ToList();
                // Sort them by lowest hp first
                Player opponent = opponentsInRange.OrderBy(x => x != null ? -x.HP : 1000).First();

                // Attack lowest hp opponent
                GameManager.instance.attackWithCurrentAIPlayer(GameManager.instance.map[(int)opponent.gridPosition.x][(int)opponent.gridPosition.y]);
                Debug.Log("AI successfuly hit " + opponent.playerName);
            }

            // Move towards nearest attack range of opponent
            else if (movementToAttackTilesInRange.Where(x => GameManager.instance.players.Where(y => y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0 && !coroutine && !noMove)
            {
                // Find opponents in range
                var opponentsInRange = movementToAttackTilesInRange.Select(x => GameManager.instance.players.Where(y => y != this && y.gridPosition == x.gridPosition).Count() > 0 ? GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).First() : null).ToList();
                // Sort them by lowest hp first
                Player opponent = opponentsInRange.OrderBy(x => x != null ? -x.HP : 1000).OrderBy(x => x != null ? -TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y]).Count() : 1000).First();

                // Add to the end of FindPath when i figure it out: GameManager.instance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != opponent.gridPosition).Select(x => x.gridPosition).ToArray()
                List<Tile> path = TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)opponent.gridPosition.x][(int)opponent.gridPosition.y]);

                Tile moveToTile = path[(int)Mathf.Max(0, path.Count - 1 - attackRange)];
                Tile attackTile = GameManager.instance.map[(int)opponent.gridPosition.x][(int)opponent.gridPosition.y];

                // As long as the tile isn't occpied the AI can move then attack
                if (!moveToTile.occupiedAlly && !moveToTile.occupiedAI)
                {
                    Debug.Log("AI successfuly moved and attacked " + opponent.playerName);
                    coroutine = true;
                    StartCoroutine(moveThenAttack(moveToTile, attackTile));
                }
                // Tile is occupied, no movement
                else
                {
                    noMove = true;
                }
                //GameManager.instance.moveCurrentAIPlayer(path[(int)Mathf.Max(0, path.Count - 1 - attackRange)]);
                //GameManager.instance.attackWithCurrentAIPlayer(GameManager.instance.map[(int)opponent.gridPosition.x][(int)opponent.gridPosition.y]);
            }
            // Move towards nearest opponent
            else if (movementTilesInRange.Where(x => GameManager.instance.players.Where(y => y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0 && !coroutine && !noMove)
            {
                // Find opponents in range
                var opponentsInRange = movementTilesInRange.Select(x => GameManager.instance.players.Where(y => y != this && y.gridPosition == x.gridPosition).Count() > 0 ? GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).First() : null).ToList();
                // Sort them by lowest hp first
                Player opponent = opponentsInRange.OrderBy(x => x != null ? -x.HP : 1000).OrderBy(x => x != null ? -TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y]).Count() : 1000).First();

                List<Tile> path = TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)opponent.gridPosition.x][(int)opponent.gridPosition.y]);
                GameManager.instance.moveCurrentAIPlayer(path[(int)Mathf.Min(Mathf.Max(path.Count - 1 - 1, 0), movementPerActionPoint - 1)]);
                Debug.Log("AI is moving to closest enemy");

                noMove = false;
            }
            else if (noMove)
            {
                noMove = false;
                actionPoints = 1;
                moving = false;
                attacking = false;
                GameManager.instance.nextAITurn();
                Debug.Log("AI skipped turn");
            }
            // end turn if nothing else (and not in a coroutine)
            else if (!coroutine && Time.timeScale != 0)
            {
                actionPoints = 1;
                moving = false;
                attacking = false;
                GameManager.instance.nextAITurn();
                Debug.Log("AI killed everyone");
                Time.timeScale = 0;
            }
            if (Time.timeScale == 0)
            {
                coroutine = true;
                StartCoroutine(allPlayersAreDead());
            }

        }
        base.TurnUpdate();
    }

    public override void TurnOnGUI()
    {
        float buttonHeight = 50;
        float buttonWidth = 150;

        Rect buttonRect = new Rect(0, Screen.height - buttonHeight * 4, buttonWidth, buttonHeight);
        // Restart Mission button
        buttonRect = new Rect(Screen.width - buttonWidth, Screen.height - buttonHeight, buttonWidth, buttonHeight);
        if (GUI.Button(buttonRect, "Restart Mission"))
        {
            GameManager.instance.removeTileHighlights();
            actionPoints = 1;
            moving = false;
            attacking = false;
            GameManager.instance.restartMission();
        }
        base.TurnOnGUI();
    }

    IEnumerator moveThenAttack(Tile moveToTile, Tile attackTile)
    {
        // VERY HACKY WAY OF SOLVING AI MOVE + ATTACK WITHOUT REFACTORING
        actionPoints = 2;
        // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        GameManager.instance.moveCurrentAIPlayer(moveToTile);

        yield return new WaitForSeconds(0.2f);

        GameManager.instance.attackWithCurrentAIPlayer(attackTile);

        yield return new WaitForSeconds(0.2f);
        coroutine = false;
    }

    IEnumerator allPlayersAreDead()
    {
        Time.timeScale = 1;

        yield return new WaitForSeconds(1);

        GameManager.instance.gameOver();

        yield return new WaitForSeconds(0.2f);
        coroutine = false;
    }
}