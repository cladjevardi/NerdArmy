using UnityEngine;
using System;
using System.Collections.Generic;
using NerdArmy;

public class MapManager : MonoBehaviour
{
    /// <summary>The prefab of the tile to get render information from.</summary>
    public GameObject tilePrefab;

    /// <summary>Map row count.</summary>
    public int rows = 8;

    /// <summary>Map column count.</summary>
    public int columns = 8;

    /// <summary>The list of available tiles.</summary>
    [HideInInspector]
    public List<List<Tile>> map = new List<List<Tile>>();

    [HideInInspector]
    public List<Actor> actors = new List<Actor>();

    /// <summary>The different unit control groups.</summary>
    private enum PlayerType { HUMAN, COMPUTER };

    /// <summary>The factions that make up players and opposition.</summary>
    // TODO: Make this mission specific.
    private PlayerType player1 = PlayerType.HUMAN;
    private PlayerType player2 = PlayerType.COMPUTER;

    /// <summary>
    /// A variable to store a reference to the transform of our entire map.
    /// </summary>
    private Transform mapHolder;

    private void InitializeMap(int rows, int column)
    {
        // Clear any previous map information.
        map.Clear();

        // Instantiate a "Map" object and set transform.
        mapHolder = new GameObject("Map").transform;

        // Allocate the map
        map = new List<List<Tile>>();
        for (int i = 0; i < columns; i++)
        {
            List<Tile> row = new List<Tile>();
            for (int j = 0; j < rows; j++)
            {
                // Create a tile at that grid position.
                Vector2 tileVector = new Vector2(i - Mathf.Floor(columns / 2), -j + Mathf.Floor(rows / 2));
                Quaternion quaternion = Quaternion.Euler(new Vector3());
                Tile tile = Instantiate(tilePrefab, tileVector, quaternion).GetComponent<Tile>();
                tile.gridPosition = new Vector2(i, j);
                row.Add(tile);
            }
            map.Add(row);
        }
    }

    private void AddActors(List<Unit> roster)
    {
        actors.Clear();

        // Iterate through each member of the roster and add units.
        foreach (Unit unit in roster)
        {
            Actor actor = new Actor();
            actor.unit = unit;
            actor.owner = Owner.PLAYER1; // Assign player to roster.
            actor.health = unit.baseMaxHealth;

            // Add the actor to the mission.
            actors.Add(actor);
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
        int rows = 8;
        int columns = 8;

        // Allocate the tile map.
        InitializeMap(rows, columns);

        // Add the list of unit actor to the board.
        AddActors(roster);
    }
}
