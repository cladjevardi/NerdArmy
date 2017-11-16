using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    /// <summary>
    /// The static instance of the world manager. Allows other scripts to
    /// reference it.
    /// </summary>
    public static WorldManager instance = null;

    /// <summary>The world of the current mission. Tied to a tilemap theme.</summary>
    public int world = 0;

    /// <summary>The level of the current mission in that world.</summary>
    public int level = 0;

    /// <summary>The players current roster.</summary>
    public List<Unit> roster = new List<Unit>();

    /// <summary>The entire map and list of units and enemies.</summary>
    public TileMap tileMap = new TileMap();

    private void LoadLevel()
    {
        tileMap.GenerateMap(roster, world, level);
    }

    private void Start()
    {
        // Give the player a main character to start the game.
        roster.Add(UnitFactory.Create(UnitType.MAINCHARACTER));

        // Start the game off at 1-1.
        world = 1;
        level = 1;

        // TODO: Display some sort of world selection.
    }

    private void Awake()
    {
        // Keep track of our singleton instance
        if (instance == null)
            instance = this;

        // There can only be one world manager instance.
        else if (instance != this)
            Destroy(gameObject);

        // Reloading scene will not trigger the world manager to be destroyed.
        DontDestroyOnLoad(gameObject);
    }
}