using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission : MonoBehaviour
{
    /// <summary>The entire map and list of units and enemies.</summary>
    private GameObject tileMap = null;

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
            actor.transform.parent = transform;
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
            actor.transform.parent = transform;
            actor.transform.position = enemy.position;
            actor.unit = unit;
            actor.owner = Owner.PLAYER2;
            actor.health = unit.baseMaxHealth;

            // Add the actor to the mission.
            actors.Add(actor);
        }
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

    private Actor GetSelectedActor()
    {
        // Get the currently selected tile, if any.
        Tile tile = tileMap.GetComponent<TileMap>().GetTileSelected();
        if (tile == null)
            return null;

        // Get the position of the tile selected and look for any actors currently
        // positioned on that tile.
        Vector2 position = tile.transform.position;
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

    private void UpdateHuman()
    {
        // Display buttons and stuff.

        // If we are currently moving a unit.
        if (currentPathing != null
            && currentPathing.Count > 0
            && !currentlySelectedActor.moving)
        {
            // Set the animation to walking
            if (currentPathing[0].direction == AStarDirection.EAST)
                currentlySelectedActor.SetAnimation(ActorAnimation.WALKING_WEST); // LOL

            // Apply that smooth movement.
            StartCoroutine(currentlySelectedActor.SmoothMovement(currentPathing[0].position));

            // After were done moving remove the first entry.
            currentPathing.RemoveAt(0);
            return;
        }

        if (currentlySelectedActor == null
            && currentPathing == null)
        {
            // Assign the currently selected actor.
            currentlySelectedActor = GetSelectedActor();

            if (currentlySelectedActor != null)
            {
                // Display the tile highlights for that actor.
                tileMap.GetComponent<TileMap>().ShowActorHighlights(currentlySelectedActor);
            }
        }
        else if (currentlySelectedActor != null)
        {
            Actor actor = GetSelectedActor();
            Tile tile = tileMap.GetComponent<TileMap>().GetTileSelected();

            // First check if we selected another actor to attack.
            if (!currentlySelectedActor.done
                && actor != null
                && currentlySelectedActor.owner == currentFaction
                && actor.owner != currentlySelectedActor.owner
                && tile.highlight)
            {
                // Find the nearest path to the unit you want to attack.
                Astar pathing = new Astar(tileMap.GetComponent<TileMap>(),
                    currentlySelectedActor.transform.position, actor.transform.position,
                    currentlySelectedActor.flying);
                pathing.result.RemoveAt(0); // Ignore first entry.

                // Track our movement that needs to be applied.
                if (pathing.result.Count != 0)
                    currentPathing = pathing.result;

                // Queue up our actor to attack once we apply our pathing.
                actorToAttack = actor;

                // Deselect our currently selected actor.
                currentlySelectedActor = null;
            }

            // Next see if this is a place we can move.
            else if (tile != null
                && tile.highlight
                && tile.highlightColor == TileHighlightColor.HIGHLIGHT_BLUE
                && actor == null)
            {
                // Find the nearest path to the unit you want to attack.
                Astar pathing = new Astar(tileMap.GetComponent<TileMap>(),
                    currentlySelectedActor.transform.position, tile.transform.position,
                    currentlySelectedActor.flying);
                pathing.result.RemoveAt(0); // Ignore first entry.

                // Track our movement that needs to be applied.
                if (pathing.result.Count != 0)
                    currentPathing = pathing.result;
            }
        }

        /// Turn state.
        // No actor selected.
        // Have actor selected. (show movement)
        // Dragging actor selected.
        // Moving phase. Switch to attack highlights. (can result)
        // Attacking phase. (attack with last unit who hasn't moved)
    }

    private void UpdateComputer()
    {

    }

    private void Awake()
    {
        // Create the TileMap object.
        tileMap = new GameObject("TileMap");
        tileMap.transform.parent = transform;
        tileMap.AddComponent<TileMap>();
    }

    private void Update()
    {
        // We have not fully initialized.
        if (currentFaction == Owner.NONE)
            return;

        if (IsFactionActive(currentFaction))
        {
            Player turn = GetTurn(currentFaction);
            if (turn == Player.HUMAN)
                UpdateHuman();
            if (turn == Player.COMPUTER)
                UpdateComputer();
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
