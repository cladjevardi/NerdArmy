using System;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    /// <summary>
    /// The static instance of the world manager. Allows other scripts to
    /// reference it.
    /// </summary>
    public static WorldManager instance = null;

    /// <summary>The world of the current mission. Tied to a tilemap theme.</summary>
    public int world = 1;

    /// <summary>The level of the current mission in that world.</summary>
    public int level = 1;

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