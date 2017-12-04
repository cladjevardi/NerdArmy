using UnityEngine;
using System;
using System.Collections.Generic;

public class TileMap : MonoBehaviour
{
    /// <summary>Internal data structure for highlights.</summary>
    private class Visited
    {
        public Vector2 position;
        public TileHighlightColor color;

        /// <summary>Constructor for Visited.</summary>
        /// <param name="position">Tile position information.</param>
        /// <param name="color">Tile highlight color.</param>
        public Visited(Vector2 position, TileHighlightColor color)
        {
            this.position = position;
            this.color = color;
        }
    };

    /// <summary>In-memory data of each tile.</summary>
    [HideInInspector]
    public List<List<Tile>> tiles = new List<List<Tile>>();

    /// <summary>The list of units in the mission.</summary>
    [HideInInspector]
    public List<Actor> actors = new List<Actor>();

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

    private void InitializeMap(List<Unit> roster, MissionSchematic missionSchematic)
    {
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
                tile.transform.parent = transform;
                tile.position = new Vector2(i, j);

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

        // Add the list of player controlled actors to the map.
        AddRoster(roster, missionSchematic.rosterSpawns);

        // Add the list of enemy controlled actors to the map.
        AddEnemies(missionSchematic.enemies);
    }

    /// <summary>
    /// Add loadout roster of actors to the map.
    /// </summary>
    /// <param name="roster">The list of player controlled units.</param>
    /// <param name="validSpawnPositions">
    /// The list of valid rost spawn locations.
    /// </param>
    private void AddRoster(List<Unit> roster, List<Vector2> validSpawnPositions)
    {
        int spawnIndex = 0;

        // Iterate through each member of the roster and add units.
        foreach (Unit unit in roster)
        {
            // We can only spawn as many actors as available spawns.
            if (spawnIndex >= validSpawnPositions.Count)
                break;

            // Create the new actor.
            Vector2 position = validSpawnPositions[spawnIndex];
            string objectName = "Actor_" + Owner.PLAYER1 + "_" + unit.type.ToString();
            Actor actor = new GameObject(objectName).AddComponent<Actor>();
            actor.transform.parent = transform;
            actor.position = position;
            actor.unit = unit;
            actor.owner = Owner.PLAYER1;
            actor.health = unit.baseMaxHealth;

            // Add the actor to the mission.
            actors.Add(actor);

            // Increment the spawn location to prevent collision.
            spawnIndex++;
        }
    }

    /// <summary>
    /// Add enemy actors to the map.
    /// </summary>
    /// <param name="missionEnemies">The list of enemies.</param>
    private void AddEnemies(List<MissionEnemy> missionEnemies)
    {
        // For each enemy unit within the mission schematic, add a new Actor.
        foreach (MissionEnemy enemy in missionEnemies)
        {
            // Create a new actor.
            Unit unit = UnitFactory.Create(enemy.type);
            string objectName = "Actor_" + Owner.PLAYER2 + "_" + unit.type.ToString();
            Actor actor = new GameObject(objectName).AddComponent<Actor>();
            actor.transform.parent = transform;
            actor.position = enemy.position;
            actor.unit = unit;
            actor.owner = Owner.PLAYER2;
            actor.health = unit.baseMaxHealth;

            // Add the actor to the mission.
            actors.Add(actor);
        }
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
    private bool IsVector2InVector2List(Vector2 entry, List<Visited> list)
    {
        // Look for the given coordinate in the list of coordinates.
        bool contains = false;
        foreach (Visited item in list)
        {
            if (entry.x == item.position.x && entry.y == item.position.y)
                contains = true;
        }

        return contains;
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
    private bool ShouldAdd(Vector2 coord, List<Visited> visited, bool canFly)
    {
        // Are we within bounds of the map.
        if (coord.x < 0 || coord.y < 0 || coord.x >= width || coord.y >= height)
            return false;

        Tile tile = tiles[(int)coord.x][(int)coord.y];

        // If the coord is valid, not already visited previously, or has collision
        // add it to the list of visited
        return (tile != null
            && !IsVector2InVector2List(coord, visited)
            && (canFly ? !tile.trueCollision : !tile.groundCollision));
    }

    /// <summary>
    /// Show movement and attack highlights.
    /// </summary>
    /// <param name="actor">The actor to display.</param>
    public void ShowActorHighlights(Actor actor)
    {
        int movement = actor.unit.baseMovement;
        int minRange = actor.unit.baseMinRange;
        int maxRange = actor.unit.baseMaxRange;

        List<Visited> visited = new List<Visited>();
        List<Vector2> toCheck = new List<Vector2>();

        // Start off with what we know for sure we should check
        toCheck.Add(actor.position);

        bool shouldIgnoreGround = actor.flying;
        while (movement + minRange + maxRange > 0)
        {
            // The next group of tiles to check
            List<Vector2> newToCheck = new List<Vector2>();

            // Iterate through our toCheck
            foreach (Vector2 coord in toCheck)
            {
                // Add ourself to visited
                if (!IsVector2InVector2List(coord, visited))
                {
                    if (movement > 0)
                        visited.Add(new Visited(new Vector2(coord.x, coord.y), TileHighlightColor.HIGHLIGHT_BLUE));
                    else if (minRange > 0)
                    {
                        shouldIgnoreGround = true;
                        visited.Add(new Visited(new Vector2(coord.x, coord.y), TileHighlightColor.HIGHLIGHT_NONE));
                    }
                    else
                    {
                        shouldIgnoreGround = true;
                        visited.Add(new Visited(new Vector2(coord.x, coord.y), TileHighlightColor.HIGHLIGHT_RED));
                    }
                }

                Vector2 north = new Vector2(coord.x, coord.y - 1);
                Vector2 east = new Vector2(coord.x + 1, coord.y);
                Vector2 south = new Vector2(coord.x, coord.y + 1);
                Vector2 west = new Vector2(coord.x - 1, coord.y);

                // Check if we should add any given direction to the next
                // potential list of tiles.
                if (ShouldAdd(north, visited, shouldIgnoreGround))
                    newToCheck.Add(north);
                if (ShouldAdd(east, visited, shouldIgnoreGround))
                    newToCheck.Add(east);
                if (ShouldAdd(south, visited, shouldIgnoreGround))
                    newToCheck.Add(south);
                if (ShouldAdd(west, visited, shouldIgnoreGround))
                    newToCheck.Add(west);
            }

            // Replace list with new list
            toCheck.Clear();
            toCheck = newToCheck;

            // Subtract movement spaces in order of highlighted.
            if (movement > 0)
                movement--;
            else if (minRange > 0)
                minRange--;
            else if (maxRange > 0)
                maxRange--;
        }

        // Apply the highlight to the valid tiles to move to
        foreach (Visited visit in visited)
        {
            Tile tile = tiles[(int)visit.position.x][(int)visit.position.y];
            tile.highlight = true;
            tile.highlightColor = visit.color;
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

    /// <summary>
    /// Construct a mission from the mission database.
    /// </summary>
    /// <param name="roster">The list of player controlled units.</param>
    /// <param name="missionSchematic">The blueprints for building a mission.</param>
    public void GenerateMission(List<Unit> roster, MissionSchematic missionSchematic)
    {
        // Clear any previous map information.
        tiles.Clear();
        actors.Clear();

        // Allocate the tile map.
        InitializeMap(roster, missionSchematic);
    }
}
