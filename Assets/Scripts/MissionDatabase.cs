using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionDatabase
{
    /// <summary>World mission blueprints.</summary>
    private List<List<MissionSchematic>> worldMissionData;

    /// <summary>The constructor that builds all world mission blueprints.</summary>
    public MissionDatabase()
    {
        // TODO: Load this data from XML or level editor generated file. Not
        // entirely a scalable solution to store everything in memory.

        worldMissionData = new List<List<MissionSchematic>>() {

            // World 1
            new List<MissionSchematic>() {

                // World 1-1
                new MissionSchematic("World 1-1", 8, 3,
                    new List<MissionTile>() {
                        new MissionTile(new Vector2(0, 0), 1, -1, -1, 15, 1, true, true),
                        new MissionTile(new Vector2(1, 0), 1, -1, -1, 15, 1, true, true),
                        new MissionTile(new Vector2(2, 0), 1, -1, -1, 23, 1, true, true),
                        new MissionTile(new Vector2(3, 0), 1, -1, -1, 15, 1, true, true),
                        new MissionTile(new Vector2(4, 0), 1, -1, -1, 23, 1, true, true),
                        new MissionTile(new Vector2(5, 0), 1, -1, -1, 23, 1, true, true),
                        new MissionTile(new Vector2(6, 0), 1, -1, -1, 15, 1, true, true),
                        new MissionTile(new Vector2(7, 0), 1, -1, -1, 15, 1, true, true),
                        new MissionTile(new Vector2(0, 1), 1, -1, -1, 15, 1, true, true),
                        new MissionTile(new Vector2(1, 1), 1, -1, -1, 24, 1, false, false),
                        new MissionTile(new Vector2(2, 1), 1, -1, -1, 26, 1, false, false),
                        new MissionTile(new Vector2(3, 1), 1, -1, -1, 25, 1, false, false),
                        new MissionTile(new Vector2(4, 1), 1, -1, -1, 29, 1, false, false),
                        new MissionTile(new Vector2(5, 1), 1, -1, -1, 29, 1, false, false),
                        new MissionTile(new Vector2(6, 1), 1, -1, -1, 27, 1, false, false),
                        new MissionTile(new Vector2(7, 1), 1, -1, -1, 15, 1, true, true),
                        new MissionTile(new Vector2(0, 2), 1, -1, -1, 15, 1, true, true),
                        new MissionTile(new Vector2(1, 2), 1, -1, -1, 23, 1, true, true),
                        new MissionTile(new Vector2(2, 2), 1, -1, -1, 15, 1, true, true),
                        new MissionTile(new Vector2(3, 2), 1, -1, -1, 23, 1, true, true),
                        new MissionTile(new Vector2(4, 2), 1, -1, -1, 15, 1, true, true),
                        new MissionTile(new Vector2(5, 2), 1, -1, -1, 15, 1, true, true),
                        new MissionTile(new Vector2(6, 2), 1, -1, -1, 23, 1, true, true),
                        new MissionTile(new Vector2(7, 2), 1, -1, -1, 15, 1, true, true),
                    },
                    new List<MissionEnemy>() {
                        new MissionEnemy(new Vector2(6, 1), UnitType.GUMBALL)
                    },
                    new List<Vector2>() {
                        new Vector2(1, 1),
                        new Vector2(2, 1),
                    }
                ),

                // World 1-2
                new MissionSchematic("World 1-2", 1, 1,
                    new List<MissionTile>(),
                    new List<MissionEnemy>(),
                    new List<Vector2>()
                ),

                // World 1-3
                new MissionSchematic("World 1-3", 1, 1,
                    new List<MissionTile>(),
                    new List<MissionEnemy>(),
                    new List<Vector2>()
                ),

                // World 1-4
                new MissionSchematic("World 1-4", 1, 1,
                    new List<MissionTile>(),
                    new List<MissionEnemy>(),
                    new List<Vector2>()
                ),

                // World 1-5
                new MissionSchematic("World 1-5", 1, 1,
                    new List<MissionTile>(),
                    new List<MissionEnemy>(),
                    new List<Vector2>()
                ),
            },

            // World 2
            new List<MissionSchematic>() {

                // World 2-1
                new MissionSchematic("World 2-1", 1, 1,
                    new List<MissionTile>(),
                    new List<MissionEnemy>(),
                    new List<Vector2>()
                ),

                // World 2-2
                new MissionSchematic("World 2-2", 1, 1,
                    new List<MissionTile>(),
                    new List<MissionEnemy>(),
                    new List<Vector2>()
                ),

                // World 2-3
                new MissionSchematic("World 2-3", 1, 1,
                    new List<MissionTile>(),
                    new List<MissionEnemy>(),
                    new List<Vector2>()
                ),

                // World 2-4
                new MissionSchematic("World 2-4", 1, 1,
                    new List<MissionTile>(),
                    new List<MissionEnemy>(),
                    new List<Vector2>()
                ),

                // World 2-5
                new MissionSchematic("World 2-5", 1, 1,
                    new List<MissionTile>(),
                    new List<MissionEnemy>(),
                    new List<Vector2>()
                ),
            },

            // World 3
            new List<MissionSchematic>(),
            // World 4
            new List<MissionSchematic>(),
            // World 5
            new List<MissionSchematic>(),
        };
    }

    /// <summary>Retrieve a missions blueprints.</summary>
    /// <param name="world">The world id of the mission.</param>
    /// <param name="level">The level id of the mission.</param>
    /// <returns>Returns the missions blueprints.</returns>
    public MissionSchematic GetMission(int world, int level)
    {
        return worldMissionData[world - 1][level - 1];
    }
}

/// <summary>
/// The metadata that makes up a tile to be displayed in a mission.
/// </summary>
public class MissionTile
{
    /// <summary>The position of the tile.</summary>
    public Vector2 position;

    /// <summary>The floor tile material id. Found in GameManager.</summary>
    public int floorMaterialId;

    /// <summary>The object tile material id. Found in GameManager.</summary>
    public int objectMaterialId;

    /// <summary>The roof tile material id. Found in GameManager.</summary>
    public int roofMaterialId;

    /// <summary>The frame id from the tile material to use.</summary>
    public int frameId;

    /// <summary>The movement cost moving onto this tile.</summary>
    public int movementCost;

    /// <summary>Whether this tile disallows ground units to move through it.</summary>
    public bool groundCollision;

    /// <summary>Whether this tile disallows air units to move through it.</summary>
    public bool airCollision;

    /// <summary>
    /// The constructor for creating a mission tile.
    /// </summary>
    /// <param name="position">The position of the tile.</param>
    /// <param name="floorMaterialId">The floor material id.</param>
    /// <param name="objectMaterialId">The object material id.</param>
    /// <param name="roofMaterialId">The roof material id.</param>
    /// <param name="movementCost">The cost to move to this tile.</param>
    /// <param name="groundCollision">Disallow ground unit movement.</param>
    /// <param name="airCollision">Disallow air unit movement.</param>
    public MissionTile(Vector2 position, int floorMaterialId, int objectMaterialId,
        int roofMaterialId, int frameId, int movementCost, bool groundCollision,
        bool airCollision)
    {
        // Set tile information.
        this.position = position;
        this.floorMaterialId = floorMaterialId;
        this.objectMaterialId = objectMaterialId;
        this.roofMaterialId = roofMaterialId;
        this.frameId = frameId;
        this.movementCost = movementCost;
        this.groundCollision = groundCollision;
        this.airCollision = airCollision;
    }
}


/// <summary>
/// The metadata that makes up a list of enemies to place down during a mission.
/// </summary>
public class MissionEnemy
{
    /// <summary>The position of the enemy unit in the mission.</summary>
    public Vector2 position;

    /// <summary>The type of enemy unit to place.</summary>
    public UnitType type;

    /// <summary>
    /// The constructor for assigning enemies to a mission.
    /// </summary>
    /// <param name="position">The position of the enemy.</param>
    /// <param name="type">The type of enemy.</param>
    public MissionEnemy(Vector2 position, UnitType type)
    {
        // Set the enemy mission information.
        this.position = position;
        this.type = type;
    }
}

/// <summary>
/// Data that makes up an entire mission. Used to construct and display
/// predefined levels.
/// </summary>
public class MissionSchematic
{
    /// <summary>The name of the mission.</summary>
    public string name;

    /// <summary>The width of the map in tiles.</summary>
    public int tileWidth = 0;

    /// <summary>The height of the map in tiles.</summary>
    public int tileHeight = 0;

    /// <summary>
    /// Tile metadata that makes up how a mission is layed out.
    /// </summary>
    public List<MissionTile> tiles;

    /// <summary>
    /// The enemy units and their positional information for this mission.
    /// </summary>
    public List<MissionEnemy> enemies;

    /// <summary>
    /// The list of friendly positions to spawn units from your roster.
    /// </summary>
    public List<Vector2> rosterSpawns;

    // TODO: Add dialog

    /// <summary>
    /// The constructor for creating mission data.
    /// </summary>
    /// <param name="name">The name of the level.</param>
    /// <param name="tiles">The tile mission data.</param>
    /// <param name="enemies">The list of enemies.</param>
    /// <param name="rosterSpawns">The list of friendly spawning positions.</param>
    public MissionSchematic(string name, int tileWidth, int tileHeight,
        List<MissionTile> tiles, List<MissionEnemy> enemies,
        List<Vector2> rosterSpawns)
    {
        // Set complete mission data.
        this.name = name;
        this.tileWidth = tileWidth;
        this.tileHeight = tileHeight;
        this.tiles = tiles;
        this.enemies = enemies;
        this.rosterSpawns = rosterSpawns;

        // TODO: Add mission dialog.
    }
}
