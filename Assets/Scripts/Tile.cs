using UnityEngine;

public class Tile
{
    /// <summary>
    /// Tile constructor. Tile must have a set of coordinates.
    /// </summary>
    /// <param name="position">The grid position.</param>
    public Tile(Vector2 position)
    {
        _position = position;
    }

    /// <summary>
    /// Tile positional information.
    /// </summary>
    private Vector2 _position = Vector2.zero;
    public Vector2 position
    {
        get { return _position; }
        set
        {
            if (_renderer == null)
                _renderer = new TileRenderer(_position.x, _position.y);
            _renderer.SetPosition(_position.x, _position.y);
            _position = value;
        }
    }

    /// <summary>
    /// The renderer for the tile.
    /// </summary>
    private TileRenderer _renderer = null;
    public TileRenderer renderer
    {
        get { return _renderer; }
        set { _renderer = value; }
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
}
