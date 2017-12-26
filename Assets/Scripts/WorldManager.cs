using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    /// <summary>The world of the current mission. Tied to a tilemap theme.</summary>
    public int world = 0;

    /// <summary>The level of the current mission in that world.</summary>
    public int level = 0;

    /// <summary>Mission database.</summary>
    private MissionDatabase missionDatabase = new MissionDatabase();

    /// <summary>The players current roster.</summary>
    private List<Unit> roster = new List<Unit>();

    /// <summary>The entire map and list of units and enemies.</summary>
    private GameObject mission = null;

    private void LoadLevel()
    {
        // Tell TileMap to generate the level.
        MissionSchematic missionSchematic = missionDatabase.GetMission(world, level);
        Debug.Log("Loading " + missionSchematic.name);

        mission.GetComponent<Mission>().Initialize(roster, missionSchematic);
    }

    private void Start()
    {
        // Setup the database.
        missionDatabase.Initialize();

        // Give the player a main character to start the game.
        roster.Add(new Unit("main_character"));

        // Start the game off at 1-1.
        world = 1;
        level = 1;

        // TODO: Display some sort of world selection.
        LoadLevel();
    }

    private void Awake()
    {
        // Create the TileMap object.
        mission = new GameObject("Mission");
        mission.transform.SetParent(transform);
        mission.AddComponent<Mission>();
    }
}