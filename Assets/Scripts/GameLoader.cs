using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    /// <summary>
    /// The single instance of a game manager. This class manages
    /// gameplay.
    /// </summary>
    public GameObject gameManager;

    /// <summary>
    /// Initialize our global statics to be referenced from other
    /// scripts.
    /// </summary>
    void Start()
    {
        if (GameManager.instance == null)
            Instantiate(gameManager, transform);
    }
}
