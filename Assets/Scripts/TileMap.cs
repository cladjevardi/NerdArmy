using UnityEngine;
using System;
using System.Collections.Generic;

public class TileMap : MonoBehaviour
{
    /// <summary>In-memory data of each tile.</summary>
    [HideInInspector]
    public List<List<Tile>> tiles = new List<List<Tile>>();

    /// <summary>The height of the tilemap.</summary>
    private int _height;
    public int height
    {
        get { return _height; }
        internal set { _height = value; }
    }

    /// <summary>The width of the tilemap.</summary>
    private int _width;
    public int width
    {
        get { return _width; }
        internal set { _width = value; }
    }

    /// <summary>
    /// Find a Vector2 in a list of Vector2s.
    /// TODO: Find a better way to do this!
    /// </summary>
    /// <param name="entry">The coordinate to find.</param>
    /// <param name="list">The list of coordinates to check.</param>
    /// <returns>
    /// Returns whether or not the cooridnate exists in the list.
    /// </returns>
    private bool IsVector2InVector2List(Vector2 entry, List<Vector2> list)
    {
        // Look for the given coordinate in the list of coordinates.
        bool contains = false;
        foreach (Vector2 item in list)
        {
            if (entry.x == item.x && entry.y == item.y)
                contains = true;
        }

        return contains;
    }

    /// <summary>
    /// Get the actor that matches the coordinate specified.
    /// </summary>
    /// <param name="coord">The coordinate to look for an actor.</param>
    /// <param name="actors">The list of actors to check against.</param>
    /// <returns></returns>
    private Actor GetSelectedActor(Vector2 coord, List<Actor> actors)
    {
        // Are we within bounds of the map.
        if (coord.x < 0 || coord.y < 0 || coord.x >= width || coord.y >= height)
            return null;

        // Get the actor of the position, if any.
        foreach (Actor actor in actors)
        {
            if (actor.transform.position.x == coord.x
                && actor.transform.position.y == coord.y)
            {
                // This may be an enemies actor or a players actor.
                // Display their current highlights.
                return actor;
            }
        }

        // No actor found at that location.
        return null;
    }

    /// <summary>
    /// Check if the cooridnate specified should be added to the list of
    /// potential visitors.
    /// </summary>
    /// <param name="coord">The coordinate to check.</param>
    /// <param name="visited">The list of visited tiles already.</param>
    /// <param name="canFly">Whether we check for air or ground collision.</param>
    /// <returns>
    /// Returns whether or not the coordinate should be checked next pass.
    /// </returns>
    private bool ShouldAdd(Vector2 coord, List<Vector2> visited, Owner owner, List<Actor> actors, bool canFly)
    {
        // Are we within bounds of the map.
        if (coord.x < 0 || coord.y < 0 || coord.x >= width || coord.y >= height)
            return false;

        Tile tile = tiles[(int)coord.x][(int)coord.y];
        Actor actor = GetSelectedActor(coord, actors);

        bool actorCheck = owner == Owner.NONE ?
            (actor == null || (actor != null && actor.owner != owner)) :
            (actor == null || (actor != null && actor.owner == owner));

        // If the coord is valid, not already visited previously, or has collision
        // add it to the list of visited
        return (tile != null
            && !IsVector2InVector2List(coord, visited)
            && actorCheck
            && (canFly ? !tile.trueCollision : !tile.groundCollision));
    }

    /// <summary>
    /// Determines if a coordinate is within the tilemap.
    /// </summary>
    /// <param name="x">The x position of the coordinate.</param>
    /// <param name="y">The y position of the coordinate.</param>
    /// <returns>Returns whether the coordinate is valid.</returns>
    private bool IsValidTile(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    /// <summary>
    /// Get the string mask from a tiles highlighted neighbors.
    /// </summary>
    /// <param name="x">The x coordinate of the tile to check.</param>
    /// <param name="y">The y coordinate of the tile to check.</param>
    /// <param name="color">The highlight color to check against.</param>
    /// <returns>Returns the string mask of the highlighted neighbor tiles.</returns>
    private byte[] GetTileHighlightMask(int x, int y, TileHighlightColor color)
    {
        int N = y + 1;
        int S = y - 1;
        int E = x + 1;
        int W = x - 1;

        // Create a byte array and clear it out.
        byte[] mask = new byte[8];
        mask[0] = 0x0; mask[1] = 0x0; mask[2] = 0x0; mask[3] = 0x0;
        mask[4] = 0x0; mask[5] = 0x0; mask[6] = 0x0; mask[7] = 0x0;

        // North
        if (IsValidTile(x, N) && tiles[x][N].highlight == true && tiles[x][N].highlightColor == color)
            mask[0] = 0x1;
        // North East
        if (IsValidTile(E, N) && tiles[E][N].highlight == true && tiles[E][N].highlightColor == color)
            mask[1] = 0x1;
        // East
        if (IsValidTile(E, y) && tiles[E][y].highlight == true && tiles[E][y].highlightColor == color)
            mask[2] = 0x1;
        // South East
        if (IsValidTile(E, S) && tiles[E][S].highlight == true && tiles[E][S].highlightColor == color)
            mask[3] = 0x1;
        // South
        if (IsValidTile(x, S) && tiles[x][S].highlight == true && tiles[x][S].highlightColor == color)
            mask[4] = 0x1;
        // South West
        if (IsValidTile(W, S) && tiles[W][S].highlight == true && tiles[W][S].highlightColor == color)
            mask[5] = 0x1;
        // West
        if (IsValidTile(W, y) && tiles[W][y].highlight == true && tiles[W][y].highlightColor == color)
            mask[6] = 0x1;
        // North West
        if (IsValidTile(W, N) && tiles[W][N].highlight == true && tiles[W][N].highlightColor == color)
            mask[7] = 0x1;

        return mask;
    }

    /// <summary>Get the Tile selected when clicking on the tilemap.</summary>
    /// <returns>Returns the Tile selected.</returns>
    public Tile GetTileSelected()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log(hit.collider.name);
                return hit.collider.gameObject.GetComponent<Tile>();
            }
        }

        return null;
    }

    /// <summary>
    /// Get the list of tile positions that should be highlighted for movement for an actor.
    /// </summary>
    /// <param name="actor">The actor that needs its movement highlights displayed.</param>
    /// <param name="actors">The list of actors on the field.</param>
    /// <returns>Returns a list of Tiles that need to be</returns>
    public List<Vector2> GetMovementTiles(Actor actor, List<Actor> actors)
    {
        int movement = actor.unit.baseMovement;

        List<Vector2> visited = new List<Vector2>();
        List<Vector2> toCheck = new List<Vector2>();

        // Start off with what we know for sure we should check.
        toCheck.Add(actor.transform.position);

        bool shouldIgnoreGround = actor.flying;
        while (movement >= 0)
        {
            // The next group of tiles to check.
            List<Vector2> newToCheck = new List<Vector2>();

            // Iterate through our toCheck.
            foreach (Vector2 coord in toCheck)
            {
                // Add ourself to visited.
                if (!IsVector2InVector2List(coord, visited))
                    visited.Add(new Vector2(coord.x, coord.y));

                Vector2 north = new Vector2(coord.x, coord.y - 1);
                Vector2 east = new Vector2(coord.x + 1, coord.y);
                Vector2 south = new Vector2(coord.x, coord.y + 1);
                Vector2 west = new Vector2(coord.x - 1, coord.y);

                // Check if we should add any given direction to the next
                // potential list of tiles.
                if (ShouldAdd(north, visited, actor.owner, actors, shouldIgnoreGround))
                    newToCheck.Add(north);
                if (ShouldAdd(east, visited, actor.owner, actors, shouldIgnoreGround))
                    newToCheck.Add(east);
                if (ShouldAdd(south, visited, actor.owner, actors, shouldIgnoreGround))
                    newToCheck.Add(south);
                if (ShouldAdd(west, visited, actor.owner, actors, shouldIgnoreGround))
                    newToCheck.Add(west);
            }

            toCheck = newToCheck;
            movement--;
        }

        foreach (Actor unit in actors)
        {
            // Remove all actor locations from the visitor list.
            for (int index = 0; index != visited.Count; ++index)
            {
                if (unit.transform.position.x == visited[index].x
                    && unit.transform.position.y == visited[index].y)
                {
                    visited.RemoveAt(index);
                    break;
                }
            }
        }

        // Always add myself to the end list.
        visited.Add(new Vector2(actor.transform.position.x, actor.transform.position.y));

        return visited;
    }

    public List<Vector2> GetAttackTiles(List<Vector2> validMovementTiles, Actor actor, List<Actor> actors)
    {
        // The full list of attack tiles to highlight.
        List<Vector2> attackTiles = new List<Vector2>();

        // Iterate through each valid movement tile and add all the unique entries to the attackTiles list.
        foreach (Vector2 validMovement in validMovementTiles)
        {
            int minRange = actor.unit.baseMinRange;
            int maxRange = actor.unit.baseMaxRange - actor.unit.baseMinRange;

            List<Vector2> visited = new List<Vector2>();
            List<Vector2> toCheck = new List<Vector2>();

            // Start off with what we know for sure we should check.
            toCheck.Add(validMovement);

            bool shouldIgnoreGround = actor.flying;
            while (minRange + maxRange >= 0)
            {
                // The next group of tiles to check.
                List<Vector2> newToCheck = new List<Vector2>();

                // Iterate through our toCheck.
                foreach (Vector2 coord in toCheck)
                {
                    // Add ourself to visited.
                    if (!IsVector2InVector2List(coord, visited) && minRange == 0)
                        visited.Add(new Vector2(coord.x, coord.y));

                    Vector2 north = new Vector2(coord.x, coord.y - 1);
                    Vector2 east = new Vector2(coord.x + 1, coord.y);
                    Vector2 south = new Vector2(coord.x, coord.y + 1);
                    Vector2 west = new Vector2(coord.x - 1, coord.y);

                    // Check if we should add any given direction to the next
                    // potential list of tiles.
                    if (ShouldAdd(north, visited, Owner.NONE, actors, shouldIgnoreGround))
                        newToCheck.Add(north);
                    if (ShouldAdd(east, visited, Owner.NONE, actors, shouldIgnoreGround))
                        newToCheck.Add(east);
                    if (ShouldAdd(south, visited, Owner.NONE, actors, shouldIgnoreGround))
                        newToCheck.Add(south);
                    if (ShouldAdd(west, visited, Owner.NONE, actors, shouldIgnoreGround))
                        newToCheck.Add(west);
                }

                toCheck = newToCheck;

                // Adjust minimum range first before subtracting against maxRange.
                if (minRange > 0)
                    minRange--;
                else
                    maxRange--;
            }

            // Add all the list of current visited to our giant attack list.
            // Exclude duplicates.
            foreach (Vector2 tile in visited)
            {
                // If we are not in the list, nor are we our current actor position. Add.
                if (!IsVector2InVector2List(tile, attackTiles)
                    && !(tile.x == actor.transform.position.x && tile.y == actor.transform.position.y))
                    attackTiles.Add(tile);
            }
        }

        return attackTiles;
    }

    /// <summary>
    /// Take a list of coordinates and highlight all those tiles the color specified.
    /// </summary>
    /// <param name="coordinates">The list of tile coordinates to highlight.</param>
    /// <param name="color">The color the tiles should be highlighted.</param>
    public void HighlightTiles(List<Vector2> coordinates, TileHighlightColor color)
    {
        foreach (Vector2 coordinate in coordinates)
        {
            Tile tile = tiles[(int)coordinate.x][(int)coordinate.y];
            tile.highlight = true;
            tile.highlightColor = color;
        }
    }

    /// <summary>
    /// When the highlighted tiles are finalized. Update the image of the highlights to match what
    /// images they should be showing depending on their neighbors.
    /// </summary>
    public void AdjustHighlightFrameIds()
    {
        // Iterate through each color and fix the frame id of the highlight image.
        for (int highlightColor = 0; highlightColor < (int)TileHighlightColor.HIGHLIGHT_COUNT; ++highlightColor)
        {
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    Tile tile = tiles[(int)x][(int)y];
                    if (tile.highlight && tile.highlightColor == (TileHighlightColor)highlightColor)
                    {
                        tile.SetHighlightMask(GetTileHighlightMask(x, y, (TileHighlightColor)highlightColor));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Remove all highlighted effects from the tilemap.
    /// </summary>
    public void RemoveAllHighlights()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                tiles[x][y].highlight = false;
                tiles[x][y].highlightColor = TileHighlightColor.HIGHLIGHT_NONE;
            }
        }
    }

    /// <summary>
    /// Draw an arrow from point a to point b keeping collision in mind.
    /// </summary>
    /// <param name="fromPosition">Where the arrow begins.</param>
    /// <param name="toPosition">Where the arrow ends.</param>
    /// <param name="canFly">Whether pathing ignores ground collision.</param>
    public void ShowPath(Vector2 fromPosition, Vector2 toPosition,
        bool canFly = false)
    {
        Astar pathing = new Astar(this, fromPosition, toPosition, canFly);

        bool start = true;
        foreach (AStarVector vector in pathing.result)
        {
            Tile tile = tiles[(int)vector.position.x][(int)vector.position.y];
            string mask = BitConverter.ToString(vector.mask, 0);
            Debug.Log("ShowPath mask: " + mask);

            tile.SetGridArrowMask(start, mask);
            start = false;
        }
    }

    /// <summary>Initialize the list of Tiles for a tilemap.</summary>
    /// <param name="missionSchematic">The schematic that makes up a tilemap.</param>
    public void Initialize(MissionSchematic missionSchematic)
    {
        // Just incase, clear whatever tiles might be left.
        tiles.Clear();

        // Set the base tilemap information.
        width = missionSchematic.tileWidth;
        height = missionSchematic.tileHeight;

        // Clear any previous map information.
        tiles.Clear();

        // Allocate the map
        tiles = new List<List<Tile>>();
        for (int i = 0; i < missionSchematic.tileWidth; i++)
        {
            List<Tile> row = new List<Tile>();
            for (int j = 0; j < missionSchematic.tileHeight; j++)
            {
                // Find the mission tile that represents this tile.
                MissionTile missionTile = null;
                foreach (MissionTile mTile in missionSchematic.tiles)
                {
                    if (mTile.position.x == i && mTile.position.y == j)
                    {
                        missionTile = mTile;
                        break;
                    }
                }

                // Basic tile information.
                Tile tile = new GameObject("Tile_" + i + "_" + j).AddComponent<Tile>();
                tile.transform.SetParent(transform);
                tile.transform.position = new Vector2(i, j);

                // Tile found in mission tile list. Construct it.
                if (missionTile != null)
                {
                    tile.movementCost = missionTile.movementCost;
                    tile.trueCollision = missionTile.trueCollision;
                    tile.groundCollision = missionTile.groundCollision;

                    foreach (MissionMaterial material in missionTile.materials)
                    {
                        if (material.layer == MissionMaterial.Layer.FLOOR)
                            tile.SetFloorMaterial(material.materialId, material.frameId);
                        if (material.layer == MissionMaterial.Layer.OBJECT)
                            tile.SetObjectMaterial(material.materialId, material.frameId);
                        if (material.layer == MissionMaterial.Layer.ROOF)
                            tile.SetRoofMaterial(material.materialId, material.frameId);
                    }
                }
                else
                {
                    // Basic floor tile without mission information.
                    tile.groundCollision = true;
                    tile.trueCollision = true;
                    tile.SetFloorMaterial(0);
                }

                // Add the tile to the map.
                row.Add(tile);
            }

            // Add the row to the entire map.
            tiles.Add(row);
        }
    }
}
