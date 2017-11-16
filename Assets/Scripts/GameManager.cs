using UnityEngine;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The static instance of the game manager. Allows other scripts to
    /// reference it.
    /// </summary>
    public static GameManager instance = null;

    /// <summary>
    /// The sound manager for the game. This class can be invoked through
    /// GameManager.instance.soundManager.
    /// </summary>
    private GameObject _soundManager = null;
    public GameObject soundManager
    {
        get { return _soundManager; }
        internal set { _soundManager = value; }
    }

    /// <summary>
    /// The world manager for the game. This class can be invoked through
    /// GameManager.instance.worldManager.
    /// </summary>
    private GameObject _worldManager = null;
    public GameObject worldManager
    {
        get { return _worldManager; }
        internal set { _worldManager = value; }
    }

    /// <summary>
    /// The material pallete off tiles to use for tile renderer.
    /// </summary>
    public Material[] tileMaterials;

    /// <summary>
    /// The material pallete of effects to use for tile renderer.
    /// </summary>
    public Material[] effectMaterials;

    /// <summary>
    /// The list of unit materials.
    /// </summary>
    public Material[] unitMaterials;

    /// <summary>Setup our singleton instance.</summary>
    private void Awake()
    {
        // Keep track of our singleton instance
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        // Reloading scene will not trigger the game manager to be destroyed.
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Assign the GameManager as the parent.
        if (soundManager == null)
        {
            soundManager = new GameObject("SoundManager");
            soundManager.transform.parent = transform;
            soundManager.AddComponent<SoundManager>();
        }

        if (worldManager == null)
        {
            worldManager = new GameObject("WorldManager");
            worldManager.transform.parent = transform;
            worldManager.AddComponent<WorldManager>();
        }
    }
}
