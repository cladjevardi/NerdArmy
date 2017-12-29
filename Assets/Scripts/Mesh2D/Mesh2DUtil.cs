using UnityEngine;
using System.Collections.Generic;

/// <summary>The material lookup table to use.</summary>
public enum MaterialType
{
    NONE = -1,
    TILE,
    UNIT,
    EFFECT,
}

/// <summary>
/// The layer of a 2d mesh. This adjusts the z to draw the tile.
/// Above or below other tiles.
/// </summary>
public enum Mesh2DLayer
{
    /// <summary>This unit is invisible and should not be drawn.</summary>
    LAYER_INVISIBLE = 0,

    /// <summary>
    /// The layer for all units hidden below the
    /// floor.
    /// </summary>
    LAYER_BURROWED,

    /// <summary>The layer for all floor tiles.</summary>
    LAYER_FLOOR,

    /// <summary>
    /// The layer of the grid. Or special effects.
    /// </summary>
    LAYER_GRID,

    /// <summary>
    /// The layer for objects, items, or floor tiles with
    /// transparency.
    /// </summary>
    LAYER_OBJECT,

    /// <summary>The layer for all units.</summary>
    LAYER_UNITS,

    /// <summary>
    /// The layer that can hide ground units. Has transparency and
    /// adjustable alpha when gaining vision underneath.
    /// </summary>
    LAYER_ROOF,

    /// <summary>
    /// The layer that flying units are on. These units appear above
    /// trees and roofs.
    /// </summary>
    LAYER_FLYINGUNITS,

    /// <summary>
    /// The layers that displays health.
    /// </summary>
    LAYER_HEALTH_BASE,
    LAYER_HEALTH_DAMAGED,
    LAYER_HEALTH_REMAINING,

    /// <summary>
    /// The layer that displays the bomb attack used by the bomber.
    /// </summary>
    LAYER_EMP,

    /// <summary>
    /// The layer that buffs are rendered on.
    /// </summary>
    LAYER_BUFFS,

    /// <summary>
    /// The layer that displays damage.
    /// </summary>
    LAYER_DAMAGE,

    /// <summary>The layer for red tile highlighted effects.</summary>
    LAYER_ATTACK_HIGHLIGHTS,

    /// <summary>The layer for blue tile highlighted effects.</summary>
    LAYER_MOVEMENT_HIGHLIGHTS,

    /// <summary>The layer for astart pathing arrow effects.</summary>
    LAYER_ARROW_HIGHLIGHTS,

    /// <summary>
    /// The layer for the attack marker.
    /// </summary>
    LAYER_ATTACK_MARKER,

    /// <summary>Total count of layers.</summary>
    LAYER_COUNT,
}

/// <summary>A class that identifies a given material from GameManager.</summary>
public class Mesh2DMaterial
{
    /// <summary>The materoal lookup table type.</summary>
    private MaterialType _type = MaterialType.NONE;
    public MaterialType type
    {
        get { return _type; }
        set { _type = value; }
    }

    /// <summary>The identifier of the material. Specified in GameManager.</summary>
    private int _id = 0;
    public int id
    {
        get { return _id; }
        set { _id = value; }
    }

    /// <summary>
    /// The width of each cell in a sprite map. Cell width of -1
    /// uses the entire width of the material.
    /// </summary>
    private int _cellWidth = -1;
    public int cellWidth
    {
        get { return _cellWidth; }
        set { _cellWidth = value; }
    }

    /// <summary>
    /// The height of each cell in a sprite map. Cell height of -1
    /// uses the entire width of the material.
    /// </summary>
    private int _cellHeight = -1;
    public int cellHeight
    {
        get { return _cellHeight; }
        set { _cellHeight = value; }
    }

    /// <summary>
    /// From the 0 to 1, left to right, how much of the image is drawn.
    /// Utilized primarily for progress bars.
    /// </summary>
    private double _percent = 1.0;
    public double percent
    {
        get { return _percent; }
        set { _percent = value; }
    }

    /// <summary>
    /// The frame id to use for the texture.
    /// </summary>
    private int _frameId = 0;
    public int frameId
    {
        get { return _frameId; }
        set { _frameId = value; }
    }

    /// <summary>
    /// Flip the material horizontally.
    /// </summary>
    private bool _flipX = false;
    public bool flipX
    {
        get { return _flipX; }
        set { _flipX = value; }
    }

    /// <summary>
    /// Flip the material vertically.
    /// </summary>
    private bool _flipY = false;
    public bool flipY
    {
        get { return _flipY; }
        set { _flipY = value; }
    }

    /// <summary>
    /// The animation manager that handles a given sequence of material
    /// frames by animation names.
    /// </summary>
    private Mesh2DAnimator _animator = new Mesh2DAnimator();

    /// <summary>
    /// Get the uv map to apply to a mesh to get the exact coordinates
    /// of a frame id in an animation or sprite map.
    /// </summary>
    /// <param name="frameId">
    /// The frame sequence id of the sprite in a sprite map.</param>
    /// <returns></returns>
    public Vector2[] uv()
    {
        int frameIdentifier = _animator.IsValid() ?
            _animator.GetCurrentFrame() : _frameId;

        // Get the material of the current id/type.
        Material material = GetMaterial();

        // Find the location of the sprite within the material.
        int width = material.mainTexture.width;
        int height = material.mainTexture.height;

        // How many frames from left to right is the image.
        // Convert -1 to the maximum width and height of the material.
        int frameWidthCount = width / (cellWidth == -1 ? width : cellWidth);
        int frameHeightCount = height / (cellHeight == -1 ? height : cellHeight);

        // Which frame in the sprite map are we.
        int frameX = frameIdentifier % frameWidthCount;
        int frameY = frameIdentifier / frameWidthCount;

        // Calculate the UV ratio per frame.
        float xStride = 1.0f / frameWidthCount;
        float yStride = 1.0f / frameHeightCount;

        // Determine uv map facing.
        float x1 = !flipX ? frameX * xStride : (frameX + 1) * xStride;
        float x2 = !flipX ? (frameX + 1) * xStride : frameX * xStride;
        float y1 = !flipY ? frameY * yStride : (frameY + 1) * yStride;
        float y2 = !flipY ? (frameY + 1) * yStride : frameY * yStride;

        // We can now generate our list of uv vectors.
        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(x1, y1); // (0.0f, 0.0f)
        uv[1] = new Vector2(x2, y1); // (1.0f, 0.0f)
        uv[2] = new Vector2(x1, y2); // (0.0f, 1.0f)
        uv[3] = new Vector2(x2, y2); // (1.0f, 1.0f)
        return uv;
    }

    /// <summary>
    /// Get the material from the global material table in GameManager.
    /// </summary>
    public Material GetMaterial()
    {
        // First verify if the information associated with this MaterialId
        // will actually give a proper Material.
        if (!IsValid())
        {
            // The material failed to load.
            Debug.LogError("Invalid MaterialId (type=" + type.ToString() + " id=" + id + ")");
            return null;
        }

        // Loopup the material from the global material table.
        switch (type)
        {
            case MaterialType.TILE:
                return GameManager.instance.tileMaterials[id];
            case MaterialType.UNIT:
                return GameManager.instance.unitMaterials[id];
            case MaterialType.EFFECT:
                return GameManager.instance.effectMaterials[id];

            // We should never reach here is IsValid is doing its
            // job properly.
            default:
                return null;
        }
    }
    
    /// <summary>
    /// Optional constructor for a MaterialId.
    /// </summary>
    /// <param name="id">The identifier for the material from GameManager.</param>
    /// <param name="type">The material lookup table to pick from.</param>
    /// <param name="cellWidth">
    /// The sprite cell width size. Leaving as -1 means to use the
    /// entire width of the file.
    /// </param>
    /// <param name="cellHeight">
    /// The sprite cell height size. Leaving as -1 means to use the
    /// entire height of the file.
    /// </param>
    /// <param name="frameId">The sprite frame inside the image.</param>
    /// <param name="animations">
    /// The list of animations for this material.
    /// </param>
    /// <param name="defaultAnimation">
    /// The default animation to play.
    /// </param>
    /// <param name="playImmediately">
    /// Whether the default animation should play immediately. The default
    /// animation must be set to work.
    /// </param>
    public Mesh2DMaterial(int id, MaterialType type = MaterialType.TILE,
        int cellWidth = -1, int cellHeight = -1, int frameId = 0,
        List<Mesh2DAnimation> animations = null,
        string defaultAnimation = "", bool playImmediately = false,
        GameObject listener = null)
    {
        this.id = id;
        this.type = type;
        this.cellWidth = cellWidth;
        this.cellHeight = cellHeight;
        this.frameId = frameId;
        SetAnimationListener(listener);

        if (animations != null)
        {
            // Add each animation to the meshes animator.
            foreach (Mesh2DAnimation animation in animations)
            {
                _animator.AddAnimation(animation);
            }

            // Set the default animation.
            if (defaultAnimation.Length != 0)
            {
                _animator.SetCurrentAnimation(defaultAnimation);

                // Play the default animation if set.
                if (playImmediately)
                    _animator.PlayAnimation();
            }
        }
    }

    /// <summary>
    /// Optional constructor for a MaterialId.
    /// </summary>
    /// <param name="id">The identifier for the material from GameManager.</param>
    /// <param name="type">The material lookup table to pick from.</param>
    /// <param name="cellWidth">
    /// The sprite cell width size. Leaving as -1 means to use the
    /// entire width of the file.
    /// </param>
    /// <param name="cellHeight">
    /// The sprite cell height size. Leaving as -1 means to use the
    /// entire height of the file.
    /// </param>
    /// <param name="frameId">The sprite frame inside the image.</param>
    /// <param name="percent">
    /// The percent of the animation drawn from left to right.
    /// </param>
    public Mesh2DMaterial(int id, MaterialType type = MaterialType.TILE,
        int cellWidth = -1, int cellHeight = -1, int frameId = 0,
        double percent = 1.0f)
    {
        this.id = id;
        this.type = type;
        this.cellWidth = cellWidth;
        this.cellHeight = cellHeight;
        this.frameId = frameId;
        this.percent = percent;
    }

    /// <summary>Set the current animation.</summary>
    /// <param name="name">The name of the animation to select.</param>
    public void SetCurrentAnimation(string name, bool playImmediately = false)
    {
        _animator.SetCurrentAnimation(name);

        // Play the current animation if set.
        if (playImmediately)
            _animator.PlayAnimation();
    }

    /// <summary>Play the currently set animation.</summary>
    public void PlayAnimation()
    {
        _animator.PlayAnimation();
    }

    /// <summary>Pause the currently set animation.</summary>
    public void PauseAnimation()
    {
        _animator.PauseAnimation();
    }

    /// <summary>Set if the animation should loop indefinately.</summary>
    /// <param name="shouldLoop">Whether the animation should loop</param>
    public void SetAnimationLoop(bool shouldLoop)
    {
        _animator.SetAnimationLoop(shouldLoop);
    }

    /// <summary>
    /// Set the listener for animation triggers.
    /// </summary>
    /// <param name="listener">The game object to send events to.</param>
    public void SetAnimationListener(GameObject listener)
    {
        _animator.listener = listener;
    }

    /// <summary>
    /// This should be called every frame to handle animation properly.
    /// </summary>
    public void Update()
    {
        _animator.Update();
    }

    /// <summary>Clear any references to older material information</summary>
    public void Clear()
    {
        // Reset any values to their defaults.
        id = 0;
        type = MaterialType.NONE;
    }

    /// <summary>
    /// Whether the material will be found within the game manager appropriately.
    /// </summary>
    /// <returns>Returns whether the material will be loaded properly.</returns>
    public bool IsValid()
    {
        // Get the global tile map array sizes before lookup.
        int arrayLength = 0;
        if (type == MaterialType.TILE)
            arrayLength = GameManager.instance.tileMaterials.Length;
        if (type == MaterialType.UNIT)
            arrayLength = GameManager.instance.unitMaterials.Length;
        if (type == MaterialType.EFFECT)
            arrayLength = GameManager.instance.effectMaterials.Length;

        // The id cannot exceed the bounds of the array nor be assigned to none.
        return id < arrayLength
            && id >= -1
            && type != MaterialType.NONE;
    }
}
