using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>The direction for AStar result information.</summary>
public enum AStarDirection
{
    NONE,
    NORTH,
    EAST,
    SOUTH,
    WEST,
}

/// <summary>
/// An a* pathing positional information found in a result.
/// </summary>
public class AStarVector
{
    /// <summary>The position of the pathing tile.</summary>
    private Vector2 _position;
    public Vector2 position
    {
        get { return _position; }
        set { _position = value; }
    }

    /// <summary>
    /// The directional information of the path tile. Ordered as North,
    /// East, South, West. 0 meaning pathing not pointing in this direction.
    /// There should only be 2 values set.
    /// </summary>
    private byte[] _mask = new byte[4];
    public byte[] mask
    {
        get { return _mask; }
        set { _mask = value; }
    }

    /// <summary>
    /// The direction the pathing tile is pointing. A direction of NONE
    /// means this is the destination tile.
    /// </summary>
    private AStarDirection _direction;
    public AStarDirection direction
    {
        get { return _direction; }
        set { _direction = value; }
    }
}

public class Astar
{
    /// <summary>The resulting path with more information.</summary>
    public List<AStarVector> result = new List<AStarVector>();

    /// <summary>The resulting path.</summary>
    public List<Vector2> AsListVector2()
    {
        List<Vector2> list = new List<Vector2>();
        foreach (AStarVector vec in result)
            list.Add(vec.position);
        return list;
    }

    /// <summary>
    /// Get the last position in the aStarPath
    /// </summary>
    /// <returns>Returns the last position in the list.</returns>
    public Vector2 GetLastPosition()
    {
        List<Vector2> list = AsListVector2();
        return list[list.Count - 1];
    }

    /// <summary>Tile metadata for the a* algorithm.</summary>
    private class AStarTile
    {
        /// <summary>The position of the tile.</summary>
        private Vector2 _position;
        public Vector2 position
        {
            get { return _position; }
            set { _position = value; }
        }

        /// <summary>
        /// The f value of a given node. The lower the f value, the most likely this node
        /// is the best pick for traversing next to reach a given destination.
        /// </summary>
        private double _fScore = float.MaxValue;
        public double fScore
        {
            get { return _fScore; }
            set { _fScore = value; }
        }

        /// <summary>
        /// Represents a deterministic positional score. Best used to determining an fscore.
        /// </summary>
        private double _gScore = float.MaxValue;
        public double gScore
        {
            get { return _gScore; }
            set { _gScore = value; }
        }

        /// <summary>The heuristic algorithm to determine the value of a given node.</summary>
        private double _heuristic = 0.0f;
        public double heuristic
        {
            get { return _heuristic; }
            set { _heuristic = value; }
        }

        /// <summary>The index value to lookup when referencing tile in an array.</summary>
        private int _index = 0;
        public int index
        {
            get { return _index; }
            set { _index = value; }
        }

        /// <summary>
        /// The parent of the current astar pathing tile. This keeps track of a result path.
        /// </summary>
        private AStarTile _parent = null;
        public AStarTile parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <summary>Constructor for an AStartTile.</summary>
        /// <param name="position">The index position of the tile.</param>
        public AStarTile(Vector2 position)
        {
            this.position = position;
        }

        /// <summary>Alternative constructor for an AStartTile.</summary>
        /// <param name="position">The position of the tile.</param>
        /// <param name="index">The unique identifier for the tile.</param>
        /// <param name="fScore">The score of the tile based on heuristic value.</param>
        /// <param name="gScore">The positional value of the tile.</param>
        public AStarTile(Vector2 position, int index,
            double fScore = float.MaxValue, double gScore = float.MaxValue)
        {
            this.position = position;
            this.index = index;
            this.fScore = fScore;
            this.gScore = gScore;
        }
    }

    /// <summary>
    /// Get a list of open neighbor tiles from a given position. By open the
    /// tile must have ground collision for ground units or air collision for
    /// air units.
    /// </summary>
    /// <param name="position">
    /// The original position to check for neighbors.
    /// </param>
    /// <param name="tileMap">
    /// The tilemap data with collision data to check against.
    /// </param>
    /// <param name="canFly">
    /// Whether the unit is chcking against the ground of air collision
    /// tilemap information.</param>
    /// <returns>
    /// Returns list of up to 4 potential neighbors to process as candidates
    /// for pathing.
    /// </returns>
    private AStarTile[] Successors(Vector2 position, TileMap tileMap, Vector2 end, Owner owner = Owner.NONE, bool canFly = false)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int N = y + 1;
        int S = y - 1;
        int E = x + 1;
        int W = x - 1;

        int i = 0;
        AStarTile[] result = new AStarTile[4];
        if (N < tileMap.height
            && (canFly ? !tileMap.tiles[x][N].trueCollision : !tileMap.tiles[x][N].groundCollision)
            && (owner == Owner.NONE ? true : (end.x == x && end.y == N) || !tileMap.IsEnemyActor(new Vector2(x, N), owner)))
            result[i++] = new AStarTile(new Vector2(x, N));
        if (E < tileMap.width
            && (canFly ? !tileMap.tiles[E][y].trueCollision : !tileMap.tiles[E][y].groundCollision)
            && (owner == Owner.NONE ? true : (end.x == E && end.y == y) || !tileMap.IsEnemyActor(new Vector2(E, y), owner)))
            result[i++] = new AStarTile(new Vector2(E, y));
        if (S > -1 
            && (canFly ? !tileMap.tiles[x][S].trueCollision : !tileMap.tiles[x][S].groundCollision)
            && (owner == Owner.NONE ? true : (end.x == x && end.y == S) || !tileMap.IsEnemyActor(new Vector2(x, S), owner)))
            result[i++] = new AStarTile(new Vector2(x, S));
        if (W > -1
            && (canFly ? !tileMap.tiles[W][y].trueCollision : !tileMap.tiles[W][y].groundCollision)
            && (owner == Owner.NONE ? true : (end.x == W && end.y == y) || !tileMap.IsEnemyActor(new Vector2(W, y), owner)))
            result[i++] = new AStarTile(new Vector2(W, y));
        return result;
    }

    /// <summary>
    /// The algorithm for comparing the distance between 2 nodes.
    /// </summary>
    /// <param name="start">The starting position.</param>
    /// <param name="end">The ending position.</param>
    /// <returns>
    /// Returns a normalized double value that represents how close
    /// 2 points are from one another.
    /// </returns>
    private double Manhattan(AStarTile start, AStarTile end)
    {
        return Math.Abs(start.position.x - end.position.x)
            + Math.Abs(start.position.y - end.position.y);
    }

    /// <summary>
    /// Get the unique index to be used to find a tile within a single list.
    /// </summary>
    /// <param name="position">
    /// The position of the tile to convert to a unique index.
    /// </param>
    /// <param name="columnCount">The grids current column count.</param>
    /// <returns>Returns the unique index represention of a Vector2.</returns>
    private int GetIndex(Vector2 position, int columnCount)
    {
        return (int)(position.x + position.y * columnCount);
    }

    /// <summary>
    /// Search the list for the existence of a single index.
    /// </summary>
    /// <param name="indexChecked">The list of indexes to look in.</param>
    /// <param name="indexToFind">The index to find.</param>
    /// <returns>Returns whether the index is in the list already.</returns>
    private bool InList(List<int> indexChecked, int indexToFind)
    {
        bool inList = false;
        foreach (int index in indexChecked)
        {
            if (indexToFind == index)
                inList = true;
        }
        return inList;
    }

    /// <summary>
    /// Compares to adjacent grid positions and assigns a directional value.
    /// </summary>
    /// <param name="from">The position to compare from.</param>
    /// <param name="to">The position to compare to.</param>
    /// <returns>Returns the direction 2 adjacent vectors.</returns>
    private AStarDirection GetDirection(Vector2 from, Vector2 to)
    {
        Vector2 N = new Vector2(from.x, from.y + 1);
        Vector2 S = new Vector2(from.x, from.y - 1);
        Vector2 E = new Vector2(from.x + 1, from.y);
        Vector2 W = new Vector2(from.x - 1, from.y);

        AStarDirection direction = AStarDirection.NONE;
        if (N.x == to.x && N.y == to.y)
            direction = AStarDirection.NORTH;
        if (E.x == to.x && E.y == to.y)
            direction = AStarDirection.EAST;
        if (S.x == to.x && S.y == to.y)
            direction = AStarDirection.SOUTH;
        if (W.x == to.x && W.y == to.y)
            direction = AStarDirection.WEST;
        return direction;
    }

    /// <summary>
    /// Takes a result pathing tile with parent information and fills in
    /// information about the current path node.
    /// </summary>
    /// <param name="current">The current pathing tile. Generally has parent
    /// information, otherwise it is the end of the path.</param>
    /// <param name="previous">The previous pathing tile.</param>
    /// <returns></returns>
    private AStarVector GetPathingTile(AStarTile current, AStarTile previous)
    {
        AStarVector aStarVector = new AStarVector();
        aStarVector.position = current.position;

        // Zero out the position mask and direction.
        aStarVector.direction = AStarDirection.NONE;
        aStarVector.mask[0] = 0x0;
        aStarVector.mask[1] = 0x0;
        aStarVector.mask[2] = 0x0;
        aStarVector.mask[3] = 0x0;

        // Compare the current position to parent position.
        if (current.parent != null)
        {
            switch (GetDirection(current.position, current.parent.position))
            {
                case AStarDirection.NORTH:
                    aStarVector.mask[0] = 0x1;
                    aStarVector.direction = AStarDirection.SOUTH;
                    break;
                case AStarDirection.EAST:
                    aStarVector.mask[1] = 0x1;
                    aStarVector.direction = AStarDirection.WEST;
                    break;
                case AStarDirection.SOUTH:
                    aStarVector.mask[2] = 0x1;
                    aStarVector.direction = AStarDirection.NORTH;
                    break;
                case AStarDirection.WEST:
                    aStarVector.mask[3] = 0x1;
                    aStarVector.direction = AStarDirection.EAST;
                    break;
            }
        }

        // Check if this is the end of the pathing.
        if (previous != null)
        {
            // Compare the current position to the previous position.
            switch (GetDirection(current.position, previous.position))
            {
                case AStarDirection.NORTH:
                    aStarVector.mask[0] = 0x1;
                    aStarVector.direction = AStarDirection.NORTH;
                    break;
                case AStarDirection.EAST:
                    aStarVector.mask[1] = 0x1;
                    aStarVector.direction = AStarDirection.EAST;
                    break;
                case AStarDirection.SOUTH:
                    aStarVector.mask[2] = 0x1;
                    aStarVector.direction = AStarDirection.SOUTH;
                    break;
                case AStarDirection.WEST:
                    aStarVector.mask[3] = 0x1;
                    aStarVector.direction = AStarDirection.WEST;
                    break;
            }
        }

        return aStarVector;
    }

    /// <summary>
    /// Determine the path a unit should take from point A to point B given
    /// a current grid of tiles with possible collision data.
    /// </summary>
    /// <param name="tileMap">
    /// The tilemap with collision data to make checks against.
    /// </param>
    /// <param name="_start">
    /// The position the unit before calculating pathing.
    /// </param>
    /// <param name="_end">The position the unit wishes to move to.</param>
    /// <param name="canFly">
    /// Whether or not the unit can ignore ground collision.
    /// </param>
    public Astar(TileMap tileMap, Vector2 _start, Vector2 _end, Owner owner = Owner.NONE, bool canFly = false)
    {
        // Create our destination tile.
        AStarTile end = new AStarTile(_end, GetIndex(_end, tileMap.width), 0, 0);

        // Keep track of all tiles that need to be traversed.
        List<AStarTile> open = new List<AStarTile>();

        // Keep track of all tile indexes already checked.
        List<int> indexChecked = new List<int>();

        // Start the algorithm off by placing in the start position.
        open.Add(new AStarTile(_start, GetIndex(_start, tileMap.width), 0.0f, 0.0f));

        // As long as we still have tiles to check for the destination, continue.
        while (open.Count > 0)
        {
            // Select the next tile in the list of open tiles by ordering
            // the entire list by thier fScore. Open can eventually become a
            // very large list.
            double max = tileMap.width * tileMap.height;
            int min = 0;
            for (int i = 0; i < open.Count; i++)
            {
                if (open[i].fScore < max)
                {
                    max = open[i].fScore;
                    min = i;
                }
            }

            // Pull the best match tile out of the current list of tiles to
            // traverse.
            AStarTile current = open[min];
            open.RemoveAt(min);

            // Check if we are the destination tile.
            if (current.index != end.index)
            {
                // We are not the destination tile.
                // Get the list of the current tiles neighbors.
                AStarTile[] next = Successors(current.position, tileMap, _end, owner, canFly);

                // Iterate through the neighbors found.
                for (int i = 0; i < next.Length; ++i)
                {
                    if (next[i] == null)
                        continue;

                    // Check to see if the tile has already been checked.
                    int adjacentIndex = GetIndex(next[i].position, tileMap.width);
                    if (InList(indexChecked, adjacentIndex))
                    {
                        // We do not need to add this tile to the list of open tiles.
                        continue;
                    }

                    // Create a new tile to be added to be traverse list.
                    AStarTile adjacent = new AStarTile(next[i].position, adjacentIndex, 0, 0);

                    // Keep track of where this tile was found from.
                    adjacent.parent = current;

                    // Calculate the fScore of the new open tile.
                    adjacent.gScore = current.gScore + Manhattan(adjacent, current);
                    adjacent.fScore = adjacent.gScore + Manhattan(adjacent, end);

                    // Add the open tile to the list of available tiles to check.
                    open.Add(adjacent);

                    // Lastly, mark the tile index as processed.
                    indexChecked.Add(adjacent.index);
                }
            }
            else
            {
                // A match was found. Move backwards through the list of linked
                // parents and add them to the result.

                // Keep track of where we traversed backwards from
                // for determining directional bytemask information.
                AStarTile previous = null;

                // Iterate through each parent until we construct a full path.
                do
                {
                    result.Add(GetPathingTile(current, previous));
                    previous = current;
                } while ((current = current.parent) != null);

                // Reverse the list of results to point fowards.
                result.Reverse();

                // We found a result. Clear out any other open tiles to exit loop.
                open.Clear();
            }
        }
    }
}
