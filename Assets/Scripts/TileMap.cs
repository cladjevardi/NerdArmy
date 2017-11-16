using UnityEngine;
using System;
using System.Collections.Generic;
using NerdArmy;

public class TileMap : MonoBehaviour
{
    /// <summary>The prefab of the tile to get render information from.</summary>
    public GameObject tilePrefab;

    /// <summary>Map row count.</summary>
    public int rows = 8;

    /// <summary>Map column count.</summary>
    public int columns = 8;

    /// <summary>
    /// The material pallete to use for tile renderer.
    /// </summary>
    public Material[] tileMaterials;

    /// <summary>
    /// The list of unit materials.
    /// </summary>
    public Material[] unitMaterials;

    /// <summary>In-memory data of each tile.</summary>
    [HideInInspector]
    public List<List<Tile>> tiles = new List<List<Tile>>();

    /// <summary>The list of units in the mission.</summary>
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
        tiles.Clear();

        // Instantiate a "Map" object and set transform.
        mapHolder = new GameObject("Map").transform;

        // Allocate the map
        tiles = new List<List<Tile>>();
        for (int i = 0; i < columns; i++)
        {
            List<Tile> row = new List<Tile>();
            for (int j = 0; j < rows; j++)
            {
                // Create a tile at that grid position.
                Vector2 tileVector = new Vector2(i - Mathf.Floor(columns / 2), -j + Mathf.Floor(rows / 2));
                Quaternion quaternion = Quaternion.Euler(new Vector3());
                Tile tile = Instantiate(tilePrefab, tileVector, quaternion).GetComponent<Tile>();
                tile.position = new Vector2(i, j);
                tile.renderer.transform.parent = mapHolder;

                // TODO: This should be based on the world tile pallete.
                tile.renderer.SetMaterial(
                    TileRenderer.TileLayer.LAYER_FLOOR,
                    tileMaterials[0]);

                // Add the tile to the map.
                row.Add(tile);
            }
            tiles.Add(row);
        }
    }

    private Material GetUnitMaterial(UnitType unitType)
    {
        switch(unitType)
        {
            // Hero units.
            case UnitType.MAINCHARACTER:
                return unitMaterials[0];
            case UnitType.CHARGER:
                return unitMaterials[1];
            case UnitType.MAGICIAN:
                return unitMaterials[2];
            case UnitType.ELEMENTALIST:
                return unitMaterials[3];
            case UnitType.BOMBER:
                return unitMaterials[4];

            // Enemy units.
            case UnitType.GUMBALL:
                return unitMaterials[5];
            case UnitType.EAGLE:
                return unitMaterials[6];
            case UnitType.RUNNINGMAN:
                return unitMaterials[7];
            case UnitType.REDARCHER:
                return unitMaterials[8];
            case UnitType.BLACKARCHER:
                return unitMaterials[9];
            case UnitType.BACKPACK:
                return unitMaterials[10];
            case UnitType.SHIELD:
                return unitMaterials[11];
            case UnitType.DITTO:
                return unitMaterials[12];
            case UnitType.LIGHTBULB:
                return unitMaterials[13];
            case UnitType.HEDGEHOG:
                return unitMaterials[14];

            // Not implemented or NONE will trigger this.
            default:
                throw new ArgumentException("Invalid unit type", "type");
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
            Actor actor = new Actor();
            actor.unit = unit;
            actor.owner = Owner.PLAYER1; // Assign player to roster.
            actor.health = unit.baseMaxHealth;
            actor.position = GetSpawnLocations(Owner.PLAYER1)[spawnIndex++];
            actor.renderer.SetMaterial(
                TileRenderer.TileLayer.LAYER_UNITS,
                GetUnitMaterial(unit.type));

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
