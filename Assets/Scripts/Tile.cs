using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>The list of highlight animations.</summary>
public enum TileHighlightColor
{
    HIGHLIGHT_NONE = -1,
    HIGHLIGHT_BLUE,
    HIGHLIGHT_RED,
    HIGHLIGHT_GREEN,
    HIGHLIGHT_COUNT,
}

public class Tile : Mesh2D
{
    /// <summary>
    /// Attach a box collider to each tile which allows for raycast detection
    /// </summary>
    public void Start()
    {
        gameObject.AddComponent<BoxCollider2D>();
        Collider2D collider = this.gameObject.GetComponent<Collider2D>();
        collider.transform.position = transform.position;
        collider.offset = new Vector2(.5f, .5f);
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
        SetMaterial(Mesh2DLayer.LAYER_HIGHLIGHTS, tileId, MaterialType.EFFECT,
            66, 66, frameId);
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

    public void SetHighlightMask(byte[] mask)
    {
        //           n  ne e  se s  sw w  nw
        if (mask[0] == 0x0 && mask[2] == 0x0 && mask[4] == 0x0 && mask[6] == 0x0)
            SetHighlightMaterial(1, 0);

        // 1 highlight
        else if (mask[0] == 0x1 && mask[2] == 0x0 && mask[4] == 0x0 && mask[6] == 0x0)
            SetHighlightMaterial(1, 14);
        else if (mask[0] == 0x0 && mask[2] == 0x1 && mask[4] == 0x0 && mask[6] == 0x0)
            SetHighlightMaterial(1, 12);
        else if (mask[0] == 0x0 && mask[2] == 0x0 && mask[4] == 0x1 && mask[6] == 0x0)
            SetHighlightMaterial(1, 13);
        else if (mask[0] == 0x0 && mask[2] == 0x0 && mask[4] == 0x0 && mask[6] == 0x1)
            SetHighlightMaterial(1, 11);

        // 2 highlights
        else if (mask[0] == 0x1 && mask[2] == 0x1 && mask[4] == 0x0 && mask[6] == 0x0)
        {
            if (mask[1] == 0x1)
                SetHighlightMaterial(1, 24);
            else
                SetHighlightMaterial(1, 19);
        }
        else if (mask[0] == 0x1 && mask[2] == 0x0 && mask[4] == 0x1 && mask[6] == 0x0)
        {
            SetHighlightMaterial(1, 10);
        }
        else if (mask[0] == 0x1 && mask[2] == 0x0 && mask[4] == 0x0 && mask[6] == 0x1)
        {
            if (mask[7] == 0x1)
                SetHighlightMaterial(1, 22);
            else
                SetHighlightMaterial(1, 18);
        }
        else if (mask[0] == 0x0 && mask[2] == 0x1 && mask[4] == 0x1 && mask[6] == 0x0)
        {
            if (mask[3] == 0x1)
                SetHighlightMaterial(1, 23);
            else
                SetHighlightMaterial(1, 9);
        }
        else if (mask[0] == 0x0 && mask[2] == 0x1 && mask[4] == 0x0 && mask[6] == 0x1)
        {
            SetHighlightMaterial(1, 20);
        }
        else if (mask[0] == 0x0 && mask[2] == 0x0 && mask[4] == 0x1 && mask[6] == 0x1)
        {
            if (mask[5] == 0x1)
                SetHighlightMaterial(1, 21);
            else
                SetHighlightMaterial(1, 8);
        }

        // 3 highlights
        else if (mask[0] == 0x1 && mask[2] == 0x1 && mask[4] == 0x1 && mask[6] == 0x0)
        {
            if (mask[1] == 0x0 && mask[3] == 0x0)
                SetHighlightMaterial(1, 27);
            else if (mask[1] == 0x1 && mask[3] == 0x0)
                SetHighlightMaterial(1, 25);
            else if (mask[1] == 0x0 && mask[3] == 0x1)
                SetHighlightMaterial(1, 26);
            else if (mask[1] == 0x1 && mask[3] == 0x1)
                SetHighlightMaterial(1, 32);
        }
        else if (mask[0] == 0x1 && mask[2] == 0x1 && mask[4] == 0x0 && mask[6] == 0x1)
        {
            if (mask[1] == 0x0 && mask[7] == 0x0)
                SetHighlightMaterial(1, 37);
            else if (mask[1] == 0x1 && mask[7] == 0x0)
                SetHighlightMaterial(1, 36);
            else if (mask[1] == 0x0 && mask[7] == 0x1)
                SetHighlightMaterial(1, 35);
            else if (mask[1] == 0x1 && mask[7] == 0x1)
                SetHighlightMaterial(1, 33);
        }
        else if (mask[0] == 0x0 && mask[2] == 0x1 && mask[4] == 0x1 && mask[6] == 0x1)
        {
            if (mask[3] == 0x0 && mask[5] == 0x0)
                SetHighlightMaterial(1, 17);
            else if (mask[3] == 0x1 && mask[5] == 0x0)
                SetHighlightMaterial(1, 16);
            else if (mask[3] == 0x0 && mask[5] == 0x1)
                SetHighlightMaterial(1, 15);
            else if (mask[3] == 0x1 && mask[5] == 0x1)
                SetHighlightMaterial(1, 34);
        }
        else if (mask[0] == 0x1 && mask[2] == 0x0 && mask[4] == 0x1 && mask[6] == 0x1)
        {
            if (mask[5] == 0x0 && mask[7] == 0x0)
                SetHighlightMaterial(1, 7);
            else if (mask[5] == 0x1 && mask[7] == 0x0)
                SetHighlightMaterial(1, 5);
            else if (mask[5] == 0x0 && mask[7] == 0x1)
                SetHighlightMaterial(1, 6);
            else if (mask[5] == 0x1 && mask[7] == 0x1)
                SetHighlightMaterial(1, 31);
        }

        // 4 highlights
        else if (mask[0] == 0x1 && mask[2] == 0x1 && mask[4] == 0x1 && mask[6] == 0x1)
        {
            // 0 remove
            if (mask[1] == 0x0 && mask[3] == 0x0 && mask[5] == 0x0 && mask[7] == 0x0)
                SetHighlightMaterial(1, 30);

            // 1 remove
            else if (mask[1] == 0x1 && mask[3] == 0x0 && mask[5] == 0x0 && mask[7] == 0x0)
                SetHighlightMaterial(1, 30);
            else if (mask[1] == 0x0 && mask[3] == 0x1 && mask[5] == 0x0 && mask[7] == 0x0)
                SetHighlightMaterial(1, 30);
            else if (mask[1] == 0x0 && mask[3] == 0x0 && mask[5] == 0x1 && mask[7] == 0x0)
                SetHighlightMaterial(1, 30);
            else if (mask[1] == 0x0 && mask[3] == 0x0 && mask[5] == 0x0 && mask[7] == 0x1)
                SetHighlightMaterial(1, 30);

            // 2 remove
            else if (mask[1] == 0x1 && mask[3] == 0x1 && mask[5] == 0x0 && mask[7] == 0x0)
                SetHighlightMaterial(1, 30);
            else if (mask[1] == 0x1 && mask[3] == 0x0 && mask[5] == 0x1 && mask[7] == 0x0)
                SetHighlightMaterial(1, 30);
            else if (mask[1] == 0x1 && mask[3] == 0x0 && mask[5] == 0x0 && mask[7] == 0x1)
                SetHighlightMaterial(1, 30);
            else if (mask[1] == 0x0 && mask[3] == 0x1 && mask[5] == 0x1 && mask[7] == 0x0)
                SetHighlightMaterial(1, 30);
            else if (mask[1] == 0x0 && mask[3] == 0x1 && mask[5] == 0x0 && mask[7] == 0x1)
                SetHighlightMaterial(1, 30);
            else if (mask[1] == 0x0 && mask[3] == 0x0 && mask[5] == 0x1 && mask[7] == 0x1)
                SetHighlightMaterial(1, 30);

            // 3 remove
            else if (mask[1] == 0x1 && mask[3] == 0x1 && mask[5] == 0x1 && mask[7] == 0x0)
                SetHighlightMaterial(1, 30);
            else if (mask[1] == 0x1 && mask[3] == 0x1 && mask[5] == 0x0 && mask[7] == 0x1)
                SetHighlightMaterial(1, 30);
            else if (mask[1] == 0x1 && mask[3] == 0x0 && mask[5] == 0x1 && mask[7] == 0x1)
                SetHighlightMaterial(1, 30);
            else if (mask[1] == 0x0 && mask[3] == 0x1 && mask[5] == 0x1 && mask[7] == 0x1)
                SetHighlightMaterial(1, 30);

            // 4 remove
            else if (mask[1] == 0x1 && mask[3] == 0x1 && mask[5] == 0x1 && mask[7] == 0x1)
                SetHighlightMaterial(1, 30);
        }
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
        set {
            if (value)
                SetHighlightMaterial(1, 0);
            else
                RemoveLayer(Mesh2DLayer.LAYER_HIGHLIGHTS);
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
        set {
            switch(value)
            {
                case TileHighlightColor.HIGHLIGHT_BLUE:
                    mesh.SetColor(Mesh2DLayer.LAYER_HIGHLIGHTS, Color.blue);
                    break;
                case TileHighlightColor.HIGHLIGHT_RED:
                    mesh.SetColor(Mesh2DLayer.LAYER_HIGHLIGHTS, Color.red);
                    break;
                case TileHighlightColor.HIGHLIGHT_GREEN:
                    mesh.SetColor(Mesh2DLayer.LAYER_HIGHLIGHTS, Color.green);
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
