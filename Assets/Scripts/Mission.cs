using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission : MonoBehaviour
{
    /// <summary>The entire map and list of units and enemies.</summary>
    private GameObject tileMap = null;

    /// <summary>The transition screen.</summary>
    private GameObject canvas = null;

    /// <summary>Transition image object.</summary>
    private GameObject transitionImage = null;

    /// <summary>Whether the mission is transitioning between factions.</summary>
    private bool transitioning = false;

    /// <summary>The list of units in the mission.</summary>
    private List<Actor> actors = new List<Actor>();

    /// <summary>Movement not yet done.</summary>
    private List<AStarVector> currentPathing = null;

    /// <summary>The tile the currently selected actor is moving towards.</summary>
    private AStarVector currentMoving = null;

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

    /// <summary>
    /// Add loadout roster of actors to the map.
    /// </summary>
    /// <param name="roster">The list of player controlled units.</param>
    /// <param name="validSpawnPositions">
    /// The list of valid rost spawn locations.
    /// </param>
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

    /// <summary>
    /// Add enemy actors to the map.
    /// </summary>
    /// <param name="missionEnemies">The list of enemies.</param>
    private void AddEnemies(List<MissionEnemy> missionEnemies)
    {
        // For each enemy unit within the mission schematic, add a new Actor.
        foreach (MissionEnemy enemy in missionEnemies)
        {
            // Create a new actor.
            Unit unit = UnitFactory.Create(enemy.type);
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
    public IEnumerator SmoothMovement(Actor actor, Vector3 end)
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

        // Tell everyone that moving is complete.
        actor.moving = false;
    }

    public IEnumerator TransitionTurns()
    {
        transitioning = true;

        // Clear out any currently selected actor.
        currentlySelectedActor = null;

        // Set the current player to the next players turn.
        UpdateCurrentFaction();
        Debug.LogFormat("Transitioning to {0}", currentFaction.ToString());

        yield return new WaitForSeconds(2);
        transitioning = false;
    }

    private bool IsFactionActive(Owner faction)
    {
        foreach (Actor actor in actors)
        {
            if (actor.owner == faction)
                return true;
        }

        return false;
    }

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

    private void UpdateCurrentFaction()
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
                Debug.Log("Game over!");
                transitioning = true;
            }
        } while (!IsFactionActive(currentFaction));


        // Reset all actors before initiating the next factions turn.
        foreach (Actor actor in actors)
        {
            actor.done = false;
        }
    }

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

    private Actor GetSelectedActor(Tile tile)
    {
        // Get the currently selected tile, if any.
        if (tile == null)
            return null;

        // Get the position of the tile selected and look for any actors currently
        // positioned on that tile.
        return GetActor(tile.transform.position);
    }

    private bool CheckIfDone()
    {
        // Iterate through all of the current players actors for their done state.
        foreach (Actor actor in actors)
        {
            if (actor.moving || (currentPathing != null && currentPathing.Count != 0))
                return false;

            if (actor.owner == currentFaction && !actor.done)
                return false;
        }

        return true;
    }

    private bool ActorCurrentlyMoving()
    {
        if (currentPathing != null
            && currentPathing.Count > 0
            && currentlySelectedActor != null
            && !currentlySelectedActor.moving)
        {
            // Set the animation to walking
            if (currentPathing[0].direction == AStarDirection.EAST)
                currentlySelectedActor.SetAnimation(ActorAnimation.WALKING_WEST); // LOL

            // If this is a movement and attack, see if we can just stop here and attack.
            if (actorToAttack != null)
            {
                // Check and see if we can stop here!
                Actor actor = GetActor(new Vector2(currentlySelectedActor.transform.position.x, currentlySelectedActor.transform.position.y));
                if (currentPathing.Count <= currentlySelectedActor.unit.baseMaxRange
                    && currentPathing.Count >= currentlySelectedActor.unit.baseMinRange
                    && actor == null)
                {
                    currentPathing.Clear();
                }
            }

            // Apply that smooth movement.
            StartCoroutine(SmoothMovement(currentlySelectedActor, currentPathing[0].position));

            // After were done moving remove the first entry.
            currentPathing.RemoveAt(0);
            return true;
        }

        return false;
    }

    private bool ActorCurrentlyAttacking()
    {
        if (actorToAttack != null)
        {
            // Apply damage to unit.
            actorToAttack.health -= currentlySelectedActor.unit.baseDamage;

            // Check if the unit is dead.
            if (actorToAttack.health <= 0)
            {
                // Find the actor within the list and remove it.
                for (int index = 0; index < actors.Count; ++index)
                {
                    if (actors[index] == actorToAttack)
                        actors.RemoveAt(index);
                }
            }
            
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

    private void DisplayHighlights()
    {
        if (currentlySelectedActor != null
            && currentFaction == currentlySelectedActor.owner
            && !currentlySelectedActor.done)
        {
            // Display the actors movement and attack highlights.
            List<Vector2> movementTiles = tileMap.GetComponent<TileMap>().GetMovementTiles(currentlySelectedActor, actors);
            movementTiles.Add(new Vector2(currentlySelectedActor.transform.position.x, currentlySelectedActor.transform.position.y));
            List<Vector2> attackTiles = tileMap.GetComponent<TileMap>().GetAttackTiles(movementTiles, currentlySelectedActor, actors);
            tileMap.GetComponent<TileMap>().HighlightTiles(attackTiles, TileHighlightColor.HIGHLIGHT_RED);
            tileMap.GetComponent<TileMap>().HighlightTiles(tileMap.GetComponent<TileMap>().GetMovementTiles(currentlySelectedActor, actors), TileHighlightColor.HIGHLIGHT_BLUE);
        }
        else
        {
            // The actor is not owned by the current players. Display only the actors movement.
            tileMap.GetComponent<TileMap>().HighlightTiles(tileMap.GetComponent<TileMap>().GetMovementTiles(currentlySelectedActor, actors), TileHighlightColor.HIGHLIGHT_BLUE);
        }
    }

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

    private bool ShouldAttackUnit(Actor actor, Tile tile)
    {
        // Check if we selected another actor to attack.
        if (!currentlySelectedActor.done
            && actor != null
            && currentlySelectedActor.owner == currentFaction
            && actor.owner != currentlySelectedActor.owner
            && tile != null
            && tile.highlight
            && tile.highlightColor == TileHighlightColor.HIGHLIGHT_RED)
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

    private void IssueMove(Actor actor, Tile tile)
    {
        // See if this was a simple movement click.
        if (tile != null
            && tile.highlight
            && tile.highlightColor == TileHighlightColor.HIGHLIGHT_BLUE
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
            // For now issue the unit as done and remove highlights.
            currentlySelectedActor.done = true;
            tileMap.GetComponent<TileMap>().RemoveAllHighlights();
        }
        else
            ShouldUnselectUnit(actor, tile);
    }

    private void UpdateHuman()
    {
        // Display buttons and stuff.

        // If we are currently moving a unit.
        if (ActorCurrentlyMoving())
            return;

        if (ActorCurrentlyAttacking())
            return;

        // Figure out if we're clicking on an actor.
        Tile tile = tileMap.GetComponent<TileMap>().GetTileSelected();
        Actor actor = GetSelectedActor(tile);

        // If we do not have an actor check the tile for an actor.
        if (currentlySelectedActor == null
            && actor != null)
        {
            currentlySelectedActor = actor;
            DisplayHighlights();
            return;
        }
        else if (currentlySelectedActor != null)
        {
            if (ShouldAttackUnit(actor, tile))
                return;

            IssueMove(actor, tile);
        }
    }

    private void UpdateComputer()
    {
        foreach (Actor actor in actors)
        {
            if (actor.owner == currentFaction)
                actor.done = true;
        }
    }

    private void Awake()
    {
        // Create the TileMap object.
        tileMap = new GameObject("TileMap");
        tileMap.transform.SetParent(transform);
        tileMap.AddComponent<TileMap>();

        /*
        canvas = new GameObject("Canvas");
        canvas.transform.SetParent(transform);
        canvas.AddComponent<Canvas>();
        var rectTransform = canvas.AddComponent<RectTransform>();
        */
    }

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
