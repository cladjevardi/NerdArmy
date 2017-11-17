using UnityEngine;
using System;
using System.Collections.Generic;

public class TileMap : MonoBehaviour
{
    /// <summary>In-memory data of each tile.</summary>
    [HideInInspector]
    public List<List<Tile>> tiles = new List<List<Tile>>();

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

    private void InitializeMap(int rows, int columns)
    {
        // Clear any previous map information.
        tiles.Clear();

        // Allocate the map
        tiles = new List<List<Tile>>();
        for (int i = -3; i < columns - 3; i++)
        {
            List<Tile> row = new List<Tile>();
            for (int j = -2; j < rows - 2; j++)
            {
                Vector2 position = new Vector2(i, j);
                Tile tile = new GameObject("Tile_" + i + "_" + j).AddComponent<Tile>();
                tile.transform.parent = transform;
                tile.position = position;

                // TODO: This should be based on the world tile pallete.
                tile.SetFloorMaterial(0);

                // Add the tile to the map.
                row.Add(tile);
            }
            tiles.Add(row);
        }
    }

    private Vector2[] GetSpawnLocations(Owner owner)
    {
        // TODO: This should be setup per owner faction
        // in each mission.
        switch (owner)
        {
        default:
            return new Vector2[6] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(2, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(2, 1)
            };
        }
    }

    private void AddActors(List<Unit> roster)
    {
        actors.Clear();
        int spawnIndex = 0;

        // Iterate through each member of the roster and add units.
        foreach (Unit unit in roster)
        {
            // Create the new actor.
            Vector2 position = GetSpawnLocations(Owner.PLAYER1)[spawnIndex];
            string objectName = "Actor_" + Owner.PLAYER1 + "_" 
                + spawnIndex + "_" + unit.type.ToString();
            Actor actor = new GameObject(objectName).AddComponent<Actor>();
            actor.transform.parent = transform;
            actor.position = position;
            actor.unit = unit;
            actor.owner = Owner.PLAYER1; // Assign player to roster.
            actor.health = unit.baseMaxHealth;
            
            // Add the actor to the mission.
            actors.Add(actor);

            // Increment the spawn location to prevent collision.
            spawnIndex++;
        }

        // TODO: Based on mission add enemy actors
    }

    /// <summary>
    /// Initialize the map with the tiles from level data.
    /// </summary>
    /// <param name="world">The world or tile theme to use.</param>
    /// <param name="level">The level within the world.</param>
    public void GenerateMap(List<Unit> roster, int world, int level)
    {
        // TODO: Lookup the world and level get the dimensions we need to create.
        int rows = 4;
        int columns = 6;

        // Allocate the tile map.
        InitializeMap(rows, columns);

        // Add the list of unit actor to the board.
        AddActors(roster);
    }
}
