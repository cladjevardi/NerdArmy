using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    /// <summary>
    /// The renderer for the tile.
    /// </summary>
    private GameObject tileRenderer = null;

    /// <summary>
    /// Set the material of the the floor tile that renders below all other tiles.
    /// </summary>
    /// <param name="tileId">The GameManager tile identifier of the material.</param>
    public void SetFloorMaterial(int tileId)
    {
        tileRenderer.GetComponent<TileRenderer>()
            .SetTileMaterial(TileRenderer.TileLayer.LAYER_FLOOR, tileId);
    }

    /// <summary>
    /// An object renders above floor tiles and has a transparency layer to
    /// show the floor beneath it. This can be used for immoveable textures
    /// that don't really fit the behaviour of an Actor.
    /// </summary>
    /// <param name="tileId">The GameManager tile identifier of the material.</param>
    public void SetObjectMaterial(int tileId)
    {
        tileRenderer.GetComponent<TileRenderer>()
            .SetTileMaterial(TileRenderer.TileLayer.LAYER_OBJECT, tileId);
    }

    /// <summary>
    /// Set the material of a tile that renders above the unit layer. This can be
    /// used for trees, roofs, or brush where you want to obscure presence. Flying
    /// units are the only exception and render above the roof layer.
    /// </summary>
    /// <param name="tileId">The GameManager tile identifier of the material.</param>
    public void SetRoofMaterial(int tileId)
    {
        // If the acting faction can see under this roof tile.
        tileRenderer.GetComponent<TileRenderer>().SetColor(
            TileRenderer.TileLayer.LAYER_ROOF,
            new Color(1.0f, 1.0f, 1.0f, _canSeeUnder ? 0.2f : 1.0f));
        tileRenderer.GetComponent<TileRenderer>()
            .SetTileMaterial(TileRenderer.TileLayer.LAYER_ROOF, tileId);
    }

    /// <summary>
    /// Tile positional information.
    /// </summary>
    private Vector2 _position = Vector2.zero;
    public Vector2 position
    {
        get { return tileRenderer.GetComponent<TileRenderer>().GetPosition(); }
        set { tileRenderer.GetComponent<TileRenderer>().SetPosition(value); }
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

    /// <summary>Whether a roof tile can be seen under.</summary>
    private bool _canSeeUnder = false;
    public bool canSeeUnder
    {
        get { return _canSeeUnder; }
        set { _canSeeUnder = value; }
    }

    /// <summary>
    /// Whether there is a movement highlight effect
    /// being displayed on this tile.
    /// </summary>
    private bool _moveHighlight = false;
    public bool moveHighlight
    {
        get { return _moveHighlight; }
        set
        {
            if (value)
            {
                // Assign the unit and setup unit animation.
                MaterialId materialId = new MaterialId(2, MaterialType.EFFECT);
                materialId.cellWidth = 15;
                materialId.cellHeight = 15;
                materialId.animationManager.AddAnimation("highlight_blue",
                    new List<int>() { 32, 33, 34, 35, 36, 37, 38, 39,
                        40, 41, 42, 43, 44, 45, 46, 47 }, 0.1f);
                materialId.animationManager.AddAnimation("highlight_red",
                    new List<int>() { 16, 17, 18, 19, 20, 21, 22, 23,
                        24, 25, 26, 27, 28, 29, 30, 31 }, 0.1f);
                materialId.animationManager.SetCurrentAnimation("highlight_red");
                materialId.animationManager.PlayAnimation();

                // Assign the material to the unit.
                tileRenderer.GetComponent<TileRenderer>()
                    .SetMaterial(TileRenderer.TileLayer.LAYER_HIGHLIGHTS, materialId);
            }
            _moveHighlight = value;
        }
    }

    /// <summary>Call at creation of object.</summary>
    private void Awake()
    {
        tileRenderer = new GameObject("TileRenderer");
        tileRenderer.transform.parent = transform;
        tileRenderer.AddComponent<TileRenderer>();
    }
}
