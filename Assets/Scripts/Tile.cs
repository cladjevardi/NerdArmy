using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>The list of highlight animations.</summary>
public enum TileHighlightColor
{
    HIGHLIGHT_NONE,
    HIGHLIGHT_BLUE,
    HIGHLIGHT_RED,
    HIGHLIGHT_GREEN,
}

public class Tile : Mesh2D
{
    /// <summary>
    /// Attach a box collider to each tile which allows for raycast detection
    /// </summary>
    public void Start()
    {
        gameObject.AddComponent<BoxCollider>();
        BoxCollider collider = this.gameObject.GetComponent<BoxCollider>();

        float gridScale = GameManager.instance.gridScale;
        collider.size = new Vector3(gridScale, gridScale, 0);
        collider.center = new Vector3(transform.position.x * gridScale + gridScale / 2,
            transform.position.y * gridScale + gridScale / 2, 0);
    }

    /// <summary>
    /// Set the material of the the floor tile that renders below all other tiles.
    /// </summary>
    /// <param name="tileId">The GameManager tile identifier of the material.</param>
    /// <param name="frameId">The sprite index to display inside the material.</param>
    public void SetFloorMaterial(int tileId, int frameId = 0)
    {
        SetMaterial(Mesh2DLayer.LAYER_FLOOR, tileId, MaterialType.TILE, 32, 32, frameId);
    }

    /// <summary>
    /// An object renders above floor tiles and has a transparency layer to
    /// show the floor beneath it. This can be used for immoveable textures
    /// that don't really fit the behaviour of an Actor.
    /// </summary>
    /// <param name="tileId">The GameManager tile identifier of the material.</param>
    /// <param name="frameId">The sprite index to display inside the material.</param>
    public void SetObjectMaterial(int tileId, int frameId = 0)
    {
        SetMaterial(Mesh2DLayer.LAYER_OBJECT, tileId, MaterialType.TILE, 32, 32, frameId);
    }

    /// <summary>
    /// Set the material of a tile that renders above the unit layer. This can be
    /// used for trees, roofs, or brush where you want to obscure presence. Flying
    /// units are the only exception and render above the roof layer.
    /// </summary>
    /// <param name="tileId">The GameManager tile identifier of the material.</param>
    /// <param name="frameId">The sprite index to display inside the material.</param>
    public void SetRoofMaterial(int tileId, int frameId = 0)
    {
        // If the acting faction can see under this roof tile.
        mesh.SetColor(Mesh2DLayer.LAYER_ROOF, 
            new Color(1.0f, 1.0f, 1.0f, _canSeeUnder ? 0.2f : 1.0f));
        SetMaterial(Mesh2DLayer.LAYER_ROOF, tileId, MaterialType.TILE, 32, 32, frameId);
    }

    /// <summary>
    /// Set the highlight material.
    /// </summary>
    /// <param name="tileId">The GameManager tile identifier of the material.</param>
    /// <param name="frameId">The sprite index to display inside the material.</param>
    public void SetHighlightMaterial(int tileId, int frameId = 0)
    {
        // Assign the unit and setup unit animation.
        List<Mesh2DAnimation> animations = new List<Mesh2DAnimation>() {
            new Mesh2DAnimation("highlight_green",
                new List<int>() { 48, 49, 50, 51, 52, 53, 54, 55,
                    56, 57, 58, 59, 60, 61, 62, 63 }, 0.1f),
            new Mesh2DAnimation("highlight_blue",
                new List<int>() { 32, 33, 34, 35, 36, 37, 38, 39,
                    40, 41, 42, 43, 44, 45, 46, 47 }, 0.1f),
            new Mesh2DAnimation("highlight_red",
                new List<int>() { 16, 17, 18, 19, 20, 21, 22, 23,
                    24, 25, 26, 27, 28, 29, 30, 31 }, 0.1f),
        };

        SetMaterial(Mesh2DLayer.LAYER_HIGHLIGHTS, tileId, MaterialType.EFFECT,
            15, 15, frameId, animations, "highlight_blue", true);
    }

    /// <summary>
    /// Set the material for the grid effect.
    /// </summary>
    /// <param name="tileId">The effect tile id to use.</param>
    /// <param name="frameId">The frame id inside the effect tile to use.</param>
    public void SetGridMaterial(int tileId, int frameId = 0)
    {
        SetMaterial(Mesh2DLayer.LAYER_GRID, tileId, MaterialType.EFFECT, 32, 32, frameId);
    }

    /// <summary>
    /// Take a directional mask string and convert it to an arrow GridMaterial.
    /// </summary>
    /// <param name="start">
    /// Whether to draw the beginning of the path or the end of the path image.
    /// </param>
    /// <param name="mask">The arrow image mask to convert to a frame id.</param>
    public void SetGridArrowMask(bool start, string mask)
    {
        if (start)
        {
            // Use the arrow begin frame over the arrow point.
            if (mask == "01-00-00-00")
                SetGridMaterial(3, 7);
            if (mask == "00-01-00-00")
                SetGridMaterial(3, 1);
            if (mask == "00-00-01-00")
                SetGridMaterial(3, 0);
            if (mask == "00-00-00-01")
                SetGridMaterial(3, 8);
        }
        else
        {
            // Draw the direction based arrow sprites.
            if (mask == "01-00-00-00")
                SetGridMaterial(3, 6);
            if (mask == "00-01-00-00")
                SetGridMaterial(3, 13);
            if (mask == "00-00-01-00")
                SetGridMaterial(3, 12);
            if (mask == "00-00-00-01")
                SetGridMaterial(3, 5);
        }
                
        if (mask == "01-01-00-00")
            SetGridMaterial(3, 10);
        if (mask == "01-00-01-00")
            SetGridMaterial(3, 2);
        if (mask == "01-00-00-01")
            SetGridMaterial(3, 11);
        if (mask == "00-01-01-00")
            SetGridMaterial(3, 4);
        if (mask == "00-01-00-01")
            SetGridMaterial(3, 9);
        if (mask == "00-00-01-01")
            SetGridMaterial(3, 3);
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
    private bool _trueCollision = false;
    public bool trueCollision
    {
        get { return _trueCollision; }
        set { _trueCollision = value; }
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
    private bool _highlight = false;
    public bool highlight
    {
        get { return _highlight; }
        set
        {
            SetHighlightMaterial(2, 0);
            _highlight = value;
        }
    }

    /// <summary>
    /// The highlight animation color. Must be enabled first.
    /// </summary>
    private TileHighlightColor _highlightColor = TileHighlightColor.HIGHLIGHT_BLUE;
    public TileHighlightColor highlightColor
    {
        get { return _highlightColor; }
        set
        {
            switch(value)
            {
                case TileHighlightColor.HIGHLIGHT_BLUE:
                    SetAnimation(Mesh2DLayer.LAYER_HIGHLIGHTS, "highlight_blue");
                    break;
                case TileHighlightColor.HIGHLIGHT_RED:
                    SetAnimation(Mesh2DLayer.LAYER_HIGHLIGHTS, "highlight_red");
                    break;
                case TileHighlightColor.HIGHLIGHT_GREEN:
                    SetAnimation(Mesh2DLayer.LAYER_HIGHLIGHTS, "highlight_green");
                    break;
            }
            _highlightColor = value;
        }
    }

    /// <summary>
    /// Called via SendMessage from Mesh2DAnimation when an animation is complete.
    /// Should not be invoked from anywhere else.
    /// </summary>
    /// <param name="name">The name of the animation that completed.</param>
    protected override void OnAnimationComplete(object name)
    {
        // No tile animations need monitoring at this time.
    }
}
