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
                tile.SetFloorMaterial(missionTile.floorMaterialId);
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
        foreach (MissionEnemy enemy in missionEnemies)
        {
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
