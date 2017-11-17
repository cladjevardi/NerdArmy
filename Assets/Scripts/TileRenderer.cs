using UnityEngine;

public class TileRenderer : MonoBehaviour
{
    /// <summary>
    /// The layer of the tile mesh. This adjusts the z to draw the tile.
    /// Above or below other tiles.
    /// </summary>
    public enum TileLayer
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

        /// <summary>The layer for tile highlighted effects.</summary>
        LAYER_HIGHLIGHTS,

        /// <summary>Total count of layers.</summary>
        LAYER_COUNT,
    }

    /// <summary>The visual scale of the grid.</summary>
    private float scale = 2.5f;

    /// <summary>The position of the tile.</summary>
    private Vector2 position;
    
    /// <summary>Tile materials at every layer.</summary>
    private Material[] materials = new Material[(int)TileLayer.LAYER_COUNT];

    /// <summary>Tile meshes at every layer.</summary>
    private GameObject[] gameObjects = new GameObject[(int)TileLayer.LAYER_COUNT];

    /// <summary>
    /// Reset the coordinate of the TileRenderer. This moves all layers to the
    /// new coordinate by destroying all meshes and regenerating them.
    /// </summary>
    /// <param name="position">The new position.</param>
    public void SetPosition(Vector2 position)
    {
        // Store the values.
        this.position = position;

        // Redraw all meshes in new coordinate.
        RegenerateMeshes();
    }

    /// <summary>Sets the material of the specified layer.</summary>
    /// <param name="layer">The layer of the material you want to set.</param>
    /// <param name="material">The material you want to set the tile to.</param>
    /// <returns>The previously assigned material.</returns>
    public Material SetMaterial(TileLayer layer, Material material)
    {
        // Store the material that was currently set.
        Material oldMaterial = materials[(int)layer];

        // Ensure there is a game object at this layer.
        if (gameObjects[(int)layer] == null)
            AllocateMesh(layer);

        // Get the renderer component and assign the current material to the mesh.
        MeshRenderer renderer = gameObjects[(int)layer].GetComponent<MeshRenderer>();
        renderer.material = material;

        // Store the material for later.
        materials[(int)layer] = material;

        // Regenerate the mesh on that layer.
        if (material != null)
            GenerateMesh(layer);

        // Return the material we changed from.
        return oldMaterial;
    }

    /// <summary>Get the material of the mesh from the specified layer.</summary>
    /// <param name="layer">The layer to pull the material from.</param>
    /// <returns>Returns the material at that layer.</returns>
    public Material GetMaterial(TileLayer layer)
    {
        return materials[(int)layer];
    }

    /// <summary>Removes the material from that layer, and removes the mesh.</summary>
    /// <param name="layer">The layer to remove the material from.</param>
    public void RemoveMaterial(TileLayer layer)
    {
        // Store the material for later.
        materials[(int)layer] = null;

        // Delete any drawn mesh.
        MeshFilter filter = gameObjects[(int)layer].GetComponent<MeshFilter>();
        filter.mesh.Clear();
    }

    /// <summary>
    /// Regenerates all meshes from every layer. Does not draw meshes for
    /// layers that have no material assigned.
    /// </summary>
    private void RegenerateMeshes()
    {
        // Regenerate all tiles with the appropriate coordinate.
        for (int index = 0; index < (int)TileLayer.LAYER_COUNT; ++index)
        {
            if (materials[index] != null)
            {
                GenerateMesh((TileLayer)index);
            }
        }
    }

    /// <summary>
    /// Destroy a mesh at specified layer.
    /// </summary>
    /// <param name="layer">The layer of the mesh to destroy.</param>
    private void DestroyMesh(TileLayer layer)
    {
        // Destroy the mesh at that layer.
        if (gameObjects[(int)layer] != null)
        {
            DestroyObject(gameObjects[(int)layer]);
            gameObjects[(int)layer] = null;
        }
    }

    /// <summary>
    /// Allocate mesh at specified layer.
    /// </summary>
    /// <param name="layer">The layer of the mesh to create.</param>
    private void AllocateMesh(TileLayer layer)
    {
        // Create a custom mesh at the specified layer.
        if (gameObjects[(int)layer] == null)
        {
            gameObjects[(int)layer] = new GameObject("GameObject_" + layer.ToString());
            gameObjects[(int)layer].transform.parent = transform;
            gameObjects[(int)layer].AddComponent<MeshFilter>();
            gameObjects[(int)layer].AddComponent<MeshRenderer>();
        }
    }

    /// <summary>
    /// Generates the mesh for the current cooridnate and material. This
    /// function is safe to call again
    /// </summary>
    /// <param name="layer">The layer to generate the mesh on.</param>
    private void GenerateMesh(TileLayer layer)
    {
        // Add a mesh filter and mesh renderer.
        MeshFilter filter = gameObjects[(int)layer].GetComponent<MeshFilter>();
        MeshRenderer renderer = gameObjects[(int)layer].GetComponent<MeshRenderer>();

        // Reset the mesh.
        filter.mesh.Clear();

        // Assign the tile material to the renderer.
        renderer.material = materials[(int)layer];

        // Adjust the alpha for the highlight layer.
        if (layer == TileLayer.LAYER_HIGHLIGHTS)
            renderer.material.color = new Color(1f, 1f, 1f, 0.4f);

        // Get positional information.
        // Get the z order from the layer.
        float x = position.x;
        float y = position.y;
        float z = GetZOrder(layer);

        // Create the list of vertices.
        Vector3[] verts = new Vector3[4];
        verts[0] = new Vector3(x * scale            , y * scale            , z * scale);
        verts[1] = new Vector3(x * scale + 1 * scale, y * scale            , z * scale);
        verts[2] = new Vector3(x * scale            , y * scale + 1 * scale, z * scale);
        verts[3] = new Vector3(x * scale + 1 * scale, y * scale + 1 * scale, z * scale);

        // Create the list of uvmap coordinates pull from the texture. Currently set
        // to use the entire material image.
        // TODO: We should be using a tile spritemap and searching for each texture.
        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(x    , y    );
        uv[1] = new Vector2(x + 1, y    );
        uv[2] = new Vector2(x    , y + 1);
        uv[3] = new Vector2(x + 1, y + 1);

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
        filter.mesh.RecalculateNormals();
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
    private float GetZOrder(TileLayer layer)
    {
        switch (layer)
        {
            case TileLayer.LAYER_INVISIBLE:
                return -1f;
            case TileLayer.LAYER_BURROWED:
                return 0.01f;
            case TileLayer.LAYER_FLOOR:
                return 0f;
            case TileLayer.LAYER_OBJECT:
                return -0.01f;
            case TileLayer.LAYER_UNITS:
                return -0.02f;
            case TileLayer.LAYER_ROOF:
                return -0.03f;
            case TileLayer.LAYER_FLYINGUNITS:
                return -0.04f;
            case TileLayer.LAYER_HIGHLIGHTS:
                return -0.05f;
            default:
                return 0f;
        }
    }
}
