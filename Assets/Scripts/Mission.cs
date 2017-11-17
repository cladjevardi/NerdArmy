using System;
using System.Collections.Generic;
using UnityEngine;

class WorldMissionData
{
    private List<MissionData> world1missionData = new List<MissionData>();
    private List<MissionData> world2missionData = new List<MissionData>();
    private List<MissionData> world3missionData = new List<MissionData>();
    private List<MissionData> world4missionData = new List<MissionData>();

    public WorldMissionData()
    {
        // Add World 1-1
        world1missionData.Add(
            new MissionData("World 1-1",
                new List<MissionData.MissionTile>() {
                    new MissionData.MissionTile(new Vector2(-2, 0), 0, -1, -1, 1, false, false),
                    new MissionData.MissionTile(new Vector2(-1, 0), 0, -1, -1, 1, false, false),
                    new MissionData.MissionTile(new Vector2(-0, 0), 0, -1, -1, 1, false, false),
                    new MissionData.MissionTile(new Vector2(1, 0), 0, -1, -1, 1, false, false),
                    new MissionData.MissionTile(new Vector2(2, 0), 0, -1, -1, 1, false, false),
                    new MissionData.MissionTile(new Vector2(3, 0), 0, -1, -1, 1, false, false),
                },
                new List<MissionData.MissionEnemy>() {
                    new MissionData.MissionEnemy(new Vector2(3, 0), UnitType.GUMBALL)
                },
                new List<Vector2>() {
                    new Vector2(-2, 0),
                    new Vector2(-1, 0),
                }
            )
        );
    }

    MissionData GetMission(int world, int level)
    {
        switch(world)
        {
            case 1:
                return world1missionData[level];

            // Not implemented or NONE will trigger this.
            default:
                throw new ArgumentException("Invalid mission selected", "mission");
        }
    }
}

class MissionData
{
    public string name;
    public List<MissionTile> tiles;
    public List<MissionEnemy> enemies;
    public List<Vector2> rosterSpawns;

    public MissionData(string name, List<MissionTile> tiles, List<MissionEnemy> enemies, List<Vector2> rosterSpawns)
    {
        this.name = name;
        this.tiles = tiles;
        this.enemies = enemies;
        this.rosterSpawns = rosterSpawns;
    }

    public class MissionTile
    {
        public MissionTile(Vector2 position, int floorMaterialId, int objectMaterialId,
            int roofMaterialId, int movementCost, bool groundCollision, bool airCollision)
        {
            this.position = position;
            this.floorMaterialId = floorMaterialId;
            this.objectMaterialId = objectMaterialId;
            this.roofMaterialId = roofMaterialId;
            this.movementCost = movementCost;
            this.groundCollision = groundCollision;
            this.airCollision = airCollision;
        }

        public Vector2 position;
        public int floorMaterialId;
        public int objectMaterialId;
        public int roofMaterialId;
        public int movementCost;
        public bool groundCollision;
        public bool airCollision;
    }

    public class MissionEnemy
    {
        public MissionEnemy(Vector2 position, UnitType type)
        {
            this.position = position;
            this.type = type;
        }

        public Vector2 position;
        public UnitType type;
    }
}
