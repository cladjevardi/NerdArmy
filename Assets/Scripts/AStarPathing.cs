using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Random = UnityEngine.Random;

/// <summary>The direction for AStar result information.</summary>
public enum AStarDirection
{
    NONE,
    NORTH,
    EAST,
    SOUTH,
    WEST,
}

[Serializable()]
public class AStarVector
{
    /// <summary>The position of the pathing tile.</summary>
    private float _x;
    public float x
    {
        get { return _x; }
        set { _x = value; }
    }

    private float _y;
    public float y
    {
        get { return _y; }
        set { _y = value; }
    }

    public Vector2 Position()
    {
        return new Vector2(x, y);
    }
    public void Position(Vector2 position)
    {
        _x = position.x;
        _y = position.y;
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

    /// <summary>
    /// The tile to stop if attacking.
    /// </summary>
    private bool _withinRange = false;
    public bool withinRange
    {
        get { return _withinRange; }
        set { _withinRange = value; }
    }

    /// <summary>
    /// The id of the pathing result for stitching.
    /// </summary>
    private int _pathId = 0;
    public int pathId
    {
        get { return _pathId; }
        set { _pathId = value; }
    }

    public static object DeepClone(object obj)
    {
        object objResult = null;
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, obj);

            ms.Position = 0;
            objResult = bf.Deserialize(ms);
        }
        return objResult;
    }
}

public class AStarPath
{
    private void CalculateScore()
    {
        xScore = 0; yScore = 0;
        foreach (AStarVector step in _path)
        {
            xScore += step.Position().x;
            yScore += step.Position().x;
        }
    }

    List<AStarVector> _path = new List<AStarVector>();
    public List<AStarVector> path
    {
        get { return _path; }
        set { _path = value; CalculateScore(); }
    }

    private float _xScore = 0;
    public float xScore
    {
        get { return _xScore; }
        internal set { _xScore = value; }
    }

    private float _yScore = 0;
    public float yScore
    {
        get { return _yScore; }
        internal set { _yScore = value; }
    }
}

/// <summary>Tile metadata for the a* algorithm.</summary>
public class AStarTile
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

    /// <summary>
    /// The parent of the current astar pathing tile. This keeps track of a result path.
    /// </summary>
    private AStarTile _parent = null;
    public AStarTile parent
    {
        get { return _parent; }
        set { _parent = value; }
    }

    /// <summary>
    /// Keep track of if we are within (ignore collision) range.
    /// </summary>
    private bool _withinRange = false;
    public bool withinRange
    {
        get { return _withinRange; }
        set { _withinRange = value; }
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
    public AStarTile(Vector2 position, double fScore = float.MaxValue, double gScore = float.MaxValue)
    {
        this.position = position;
        this.fScore = fScore;
        this.gScore = gScore;
    }
}

public enum AStarDestinationType
{
    NONE = -1,
    MOVEMENT,
    ATTACK,
}

public class AStarDestination
{
    private Vector2 _position;
    public Vector2 position
    {
        get { return _position; }
        set { _position = value; }
    }

    private AStarDestinationType _type = AStarDestinationType.NONE;
    public AStarDestinationType type
    {
        get { return _type; }
        set { _type = value; }
    }

    public AStarDestination(Vector2 position, AStarDestinationType type = AStarDestinationType.NONE)
    {
        this.position = position;
        this.type = type;
    }
}

public class AStar
{
    private TileMap tileMap;
    private Actor actor;
    private List<AStarDestination> destinations = new List<AStarDestination>();
    private List<AStarPath> results = new List<AStarPath>();

    private bool ShouldAdd(Vector2 position, AStarDestinationType type)
    {
        // Check if out of bounds.
        if (!(position.x >= 0 && position.x < tileMap.width
            && position.y >= 0 && position.y < tileMap.height))
            return false;

        Tile tile = tileMap.GetTile(position);

        // Check based on if the destination ignores/honors collision
        if (type == AStarDestinationType.MOVEMENT)
        {
            if (tile.groundCollision && !actor.unit.HasPassive(PassiveType.FLYING))
                return false;
            if (tile.trueCollision)
                return false;
            if (tileMap.IsEnemyActor(position, actor.owner))
                return false;
        }
        else if (type == AStarDestinationType.ATTACK)
        {
            if (tile.trueCollision)
                return false;
        }

        // Nothing is preventing the tile from being added to check.
        return true;
    }

    private double Manhattan(Vector2 start, Vector2 end)
    {
        return Math.Abs(start.x - end.x) 
            + Math.Abs(start.y - end.y);
    }

    private AStarTile CreateNewTile(Vector2 position, AStarTile parent, AStarDestination destination)
    {
        AStarTile tile = new AStarTile(position);
        tile.parent = parent;
        tile.gScore = parent.gScore + Manhattan(tile.position, parent.position);
        tile.fScore = /*tile.gScore*/ 0 + Manhattan(tile.position, destination.position);
        return tile;
    }

    private List<AStarTile> GetSuccessors(AStarTile parent, AStarDestination destination)
    {
        List<AStarTile> foundTiles = new List<AStarTile>();

        int x = (int)parent.position.x;
        int y = (int)parent.position.y;
        Vector2 north = new Vector2(x, y + 1);
        Vector2 east = new Vector2(x + 1, y);
        Vector2 south = new Vector2(x, y - 1);
        Vector2 west = new Vector2(x - 1, y);

        // Check all of the connected nodes for traversing eligibilty.
        if (ShouldAdd(north, destination.type))
            foundTiles.Add(CreateNewTile(north, parent, destination));
        if (ShouldAdd(east, destination.type))
            foundTiles.Add(CreateNewTile(east, parent, destination));
        if (ShouldAdd(south, destination.type))
            foundTiles.Add(CreateNewTile(south, parent, destination));
        if (ShouldAdd(west, destination.type))
            foundTiles.Add(CreateNewTile(west, parent, destination));
        return foundTiles;
    }

    private List<int> GetLowestFScore(List<AStarTile> tiles)
    {
        List<int> lowestIndex = new List<int>();
        for (int index = 0; index < tiles.Count; ++index)
        {
            if (lowestIndex.Count == 0)
            {
                lowestIndex.Clear();
                lowestIndex.Add(index);
                continue;
            }

            double fScore1 = Math.Abs(tiles[index].fScore -
                Manhattan(tiles[index].position, tiles[lowestIndex[0]].position));
            double fScore2 = Math.Abs(tiles[lowestIndex[0]].fScore -
                Manhattan(tiles[index].position, tiles[lowestIndex[0]].position));

            if (tiles[index].fScore < tiles[lowestIndex[0]].fScore)
            {
                lowestIndex.Clear();
                lowestIndex.Add(index);
            }
            else if (tiles[index].fScore == tiles[lowestIndex[0]].fScore)
                lowestIndex.Add(index);
        }

        return lowestIndex;
    }

    private bool InList(AStarTile tileToCheck, List<AStarTile> tiles)
    {
        bool found = false;
        foreach (AStarTile tile in tiles)
        {
            if (tileToCheck.position.x == tile.position.x
                && tileToCheck.position.y == tile.position.y)
            {
                found = true;
            }
        }
        return found;
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
        aStarVector.Position(current.position);
        aStarVector.withinRange = current.withinRange;

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

    private bool IsAllPathsCompleted(List<List<AStarTile>> paths)
    {
        bool complete = true;
        foreach (List<AStarTile> path in paths)
        {
            if (path.Count != 0)
                complete = false;
        }
        return complete;
    }

    private AStarPath CombinePath(AStarPath pathing1, AStarPath pathing2)
    {
        if (pathing2.path.Count == 0)
            return pathing1;

        // Construct the new path by adding the range of both.
        AStarPath result = new AStarPath();
        pathing1.path.ForEach((AStarVector item) => {
            result.path.Add((AStarVector)AStarVector.DeepClone(item));
        });

        bool skipFirst = false;
        pathing2.path.ForEach((AStarVector item) => {
            if (!skipFirst)
            {
                // Stitch the 2 pathings together.
                byte[] newMask = item.mask;
                if (result.path[result.path.Count - 1].mask[0] == 0x1)
                    newMask[0] = 0x1;
                if (result.path[result.path.Count - 1].mask[1] == 0x1)
                    newMask[1] = 0x1;
                if (result.path[result.path.Count - 1].mask[2] == 0x1)
                    newMask[2] = 0x1;
                if (result.path[result.path.Count - 1].mask[3] == 0x1)
                    newMask[3] = 0x1;
                result.path[result.path.Count - 1].mask = newMask;
                skipFirst = true;
            }
            else
                result.path.Add((AStarVector)AStarVector.DeepClone(item));
        });

        return result;
    }

    private List<AStarPath> ConcatinatePaths(List<AStarPath> paths, List<AStarPath> toCombine)
    {
        // Exit early if we don't have any paths to combine with nothing.
        if (paths.Count == 0)
            return toCombine;

        List<AStarPath> newPaths = new List<AStarPath>();
        foreach (AStarPath path1 in paths)
        {
            foreach (AStarPath path2 in toCombine)
            {
                AStarPath newPath1 = new AStarPath();
                path1.path.ForEach((AStarVector item) => {
                    newPath1.path.Add((AStarVector)AStarVector.DeepClone(item));
                });
                AStarPath newPath2 = new AStarPath();
                path2.path.ForEach((AStarVector item) => {
                    newPath2.path.Add((AStarVector)AStarVector.DeepClone(item));
                });

                newPaths.Add(CombinePath(newPath1, newPath2));
            }
        }

        // Return the new combined list of pathing results.
        return newPaths;
    }

    public void CalculatePathing()
    {
        AStarDestination lastDestination = new AStarDestination(actor.transform.position);

        List<List<AStarPath>> destinationResults = new List<List<AStarPath>>();
        for (int destinationIndex = 0; destinationIndex < destinations.Count; ++destinationIndex)
        {
            // Create a new list of results for this destination.
            destinationResults.Add(new List<AStarPath>());

            // Keep track of all tiles that need to be traversed.
            List<List<AStarTile>> visited = new List<List<AStarTile>>();
            List<List<AStarTile>> toCheck = new List<List<AStarTile>>();
            visited.Add(new List<AStarTile>());
            toCheck.Add(new List<AStarTile>());

            // Keep track of the current time in history 
            int currentPath = 0;

            // Kick off the pathing with the first 
            toCheck[currentPath].Add(new AStarTile(lastDestination.position, 0f, 0f));

            // Iterate through tiles to check until we have found the destination or have run
            // out of options.
            while (toCheck[currentPath].Count > 0)
            {
                // Determine if we need to spawn off a new path node.
                AStarTile currentTile = null;
                List<int> lowestIndices = GetLowestFScore(toCheck[currentPath]);
                if (lowestIndices.Count > 2)
                    Debug.Log("Something is wrong!");
                foreach (int lowestIndex in lowestIndices)
                {
                    if (currentTile == null)
                    {
                        currentTile = toCheck[currentPath][lowestIndex];
                        toCheck[currentPath].RemoveAt(lowestIndex);
                    }
                    else
                    {
                        visited.Add(new List<AStarTile>());
                        toCheck.Add(new List<AStarTile>());
                        visited[visited.Count - 1].AddRange(visited[currentPath]);
                        //visited[visited.Count - 1].Add(currentTile);
                        toCheck[toCheck.Count - 1].AddRange(toCheck[currentPath]);
                    }
                }

                if (currentTile.position.x == destinations[destinationIndex].position.x
                    && currentTile.position.y == destinations[destinationIndex].position.y)
                {
                    // A match was found. Move backwards through the list of linked
                    // parents and add them to the result.
                    // Keep track of where we traversed backwards from
                    // for determining directional bytemask information.

                    AStarPath resultPath = new AStarPath();
                    AStarTile previous = null;
                    do
                    {
                        resultPath.path.Add(GetPathingTile(currentTile, previous));
                        previous = currentTile;
                    } while ((currentTile = currentTile.parent) != null);

                    // Reverse the list of results to point fowards.
                    resultPath.path.Reverse();
                    destinationResults[destinationIndex].Add(resultPath);

                    // We found a result. Clear out any other open tiles and clear this path.
                    // Move on to the next alternate path if any.
                    toCheck[currentPath].Clear();
                    if (currentPath != toCheck.Count - 1)
                        currentPath++;
                }
                else
                {
                    List<AStarTile> foundTiles = GetSuccessors(currentTile, destinations[destinationIndex]);
                    foreach (AStarTile tile in foundTiles)
                    {
                        if (!InList(tile, visited[currentPath])
                            && !InList(tile, toCheck[currentPath]))
                            toCheck[currentPath].Add(tile);
                    }

                    // Make sure we never traverse over the same tile.
                    visited[currentPath].Add(currentTile);
                }
            }

            if (destinationResults[destinationIndex].Count == 0)
            {
                // Exit early. If we cannot find pathing from a given destination to another
                // destination, then we should fail the entire pathing.
                results.Clear();
                return;
            }

            lastDestination = destinations[destinationIndex];
        }

        // Create the combined list of pathing results.
        List<AStarPath> paths = new List<AStarPath>();
        for (int destinationIndex = 0; destinationIndex < destinations.Count; ++destinationIndex)
        {
            paths = ConcatinatePaths(paths, destinationResults[destinationIndex]);
        }

        int lowestCount = int.MaxValue;
        foreach (AStarPath path in paths)
        {
            if (path.path.Count < lowestCount)
            {
                lowestCount = path.path.Count;
                results.Clear();
                results.Add(path);
            }
            else if (path.path.Count == lowestCount)
                results.Add(path);
        }

        Debug.Log("Results found: " + (results.Count).ToString() + "(" + lowestCount.ToString() + ")");
    }

    public AStarPath GetBestResult()
    {
        // Return the top result. We do not care about what it is.
        return results[(int)Random.Range(0, results.Count)];
    }

    public List<AStarPath> GetTopResults()
    {
        int highestXScoreIndex = 0;
        int lowestXScoreIndex = int.MaxValue;
        int highestYScoreIndex = 0;
        int lowestYScoreIndex = int.MaxValue;
        for (int index = 0; index < results.Count; ++index)
        {
            if (results[index].xScore > results[highestXScoreIndex].xScore)
                highestXScoreIndex = index;
            if (results[index].yScore > results[highestXScoreIndex].yScore)
                highestYScoreIndex = index;
            if (results[index].xScore < results[lowestXScoreIndex].xScore)
                lowestXScoreIndex = index;
            if (results[index].yScore < results[lowestYScoreIndex].yScore)
                lowestYScoreIndex = index;
        }

        List<AStarPath> topResults = new List<AStarPath>();
        topResults.Add(results[highestXScoreIndex]);
        if (lowestXScoreIndex != highestXScoreIndex)
            topResults.Add(results[lowestXScoreIndex]);
        if (highestYScoreIndex != lowestXScoreIndex
            && highestYScoreIndex != highestXScoreIndex)
            topResults.Add(results[highestYScoreIndex]);
        if (lowestYScoreIndex != lowestXScoreIndex
            && lowestYScoreIndex != highestYScoreIndex
            && lowestYScoreIndex != highestXScoreIndex)
            topResults.Add(results[lowestYScoreIndex]);
        return topResults;
    }

    public void AddMovementDestination(Vector2 position)
    {
        if (destinations.Count == 0 || !(destinations[destinations.Count - 1].position.x == position.x
            && destinations[destinations.Count - 1].position.y == position.y))
            destinations.Add(new AStarDestination(position, AStarDestinationType.MOVEMENT));
    }

    public void AddAttackDestination(Vector2 position)
    {
        if (destinations.Count == 0 || !(destinations[destinations.Count - 1].position.x == position.x
            && destinations[destinations.Count - 1].position.y == position.y))
            destinations.Add(new AStarDestination(position, AStarDestinationType.ATTACK));
    }

    public AStar(TileMap tileMap, Actor actor)
    {
        this.tileMap = tileMap;
        this.actor = actor;
    }
}