using UnityEngine;

public class TileGrid : MonoBehaviour
{
    /// <summary>The visual scale of the grid.</summary>
    public float scale = 2.5f;

    /// <summary>Available tile materials that can be loaded.</summary>
    public Material[] materials;

    /// <summary>
    /// The layer of the tile mesh. This adjusts the z to draw the tile. Above or below
    /// other tiles.
    /// </summary>
    private enum Layer
    {
        /// <summary>The layer for all floor tiles.</summary>
        LAYER_GROUND = 0,

        /// <summary>The layer for all units.</summary>
        LAYER_UNITS,

        /// <summary>The layer for tile highlighted effects.</summary>
        LAYER_HIGHLIGHTS,
    }

    private float GetZFromLayer(Layer layer)
    {
        switch(layer)
        {
            case Layer.LAYER_GROUND:
                return 0f;
            case Layer.LAYER_UNITS:
                return -0.1f;
            case Layer.LAYER_HIGHLIGHTS:
                return -0.2f;
            default:
                return 0;
        }
    }

    private string GetTileName(int x, int y, Layer layer)
    {
        return "Tile_" + x.ToString() + "_" + y.ToString() + "_" + layer.ToString();
    }

    private void CreateTile(int x, int y, Layer layer, int materialId)
    {
        // Create the game object.
        GameObject gameObject = new GameObject(GetTileName(x, y, layer));
        gameObject.transform.parent = transform;

        // Add a mesh filter and mesh renderer.
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        
        // Assign the tile material to the renderer.
        renderer.material = materials[materialId];

        // Get the z order.
        float z = GetZFromLayer(layer);

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
        filter.mesh.name = "Procedural Tile";
        filter.mesh.vertices = verts;
        filter.mesh.triangles = triangles;
        filter.mesh.uv = uv;
        filter.mesh.RecalculateNormals();
    }

    // TODO: This will go away.
    private void Awake()
    {
        // Create a visual representation of a tile grid.
        CreateTile(0, 0, Layer.LAYER_GROUND, 0);
        CreateTile(0, 1, Layer.LAYER_GROUND, 0);
        CreateTile(1, 1, Layer.LAYER_GROUND, 0);
        CreateTile(1, 1, Layer.LAYER_UNITS, 2);
    }

    /*
    public LoadMission(Mission mission)
    {
        
    }
    */
}
