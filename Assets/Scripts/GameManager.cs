using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The static instance of the game manager. Allows other scripts to
    /// reference it.
    /// </summary>
    public static GameManager instance = null;

    /// <summary>
    /// The class that manages which world we go to.
    public GameObject worldManager;

    private void Awake()
    {
        // Keep track of our singleton instance
        if (instance == null)
            instance = this;

        // There can only be one game manager instance.
        else if (instance != this)
            Destroy(gameObject);

        // Create the global instance of the world manager.
        if (WorldManager.instance == null)
            Instantiate(worldManager);

        // Reloading scene will not trigger the game manager to be destroyed.
        DontDestroyOnLoad(gameObject);
    }
}
