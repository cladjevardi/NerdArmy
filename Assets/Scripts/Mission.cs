using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Mission : MonoBehaviour
{
    /// <summary>The game object of the tileMap.</summary>
    private GameObject tileMapObject = null;

    /// <summary>The entire map and list of units and enemies.</summary>
    private TileMap tileMap = null;

    /// <summary>The canvas for UI.</summary>
    private GameObject canvas = null;

    /// <summary>The sideBar for button UI.</summary>
    private GameObject sideBar = null;

    /// <summary>Whether the mission is transitioning between factions.</summary>
    private bool transitioning = false;

    /// <summary>Movement not yet done.</summary>
    private List<AStarVector> currentPathing = new List<AStarVector>();

    /// <summary>The currently selected actor.</summary>
    private Actor currentlySelectedActor = null;

    /// <summary>The actor to attack after applying movement.</summary>
    private Actor actorToAttack = null;

    /// <summary>The actor that is being dragged.</summary>
    private Actor draggedActor = null;

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

    /// <summary>An async movement call that moves a Mesh from its current position, to the next.</summary>
    /// <param name="actor">The actor to move.</param>
    /// <param name="end">The end position for the mesh to move towards.</param>
    /// <returns>Coruitine.</returns>
    private IEnumerator SmoothMovement(Actor actor, Vector3 end, bool withinRange)
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

        // Determing if we should stop early because we are within range.
        if (withinRange)
            currentPathing.Clear();

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

        // Add a small bit of wait to let the player process what
        // happened in the turn.
        yield return new WaitForSeconds(.75f);

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
        } while (!tileMap.IsFactionActive(currentFaction));

        // Reset all actors before initiating the next factions turn.
        tileMap.ResetActorsDone();

        return true;
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
        return tileMap.GetActor(tile.transform.position);
    }

    /// <summary>
    /// Convert world mouse coordinate to Tile.
    /// </summary>
    /// <param name="mousePosition">The world coordinate to lookup.</param>
    /// <returns>
    /// Returns the Tile at the given location. Returns null if
    /// no Tile is found.
    /// </returns>
    private Tile WorldCoordinateToTile(Vector3 mousePos)
    {
        // Convert to vector2.
        Vector2 mousePosition = new Vector2(mousePos.x, mousePos.y);

        // Initiate a raycast for any colliders.
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        // Return the Tile of the collider found.
        // TODO: Check if the UI buttons have been pressed.
        if (hit.collider != null)
        {
            Debug.Log(hit.collider.name);
            return hit.collider.gameObject.GetComponent<Tile>();
        }

        return null;
    }

    /// <summary>Get the Tile selected when clicking on the tilemap.</summary>
    /// <returns>Returns the Tile selected.</returns>
    private Tile GetTileSelected()
    {
        if (Input.GetMouseButtonUp(0) && !mouseDrag)
        {
            return WorldCoordinateToTile(
                Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        return null;
    }

    /// <summary>Check if the current player has finished moving all of their actors.</summary>
    /// <returns>Returns whether all current factions actors have made their moves.</returns>
    private bool CheckIfDone()
    {
        // Iterate through all of the current players actors for their done state.
        foreach (Actor actor in tileMap.actors)
        {
            // Do not move to transition screen before animations are finished.
            if (actor.moving || actor.animatingDamage || currentPathing.Count != 0)
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
        attacked.TakeDamage(damage);
        Debug.LogFormat("Damage dealt: {0}", damage);

        // Check if the unit will be dead. Damage is truely dealt
        // after the damage animation is complete.
        if (attacked.health - damage <= 0)
            tileMap.RemoveActor(attacked);
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

            // Issue the currently selected actor as finished for this
            // turn. On animation complete of the attack, the actor
            // will be set to done.
            //currentlySelectedActor.done = true;
            currentlySelectedActor = null;
            tileMap.RemoveAllHighlights();
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

                // Apply that smooth movement.
                StartCoroutine(SmoothMovement(currentlySelectedActor, currentPathing[0].position,
                    actorToAttack == null ? false : currentPathing[0].withinRange));
                
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
            tileMap.ClearCurrentPath();
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

            // Detect if the mouse is being dragged from an actor we can move.
            Tile tile = WorldCoordinateToTile(mouseOriginalPosition);
            if (tile)
            {
                Actor actor = tileMap.GetActor(tile.transform.position);
                if (actor != null && actor.owner == currentFaction && !actor.done)
                    draggedActor = tileMap.GetActor(tile.transform.position);
            }
        }

        // Perform dragging of camera position.
        if (mouseDrag)
        {
            if (draggedActor == null)
            {
                // Find the difference of the mouse movement and adjust the camera.
                Vector3 difference = (Camera.main.ScreenToWorldPoint(Input.mousePosition)) - Camera.main.transform.position;
                Camera.main.transform.position = mouseOriginalPosition - difference;

                // Clamp the camera position to the bounds of the tilemap.
                Camera.main.transform.position = new Vector3(
                    Mathf.Clamp(Camera.main.transform.position.x, 1, tileMap.width),
                    Mathf.Clamp(Camera.main.transform.position.y, 0, tileMap.height),
                    Camera.main.transform.position.z);
            }
            else
            {
                // Handle screen movement when hovering over edge of the
                // visible play area.
                Tile tile = WorldCoordinateToTile(mousePosition);
                if (tile != null)
                {
                    // This auto-selects the dragged actor as the
                    // current actor.
                    if (currentlySelectedActor != draggedActor)
                    {
                        tileMap.RemoveAllHighlights();
                        currentlySelectedActor = draggedActor;
                        DisplayHighlights();
                    }

                    // Display arrows
                    tileMap.ShowPath(draggedActor.transform.position,
                        tile.transform.position, currentFaction,
                        draggedActor.flying);
                }
            }

            // We handle the drag event.
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determine if we need to issue the mouse up event at the end of a
    /// dragged actor.
    /// </summary>
    /// <param name="tile">The new tile that was mouse released on.</param>
    /// <param name="actor">The new actor that was mouse released on.</param>
    private bool DraggedActorMouseUp(ref Tile tile, ref Actor actor)
    {
        // We let go of the mouse button with a dragged actor, reassign tile.
        if (draggedActor != null)
        {
            tile = GetTileSelected();
            actor = GetSelectedActor(tile);
            draggedActor = null;

            // Ignore the mouse up when mouse up is issued on an attack tile
            // with no one on it. We do not want to issue a deselect of the
            // currently selected unit.
            if (tile != null && !tile.movementHighlight && actor == null)
                return true;
        }

        return false;
    }

    /// <summary>Check the update state to determine an actor selection.</summary>
    /// <param name="actor">The clicked actor (could be null)</param>
    /// <returns>Returns whether the update loop should process the current frame as handled.</returns>
    private bool ActorSelected(Actor actor)
    {
        if (currentlySelectedActor == null && actor != null)
        {
            currentlySelectedActor = actor;
            return true;
        }
        return false;
    }

    /// <summary>Display the highlights for the currently selected actor.</summary>
    private void DisplayHighlights()
    {
        // The currently selected actor can be either the owners or any
        // other faction. We allow selecting non-owned units to see their
        // movements/attacks to base decisions from.
        if (currentlySelectedActor != null
            && !tileMap.displayingHighlights
            && !currentlySelectedActor.done)
        {
            // Display the actors movement and attack highlights.
            tileMap.HighlightActor(currentlySelectedActor);
        }

        // TODO: REMOVE ME WHEN ADDING END TURN BUTTON.
        if (currentlySelectedActor != null
            && Input.GetMouseButtonUp(1))
        {
            currentlySelectedActor.done = true;
            tileMap.RemoveAllHighlights();
        }
    }

    /// <summary>Whether the currently selected actor should be deselected.</summary>
    /// <param name="actor">The clicked actor within this update loop.</param>
    /// <param name="tile">The clicked tile within this update loop.</param>
    /// <returns>Returns whether the update loop should process the current frame as handled.</returns>
    private bool ShouldUnselectUnit(Actor actor, Tile tile)
    {
        if (currentlySelectedActor != null
            && actor == null
            && tile != null)
        {
            // Unselect the current actor and hide all highlights.
            currentlySelectedActor = null;
            tileMap.RemoveAllHighlights();
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
        if (currentlySelectedActor != null
            && !currentlySelectedActor.done
            && actor != null
            && currentlySelectedActor.owner == currentFaction
            && actor.owner != currentlySelectedActor.owner
            && tile != null
            && tile.attackHighlight)
        {
            // Calculate fuzzy pathing.
            Vector2 bestPosition = actor.transform.position;
            List<AStarVector> pathing = tileMap.GetBestPath(
                currentlySelectedActor.transform.position,
                actor.transform.position, ref bestPosition);
            
            // Track our movement that needs to be applied.
            if (pathing.Count != 0)
                currentPathing = pathing;

            // Queue up our actor to attack once we apply our pathing.
            actorToAttack = actor;

            // Deselect our currently selected actor.
            tileMap.RemoveAllHighlights();
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
        if (currentlySelectedActor != null
            && tile != null
            && tile.movementHighlight
            && actor == null)
        {
            // Find the nearest path to the tile selected.
            Astar pathing = new Astar(tileMap,
                currentlySelectedActor.transform.position,
                tile.transform.position, currentFaction,
                currentlySelectedActor.flying);
            pathing.result.RemoveAt(0); // Ignore first entry.

            // Track our movement that needs to be applied.
            if (pathing.result.Count != 0)
                currentPathing = pathing.result;

            // Movement is complete. Remove all current highlights until
            // we reach our destination.
            currentlySelectedActor.movementDone = true;
            tileMap.RemoveAllHighlights();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Determine if anyone is moving.
    /// </summary>
    /// <returns>Returns whether any actor is moving.</returns>
    private bool IsAnyoneMoving()
    {
        foreach (Actor actor in tileMap.actors)
        {
            if (actor.moving
                || actor.animatingDamage
                || actor.animatingAttacking)
                return true;
        }
        return false;
    }

    /// <summary>If the AI should select the next actor.</summary>
    /// <returns>Whether a new actor was selected.</returns>
    private bool SelectingActor()
    {
        if (currentlySelectedActor == null
            || (currentlySelectedActor != null
                && currentlySelectedActor.done))
        {
            currentlySelectedActor = GetNextActor();

            // TODO: Determine if we need to move the camera
            // to the location of the selected unit.
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
        foreach (Actor actor in tileMap.actors)
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

    /// <summary>Check if the computer should pass immediately.</summary>
    /// <returns>Whether the computer passed.</returns>
    private bool ShouldPass()
    {
        if (currentlySelectedActor != null
            && currentlySelectedActor.strategy == ActorStrategy.NONE)
        {
            currentlySelectedActor.done = true;
            tileMap.RemoveAllHighlights();
            return true;
        }
        return false;
    }

    /// <summary>Issue an attack from current tile.</summary>
    /// <returns>Returns whether an attack from current range was issued.</returns>
    private bool CanIssueAttackFromCurrentRange()
    {
        if (currentlySelectedActor != null)
        {
            return IssueAnAttackWithinRange(tileMap.GetAttackTiles(currentlySelectedActor));
        }
        return false;
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
            Actor actor = tileMap.GetActor(position);
            Tile tile = tileMap.GetTile(position);
            if (actor != null
                && actor.owner != currentFaction)
            {
                if (ShouldAttackUnit(actor, tile))
                    return true;
            }
        }

        return false;
    }

    /// <summary>Check if we can execute an AI strategy.</summary>
    /// <returns>Returns whether we took an action.</returns>
    private bool CanExecuteStrategy()
    {
        if (currentlySelectedActor != null
            && !currentlySelectedActor.movementDone)
        {
            // By this point we've already checked for attacks within our range. Pass.
            if (currentlySelectedActor.strategy == ActorStrategy.WAITS)
                currentlySelectedActor.movementDone = true;

            // Check if within our movement range we can issue an attack.
            List<Vector2> movementTiles = tileMap.GetMovementTiles(currentlySelectedActor);
            if (currentlySelectedActor.strategy != ActorStrategy.COWERS
                && IssueAnAttackWithinRange(tileMap.GetAttackTiles(
                    currentlySelectedActor, movementTiles)))
            {
                return true;
            }

            // Issue a move. Choose the tile that best suits our AI strategy.
            Tile tile = GetBestMovementTileBasedOnStrategy(movementTiles);

            // We're an archer or something that's boxed in/cannot move. Pass.
            if (tile == null)
                currentlySelectedActor.movementDone = true;

            // Issue the movement.
            IssueMove(null, tile);
            return true;
        }
        return false;
    }

    /// <summary>Determine the best tile to move to based on AI strategy.</summary>
    /// <param name="movementTiles">The list of movement tiles to choose from.</param>
    /// <returns>The tile that best suits the AI's playstyle.</returns>
    private Tile GetBestMovementTileBasedOnStrategy(List<Vector2> movementTiles)
    {
        // Convert a list of movement tiles to a list of potential movement
        // tiles based on some threat/attacker calculations.
        List<TileInfo> possibilities = tileMap.CalculateThreat(movementTiles, currentFaction);
        if (possibilities.Count == 0)
            return null;

        // Get our selected movement tile based on the strategy applied.
        if (currentlySelectedActor.strategy == ActorStrategy.CHARGE_IN)
        {
            // Choose the tile with the highest threat. Get close
            // to the enemy.
            possibilities.Reverse();
            return tileMap.GetTile(possibilities[0].position);
        }
        else if (currentlySelectedActor.strategy == ActorStrategy.COWERS)
        {
            // Choose the tile that has the least threat and attackers always.
            return tileMap.GetTile(possibilities[0].position);
        }
        else if (currentlySelectedActor.strategy == ActorStrategy.CAUTIOUS)
        {
            // Choose a tile that is as close as possible without invoking
            // as many attackers.
            return tileMap.GetTile(possibilities[0].position);
        }
        else
        {
            // Choose a random tile to move to.
            int value = Random.Range(0, possibilities.Count);
            return tileMap.GetTile(possibilities[value].position);
        }
    }

    /// <summary>Handle a human game loop frame.</summary>
    private void UpdateHuman()
    {
        // Figure out if we're clicking on an actor.
        Tile tile = GetTileSelected();
        Actor actor = GetSelectedActor(tile);

        // Detect mouse drag inputs. Move the camera.
        if (DragDetection())
            return;

        // We let go of the mouse button with a dragged actor.
        // This will reassign tile and actor underneath.
        if (DraggedActorMouseUp(ref tile, ref actor))
            return;

        // Detect if an actor was selected (assuming no actor is selected
        // currently).
        if (ActorSelected(actor))
            return;

        // Detect if we need to display highlights.
        DisplayHighlights();

        // While actor was selected, another actor was selected.
        if (ShouldAttackUnit(actor, tile))
            return;

        // We clicked on a valid movement tile for the selected actor.
        if (IssueMove(actor, tile))
            return;

        // We clicked off the highlights. Unselect.
        if (ShouldUnselectUnit(actor, tile))
            return;
    }

    /// <summary>Handle a computer game loop frame.</summary>
    private void UpdateComputer()
    {
        // Select the next actor.
        if (SelectingActor())
            return;

        // Display the highlights of the unit.
        DisplayHighlights();

        // Check if we should do anything.
        if (ShouldPass())
            return;

        // If we can attack from where we currently are, do it.
        if (CanIssueAttackFromCurrentRange())
            return;

        // Issue a move based on the strategy if can.
        if (CanExecuteStrategy())
            return;

        // A movement was issued and no one is withen attack range.
        // Just pass.
        if (currentlySelectedActor != null)
        {
            currentlySelectedActor.done = true;
            tileMap.RemoveAllHighlights();
        }
    }

    /// <summary>Unity event representing creation of a mission object.</summary>
    private void Awake()
    {
        // Create the TileMap object.
        tileMapObject = new GameObject("TileMap");
        tileMapObject.transform.SetParent(transform);
        tileMap = tileMapObject.AddComponent<TileMap>();

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

        // Check if the current faction exists on the battlefield.
        if (!tileMap.IsFactionActive(currentFaction))
            return;

        // Check if there are no actors left to move;
        if (CheckIfDone())
            StartCoroutine(TransitionTurns());

        // If we are currently moving a unit.
        if (ActorCurrentlyMoving())
            return;

        // If we've queued up an attack.
        if (ActorCurrentlyAttacking())
            return;

        // Actors could still be moving by this point.
        // Stop until they are done done.
        if (IsAnyoneMoving())
            return;

        Player turn = GetTurn(currentFaction);
        if (turn == Player.HUMAN)
            UpdateHuman();
        if (turn == Player.COMPUTER)
            UpdateComputer();
    }

    /// <summary>Initialize a mission.</summary>
    /// <param name="roster">The current list of units to add to the mission.</param>
    /// <param name="missionSchematic">The schematic that represents how to create this mission.</param>
    public void Initialize(List<Unit> roster, MissionSchematic missionSchematic)
    {
        // Setup and draw the tilemap according to the mission.
        tileMap.Initialize(missionSchematic, roster);
        
        // Set the current player.
        currentFaction = missionSchematic.startingPlayer;
    }
}
