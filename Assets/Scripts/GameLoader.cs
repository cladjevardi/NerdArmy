using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    /// <summary>
    /// The single instance of a game manager. This class is just there.
    /// </summary>
    public GameObject gameManager;

    /// <summary>
    /// The singleton instance of a sound manager. This class manages
    /// correcting repetitive sound effects and background music
    /// outside the scope of each scene for continuous playback.
    /// </summary>
    public GameObject soundManager;

    /// <summary>
    /// Initialize our global statics to be referenced from other
    /// scripts.
    /// </summary>
    void Start()
    {
        if (GameManager.instance == null)
            Instantiate(gameManager);

        if (SoundManager.instance == null)
            Instantiate(soundManager);
    }
}
