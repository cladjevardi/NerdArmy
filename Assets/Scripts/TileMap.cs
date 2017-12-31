using UnityEngine;
using System;
using System.Collections.Generic;

public class ThreatTile
{
    public Vector2 position = Vector2.zero;
    public double threat = 0f;
    public int attackers = 0;
}

public class TileMap : MonoBehaviour
{
    /// <summary>In-memory data of each tile.</summary>
    public List<List<Tile>> tiles = new List<List<Tile>>();

    /// <summary>The list of units in the mission.</summary>
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

    /// <summary>Whether we are already displaying highlights.</summary>
    private bool _displayingHighlights = false;
    public bool displayingHighlights
    {
        get { return _displayingHighlights; }
        internal set { _displayingHighlights = value; }
    }

    /// <summary>The pathing arrow being displayed.</summary>
    private List<AStarVector> currentArrowPathing = new List<AStarVector>();

    /// <summary>Populate the list of tiles of the tilemap.</summary>
    /// <param name="width">The width of the tilemap.</param>
    /// <param name="height">The height of the tilemap</param>
    /// <param name="tiles">The list of tilemeta data to convert to real tiles.</param>
    private void AddTiles(int width, int height, List<MissionTile> tiles)
    {
        // Set the base tilemap information.
        this.width = width;
        this.height = height;

        // Clear any previous map information.
        this.tiles.Clear();

        // Allocate the map
        this.tiles = new List<List<Tile>>();
        for (int i = 0; i < width; i++)
        {
            List<Tile> row = new List<Tile>();
            for (int j = 0; j < height; j++)
            {
                // Find the mission tile that represents this tile.
                MissionTile missionTile = null;
                foreach (MissionTile mTile in tiles)
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
                            tile.SetFloorMaterial(material.materialId, material.frameId, material.cellWidth, material.cellHeight);
                        if (material.layer == MissionMaterial.Layer.OBJECT)
                            tile.SetObjectMaterial(material.materialId, material.frameId, material.cellWidth, material.cellHeight);
                        if (material.layer == MissionMaterial.Layer.ROOF)
                            tile.SetRoofMaterial(material.materialId, material.frameId, material.cellWidth, material.cellHeight);

                        // Determine with tile for grid to use.
                        if (i == 0 && j == 0)
                            tile.SetGridMaterial(0, 0);
                        else if (i == 0 && j != height - 1)
                            tile.SetGridMaterial(0, 3);
                        else if (i == 0 && j == height - 1)
                            tile.SetGridMaterial(0, 6);
                        else if (i != width - 1 && j == 0)
                            tile.SetGridMaterial(0, 1);
                        else if (i != width - 1 && j != height - 1)
                            tile.SetGridMaterial(0, 4);
                        else if (i != width - 1 && j == height - 1)
                            tile.SetGridMaterial(0, 7);
                        else if (i == width - 1 && j == 0)
                            tile.SetGridMaterial(0, 2);
                        else if (i == width - 1 && j != height - 1)
                            tile.SetGridMaterial(0, 5);
                        else if (i == width - 1 && j == height - 1)
                            tile.SetGridMaterial(0, 8);
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
            this.tiles.Add(row);
        }
    }

    /// <summary>Add loadout roster of actors to the map.</summary>
    /// <param name="roster">The list of player controlled units.</param>
    /// <param name="validSpawnPositions">The list of valid rost spawn locations.</param>
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
            string objectName = "Actor_" + Owner.PLAYER1 + "_" + unit.name;
            Actor actor = new GameObject(objectName).AddComponent<Actor>();
            actor.transform.SetParent(transform);
            actor.transform.position = position;
            actor.unit = unit;
            actor.owner = Owner.PLAYER1;
            actor.health = unit.baseMaxHealth;
            actor.healthBarColor = ActorHealthColor.GREEN;
            actor.facing = ActorFacing.EAST;

            // Add the actor to the mission.
            actors.Add(actor);

            // Increment the spawn location to prevent collision.
            spawnIndex++;
        }
    }

    /// <summary>Add enemy actors to the map.</summary>
    /// <param name="missionEnemies">The list of enemies.</param>
    private void AddEnemies(List<MissionEnemy> missionEnemies)
    {
        // For each enemy unit within the mission schematic, add a new Actor.
        foreach (MissionEnemy enemy in missionEnemies)
        {
            // Create a new actor.
            Unit unit = new Unit(enemy.name);
            string objectName = "Actor_" + Owner.PLAYER2 + "_" + unit.name;
            Actor actor = new GameObject(objectName).AddComponent<Actor>();
            actor.transform.SetParent(transform);
            actor.transform.position = enemy.position;
            actor.unit = unit;
            actor.owner = Owner.PLAYER2;
            actor.health = unit.baseMaxHealth;
            actor.strategy = enemy.strategy;
            actor.healthBarColor = ActorHealthColor.RED;
            actor.facing = ActorFacing.WEST;

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

        Tile tile = GetTile(coord);
        Actor actor = GetActor(coord);

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
    /// Helper function to determine if a tile is specifically colored.
    /// </summary>
    /// <param name="x">The x coordinate to check.</param>
    /// <param name="y">The x coordinate to check.</param>
    /// <param name="color">The color we are looking for.</param>
    /// <returns>Returns whether the tile is highlighted that color.</returns>
    private bool IsTileColored(int x, int y, TileHighlightColor color)
    {
        if (!IsValidTile(x, y))
            return false;

        else if (color == TileHighlightColor.HIGHLIGHT_BLUE)
            return tiles[x][y].movementHighlight;
        else if (color == TileHighlightColor.HIGHLIGHT_RED)
            return tiles[x][y].attackHighlight;
        else if (color == TileHighlightColor.HIGHLIGHT_GREEN)
            return false; // Twiddle thumbs until dave needs this.

        return false;
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
        if (IsTileColored(x, N, color))
            mask[0] = 0x1;
        // North East
        if (IsTileColored(E, N, color))
            mask[1] = 0x1;
        // East
        if (IsTileColored(E, y, color))
            mask[2] = 0x1;
        // South East
        if (IsTileColored(E, S, color))
            mask[3] = 0x1;
        // South
        if (IsTileColored(x, S, color))
            mask[4] = 0x1;
        // South West
        if (IsTileColored(W, S, color))
            mask[5] = 0x1;
        // West
        if (IsTileColored(W, y, color))
            mask[6] = 0x1;
        // North West
        if (IsTileColored(W, N, color))
            mask[7] = 0x1;

        return mask;
    }

    /// <summary>
    /// Take a list of coordinates and highlight all those tiles the color specified.
    /// </summary>
    /// <param name="coordinates">The list of tile coordinates to highlight.</param>
    /// <param name="color">The color the tiles should be highlighted.</param>
    private void HighlightTiles(List<Vector2> coordinates, TileHighlightColor color)
    {
        foreach (Vector2 coordinate in coordinates)
        {
            Tile tile = GetTile(coordinate);
            if (color == TileHighlightColor.HIGHLIGHT_BLUE)
                tile.movementHighlight = true;
            else
                tile.attackHighlight = true;
        }
    }

    /// <summary>
    /// When the highlighted tiles are finalized. Update the image of the highlights to match what
    /// images they should be showing depending on their neighbors.
    /// </summary>
    private void AdjustHighlightFrameIds()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                Tile tile = tiles[x][y];
                if (tile.movementHighlight)
                    tile.SetMovementHighlightMaterial(2, GetTileHighlightMask(x, y, TileHighlightColor.HIGHLIGHT_BLUE));
                if (tile.attackHighlight)
                    tile.SetAttackHighlightMaterial(3, GetTileHighlightMask(x, y, TileHighlightColor.HIGHLIGHT_RED));
            }
        }
    }

    /// <summary>
    /// Display attack indicators over all other factions within attack range.
    /// </summary>
    /// <param name="attackTiles">The tiles that are available to attack.</param>
    /// <param name="currentFaction">The faction of the current player.</param>
    private void DisplayAttackIndicators(List<Vector2> attackTiles, Owner currentFaction)
    {
        // Iterate through all attack tiles and look for any actors.
        foreach (Vector2 attackTile in attackTiles)
        {
            Actor actor = GetActor(attackTile);

            // Detect if an enemy is within attack range. Place an indicator
            // over their head to indicate they can be attacked by the
            // current player.
            if (actor != null && actor.owner != currentFaction)
                actor.attackIndicator = true;
        }
    }

    /// <summary>
    /// Determine if an actor can attack a given position.
    /// </summary>
    /// <param name="actor">The actor to check.</param>
    /// <param name="position">The position to check against.</param>
    /// <returns>
    /// Returns whether the actor is within attack range of a given position.
    /// </returns>
    private bool CanActorAttackPosition(Actor actor, Vector2 position)
    {
        // Determine if the enemy is within attack range of a position.
        List<Vector2> movementTiles = GetMovementTiles(actor);
        List<Vector2> attackTiles = GetAttackTiles(actor, movementTiles);
        foreach (Vector2 attackTile in attackTiles)
        {
            if (position.x == attackTile.x && position.y == attackTile.y)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Whether the actor at this positon is an enemy unit.
    /// </summary>
    /// <param name="postion">The position to check.</param>
    /// <param name="owner">The owner to check against.</param>
    /// <returns>Returns whether the position has an enemy unit.</returns>
    public bool IsEnemyActor(Vector2 postion, Owner owner)
    {
        if (!IsValidTile((int)postion.x, (int)postion.y))
            return false;

        // If no actor is there, no collision
        Actor actor = GetActor(postion);
        if (actor == null)
            return false;

        // Check against the owner.
        return actor.owner != owner;
    }

    /// <summary>Get the actor at the given coordinate.</summary>
    /// <param name="position">The coordinate to check.</param>
    /// <returns>Returns the actor at the given coordinate if found or null if not.</returns>
    public Actor GetActor(Vector2 position)
    {
        // Are we within bounds of the map.
        if (position.x < 0 || position.y < 0 || position.x >= width || position.y >= height)
            return null;

        // Get the actor of the position, if any.
        foreach (Actor actor in actors)
        {
            if (actor.transform.position.x == position.x
                && actor.transform.position.y == position.y)
            {
                return actor;
            }
        }

        // No actor found at that location.
        return null;
    }

    /// <summary>
    /// Get the Tile of the position given.
    /// </summary>
    /// <param name="position">The coordinate of the tile.</param>
    /// <returns>
    /// Returns the Tile associated with the given position (rounded down).
    /// </returns>
    public Tile GetTile(Vector2 position)
    {
        // Round down to the nearest int.
        Vector2Int coordinate = new Vector2Int(
            (int)Math.Floor(position.x),
            (int)Math.Floor(position.y));
        // Validate the tile position.
        if (!IsValidTile(coordinate.x, coordinate.y))
            return null;
        return tiles[coordinate.x][coordinate.y];
    }

    /// <summary>
    /// Remove a specific actor from the list of tileMap actors.
    /// </summary>
    /// <param name="actor">The actor to remove.</param>
    public void RemoveActor(Actor actor)
    {
        // Find the actor within the list and remove it.
        for (int index = 0; index < actors.Count; ++index)
        {
            if (actors[index] == actor)
            {
                actors.RemoveAt(index);
                break;
            }
        }
    }

    /// <summary>Returns whether a given faction exists on the battlefield.</summary>
    /// <param name="faction">The faction to check.</param>
    /// <returns>Returns whether the faction is still active.</returns>
    public bool IsFactionActive(Owner faction)
    {
        foreach (Actor actor in actors)
        {
            if (actor.owner == faction)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Get the list of tile positions that should be highlighted for movement for an actor.
    /// </summary>
    /// <param name="actor">The actor that needs its movement highlights displayed.</param>
    /// <param name="actors">The list of actors on the field.</param>
    /// <returns>Returns a list of Tiles that need to be</returns>
    public List<Vector2> GetMovementTiles(Actor actor)
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

    /// <summary>
    /// Get the list of tiles that the actor can issue an attack on.
    /// </summary>
    /// <param name="validMovementTiles">The list of movement tiles to check.</param>
    /// <param name="actor">The current actor to get the list of attack tiles for.</param>
    /// <param name="actors">The full list of actors on the board.</param>
    /// <returns></returns>
    public List<Vector2> GetAttackTiles(Actor actor, List<Vector2> validMovementTiles = null)
    {
        // If no tiles were passed. Use current actors position as
        // the only tile to get attack information from.
        if (validMovementTiles == null)
        {
            validMovementTiles = new List<Vector2>();
            validMovementTiles.Add(new Vector2(actor.transform.position.x, actor.transform.position.y));
        }

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
    /// Highlight an actors attack and movement tiles. Display attack
    /// indicators over all other attackable actors.
    /// </summary>
    /// <param name="actor">The actor to focus highlights for.</param>
    public void HighlightActor(Actor actor)
    {
        // Determine if movement has been completed.
        List<Vector2> movementTiles = new List<Vector2>();
        if (!actor.done && actor.movementDone)
            movementTiles.Add(actor.transform.position);
        else
            movementTiles = GetMovementTiles(actor);

        // Display the actors movement and attack highlights.
        List<Vector2> attackTiles = GetAttackTiles(actor, movementTiles);

        // First display all attack tiles.
        HighlightTiles(attackTiles, TileHighlightColor.HIGHLIGHT_RED);

        // Display all movement tiles OVER the attack tiles. This takes into account enemies on tiles.
        if (!actor.movementDone)
            HighlightTiles(movementTiles, TileHighlightColor.HIGHLIGHT_BLUE);

        // Adjust the highlighted tiles images.
        AdjustHighlightFrameIds();

        // Add attack indicators over all attackable enemy actors.
        DisplayAttackIndicators(attackTiles, actor.owner);

        // Inform anyone interested if we are already displaying
        // highlights.
        displayingHighlights = true;
    }

    /// <summary>
    /// Remove all highlighted effects from the tilemap.
    /// </summary>
    public void RemoveAllHighlights()
    {
        // Remove any highlight states.
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                tiles[x][y].movementHighlight = false;
                tiles[x][y].attackHighlight = false;
            }
        }

        // Remove any attack indicator currently over any actor.
        foreach (Actor actor in actors)
        {
            actor.attackIndicator = false;
        }

        // Reset internal state for detection. 
        displayingHighlights = false;
    }

    /// <summary>
    /// Draw an arrow from point a to point b keeping collision in mind.
    /// </summary>
    /// <param name="fromPosition">Where the arrow begins.</param>
    /// <param name="toPosition">Where the arrow ends.</param>
    /// <param name="canFly">Whether pathing ignores ground collision.</param>
    public void ShowPath(Vector2 fromPosition, Vector2 toPosition,
        Owner owner = Owner.NONE, bool canFly = false)
    {
        // If we are pointing at ourself, ignore.
        if (fromPosition.x == toPosition.x && fromPosition.y == toPosition.y)
            return;

        // If we are not facing a valid movement tile or attack tile, ignore.
        Tile tile = GetTile(toPosition);
        if (tile == null)
            return;

        // Clear any previous path drawn currently.
        if (currentArrowPathing.Count >= 1)
            ClearCurrentPath();

        // If we don't have an actor in our from position, ignore. Shouldn't happen.
        Actor fromActor = GetActor(fromPosition);
        if (fromActor == null || (!tile.movementHighlight && !tile.attackHighlight))
            return;

        // Determine if a red arrow should be displayed.
        TileArrowHighlightColor color = TileArrowHighlightColor.ARROWHIGHLIGHT_BLUE;
        Actor toActor = GetActor(fromPosition);
        if ((toActor != null && fromActor.owner != toActor.owner)
            || tile.attackHighlight && !tile.movementHighlight)
        {
            color = TileArrowHighlightColor.ARROWHIGHLIGHT_RED;

            // We don't care about enemy collision so much for attack drags.
            // TODO: Discuss this.
            owner = Owner.NONE;
        }

        // Find the best path to the destination.
        Astar pathing = new Astar(this, fromPosition, toPosition, owner, canFly);

        // Keep track of pathing for future cleanup.
        currentArrowPathing = pathing.result;

        // Begin drawing arrow.
        bool start = true;
        foreach (AStarVector vector in pathing.result)
        {
            Tile aStarTile = GetTile(vector.position);
            string mask = BitConverter.ToString(vector.mask, 0);
            aStarTile.SetGridArrowMask(start, mask, color);
            start = false;
        }
    }

    /// <summary>Clear any arrow pathing being displayed currently.</summary>
    public void ClearCurrentPath()
    {
        foreach (AStarVector vector in currentArrowPathing)
        {
            Tile tile = GetTile(vector.position);
            tile.RemoveArrowHighlightMaterial();
        }
    }

    /// <summary>
    /// Reset the done state of all actors.
    /// </summary>
    public void ResetActorsDone()
    {
        foreach (Actor actor in actors)
        {
            actor.done = false;
            actor.movementDone = false;
        }
    }

    /// <summary>Convert movement tiles to a list of threat ordered tiles for AI calculations.</summary>
    /// <param name="movementTiles">The list of tiles available.</param>
    /// <returns>Returns an ordered list of tiles and their threat.</returns>
    public List<ThreatTile> CalculateThreat(List<Vector2> movementTiles, Owner currentFaction)
    {
        List<ThreatTile> threatTiles = new List<ThreatTile>();

        // Iterate through all the movement possibilities and assign data.
        foreach (Vector2 movementTile in movementTiles)
        {
            ThreatTile threatTile = new ThreatTile();
            threatTile.position = movementTile;
            foreach (Actor actor in actors)
            {
                if (actor.owner != currentFaction)
                {
                    // Use manhatten algorithm to determine threat
                    threatTile.threat += Math.Abs(movementTile.x - actor.transform.position.x)
                        + Math.Abs(movementTile.y - actor.transform.position.y);

                    // Determine if the enemy is within attack range of a position.
                    if (CanActorAttackPosition(actor, movementTile))
                        threatTile.attackers++;
                }
            }
            threatTiles.Add(threatTile);
        }

        // Order the list by lowest attackers then lowest threat.
        // Unfortunately I need to create a new list for this and remove
        // each entry from the old list.
        List<ThreatTile> threatList = new List<ThreatTile>();
        while (threatTiles.Count != 0)
        {
            int lowestIndex = 0;
            ThreatTile lowest = null;
            for (int index = 0; index < threatTiles.Count; ++index)
            {
                if (lowest == null)
                    lowest = threatTiles[index];
                if (threatTiles[index].attackers >= lowest.attackers)
                {
                    if (threatTiles[index].threat > lowest.threat)
                    {
                        lowest = threatTiles[index];
                        lowestIndex = index;
                    }
                }
            }

            threatList.Add(lowest);
            threatTiles.RemoveAt(lowestIndex);
        }

        return threatList;
    }

    /// <summary>Initialize the list of Tiles for a tilemap.</summary>
    /// <param name="missionSchematic">The schematic that makes up a tilemap.</param>
    public void Initialize(MissionSchematic missionSchematic, List<Unit> roster)
    {
        // Just incase, clear whatever tile and actors might be left.
        tiles.Clear();
        actors.Clear();

        // Generate the tilemap.
        AddTiles(missionSchematic.tileWidth, missionSchematic.tileHeight,
            missionSchematic.tiles);
        AddRoster(roster, missionSchematic.rosterSpawns);
        AddEnemies(missionSchematic.enemies);
    }
}
