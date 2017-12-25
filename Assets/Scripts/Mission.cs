using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mission : MonoBehaviour
{
    public class ThreatTile
    {
        public Vector2 position = Vector2.zero;
        public double threat = 0f;
        public int attackers = 0;
    }

    /// <summary>The entire map and list of units and enemies.</summary>
    private GameObject tileMap = null;

    /// <summary>The canvas for UI.</summary>
    private GameObject canvas = null;

    /// <summary>The sideBar for button UI.</summary>
    private GameObject sideBar = null;

    /// <summary>Whether the mission is transitioning between factions.</summary>
    private bool transitioning = false;

    /// <summary>The list of units in the mission.</summary>
    private List<Actor> actors = new List<Actor>();

    /// <summary>Movement not yet done.</summary>
    private List<AStarVector> currentPathing = new List<AStarVector>();

    /// <summary>The currently selected actor.</summary>
    private Actor currentlySelectedActor = null;

    /// <summary>The actor to attack after applying movement.</summary>
    private Actor actorToAttack = null;

    /// <summary>Current players turn.</summary>
    private Owner currentFaction = Owner.NONE;

    /// <summary>Faction 1 player.</summary>
    private Player faction1 = Player.HUMAN;

    /// <summary>Faction 2 player.</summary>
    private Player faction2 = Player.COMPUTER;

    /// <summary>Faction 3 player.</summary>
    private Player faction3 = Player.COMPUTER;

    /// <summary>Faction 4 player.</summary>
    private Player faction4 = Player.COMPUTER;

    /// <summaryThe original global position of the mouse at click down.</summary>
    private Vector3 mouseOriginalPosition = Vector3.zero;

    /// <summary>Keeps track of if the mouse is down.</summary>
    private bool mouseDown = false;

    /// <summary>Keeps track of if the mouse has been dragged.</summary>
    private bool mouseDrag = false;

    /// <summary>Add loadout roster of actors to the map.</summary>
    /// <param name="roster">The list of player controlled units.</param>
    /// <param name="validSpawnPositions">The list of valid rost spawn locations.</param>
    private void AddRoster(List<Unit> roster, List<Vector2> validSpawnPositions)
    {
        int spawnIndex = 0;

        // Iterate through each member of the roster and add units.
        foreach (Unit unit in roster)
        {
            // We can only spawn as many actors as available spawns.
            if (spawnIndex >= validSpawnPositions.Count)
                break;

            // Create the new actor.
            Vector2 position = validSpawnPositions[spawnIndex];
            string objectName = "Actor_" + Owner.PLAYER1 + "_" + unit.type.ToString();
            Actor actor = new GameObject(objectName).AddComponent<Actor>();
            actor.transform.SetParent(transform);
            actor.transform.position = position;
            actor.unit = unit;
            actor.owner = Owner.PLAYER1;
            actor.health = unit.baseMaxHealth;

            // Add the actor to the mission.
            actors.Add(actor);

            // Increment the spawn location to prevent collision.
            spawnIndex++;
        }
    }

    /// <summary>Add enemy actors to the map.</summary>
    /// <param name="missionEnemies">The list of enemies.</param>
    private void AddEnemies(List<MissionEnemy> missionEnemies)
    {
        // For each enemy unit within the mission schematic, add a new Actor.
        foreach (MissionEnemy enemy in missionEnemies)
        {
            // Create a new actor.
            Unit unit = new Unit(enemy.type);
            string objectName = "Actor_" + Owner.PLAYER2 + "_" + unit.type.ToString();
            Actor actor = new GameObject(objectName).AddComponent<Actor>();
            actor.transform.SetParent(transform);
            actor.transform.position = enemy.position;
            actor.unit = unit;
            actor.owner = Owner.PLAYER2;
            actor.health = unit.baseMaxHealth;

            // Add the actor to the mission.
            actors.Add(actor);
        }
    }

    /// <summary>An async movement call that moves a Mesh from its current position, to the next.</summary>
    /// <param name="actor">The actor to move.</param>
    /// <param name="end">The end position for the mesh to move towards.</param>
    /// <returns>Coruitine.</returns>
    private IEnumerator SmoothMovement(Actor actor, Vector3 end)
    {
        Debug.LogFormat("Moving to {0}", end.ToString());

        // Tell anyone who accesses the mesh that its moving.
        actor.moving = true;

        // We need to apply the grid scale to animation.
        Vector3 realEnd = end;

        // While that distance is greater than a very small amount (Epsilon, almost zero):
        while (actor.transform.position != realEnd)
        {
            // Find a new position proportionally closer to the end, based on the moveTime
            Vector2 newPostion = Vector2.MoveTowards(
                actor.transform.position, end, (1f / actor.speed) * Time.deltaTime);

            // Call MovePosition on attached Rigidbody2D and move it to the calculated position.
            actor.transform.position = newPostion;

            // Return and loop until sqrRemainingDistance is close enough to zero to end the function
            yield return null;
        }

        // If this is the last movement. Set to idle.
        if (currentPathing.Count == 0)
            actor.SetAnimation(ActorAnimation.IDLE);

        // Tell everyone that moving is complete.
        actor.moving = false;
    }

    /// <summary>
    /// An async transition call that switches players and draws whose turn it is.
    /// </summary>
    /// <returns>Coruitine.</returns>
    private IEnumerator TransitionTurns()
    {
        transitioning = true;

        // Reset any dragging effects while transitioning.
        mouseDown = false;
        mouseDrag = false;
        mouseOriginalPosition = Vector3.zero;

        // Clear out any currently selected actor.
        currentlySelectedActor = null;

        // Set the current player to the next players turn.
        if (UpdateCurrentFaction())
        {
            // Create a new backdrop object.
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            GameObject backdrop = CreateBackdrop(canvasRect);
            GameObject text = CreateText(canvasRect);
            RectTransform backdropRect = backdrop.GetComponent<RectTransform>();
            RectTransform textRect = text.GetComponent<RectTransform>();

            // End position for text
            Vector3 endBackdrop = new Vector3(canvasRect.rect.x - 500, canvasRect.rect.height / 2, -0.1f);
            Vector3 endText = new Vector3(canvasRect.rect.width + 250, canvasRect.rect.height / 2, -0.11f);

            // While that distance is greater than a very small amount (Epsilon, almost zero):
            while (backdropRect.transform.position.x != endBackdrop.x)
            {
                // Create a speed value based on the distance from the center
                // of the canvas.
                Vector2 canvasCenter = new Vector2(canvasRect.rect.width / 2, textRect.transform.position.y);
                float speed = Mathf.Abs(Vector2.Distance(
                    textRect.transform.position, canvasCenter)) * 5 + 20;

                // Find a new position to move to.
                Vector2 newTextPostion = Vector2.MoveTowards(
                    textRect.transform.position, endText, speed * Time.deltaTime);
                Vector2 newBackdropPostion = Vector2.MoveTowards(
                    backdropRect.transform.position, endBackdrop, speed * Time.deltaTime);
                textRect.transform.position = newTextPostion;
                backdropRect.transform.position = newBackdropPostion;

                // Return and loop until sqrRemainingDistance is close enough to zero to end the function
                yield return null;
            }

            Debug.LogFormat("Transitioning to {0}", currentFaction.ToString());

            DestroyObject(backdrop);
            DestroyObject(text);

            transitioning = false;
        }
    }

    /// <summary>Create the backdrop of the transition text.</summary>
    /// <param name="canvasRect">The rect of the canvas.</param>
    /// <returns>Returns the new object for the backdrop.</returns>
    private GameObject CreateBackdrop(RectTransform canvasRect)
    {
        GameObject transitionBackdrop = new GameObject("TransitionBackdrop");
        transitionBackdrop.transform.SetParent(canvas.gameObject.transform);
        Image backdrop = transitionBackdrop.AddComponent<Image>();
        backdrop.material = GameManager.instance.effectMaterials[7];
        backdrop.transform.position = new Vector3(0, 0, -0.1f);
        RectTransform backdropRect = backdrop.GetComponent<RectTransform>();
        backdropRect.transform.position = new Vector3(
            canvasRect.rect.width + 500,
            canvasRect.rect.height / 2, -0.1f);
        backdropRect.sizeDelta = new Vector2(1038, 116);
        return transitionBackdrop;
    }

    /// <summary>Create the text object for the transition.</summary>
    /// <param name="canvasRect">The rect of the canvas.</param>
    /// <returns>Returns the new object for the transition text.</returns>
    private GameObject CreateText(RectTransform canvasRect)
    {
        GameObject transitionTextImage = new GameObject("TransitionText");
        transitionTextImage.transform.SetParent(canvas.gameObject.transform);
        Image textImage = transitionTextImage.AddComponent<Image>();

        Player turn = GetTurn(currentFaction);
        if (turn == Player.HUMAN)
        {
            textImage.material = GameManager.instance.effectMaterials[5];
            RectTransform textImageRect = textImage.GetComponent<RectTransform>();
            textImageRect.transform.position = new Vector3(
                canvasRect.rect.x - 250,
                canvasRect.rect.height / 2, -0.11f);
            textImageRect.sizeDelta = new Vector2(340, 50);
        }
        else
        {
            textImage.material = GameManager.instance.effectMaterials[4];
            RectTransform textImageRect = textImage.GetComponent<RectTransform>();
            textImageRect.transform.position = new Vector3(
                canvasRect.rect.x - 250,
                canvasRect.rect.height / 2, -0.11f);
            textImageRect.sizeDelta = new Vector2(414, 54);
        }

        return transitionTextImage;
    }

    /// <summary>Returns whether a given faction exists on the battlefield.</summary>
    /// <param name="faction">The faction to check.</param>
    /// <returns>Returns whether the faction is still active.</returns>
    private bool IsFactionActive(Owner faction)
    {
        foreach (Actor actor in actors)
        {
            if (actor.owner == faction)
                return true;
        }

        return false;
    }

    /// <summary>Get the player of the faction specified.</summary>
    /// <param name="currentFaction">The faction to check.</param>
    /// <returns>Returns the player of the faction.</returns>
    private Player GetTurn(Owner currentFaction)
    {
        Player turn = Player.NONE;
        switch (currentFaction)
        {
            case Owner.PLAYER1:
                turn = faction1;
                break;
            case Owner.PLAYER2:
                turn = faction2;
                break;
            case Owner.PLAYER3:
                turn = faction3;
                break;
            case Owner.PLAYER4:
                turn = faction4;
                break;
        }

        return turn;
    }

    /// <summary>
    /// Adjust current players turn. This cycles through all factions
    /// until a different active faction is selected. This also detects
    /// if end game has been reached and who the winner is.
    /// </summary>
    /// <returns>Returns whether the faction was successfully cycled.</returns>
    private bool UpdateCurrentFaction()
    {
        // Cycle through each faction until a faction exists.
        Owner previousFaction = currentFaction;
        do
        {
            switch (currentFaction)
            {
                case Owner.PLAYER1:
                    currentFaction = Owner.PLAYER2;
                    break;
                case Owner.PLAYER2:
                    currentFaction = Owner.PLAYER3;
                    break;
                case Owner.PLAYER3:
                    currentFaction = Owner.PLAYER4;
                    break;
                case Owner.PLAYER4:
                    currentFaction = Owner.PLAYER1;
                    break;
            }

            // We looped over all possible factions and couldn't find one. Game over!
            if (previousFaction == currentFaction)
            {
                // Display the victor or defeat text.
                //Text text = transitionText.GetComponent<Text>();
                //Player turn = GetTurn(currentFaction);
                //if (turn == Player.HUMAN)
                //    text.text = "Mission Complete!";
                //if (turn == Player.COMPUTER)
                //    text.text = "Mission Failure!";

                return false;
            }
        } while (!IsFactionActive(currentFaction));

        // Reset all actors before initiating the next factions turn.
        foreach (Actor actor in actors)
        {
            actor.done = false;
        }

        return true;
    }

    /// <summary>Get the actor at the given coordinate.</summary>
    /// <param name="position">The coordinate to check.</param>
    /// <returns>Returns the actor at the given coordinate if found or null if not.</returns>
    private Actor GetActor(Vector2 position)
    {
        foreach (Actor actor in actors)
        {
            if (actor.transform.position.x == position.x
                && actor.transform.position.y == position.y)
            {
                // This may be an enemies actor or a players actor.
                // Display their current highlights.
                return actor;
            }
        }

        return null;
    }

    /// <summary>Get the selected actor from the position within Tile.</summary>
    /// <param name="tile">The tile to check for an actor.</param>
    /// <returns>Returns the actor found, or null if not.</returns>
    private Actor GetSelectedActor(Tile tile)
    {
        // Get the currently selected tile, if any.
        if (tile == null)
            return null;

        // Get the position of the tile selected and look for any actors currently
        // positioned on that tile.
        return GetActor(tile.transform.position);
    }

    /// <summary>Get the Tile selected when clicking on the tilemap.</summary>
    /// <returns>Returns the Tile selected.</returns>
    private Tile GetTileSelected()
    {
        if (Input.GetMouseButtonUp(0) && !mouseDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            // Initiate a raycast for any colliders.
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            // Return the Tile of the collider found.
            // TODO: Check if the UI buttons have been pressed.
            if (hit.collider != null)
            {
                Debug.Log(hit.collider.name);
                return hit.collider.gameObject.GetComponent<Tile>();
            }
        }

        return null;
    }

    /// <summary>Check if the current player has finished moving all of their actors.</summary>
    /// <returns>Returns whether all current factions actors have made their moves.</returns>
    private bool CheckIfDone()
    {
        // Iterate through all of the current players actors for their done state.
        foreach (Actor actor in actors)
        {
            if (actor.moving || currentPathing.Count != 0)
                return false;

            if (actor.owner == currentFaction && !actor.done)
                return false;
        }

        return true;
    }

    /// <summary>Helper function for dealing damage to an actor.</summary>
    /// <param name="attacker">The unit initiating the attack.</param>
    /// <param name="attacked">The unit recieving the damage.</param>
    private void ApplyDamage(Actor attacker, Actor attacked)
    {
        // Calculate the total damage to deal.
        int damage = (int)Mathf.Clamp(
            attacker.unit.baseDamage - attacked.unit.baseArmor,
            0, float.MaxValue);

        // Apply damage to unit.
        attacked.health -= damage;
        Debug.LogFormat("Damage dealt: {0}", damage);

        // Check if the unit is dead.
        if (attacked.health <= 0)
        {
            // Find the actor within the list and remove it.
            for (int index = 0; index < actors.Count; ++index)
            {
                if (actors[index] == attacked)
                {
                    DestroyObject(actors[index]);
                    actors.RemoveAt(index);
                    break;
                }
            }
        }
    }

    /// <summary>Check the update state to determine if an move and attack was issued.</summary>
    /// <returns>Returns whether the update loop should process the current frame as handled.</returns>
    private bool ActorCurrentlyAttacking()
    {
        if (actorToAttack != null)
        {
            // Display attacking animation.
            currentlySelectedActor.SetAnimation(ActorAnimation.ATTACK);

            // Deal the damage.
            ApplyDamage(currentlySelectedActor, actorToAttack);

            // Unselect the attacked unit.
            actorToAttack = null;

            // Issue the currently selected actor as finished for this turn.
            currentlySelectedActor.done = true;
            currentlySelectedActor = null;
            tileMap.GetComponent<TileMap>().RemoveAllHighlights();
            return true;
        }

        return false;
    }

    /// <summary>Checks the update state to determine if an actor is currently moving.</summary>
    /// <returns>Returns whether the update loop should process the current frame as handled.</returns>
    private bool ActorCurrentlyMoving()
    {
        if (currentlySelectedActor != null
            && currentPathing.Count > 0)
        {
            // Check if we're ready to issue the next move.
            if (!currentlySelectedActor.moving)
            {
                // Set the animation to walking
                if (currentPathing[0].direction == AStarDirection.EAST)
                {
                    currentlySelectedActor.facing = ActorFacing.EAST;
                    currentlySelectedActor.SetAnimation(ActorAnimation.WALKING);
                }
                else if (currentPathing[0].direction == AStarDirection.WEST)
                {
                    currentlySelectedActor.facing = ActorFacing.WEST;
                    currentlySelectedActor.SetAnimation(ActorAnimation.WALKING);
                }

                // If this is a movement and attack, see if we can just stop here and attack.
                if (actorToAttack != null)
                {
                    // Check and see if we can stop here!
                    Actor actor = GetActor(new Vector2(currentlySelectedActor.transform.position.x, currentlySelectedActor.transform.position.y));
                    if (currentPathing.Count <= currentlySelectedActor.unit.baseMaxRange
                        && currentPathing.Count >= currentlySelectedActor.unit.baseMinRange
                        && actor != null)
                    {
                        // Stop all movement and go strait to attacking.
                        currentPathing.Clear();
                        return false;
                    }
                }

                // Apply that smooth movement.
                StartCoroutine(SmoothMovement(currentlySelectedActor, currentPathing[0].position));

                // After were done moving remove the first entry.
                currentPathing.RemoveAt(0);
            }

            // Still in the process of moving. Skip the rest of the Update until complete.
            return true;
        }

        return false;
    }

    /// <summary>Check the update state to determine mouse dragging inputs.</summary>
    /// <returns>Returns whether the update loop should process the current frame as handled.</returns>
    private bool DragDetection()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Check if we let go of the drag
        if (!Input.GetMouseButton(0)
            && mouseOriginalPosition != Vector3.zero)
        {
            mouseDown = false;
            mouseDrag = false;
            mouseOriginalPosition = Vector3.zero;
            return false;
        }
        // Check if the mouse button was pressed this frame.
        else if (Input.GetMouseButtonDown(0)
            && !mouseDown
            && mouseOriginalPosition == Vector3.zero)
        {
            mouseDown = true;
            mouseOriginalPosition = mousePosition;
            return false;
        }
        // Check if we've exceeding a specific distance of dragging.
        else if (mouseDown
            && Input.GetMouseButton(0)
            && !mouseDrag
            && Vector3.Distance(mousePosition, mouseOriginalPosition) >= .15f)
        {
            mouseDrag = true;
        }

        // Perform dragging of camera position.
        if (mouseDrag)
        {
            // Find the difference of the mouse movement and adjust the camera.
            Vector3 difference = (Camera.main.ScreenToWorldPoint(Input.mousePosition)) - Camera.main.transform.position;
            Camera.main.transform.position = mouseOriginalPosition - difference;

            // Clamp the camera position to the bounds of the tilemap.
            Camera.main.transform.position = new Vector3(
                Mathf.Clamp(Camera.main.transform.position.x, 1, tileMap.GetComponent<TileMap>().width),
                Mathf.Clamp(Camera.main.transform.position.y, 0, tileMap.GetComponent<TileMap>().height),
                Camera.main.transform.position.z);

            // We handle the drag event.
            return true;
        }

        return false;
    }

    /// <summary>Display the highlights for the currently selected actor.</summary>
    private void DisplayHighlights()
    {
        TileMap tileMapObject = tileMap.GetComponent<TileMap>();

        if (currentlySelectedActor != null
            && currentFaction == currentlySelectedActor.owner
            && !currentlySelectedActor.done)
        {
            // Display the actors movement and attack highlights.
            List<Vector2> movementTiles = tileMapObject.GetMovementTiles(currentlySelectedActor, actors);
            List<Vector2> attackTiles = tileMapObject.GetAttackTiles(movementTiles, currentlySelectedActor, actors);

            // First display all attack tiles.
            tileMapObject.HighlightTiles(attackTiles, TileHighlightColor.HIGHLIGHT_RED);

            // Display all movement tiles OVER the attack tiles. This takes into account enemies on tiles.
            tileMapObject.HighlightTiles(movementTiles, TileHighlightColor.HIGHLIGHT_BLUE);

            // Adjust the highlighted tiles images.
            tileMapObject.AdjustHighlightFrameIds();
        }
        else
        {
            // The actor is not owned by the current players. Display only the actors movement.
            tileMapObject.HighlightTiles(
                tileMapObject.GetMovementTiles(currentlySelectedActor, actors),
                TileHighlightColor.HIGHLIGHT_BLUE);
        }
    }

    /// <summary>Whether the currently selected actor should be deselected.</summary>
    /// <param name="actor">The clicked actor within this update loop.</param>
    /// <param name="tile">The clicked tile within this update loop.</param>
    /// <returns>Returns whether the update loop should process the current frame as handled.</returns>
    private bool ShouldUnselectUnit(Actor actor, Tile tile)
    {
        if (actor == null && tile != null)
        {
            // Unselect the current actor and hide all highlights.
            currentlySelectedActor = null;
            tileMap.GetComponent<TileMap>().RemoveAllHighlights();
            return true;
        }

        return false;
    }

    /// <summary>Determine if an attack was issued this game loop.</summary>
    /// <param name="actor">The clicked actor within this update loop.</param>
    /// <param name="tile">The clicked tile within this update loop.</param>
    /// <returns>Returns whether the update loop should process the current frame as handled.</returns>
    private bool ShouldAttackUnit(Actor actor, Tile tile)
    {
        // Check if we selected another actor to attack.
        if (!currentlySelectedActor.done
            && actor != null
            && currentlySelectedActor.owner == currentFaction
            && actor.owner != currentlySelectedActor.owner
            && tile != null
            && tile.attackHighlight)
        {
            // Find the nearest path to the unit you want to attack.
            Astar pathing = new Astar(tileMap.GetComponent<TileMap>(),
                currentlySelectedActor.transform.position, actor.transform.position, currentlySelectedActor.flying);
            pathing.result.RemoveAt(0); // Ignore first entry.

            // Track our movement that needs to be applied.
            if (pathing.result.Count != 0)
                currentPathing = pathing.result;

            // Queue up our actor to attack once we apply our pathing.
            actorToAttack = actor;

            // Deselect our currently selected actor.
            tileMap.GetComponent<TileMap>().RemoveAllHighlights();
            return true;
        }

        return false;
    }

    /// <summary>Determine if a move was issued.</summary>
    /// <param name="actor">The clicked actor within this update loop.</param>
    /// <param name="tile">The clicked tile within this update loop.</param>
    /// <returns>Returns whether the update loop should process the current frame as handled.</returns>
    private bool IssueMove(Actor actor, Tile tile)
    {
        // See if this was a simple movement click.
        if (tile != null
            && tile.movementHighlight
            && actor == null)
        {
            // Find the nearest path to the tile selected.
            Astar pathing = new Astar(tileMap.GetComponent<TileMap>(),
                currentlySelectedActor.transform.position, tile.transform.position,
                currentlySelectedActor.flying);
            pathing.result.RemoveAt(0); // Ignore first entry.

            // Track our movement that needs to be applied.
            if (pathing.result.Count != 0)
                currentPathing = pathing.result;

            // TODO: Show attack highlights only.
            //tileMap.GetComponent<TileMap>().ShowPath(currentlySelectedActor.transform.position, tile.transform.position);

            // For now issue the unit as done and remove highlights.
            currentlySelectedActor.done = true;
            tileMap.GetComponent<TileMap>().RemoveAllHighlights();
            return true;
        }
        return false;
    }

    /// <summary>Get the next available actor that hasn't moved.</summary>
    /// <returns>Returns the next available actor of the current player.</returns>
    private Actor GetNextActor()
    {
        // Iterate through actors for all actors from the current players
        // turn and return the first available actor that hasn't already
        // moved.
        foreach (Actor actor in actors)
        {
            if (actor.owner == currentFaction
                && !actor.done)
            {
                return actor;
            }
        }

        // No available actor left.
        return null;
    }

    /// <summary>
    /// Take a list of attack tiles and find ones that have an enemy on it
    /// and attack that.
    /// 
    /// TODO: Add smarts if multiple enemies are within range.
    /// </summary>
    /// <param name="attackTiles">
    /// The list of attack tiles the unit can issue and attack on.
    /// </param>
    /// <returns>Returns whether an attack was issued or not.</returns>
    private bool IssueAnAttackWithinRange(List<Vector2> attackTiles)
    {
        foreach (Vector2 position in attackTiles)
        {
            Actor actor = GetActor(position);
            Tile tile = tileMap.GetComponent<TileMap>().tiles[(int)position.x][(int)position.y];
            if (actor != null
                && actor.owner != currentFaction)
            {
                if (ShouldAttackUnit(actor, tile))
                    return true;
            }
        }

        return false;
    }

    /// <summary>Return least threatening tile based on enemies position.</summary>
    /// <param name="movementTiles">The list of available movement tiles.</param>
    /// <returns>Returns the least threatening position.</returns>
    private Vector2 GetLeastThreateningMovementTile(List<Vector2> movementTiles)
    {
        double leastThreat = 0;
        Vector2 leastThreateningPosition = Vector2.zero;
        foreach (Vector2 movementTile in movementTiles)
        {
            double currentThreat = 0;

            // Iterate through all the enemies and calculate their threat sum.
            foreach (Actor actor in actors)
            {
                if (actor.owner != currentFaction)
                {
                    // Use manhatten algorithm to determine threat
                    currentThreat += Math.Abs(movementTile.x - actor.transform.position.x)
                        + Math.Abs(movementTile.y - actor.transform.position.y);
                }
            }

            // Keep track of the best tile.
            if (currentThreat > leastThreat)
            {
                leastThreat = currentThreat;
                leastThreateningPosition = movementTile;
            }
        }

        return leastThreateningPosition;
    }

    /// <summary>Convert movement tiles to a list of threat ordered tiles for AI calculations.</summary>
    /// <param name="movementTiles">The list of tiles available.</param>
    /// <returns>Returns an ordered list of tiles and their threat.</returns>
    private List<ThreatTile> CalculateThreat(List<Vector2> movementTiles)
    {
        List<ThreatTile> threatTiles = new List<ThreatTile>();

        // Iterate through all the movement possibilities and assign data.
        foreach (Vector2 movementTile in movementTiles)
        {
            ThreatTile threatTile = new ThreatTile();
            threatTile.position = movementTile;
            foreach (Actor actor in actors)
            {
                if (actor.owner != currentFaction)
                {
                    // Use manhatten algorithm to determine threat
                    threatTile.threat += Math.Abs(movementTile.x - actor.transform.position.x)
                        + Math.Abs(movementTile.y - actor.transform.position.y);

                    // Determine if the enemy is within attack range of a position.
                    List<Vector2> enemyMovementTiles = tileMap.GetComponent<TileMap>().GetMovementTiles(actor, actors);
                    List<Vector2> enemyAttackTiles = tileMap.GetComponent<TileMap>().GetAttackTiles(enemyMovementTiles, actor, actors);
                    foreach (Vector2 enemyAttackTile in enemyMovementTiles)
                    {
                        if (movementTile.x == enemyAttackTile.x
                            && movementTile.y == enemyAttackTile.y)
                        {
                            // Increment the amount of attackers for a location.
                            threatTile.attackers++;
                            break;
                        }
                    }
                }
            }
            threatTiles.Add(threatTile);
        }

        // Order the list by lowest attackers then lowest threat.
        // Unfortunately I need to create a new list for this and remove
        // each entry from the old list.
        List<ThreatTile> threatList = new List<ThreatTile>();
        while(threatTiles.Count != 0)
        {
            int lowestIndex = 0;
            ThreatTile lowest = null;
            for(int index = 0; index < threatTiles.Count; ++index)
            {
                if (lowest == null)
                    lowest = threatTiles[index];
                if (threatTiles[index].attackers <= lowest.attackers
                    && threatTiles[index].threat < lowest.threat)
                {
                    lowest = threatTiles[index];
                    lowestIndex = index;
                }
            }

            threatList.Add(lowest);
            threatTiles.RemoveAt(lowestIndex);
        }

        return threatTiles;
    }

    /// <summary>Handle a human game loop frame.</summary>
    private void UpdateHuman()
    {
        // If we are currently moving a unit.
        if (ActorCurrentlyMoving())
            return;

        // If we've queued up an attack.
        if (ActorCurrentlyAttacking())
            return;

        // Figure out if we're clicking on an actor.
        Tile tile = GetTileSelected();
        Actor actor = GetSelectedActor(tile);

        // Detect mouse drag inputs. Move the camera.
        if (DragDetection())
            return;

        // If we do not have an actor check the tile for an actor.
        if (currentlySelectedActor == null
            && actor != null)
        {
            // An actor was selected.
            currentlySelectedActor = actor;
            DisplayHighlights();
            return;
        }
        else if (currentlySelectedActor != null)
        {
            // While actor was selected, another actor was selected.
            if (ShouldAttackUnit(actor, tile))
                return;

            // We clicked on a valid movement tile for the selected actor.
            if (IssueMove(actor, tile))
                return;

            // We clicked off the highlights. Unselect.
            ShouldUnselectUnit(actor, tile);
        }
    }

    /// <summary>Handle a computer game loop frame.</summary>
    private void UpdateComputer()
    {
        // If we are currently moving a unit.
        if (ActorCurrentlyMoving())
            return;

        // If we've queued up an attack.
        if (ActorCurrentlyAttacking())
            return;

        // Select the next actor.
        currentlySelectedActor = GetNextActor();
        DisplayHighlights();

        // Calculate the list of attack tiles.
        TileMap tileMapObject = tileMap.GetComponent<TileMap>();

        // Do nothing
        if (currentlySelectedActor.strategy == ActorStrategy.NONE)
        {
            // Sit there.
            currentlySelectedActor.done = true;
            tileMapObject.RemoveAllHighlights();
        }
        // Attack anything in site and move in!
        else if (currentlySelectedActor.strategy == ActorStrategy.CHARGE_IN)
        {
            List<Vector2> movementTiles = tileMapObject.GetMovementTiles(currentlySelectedActor, actors);
            List<Vector2> attackTiles = tileMapObject.GetAttackTiles(movementTiles, currentlySelectedActor, actors);

            if (!IssueAnAttackWithinRange(attackTiles))
            {
                // Calculate worst possible move.
                List<ThreatTile> possibilities = CalculateThreat(movementTiles);
                possibilities.Reverse(); // Reverse the order to get worst move in front.
                Tile tile = tileMapObject.tiles[(int)possibilities[0].position.x][(int)possibilities[0].position.y];

                // Issue a move as close as you can to an enemy without getting surrounded.
                IssueMove(currentlySelectedActor, tile);
            }
        }
        // Stand still and attack only those next to me.
        else if (currentlySelectedActor.strategy == ActorStrategy.WAITS)
        {
            List<Vector2> movementTiles = new List<Vector2>();
            movementTiles.Add(new Vector2(currentlySelectedActor.transform.position.x, currentlySelectedActor.transform.position.y));
            List<Vector2> attackTiles = tileMapObject.GetAttackTiles(movementTiles, currentlySelectedActor, actors);
            if (!IssueAnAttackWithinRange(attackTiles))
            {
                currentlySelectedActor.done = true;
                tileMapObject.RemoveAllHighlights();
            }
        }
        // Run away as far as possible. If cornered attack.
        else if (currentlySelectedActor.strategy == ActorStrategy.COWERS)
        {
            Vector2 leastThreateningPosition = GetLeastThreateningMovementTile(
                tileMapObject.GetMovementTiles(currentlySelectedActor, actors));

            // Check if cornered and issue an attack there.
            Tile tile = tileMapObject.tiles[(int)leastThreateningPosition.x][(int)leastThreateningPosition.y];
            List<Vector2> movementTiles = new List<Vector2>();
            movementTiles.Add(new Vector2(leastThreateningPosition.x, leastThreateningPosition.y));
            List<Vector2> attackTiles = tileMapObject.GetAttackTiles(movementTiles, currentlySelectedActor, actors);
            if (!IssueAnAttackWithinRange(attackTiles))
            {
                IssueMove(null, tile);
            }
        }
        // Move as close as can get with least threat. Make them come to you.
        else if (currentlySelectedActor.strategy == ActorStrategy.CAUTIOUS)
        {
            List<Vector2> movementTiles = tileMapObject.GetMovementTiles(currentlySelectedActor, actors);
            List<Vector2> attackTiles = tileMapObject.GetAttackTiles(movementTiles, currentlySelectedActor, actors);

            // TODO: Ideally we would want to calculate threat first and attack
            // from the best spot or against the highest value target or lowerest HP enemy.
            if (!IssueAnAttackWithinRange(attackTiles))
            {
                // Calculate best move that is just out of the range of most
                // attackers, and close enough to strike within the next round.
                List<ThreatTile> possibilities = CalculateThreat(movementTiles);
                Tile tile = tileMapObject.tiles[(int)possibilities[0].position.x][(int)possibilities[0].position.y];

                // Issue a move as close as you can to an enemy without getting surrounded.
                IssueMove(currentlySelectedActor, tile);
            }
        }
    }

    /// <summary>Unity event representing creation of a mission object.</summary>
    private void Awake()
    {
        // Create the TileMap object.
        tileMap = new GameObject("TileMap");
        tileMap.transform.SetParent(transform);
        tileMap.AddComponent<TileMap>();

        // Setup the Canvas for drawing UI elements.
        canvas = new GameObject("Canvas");
        canvas.transform.SetParent(transform);
        Canvas can = canvas.AddComponent<Canvas>();
        can.renderMode = RenderMode.ScreenSpaceOverlay;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Create the sidebar to contain the buttons.
        sideBar = new GameObject("Sidebar");
        sideBar.transform.SetParent(canvas.gameObject.transform);
        Image sideBarImage = sideBar.AddComponent<Image>();
        sideBarImage.material = GameManager.instance.uiMaterials[0];
        sideBarImage.transform.position = new Vector3(0, 0, -0.1f);
        RectTransform sidebarRect = sideBarImage.GetComponent<RectTransform>();
        sidebarRect.anchorMin = new Vector2(1, 0);
        sidebarRect.anchorMax = new Vector2(1, 1);
        sidebarRect.anchoredPosition = new Vector2(-64, 0);
        sidebarRect.sizeDelta = new Vector2(128, 0);
    }

    /// <summary>The game loop for a mission.</summary>
    private void Update()
    {
        // We have not fully initialized.
        if (currentFaction == Owner.NONE)
            return;

        // Check if we are transitioning between players.
        if (transitioning)
            return;

        if (IsFactionActive(currentFaction))
        {
            Player turn = GetTurn(currentFaction);
            if (turn == Player.HUMAN)
                UpdateHuman();
            if (turn == Player.COMPUTER)
                UpdateComputer();

            // Check if there are no actors left to move;
            if (CheckIfDone())
                StartCoroutine(TransitionTurns());
        }
    }

    /// <summary>Initialize a mission.</summary>
    /// <param name="roster">The current list of units to add to the mission.</param>
    /// <param name="missionSchematic">The schematic that represents how to create this mission.</param>
    public void Initialize(List<Unit> roster, MissionSchematic missionSchematic)
    {
        // Clear any previous map information.
        actors.Clear();

        // Setup and draw the tilemap according to the mission.
        tileMap.GetComponent<TileMap>().Initialize(missionSchematic);

        // Add the players units.
        AddRoster(roster, missionSchematic.rosterSpawns);

        // Add the enemy units.
        AddEnemies(missionSchematic.enemies);

        // Set the current player.
        currentFaction = missionSchematic.startingPlayer;
    }
}
