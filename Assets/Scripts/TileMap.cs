using UnityEngine;
using System;
using System.Collections.Generic;

public class TileMap : MonoBehaviour
{
    /// <summary>In-memory data of each tile.</summary>
    [HideInInspector]
    public List<Tile> tiles = new List<Tile>();

    /// <summary>The list of units in the mission.</summary>
    [HideInInspector]
    public List<Actor> actors = new List<Actor>();

    /// <summary>The different unit control groups.</summary>
    public enum PlayerType { HUMAN, COMPUTER };

    /// <summary>The factions that make up players and opposition.</summary>
    // TODO: Make this mission specific.
    public PlayerType player1 = PlayerType.HUMAN;
    public PlayerType player2 = PlayerType.COMPUTER;

    /// <summary>
    /// A variable to store a reference to the transform of our entire map.
    /// </summary>
    private Transform mapHolder;

    /// <summary>
    /// Initialize the map of tiles.
    /// </summary>
    /// <param name="missionTiles">Mission tile data.</param>
    private void InitializeMap(List<MissionTile> missionTiles)
    {
        // Iterate through each tile from the mission loaded
        // and create the map.
        foreach (MissionTile missionTile in missionTiles)
        {
            Tile tile = new GameObject("Tile_" + missionTile.position.x + "_" + missionTile.position.y).AddComponent<Tile>();
            tile.transform.parent = transform;
            tile.position = missionTile.position;
            tile.movementCost = missionTile.movementCost;
            tile.airCollision = missionTile.airCollision;
            tile.groundCollision = missionTile.groundCollision;

            // TODO: This should be based on the world tile pallete.
            if (missionTile.floorMaterialId != -1)
                tile.SetFloorMaterial(missionTile.floorMaterialId, missionTile.frameId);
            if (missionTile.objectMaterialId != -1)
                tile.SetObjectMaterial(missionTile.objectMaterialId);
            if (missionTile.roofMaterialId != -1)
                tile.SetRoofMaterial(missionTile.roofMaterialId);

            tiles.Add(tile);
        }
    }

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
            actor.position = position;
            actor.unit = unit;
            actor.owner = Owner.PLAYER1;
            actor.health = unit.baseMaxHealth;

            // Highlight the area that the player can move.
            HighlightCenter(position, actor.unit.baseMovement, actor.flying);

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
            actor.position = enemy.position;
            actor.unit = unit;
            actor.owner = Owner.PLAYER2;
            actor.health = unit.baseMaxHealth;

            // Add the actor to the mission.
            actors.Add(actor);
        }
    }

    /// <summary>
    /// Find the tile of the coordinate.
    /// </summary>
    /// <param name="entry">The coordinate of the tile to look for.</param>
    /// <returns>Returns the Tile reference if found. Null if not.</returns>
    private Tile FindTile(Vector2 entry)
    {
        // Iterate through each mission tile and compare its coordinate.
        foreach (Tile item in tiles)
        {
            if (entry.x == item.position.x && entry.y == item.position.y)
                return item;
        }

        // No tile was found matching this coordinate.
        return null;
    }

    /// <summary>
    /// Find a Vector2 in a list of Vector2s.
    /// TODO: Find a better way to do this!
    /// </summary>
    /// <param name="entry">The coordinate to find.</param>
    /// <param name="list">The list of coordinates to check.</param>
    /// <returns>
    /// Returns whether or not the cooridnate exists in the list.
    /// </returns>
    private bool IsVector2InVector2List(Vector2 entry, List<Vector2> list)
    {
        // Look for the given coordinate in the list of coordinates.
        bool contains = false;
        foreach (Vector2 item in list)
        {
            if (entry.x == item.x && entry.y == item.y)
                contains = true;
        }

        return contains;
    }

    /// <summary>
    /// Check if the cooridnate specified should be added to the list of
    /// potential visitors.
    /// </summary>
    /// <param name="coord">The coordinate to check.</param>
    /// <param name="visited">The list of visited tiles already.</param>
    /// <param name="canFly">Whether we check for air or ground collision.</param>
    /// <returns>
    /// Returns whether or not the coordinate should be checked next pass.
    /// </returns>
    private bool ShouldAdd(Vector2 coord, List<Vector2> visited, bool canFly)
    {
        Tile tile = FindTile(coord);

        // If the coord is valid, not already visited previously, or has collision
        // add it to the list of visited
        return (tile != null
            && !IsVector2InVector2List(coord, visited)
            && (canFly ? !tile.airCollision : !tile.groundCollision));
    }

    /// <summary>
    /// Highlight a set of tiles for a given a center based on movement
    /// distance.
    /// </summary>
    /// <param name="x">The x position of the unit.</param>
    /// <param name="y">The y position of the unit.</param>
    /// <param name="distance">How far the unit can move.</param>
    /// <param name="canFly">
    /// Whether the unit checks against air or ground collision.
    /// </param>
    public void HighlightCenter(Vector2 originalPosition, int distance,
        bool canFly)
    {
        List<Vector2> visited = new List<Vector2>();
        List<Vector2> toCheck = new List<Vector2>();

        // Start off with what we know for sure we should check
        toCheck.Add(originalPosition);
        int distanceLeft = distance;

        while (distanceLeft >= 0)
        {
            // The next group of tiles to check
            List<Vector2> newToCheck = new List<Vector2>();

            // Iterate through our toCheck
            foreach (Vector2 coord in toCheck)
            {
                // Add ourself to visited
                if (!IsVector2InVector2List(coord, visited))
                    visited.Add(new Vector2(coord.x, coord.y));

                Vector2 north = new Vector2(coord.x, coord.y - 1);
                Vector2 east = new Vector2(coord.x + 1, coord.y);
                Vector2 south = new Vector2(coord.x, coord.y + 1);
                Vector2 west = new Vector2(coord.x - 1, coord.y);

                // Check if we should add any given direction to the next
                // potential list of tiles.
                if (ShouldAdd(north, visited, canFly))
                    newToCheck.Add(north);
                if (ShouldAdd(east, visited, canFly))
                    newToCheck.Add(east);
                if (ShouldAdd(south, visited, canFly))
                    newToCheck.Add(south);
                if (ShouldAdd(west, visited, canFly))
                    newToCheck.Add(west);
            }

            // Replace list with new list
            toCheck.Clear();
            toCheck = newToCheck;
            distanceLeft--;
        }

        // Apply the highlight to the valid tiles to move to
        foreach (Vector2 coord in visited)
        {
            Tile tile = FindTile(coord);
            tile.moveHighlight = true;
        }
    }

    /// <summary>
    /// Construct a mission from the mission database.
    /// </summary>
    /// <param name="roster">The list of player controlled units.</param>
    /// <param name="missionSchematic">The blueprints for building a mission.</param>
    public void GenerateMission(List<Unit> roster, MissionSchematic missionSchematic)
    {
        // Clear any previous map information.
        tiles.Clear();
        actors.Clear();

        // Allocate the tile map.
        InitializeMap(missionSchematic.tiles);

        // Add the list of player controlled actors to the map.
        AddRoster(roster, missionSchematic.rosterSpawns);

        // Add the list of enemy controlled actors to the map.
        AddEnemies(missionSchematic.enemies);
    }
}
