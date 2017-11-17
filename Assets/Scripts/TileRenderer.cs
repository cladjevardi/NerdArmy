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
    private float gridScale = 2.5f;

    /// <summary>The position of the tile.</summary>
    private Vector2 position;
    
    /// <summary>Tile materials at every layer.</summary>
    private MaterialId[] materials = new MaterialId[(int)TileLayer.LAYER_COUNT];

    /// <summary>Tile meshes at every layer.</summary>
    private GameObject[] gameObjects = new GameObject[(int)TileLayer.LAYER_COUNT];

    /// <summary>
    /// The color to apply over the material. This can be used for
    /// alpha highlights. Or highlighting an enemy/item to the payer.
    /// </summary>
    private Color[] colors = new Color[(int)TileLayer.LAYER_COUNT];

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

    /// <summary>
    /// Get the positional information of the rendered tile.
    /// </summary>
    /// <returns>Returns the current position of the tile.</returns>
    public Vector2 GetPosition()
    {
        return position;
    }

    /// <summary>
    /// Set the color of the material. This can be used to highlight a unit
    /// or tile a specific color, or to add an alpha channel.
    /// </summary>
    /// <param name="layer">The tile layer to apply the color highlight to.</param>
    /// <param name="color">The new color to assign the tile.</param>
    /// <returns>The previously assigned color.</returns>
    public Color SetColor(TileLayer layer, Color color)
    {
        // Store the color that was currently set.
        Color oldColor = colors[(int)layer];

        // When generate mesh is called this color will be applied over it.
        colors[(int)layer] = color;

        // Return the color we changed from.
        return oldColor;
    }

    /// <summary>Get the color of a specific tile layer.</summary>
    /// <param name="layer">The tile layer to get the color from.</param>
    /// <returns>The currently set color for a tile layer.</returns>
    public Color GetColor(TileLayer layer)
    {
        return colors[(int)layer];
    }

    /// <summary>Resets a layers color overlay.</summary>
    /// <param name="layer">The layer to reset.</param>
    public void ResetColor(TileLayer layer)
    {
        // Highlight layer automatically receives an alpha channel.
        colors[(int)layer] = new Color(1.0f, 1.0f, 1.0f,
            (layer == TileLayer.LAYER_HIGHLIGHTS) ? 0.4f : 1.0f);
    }

    /// <summary>
    /// Helper function for setting a tile specific material.
    /// </summary>
    /// <param name="layer">The layer of the material you want to set.</param>
    /// <param name="id">The id of the tile material.</param>
    /// <returns>The previously assigned material.</returns>
    public MaterialId SetTileMaterial(TileLayer layer, int id)
    {
        return SetMaterial(layer, new MaterialId(id, MaterialType.TILE));
    }

    /// <summary>
    /// Helper function for setting a unit specific material.
    /// </summary>
    /// <param name="layer">The layer of the material you want to set.</param>
    /// <param name="id">The id of the tile material.</param>
    /// <returns>The previously assigned material.</returns>
    public MaterialId SetUnitMaterial(TileLayer layer, int id)
    {
        return SetMaterial(layer, new MaterialId(id, MaterialType.UNIT));
    }

    /// <summary>
    /// Helper function for setting a effect specific material.
    /// </summary>
    /// <param name="layer">The layer of the material you want to set.</param>
    /// <param name="id">The id of the tile material.</param>
    /// <returns>The previously assigned material.</returns>
    public MaterialId SetEffectMaterial(TileLayer layer, int id)
    {
        return SetMaterial(layer, new MaterialId(id, MaterialType.EFFECT));
    }

    /// <summary>Sets the material of the specified layer.</summary>
    /// <param name="layer">The layer of the material you want to set.</param>
    /// <param name="material">The material you want to set the tile to.</param>
    /// <returns>The previously assigned material.</returns>
    public MaterialId SetMaterial(TileLayer layer, MaterialId materialId)
    {
        // Store the material that was currently set.
        MaterialId oldMaterial = materials[(int)layer];

        // Ensure there is a game object at this layer.
        if (gameObjects[(int)layer] == null)
            AllocateMesh(layer);

        // Get the renderer component and assign the current material to the mesh.
        MeshRenderer renderer = gameObjects[(int)layer].GetComponent<MeshRenderer>();
        renderer.material = materialId.GetMaterial();

        // Store the material for later.
        materials[(int)layer] = materialId;

        // Regenerate the mesh on that layer.
        if (renderer.material != null)
            GenerateMesh(layer);

        // Return the material we changed from.
        return oldMaterial;
    }

    /// <summary>Get the materialId of the mesh from the specified layer.</summary>
    /// <param name="layer">The layer to pull the material from.</param>
    /// <returns>Returns the material at that layer.</returns>
    public MaterialId GetMaterial(TileLayer layer)
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

        ResetColor(layer);
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

            ResetColor(layer);
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
        renderer.material = materials[(int)layer].GetMaterial();

        // Apply the tile specific color highlight.
        renderer.material.color = colors[(int)layer];

        // Get positional information.
        // Get the z order from the layer.
        float x = position.x;
        float y = position.y;
        float z = GetZOrder(layer);

        // Create the list of vertices.
        Vector3[] verts = new Vector3[4];
        verts[0] = new Vector3(
            x * gridScale, 
            y * gridScale,
            z * gridScale);
        verts[1] = new Vector3(
            x * gridScale + 1 * gridScale,
            y * gridScale,
            z * gridScale);
        verts[2] = new Vector3(
            x * gridScale,
            y * gridScale + 1 * gridScale,
            z * gridScale);
        verts[3] = new Vector3(
            x * gridScale + 1 * gridScale,
            y * gridScale + 1 * gridScale,
            z * gridScale);

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
