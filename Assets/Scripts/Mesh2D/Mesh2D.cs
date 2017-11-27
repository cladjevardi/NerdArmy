using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mesh2D : MonoBehaviour
{
    /// <summary>
    /// The renderer for the tile.
    /// </summary>
    private GameObject meshRenderer = null;

    /// <summary>
    /// When an animation stops after completion.
    /// </summary>
    /// <param name="name">
    /// The name of the animation that completed. (as String)
    /// </param>
    protected abstract void OnAnimationComplete(object name);

    /// <summary>Helper for getting the mesh renderer.</summary>
    public Mesh2DRenderer mesh
    {
        get { return meshRenderer.GetComponent<Mesh2DRenderer>(); }
    }

    /// <summary>Tile positional information.</summary>
    private Vector2 _position = Vector2.zero;
    public Vector2 position
    {
        get { return mesh.GetPosition(); }
        set { mesh.SetPosition(value); }
    }

    /// <summary>The real rect size and location of the mesh.</summary>
    public Rect _rect = Rect.zero;
    public Rect rect
    {
        get { return mesh.GetGlobalMeshRect(); }
    }

    /// <summary>
    /// Set the material of the mesh.
    /// </summary>
    /// <param name="layer">The layer to render the 2d mesh on.</param>
    /// <param name="id">
    /// The identifier for the material specified by GameManager.
    /// </param>
    /// <param name="type">The material lookup table from GameManager.</param>
    /// <param name="cellWidth">
    /// The width of each cell in the material sprite.
    /// </param>
    /// <param name="cellHeight">
    /// The height of each cell in the material sprite.
    /// </param>
    /// <param name="frameId">
    /// The frame index within the sprite to default to.
    /// </param>
    /// <param name="animations">
    /// The list of animations to add to the mesh.
    /// </param>
    /// <param name="defaultAnimation">
    /// The default name of the animation to player.
    /// </param>
    /// <param name="playImmediately"></param>
    protected void SetMaterial(Mesh2DLayer layer, int id,
        MaterialType type, int cellWidth = -1, int cellHeight = -1,
        int frameId = 0, List<Mesh2DAnimation> animations = null,
        string defaultAnimation = "", bool playImmediately = false)
    {
        if (animations == null)
            animations = new List<Mesh2DAnimation>();

        // Assign the material to the unit.
        mesh.SetMaterial(layer, new Mesh2DMaterial(id,
            type, cellWidth, cellHeight, frameId, animations,
            defaultAnimation, playImmediately));
    }

    /// <summary>
    /// Internal version of setting the animation.
    /// </summary>
    /// <param name="name">The string name of the animation.</param>
    protected void SetAnimation(Mesh2DLayer layer, string name)
    {
        // Acquire the material with the animation from the mesh renderer.
        // TODO: Figure out a way without making a copy of the material.
        Mesh2DMaterial material = mesh.GetMaterial(layer);
        material.SetCurrentAnimation(name, true);

        // Set the animation of the current material.
        mesh.SetMaterial(layer, material);
    }

    /// <summary>Call at creation of object.</summary>
    private void Awake()
    {
        meshRenderer = new GameObject("MeshRenderer");
        meshRenderer.transform.parent = transform;
        meshRenderer.AddComponent<Mesh2DRenderer>();
    }
}