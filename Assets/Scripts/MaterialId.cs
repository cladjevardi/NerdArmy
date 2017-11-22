using System.Collections.Generic;
using UnityEngine;

/// <summary>The lookup table for the material</summary>
public enum MaterialType
{
    NONE = -1,
    TILE,
    UNIT,
    EFFECT,
}

public class MaterialAnimation
{
    /// <summary>The index of the frame sequence.</summary>
    private int currentFrameSequenceIndex = 0;

    /// <summary>
    /// The current time to check the frame rate against for a sprite animation
    /// transition.
    /// </summary>
    private float currentTime = 0.0f;

    /// <summary>
    /// The name of the animation. Used as a lookup reference.
    /// </summary>
    private string _name = "";
    public string name
    {
        get { return _name; }
        set { _name = value; }
    }

    /// <summary>
    /// How fast the sprite animates.
    /// </summary>
    private float _frameRate = 0;
    public float frameRate
    {
        get { return _frameRate; }
        set { _frameRate = value; }
    }

    /// <summary>
    /// The sequence of frames to make this animation.
    /// </summary>
    private List<int> _frameSequence = new List<int>();
    public List<int> frameSequence
    {
        get { return _frameSequence; }
        set { _frameSequence = value; }
    }

    /// <summary>
    /// If the animation is playing.
    /// </summary>
    private bool _isPaused = true;
    public bool isPaused
    {
        get { return _isPaused; }
        set { _isPaused = value; }
    }

    /// <summary>If the animation should loop.</summary>
    private bool _shouldLoop = true;
    public bool shouldLoop
    {
        get { return _shouldLoop; }
        set { _shouldLoop = value; }
    }

    /// <summary>Set the game object for receiving messages.</summary>
    private GameObject _messageReceiver = null;
    public GameObject messageReceiver
    {
        get { return _messageReceiver; }
        set { _messageReceiver = value; }
    }

    /// <summary>
    /// Update the animation for GetCurrentFrame to report the correct frame index
    /// In a animation sequence. This muse be called within every frame from
    /// the Update() function that is inherited from a MonoBehaviour gameObject
    /// to remain up-to-date.
    /// </summary>
    public void Update()
    {
        if (_isPaused)
            return;

        // Update our current frame.
        currentTime += Time.deltaTime;

        // Check if we should update
        if (currentTime >= _frameRate)
        {
            // Keep the leftovers.
            currentTime = currentTime % frameRate;

            // Set the next frame index.
            if (currentFrameSequenceIndex + 1 < _frameSequence.Count)
                currentFrameSequenceIndex++;
            else
            {
                // Animation has ended. Reset.
                if (!_shouldLoop)
                {
                    _isPaused = true;

                    // Notification for when an animation completes.
                    if (_messageReceiver != null)
                        _messageReceiver.SendMessage("OnAnimationComplete", name);
                }
                else
                    currentFrameSequenceIndex = 0;
            }
        }
    }

    /// <summary>
    /// Retrieve the frame index located within a sequence of frames that
    /// make up this animation.
    /// </summary>
    /// <returns>
    /// Returns the index inside a sprite map. This is the index
    /// used for retrieving the uv map vectors.
    /// </returns>
    public int GetCurrentFrame()
    {
        return _frameSequence[currentFrameSequenceIndex];
    }

    /// <summary>Set pause to false to begin animation updating.</summary>
    public void Play()
    {
        _isPaused = false;
    }

    /// <summary>Set pause to true and stop all animation.</summary>
    public void Pause()
    {
        _isPaused = true;
    }

    /// <summary>
    /// Returns whether the animation has been initialized properly.
    /// </summary>
    /// <returns>Returns whether the animation is setup appropriately.</returns>
    public bool IsValid()
    {
        // We determine the validity of the animation by it's lack of name.
        return (name.Length > 0);
    }
}

public class AnimationManager
{
    /// <summary>
    /// The list of animations available to this meterial id.
    /// </summary>
    private List<MaterialAnimation> animations = new List<MaterialAnimation>();

    /// <summary>The index of the current animation.</summary>
    private MaterialAnimation currentAnimation = new MaterialAnimation();

    /// <summary>Set the game object for receiving messages.</summary>
    private GameObject _messageReceiver = null;
    public GameObject messageReceiver
    {
        get { return _messageReceiver; }
        set
        {
            // Update the game object for all curreny animations.
            _messageReceiver = value;
            foreach (MaterialAnimation animation in animations)
            {
                if (animation.IsValid())
                    animation.messageReceiver = value;
            }
        }
    }

    /// <summary>
    /// Add a animation to the animation list.
    /// </summary>
    /// <param name="name">The name of the animation.</param>
    /// <param name="frameIndices">
    /// The list of frame indicies that make up this animation.
    /// 
    /// For example, a sprite map that is split into 16 total images,
    /// 4 images wide and 4 images deep.
    /// 
    /// The first index would be 0 (from the top to bottom, left to right),
    /// The image right of 0 would be 1.
    /// The image below 0 would be 4.
    /// The last image would be 15.</param>
    /// <param name="shouldLoop">
    /// Whether the animation should loop automatically or stop after
    /// one sequence.
    /// </param>
    public void AddAnimation(string name, List<int> frameIndices, float frameRate = 0.25f, bool shouldLoop = true)
    {
        // Construct an animation for a material.
        MaterialAnimation animation = new MaterialAnimation();
        animation.name = name;
        animation.frameRate = frameRate;
        animation.isPaused = true;
        animation.shouldLoop = shouldLoop;
        animation.frameSequence = frameIndices;

        // Assign the default message receiver assigned to the animation manager.
        animation.messageReceiver = _messageReceiver;

        // Add the animation to the list of animations.
        animations.Add(animation);

        // If we do not already have a current animation. Set it.
        if (!IsValid())
            currentAnimation = animation;
    }

    /// <summary>Set the current animation.</summary>
    /// <param name="name">The name of the animation to select.</param>
    public void SetCurrentAnimation(string name)
    {
        // Find the animation in the current list of animations.
        foreach (MaterialAnimation animation in animations)
        {
            // Set the current animation to the referenced name.
            if (animation.name == name)
            {
                currentAnimation = animation;
                return;
            }
        }

        Debug.LogError("Invalid animation selected (animation=" + name + ")");
    }

    /// <summary>Play the currently set animation.</summary>
    public void PlayAnimation()
    {
        if (IsValid())
            currentAnimation.Play();
    }

    /// <summary>Pause the currently set animation.</summary>
    public void PauseAnimation()
    {
        if (IsValid())
            currentAnimation.Pause();
    }

    /// <summary>Set if the animation should loop indefinately.</summary>
    /// <param name="shouldLoop">Whether the animation should loop</param>
    public void SetAnimationLoop(bool shouldLoop)
    {
        if (IsValid())
            currentAnimation.shouldLoop = shouldLoop;
    }

    /// <summary>
    /// Get the current frame of the current animation.
    /// </summary>
    /// <returns>Returns the frame index of the animation playing.</returns>
    public int GetCurrentFrame()
    {
        // Retrieve the current frame of the animation player.
        if (IsValid())
            return currentAnimation.GetCurrentFrame();

        // We do not have a properly setup animation and requested
        // frame information.
        Debug.LogError("Requested frame of uninitialized animation");
        return -1;
    }

    /// <summary>
    /// This should be called every frame to handle animation properly.
    /// </summary>
    public void Update()
    {
        // Only update the animation if the current animation is valid.
        if (IsValid())
            currentAnimation.Update();
    }

    /// <summary>Whether the animation manager is setup properly.</summary>
    public bool IsValid()
    {
        return currentAnimation.IsValid();
    }
}

/// <summary>A class that identifies a given material from GameManager.</summary>
public class MaterialId
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
    /// The animation manager that handles a given sequence of material
    /// frames by animation names.
    /// </summary>
    private AnimationManager _animationManager = new AnimationManager();
    public AnimationManager animationManager
    {
        get { return _animationManager; }
        set { _animationManager = value; }
    }

    /// <summary>
    /// Get the uv map to apply to a mesh to get the exact coordinates
    /// of a frame id in an animation or sprite map.
    /// </summary>
    /// <param name="frameId">
    /// The frame sequence id of the sprite in a sprite map.</param>
    /// <returns></returns>
    public Vector2[] uv()
    {
        // If animations have not been setup. Always use frame 0.
        int frameId = _animationManager.IsValid()
            ? _animationManager.GetCurrentFrame() : 0;

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
        int frameX = frameId % frameWidthCount;
        int frameY = frameId / frameWidthCount;

        // Calculate the UV ratio per frame.
        float xStride = 1.0f / frameWidthCount;
        float yStride = 1.0f / frameHeightCount;

        // We can now generate our list of uv vectors.
        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(frameX * xStride, frameY * yStride); // (0.0f, 0.0f)
        uv[1] = new Vector2((frameX + 1) * xStride, frameY * yStride); // (1.0f, 0.0f)
        uv[2] = new Vector2(frameX * xStride, (frameY + 1) * yStride); // (0.0f, 1.0f)
        uv[3] = new Vector2((frameX + 1) * xStride, (frameY + 1) * yStride); // (1.0f, 1.0f)
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
    /// Constructor for a MaterialId
    /// </summary>
    /// <param name="id">The identifier of the material.</param>
    /// <param name="type">The material lookup table type.</param>
    public MaterialId(int id, MaterialType type = MaterialType.TILE)
    {
        this.id = id;
        this.type = type;
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