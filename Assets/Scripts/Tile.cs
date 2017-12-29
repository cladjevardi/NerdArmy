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
        SetMaterial(Mesh2DLayer.LAYER_FLOOR, tileId, MaterialType.TILE, 128, 128, frameId, null);
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
        SetMaterial(Mesh2DLayer.LAYER_OBJECT, tileId, MaterialType.TILE, 128, 128, frameId, null);
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
        SetMaterial(Mesh2DLayer.LAYER_ROOF, tileId, MaterialType.TILE, 128, 128, frameId, null);
    }

    /// <summary>
    /// Set the attack highlight material.
    /// </summary>
    /// <param name="tileId">The GameManager tile identifier of the material.</param>
    /// <param name="frameId">The sprite index to display inside the material.</param>
    public void SetAttackHighlightMaterial(int tileId, byte[] mask)
    {
        SetMaterial(Mesh2DLayer.LAYER_ATTACK_HIGHLIGHTS, tileId, MaterialType.EFFECT,
            128, 128, GetHighlightFrameId(mask), null);
    }

    /// <summary>
    /// Set the attack highlight material.
    /// </summary>
    /// <param name="tileId">The GameManager tile identifier of the material.</param>
    /// <param name="frameId">The sprite index to display inside the material.</param>
    public void SetMovementHighlightMaterial(int tileId, byte[] mask)
    {
        SetMaterial(Mesh2DLayer.LAYER_MOVEMENT_HIGHLIGHTS, tileId, MaterialType.EFFECT,
            128, 128, GetHighlightFrameId(mask), null);
    }

    /// <summary>
    /// Set the material for the grid effect.
    /// </summary>
    /// <param name="tileId">The effect tile id to use.</param>
    /// <param name="frameId">The frame id inside the effect tile to use.</param>
    public void SetGridMaterial(int tileId, int frameId = 0)
    {
        SetMaterial(Mesh2DLayer.LAYER_GRID, tileId, MaterialType.EFFECT, 128, 128, frameId, null);
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
                SetGridMaterial(0, 5);
            if (mask == "00-01-00-00")
                SetGridMaterial(0, 4);
            if (mask == "00-00-01-00")
                SetGridMaterial(0, 7);
            if (mask == "00-00-00-01")
                SetGridMaterial(0, 6);
        }
        else
        {
            // Draw the direction based arrow sprites.
            if (mask == "01-00-00-00")
                SetGridMaterial(0, 9);
            if (mask == "00-01-00-00")
                SetGridMaterial(0, 8);
            if (mask == "00-00-01-00")
                SetGridMaterial(0, 11);
            if (mask == "00-00-00-01")
                SetGridMaterial(0, 10);
        }
                
        if (mask == "01-01-00-00")
            SetGridMaterial(0, 2);
        if (mask == "01-00-01-00")
            SetGridMaterial(0, 19);
        if (mask == "01-00-00-01")
            SetGridMaterial(0, 1);
        if (mask == "00-01-01-00")
            SetGridMaterial(0, 3);
        if (mask == "00-01-00-01")
            SetGridMaterial(0, 18);
        if (mask == "00-00-01-01")
            SetGridMaterial(0, 0);
    }

    /// <summary>
    /// Take a byte array and return back the movement/attack highlight
    /// image to use.
    /// </summary>
    /// <param name="mask">
    /// The bitmask that describes directional neibor highlights.</param>
    /// <returns>Returns the frame id to use.</returns>
    private int GetHighlightFrameId(byte[] mask)
    {
        if (mask[0] == 0x0 && mask[2] == 0x0 && mask[4] == 0x0 && mask[6] == 0x0)
            return 22;

        // 1 highlight
        else if (mask[0] == 0x1 && mask[2] == 0x0 && mask[4] == 0x0 && mask[6] == 0x0)
            return 1;
        else if (mask[0] == 0x0 && mask[2] == 0x1 && mask[4] == 0x0 && mask[6] == 0x0)
            return 0;
        else if (mask[0] == 0x0 && mask[2] == 0x0 && mask[4] == 0x1 && mask[6] == 0x0)
            return 3;
        else if (mask[0] == 0x0 && mask[2] == 0x0 && mask[4] == 0x0 && mask[6] == 0x1)
            return 2;

        // 2 highlights
        else if (mask[0] == 0x1 && mask[2] == 0x1 && mask[4] == 0x0 && mask[6] == 0x0)
        {
            if (mask[1] == 0x1)
                return 20;
            else
                return 9;
        }
        else if (mask[0] == 0x1 && mask[2] == 0x0 && mask[4] == 0x1 && mask[6] == 0x0)
        {
            return 4;
        }
        else if (mask[0] == 0x1 && mask[2] == 0x0 && mask[4] == 0x0 && mask[6] == 0x1)
        {
            if (mask[7] == 0x1)
                return 18;
            else
                return 11;
        }
        else if (mask[0] == 0x0 && mask[2] == 0x1 && mask[4] == 0x1 && mask[6] == 0x0)
        {
            if (mask[3] == 0x1)
                return 21;
            else
                return 12;
        }
        else if (mask[0] == 0x0 && mask[2] == 0x1 && mask[4] == 0x0 && mask[6] == 0x1)
        {
            return 13;
        }
        else if (mask[0] == 0x0 && mask[2] == 0x0 && mask[4] == 0x1 && mask[6] == 0x1)
        {
            if (mask[5] == 0x1)
                return 19;
            else
                return 10;
        }

        // 3 highlights
        else if (mask[0] == 0x1 && mask[2] == 0x1 && mask[4] == 0x1 && mask[6] == 0x0)
        {
            if (mask[1] == 0x0 && mask[3] == 0x0)
                return 14;
            else if (mask[1] == 0x1 && mask[3] == 0x0)
                return 23;
            else if (mask[1] == 0x0 && mask[3] == 0x1)
                return 32;
            else if (mask[1] == 0x1 && mask[3] == 0x1)
                return 29;
        }
        else if (mask[0] == 0x1 && mask[2] == 0x1 && mask[4] == 0x0 && mask[6] == 0x1)
        {
            if (mask[1] == 0x0 && mask[7] == 0x0)
                return 17;
            else if (mask[1] == 0x1 && mask[7] == 0x0)
                return 26;
            else if (mask[1] == 0x0 && mask[7] == 0x1)
                return 35;
            else if (mask[1] == 0x1 && mask[7] == 0x1)
                return 28;
        }
        else if (mask[0] == 0x0 && mask[2] == 0x1 && mask[4] == 0x1 && mask[6] == 0x1)
        {
            if (mask[3] == 0x0 && mask[5] == 0x0)
                return 15;
            else if (mask[3] == 0x1 && mask[5] == 0x0)
                return 33;
            else if (mask[3] == 0x0 && mask[5] == 0x1)
                return 24;
            else if (mask[3] == 0x1 && mask[5] == 0x1)
                return 30;
        }
        else if (mask[0] == 0x1 && mask[2] == 0x0 && mask[4] == 0x1 && mask[6] == 0x1)
        {
            if (mask[5] == 0x0 && mask[7] == 0x0)
                return 16;
            else if (mask[5] == 0x1 && mask[7] == 0x0)
                return 25;
            else if (mask[5] == 0x0 && mask[7] == 0x1)
                return 34;
            else if (mask[5] == 0x1 && mask[7] == 0x1)
                return 27;
        }

        // 4 highlights
        else if (mask[0] == 0x1 && mask[2] == 0x1 && mask[4] == 0x1 && mask[6] == 0x1)
        {
            // 0 remove
            if (mask[1] == 0x0 && mask[3] == 0x0 && mask[5] == 0x0 && mask[7] == 0x0)
                return 51;

            // 1 remove
            else if (mask[1] == 0x1 && mask[3] == 0x0 && mask[5] == 0x0 && mask[7] == 0x0)
                return 44;
            else if (mask[1] == 0x0 && mask[3] == 0x1 && mask[5] == 0x0 && mask[7] == 0x0)
                return 43;
            else if (mask[1] == 0x0 && mask[3] == 0x0 && mask[5] == 0x1 && mask[7] == 0x0)
                return 42;
            else if (mask[1] == 0x0 && mask[3] == 0x0 && mask[5] == 0x0 && mask[7] == 0x1)
                return 41;

            // 2 remove
            else if (mask[1] == 0x1 && mask[3] == 0x1 && mask[5] == 0x0 && mask[7] == 0x0)
                return 45;
            else if (mask[1] == 0x1 && mask[3] == 0x0 && mask[5] == 0x1 && mask[7] == 0x0)
                return 53;
            else if (mask[1] == 0x1 && mask[3] == 0x0 && mask[5] == 0x0 && mask[7] == 0x1)
                return 48;
            else if (mask[1] == 0x0 && mask[3] == 0x1 && mask[5] == 0x1 && mask[7] == 0x0)
                return 46;
            else if (mask[1] == 0x0 && mask[3] == 0x1 && mask[5] == 0x0 && mask[7] == 0x1)
                return 52;
            else if (mask[1] == 0x0 && mask[3] == 0x0 && mask[5] == 0x1 && mask[7] == 0x1)
                return 47;

            // 3 remove
            else if (mask[1] == 0x1 && mask[3] == 0x1 && mask[5] == 0x1 && mask[7] == 0x0)
                return 37;
            else if (mask[1] == 0x1 && mask[3] == 0x1 && mask[5] == 0x0 && mask[7] == 0x1)
                return 39;
            else if (mask[1] == 0x1 && mask[3] == 0x0 && mask[5] == 0x1 && mask[7] == 0x1)
                return 36;
            else if (mask[1] == 0x0 && mask[3] == 0x1 && mask[5] == 0x1 && mask[7] == 0x1)
                return 38;

            // 4 remove
            else if (mask[1] == 0x1 && mask[3] == 0x1 && mask[5] == 0x1 && mask[7] == 0x1)
                return 31;
        }

        // Should never be hit.
        return -1;
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
    private bool _movementHighlight = false;
    public bool movementHighlight
    {
        get { return _movementHighlight; }
        set {
            if (!value)
                RemoveLayer(Mesh2DLayer.LAYER_MOVEMENT_HIGHLIGHTS);
            _movementHighlight = value;
        }
    }

    /// <summary>
    /// Whether there is a movement highlight effect
    /// being displayed on this tile.
    /// </summary>
    private bool _attackHighlight = false;
    public bool attackHighlight
    {
        get { return _attackHighlight; }
        set
        {
            if (!value)
                RemoveLayer(Mesh2DLayer.LAYER_ATTACK_HIGHLIGHTS);
            _attackHighlight = value;
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
