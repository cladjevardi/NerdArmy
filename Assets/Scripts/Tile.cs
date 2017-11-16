using UnityEngine;

public class Tile : MonoBehaviour
{
    /// <summary>
    /// The renderer for the tile.
    /// </summary>
    private GameObject _tileRenderer = null;
    public GameObject tileRenderer
    {
        get { return _tileRenderer; }
        internal set { _tileRenderer = value; }
    }

    /// <summary>
    /// Tile positional information.
    /// </summary>
    private Vector2 _position = Vector2.zero;
    public Vector2 position
    {
        get { return _position; }
        set { _position = value; }
    }

    /// <summary>
    /// The cost of movement to move over this tile.
    /// </summary>
    private int _movementCost = 1;
    public int movementCost
    {
        get { return _movementCost; }
        set { _movementCost = value; }
    }

    /// <summary>
    /// Whether this tile has movement collision for ground units.
    /// </summary>
    private bool _groundCollision = false;
    public bool groundCollision
    {
        get { return _groundCollision; }
        set { _groundCollision = value; }
    }

    /// <summary>
    /// Whether this tile has movement collision for air units.
    /// </summary>
    private bool _airCollision = false;
    public bool airCollision
    {
        get { return _airCollision; }
        set { _airCollision = value; }
    }

    /// <summary>
    /// Whether this tile has movement collision for air units.
    /// </summary>
    private Owner _occupiedFaction = Owner.NONE;
    public Owner occupiedFaction
    {
        get { return _occupiedFaction; }
        set { _occupiedFaction = value; }
    }

    /// <summary>Call at creation of object.</summary>
    private void Awake()
    {
        _tileRenderer = new GameObject("TileRenderer");
        _tileRenderer.transform.parent = transform;
        _tileRenderer.AddComponent<TileRenderer>();
    }
}
