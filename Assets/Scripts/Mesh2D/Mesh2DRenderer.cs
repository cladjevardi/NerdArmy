using UnityEngine;

/// <summary>
/// A class that represents layered 2d mesh grid rending.
/// </summary>
public class Mesh2DRenderer : MonoBehaviour
{
    /// <summary>The highlight layers default alpha.</summary>
    private float defaultHighlightAlpha = 0.6f;

    /// <summary>The highlight layers default alpha.</summary>
    private float defaultGridAlpha = 1.00f;
    
    /// <summary>Tile materials at every layer.</summary>
    private Mesh2DMaterial[] materials = new Mesh2DMaterial[(int)Mesh2DLayer.LAYER_COUNT];

    /// <summary>Tile meshes at every layer.</summary>
    private GameObject[] gameObjects = new GameObject[(int)Mesh2DLayer.LAYER_COUNT];

    /// <summary>
    /// The color to apply over the material. This can be used for
    /// alpha highlights. Or highlighting an enemy/item to the payer.
    /// </summary>
    private Color[] colors = new Color[(int)Mesh2DLayer.LAYER_COUNT];

    /// <summary>
    /// Set the color of the material. This can be used to highlight a unit
    /// or tile a specific color, or to add an alpha channel.
    /// </summary>
    /// <param name="layer">The tile layer to apply the color highlight to.</param>
    /// <param name="color">The new color to assign the tile.</param>
    /// <returns>The previously assigned color.</returns>
    public Color SetColor(Mesh2DLayer layer, Color color)
    {
        // Store the color that was currently set.
        Color oldColor = colors[(int)layer];

        // When generate mesh is called this color will be applied over it.
        colors[(int)layer] = color;

        // Regenerate the mesh on that layer.
        Mesh2DMaterial material = materials[(int)layer];
        if (material != null)
            GenerateMesh(layer);

        // Return the color we changed from.
        return oldColor;
    }

    /// <summary>Get the color of a specific tile layer.</summary>
    /// <param name="layer">The tile layer to get the color from.</param>
    /// <returns>The currently set color for a tile layer.</returns>
    public Color GetColor(Mesh2DLayer layer)
    {
        return colors[(int)layer];
    }

    /// <summary>Resets a layers color overlay.</summary>
    /// <param name="layer">The layer to reset.</param>
    public void ResetColor(Mesh2DLayer layer)
    {
        float alpha = 1.0f;
        //if (layer == Mesh2DLayer.LAYER_HIGHLIGHTS)
        //    alpha = defaultHighlightAlpha;
        //if (layer == Mesh2DLayer.LAYER_GRID)
        //    alpha = defaultGridAlpha;

        // Highlight layer automatically receives an alpha channel.
        colors[(int)layer] = new Color(1.0f, 1.0f, 1.0f, alpha);

        if (materials[(int)layer] != null)
            GenerateMesh(layer);
    }

    /// <summary>Sets the material of the specified layer.</summary>
    /// <param name="layer">The layer of the material you want to set.</param>
    /// <param name="material">The material you want to set the tile to.</param>
    /// <returns>The previously assigned material.</returns>
    public Mesh2DMaterial SetMaterial(Mesh2DLayer layer, Mesh2DMaterial material)
    {
        // Store the material that was currently set.
        Mesh2DMaterial oldMaterial = materials[(int)layer];

        // Ensure there is a game object at this layer.
        if (gameObjects[(int)layer] == null)
            AllocateMesh(layer);

        // Get the renderer component and assign the current material to the mesh.
        MeshRenderer renderer = gameObjects[(int)layer].GetComponent<MeshRenderer>();
        renderer.material = material.GetMaterial();

        // Store the material for later.
        materials[(int)layer] = material;

        // Regenerate the mesh on that layer.
        if (renderer.material != null)
            GenerateMesh(layer);

        // Return the material we changed from.
        return oldMaterial;
    }

    /// <summary>Get the material of the mesh from the specified layer.</summary>
    /// <param name="layer">The layer to pull the material from.</param>
    /// <returns>Returns the material at that layer.</returns>
    public Mesh2DMaterial GetMaterial(Mesh2DLayer layer)
    {
        return materials[(int)layer];
    }

    /// <summary>Removes the material from that layer, and removes the mesh.</summary>
    /// <param name="layer">The layer to remove the material from.</param>
    public void RemoveMaterial(Mesh2DLayer layer)
    {
        // Store the material for later.
        materials[(int)layer] = null;

        // Delete any drawn mesh.
        if (gameObjects[(int)layer] != null)
        {
            MeshFilter filter = gameObjects[(int)layer].GetComponent<MeshFilter>();
            filter.mesh.Clear();
        }
    }

    /// <summary>
    /// Regenerates all meshes from every layer. Does not draw meshes for
    /// layers that have no material assigned.
    /// </summary>
    private void RegenerateMeshes()
    {
        // Regenerate all tiles with the appropriate coordinate.
        for (int index = 0; index < (int)Mesh2DLayer.LAYER_COUNT; ++index)
        {
            if (materials[index] != null)
            {
                GenerateMesh((Mesh2DLayer)index);
            }
        }
    }

    /// <summary>
    /// Destroy a mesh at specified layer.
    /// </summary>
    /// <param name="layer">The layer of the mesh to destroy.</param>
    private void DestroyMesh(Mesh2DLayer layer)
    {
        // Destroy the mesh at that layer.
        if (gameObjects[(int)layer] != null)
        {
            DestroyObject(gameObjects[(int)layer]);
            gameObjects[(int)layer] = null;
        }

        ResetColor(layer);
    }

    /// <summary>
    /// Allocate mesh at specified layer.
    /// </summary>
    /// <param name="layer">The layer of the mesh to create.</param>
    private void AllocateMesh(Mesh2DLayer layer)
    {
        // Create a custom mesh at the specified layer.
        if (gameObjects[(int)layer] == null)
        {
            gameObjects[(int)layer] = new GameObject("GameObject_" + layer.ToString());
            gameObjects[(int)layer].transform.SetParent(transform);
            gameObjects[(int)layer].transform.localPosition = new Vector2(0, 0);
            gameObjects[(int)layer].AddComponent<MeshFilter>();
            gameObjects[(int)layer].AddComponent<MeshRenderer>();

            ResetColor(layer);
        }
    }

    /// <summary>
    /// Get the list of vertices from a display rect. 
    /// </summary>
    /// <param name="displayRect">The display rect for a tile.</param>
    /// <param name="z">The z coordinate to use.</param>
    /// <returns>Returns the list of vertices to use for a custom mesh.</returns>
    private Vector3[] GetVertices(Rect displayRect, float z)
    {
        // Create the list of vertices.
        Vector3[] verts = new Vector3[4];
        verts[0] = new Vector3(displayRect.x,
            displayRect.y, z);
        verts[1] = new Vector3(displayRect.x + displayRect.width,
            displayRect.y, z);
        verts[2] = new Vector3(displayRect.x,
            displayRect.y + displayRect.height, z);
        verts[3] = new Vector3(displayRect.x + displayRect.width,
            displayRect.y + displayRect.height, z);
        return verts;
    }

    /// <summary>
    /// Generates the mesh for the current cooridnate and material. This
    /// function is safe to call again
    /// </summary>
    /// <param name="layer">The layer to generate the mesh on.</param>
    private void GenerateMesh(Mesh2DLayer layer)
    {
        // Add a mesh filter and mesh renderer.
        MeshFilter filter = gameObjects[(int)layer].GetComponent<MeshFilter>();
        MeshRenderer renderer = gameObjects[(int)layer].GetComponent<MeshRenderer>();

        // Reset the mesh.
        filter.mesh.Clear();

        // Assign the tile material to the renderer.
        renderer.material = materials[(int)layer].GetMaterial();

        // Apply the tile specific color highlight.
        renderer.material.color = colors[(int)layer];

        // Create the list of vertices.
        Vector3[] verts = GetVertices(
            GetDisplayRect(layer, materials[(int)layer].percent),
            GetZOrder(layer));

        // Create the list of uvmap coordinates pull from the texture.
        Vector2[] uv = materials[(int)layer].uv();

        // For bumpy normal maps, create the list of tangents.
        // All tangents point to the right.
        Vector4[] tangents = new Vector4[4];
        tangents[0] = new Vector4(1f, 0f, 0f, -1f);
        tangents[1] = new Vector4(1f, 0f, 0f, -1f);
        tangents[2] = new Vector4(1f, 0f, 0f, -1f);
        tangents[3] = new Vector4(1f, 0f, 0f, -1f);

        // Create the list of triangles to display.
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[3] = triangles[2] = 1;
        triangles[4] = triangles[1] = 2;
        triangles[5] = 3;

        // Create a new custom mesh and apply verticies, uv, and triangles.
        filter.mesh = new Mesh();
        filter.mesh.name = "Mesh_" + layer.ToString();
        filter.mesh.vertices = verts;
        filter.mesh.triangles = triangles;
        filter.mesh.uv = uv;
        filter.mesh.tangents = tangents;
        filter.mesh.RecalculateNormals();
    }

    /// <summary>
    /// Update the mesh for any animation changes.
    /// </summary>
    /// <param name="layer">The layer to update.</param>
    private void UpdateMesh(Mesh2DLayer layer)
    {
        // The mesh may receive animation updates. Update the material uv map
        // accordingly.
        MeshFilter filter = gameObjects[(int)layer].GetComponent<MeshFilter>();
        if (materials[(int)layer] != null)
            filter.mesh.uv = materials[(int)layer].uv();
    }

    /// <summary>
    /// Get the z order of the layer requested. Layers that are higher are
    /// drawn closer to the camera.
    /// </summary>
    /// <param name="layer">
    /// The layer to get the specified z coordinate from.</param>
    /// <returns>
    /// Returns the z value to use for the meshes vertices data.
    /// </returns>
    private float GetZOrder(Mesh2DLayer layer)
    {
        switch (layer)
        {
            case Mesh2DLayer.LAYER_INVISIBLE:
                return -1f;
            case Mesh2DLayer.LAYER_BURROWED:
                return 0.001f;
            case Mesh2DLayer.LAYER_FLOOR:
                return 0f;
            case Mesh2DLayer.LAYER_GRID:
                return -0.001f;
            case Mesh2DLayer.LAYER_OBJECT:
                return -0.002f;
            case Mesh2DLayer.LAYER_UNITS:
                return -0.003f;
            case Mesh2DLayer.LAYER_ROOF:
                return -0.004f;
            case Mesh2DLayer.LAYER_FLYINGUNITS:
                return -0.005f;
            case Mesh2DLayer.LAYER_HEALTH_BASE:
                return -0.006f;
            case Mesh2DLayer.LAYER_HEALTH_DAMAGED:
                return -0.007f;
            case Mesh2DLayer.LAYER_HEALTH_REMAINING:
                return -0.008f;
            case Mesh2DLayer.LAYER_EMP:
                return -0.009f;
            case Mesh2DLayer.LAYER_BUFFS:
                return -0.010f;
            case Mesh2DLayer.LAYER_DAMAGE:
                return -0.011f;
            case Mesh2DLayer.LAYER_ATTACK_HIGHLIGHTS:
                return -0.012f;
            case Mesh2DLayer.LAYER_MOVEMENT_HIGHLIGHTS:
                return -0.013f;
            case Mesh2DLayer.LAYER_ARROW_HIGHLIGHTS:
                return -0.014f;
            case Mesh2DLayer.LAYER_ATTACK_MARKER:
                return -0.015f;
            case Mesh2DLayer.LAYER_BUTTON:
                return -0.016f;
            default:
                return 0f;
        }
    }
    
    /// <summary>
    /// Get the vertices rect of the tile.
    /// </summary>
    /// <param name="layer">Which layer to get the display rect for.</param>
    /// <param name="percent">
    /// From 0 to 1, left to right, how much
    /// of the image to show. Used strictly for the health bar.
    /// </param>
    /// <returns>Returns the display rect.</returns>
    private Rect GetDisplayRect(Mesh2DLayer layer, double percent = 1.0f)
    {
        // NOTE: We only use the percentage value for progress
        // bars at this moment.
        switch (layer)
        {
            case Mesh2DLayer.LAYER_HEALTH_BASE:
            case Mesh2DLayer.LAYER_HEALTH_DAMAGED:
            case Mesh2DLayer.LAYER_HEALTH_REMAINING:
                // The image is 8x8 || W: 160, H: 16
                // x + 0.03125, y + 0.03125 || H: 0.0625, W: 0.625
                return new Rect(0.03125f, 0.03125f, 0.625f * (float)percent, 0.0625f);

            case Mesh2DLayer.LAYER_ATTACK_MARKER:
                // The image is 64x224 || W: 128, H: 160
                // x + 0.25, y + 0.875 || W: 0.5, H: 0.625 
                return new Rect(0.25f, 0.875f, 0.5f, 0.625f);

            case Mesh2DLayer.LAYER_BUFFS:
                // The image is 0x196 || W: 60, H: 60
                // x + 0, y + 0.765625 || W: 0.234375, H: 0.234375
                return new Rect(0, 0.765625f, 0.234375f, 0.234375f);

            case Mesh2DLayer.LAYER_DAMAGE:
                // The image is 128x128 || W: 128, H: 128
                // x + 0.5, y + 0.5 || W: 0.5, H: 0.5
                return new Rect(0.5f, 0.5f, 0.5f, 0.5f);

            // By default we assume a tile scale is from 0 to 1.
            default:
                return new Rect(0, 0, 1, 1);
        }
    }

    /// <summary>
    /// Called every game frame. Update all tiles with animation.
    /// </summary>
    private void Update()
    {
        // Every frame, update each material with animations.
        for (int index = 0; index < (int)Mesh2DLayer.LAYER_COUNT; ++index)
        {
            if (materials[index] != null)
            {
                // Update the animation.
                materials[index].Update();

                // Update the mesh for any animation changes.
                UpdateMesh((Mesh2DLayer)index);
            }
        }
    }
}
