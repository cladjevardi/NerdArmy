using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A 2d mesh animation that is organized by an animator.
/// </summary>
public class Mesh2DAnimation
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
    private GameObject _listener = null;
    public GameObject listener
    {
        get { return _listener; }
        set { _listener = value; }
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
                    if (_listener != null)
                        _listener.SendMessage("OnAnimationComplete", name);
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

    /// <summary>
    /// The default constructor for an animation.
    /// </summary>
    public Mesh2DAnimation()
    {}

    /// <summary>
    /// The constructor for an animation.
    /// </summary>
    /// <param name="name">The lookup name for the animation.</param>
    /// <param name="frameSeqence">The frame id's that make up this animation.</param>
    /// <param name="frameRate">The speed of the animation.</param>
    /// <param name="shouldLoop">
    /// Whether the animation should loop. If
    /// ShouldLoop is not set to true, OnAnimationComplete(string name)
    /// will be invoked from the messageReciever if set.
    /// </param>
    /// <param name="messageReceiver">The receiver object to call when
    /// the animation needs to send messages back to the listener.</param>
    public Mesh2DAnimation(string name, List<int> frameSequence,
        float frameRate = 0.25f, bool shouldLoop = true,
        GameObject listener = null)
    {
        this.name = name;
        this.frameRate = frameRate;
        this.frameSequence = frameSequence;
        this.shouldLoop = shouldLoop;
        this.listener = listener;
    }
}

/// <summary>
/// A manager for mesh 2d animations.
/// </summary>
public class Mesh2DAnimator
{
    /// <summary>
    /// The list of animations available to this meterial id.
    /// </summary>
    private List<Mesh2DAnimation> animations = new List<Mesh2DAnimation>();

    /// <summary>The index of the current animation.</summary>
    private Mesh2DAnimation currentAnimation = new Mesh2DAnimation();

    /// <summary>The default listener for animation messages.</summary>
    private GameObject _listener = null;
    public GameObject listener
    {
        get { return _listener; }
        set
        {
            // Update the game object for all curreny animations.
            _listener = value;
            foreach (Mesh2DAnimation animation in animations)
            {
                if (animation.IsValid())
                    animation.listener = value;
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
        Mesh2DAnimation animation = new Mesh2DAnimation();
        animation.name = name;
        animation.frameRate = frameRate;
        animation.isPaused = true;
        animation.shouldLoop = shouldLoop;
        animation.frameSequence = frameIndices;

        AddAnimation(animation);
    }

    /// <summary>
    /// Add an animation to the animator.
    /// </summary>
    /// <param name="animation">
    /// The animation to add to the animator to keep track of. If
    /// this is the first animation added, the current animation
    /// will be set immediately.
    /// </param>
    public void AddAnimation(Mesh2DAnimation animation)
    {
        // Assign the default message receiver assigned to the animator.
        if (animation.listener == null)
            animation.listener = _listener;

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
        foreach (Mesh2DAnimation animation in animations)
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
