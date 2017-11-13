using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TilePathFinder : MonoBehaviour {

    public static List<Tile> FindPath(Tile originTile, Tile destinationTile)
    {
        // The set of tiles already evalutated
        List<Tile> closed = new List<Tile>();

        // The set of currently discovered tiles that are not evaluated yet
        List<Tile> open = new List<Tile>();

        int weight = 0;

        // Initially, only the starting tile is known.
        open.Add(originTile);

        while (open.Count > 0)
        {
            Tile current = open[0];

            for (int i = 0; i < open.Count; i++)
                if (open[i].fCost() < current.fCost() || open[i].fCost() == current.fCost())
                    if(open[i].hCost < current.hCost)
                        current = open[i];

            open.Remove(current);
            closed.Add(current);

            if (current == destinationTile)
                return ConstructPath(originTile, destinationTile);

            foreach (Tile t in current.neighbors)
            {
                if (closed.Contains(t) || t.impassible) // Ignore the neighbor which is already evaluated
                    continue;

                //if (t.impassible)
                //    weight = 10;
                if (t.occupiedAI)
                    weight = 6;
                else if (t.occupiedAlly && !t.attackTile)
                    weight = 5;
                else if (t.attackTile)
                    weight = 4;
                //else if (current.lastHighlight)
                //    weight = 0;
                else
                    weight = 0;

                int newCostToNeighbor = current.gCost + GetDistance(current, t) + weight;
                if (!open.Contains(t) || newCostToNeighbor < t.gCost)
                {
                    t.gCost = newCostToNeighbor;
                    t.hCost = GetDistance(t, destinationTile);
                    t.parent = current;

                    if (!open.Contains(t)) // Discover a new node
                        open.Add(t);
                    else
                    {
                        int index = open.IndexOf(t);
                        open[index] = t;
                    }
                }
            }
        }
        return null;
    }
    private static List<Tile> ConstructPath(Tile startTile, Tile endTile)
    {
        List<Tile> path = new List<Tile>();
        Tile currentTile = endTile;

        while(currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.parent;
        }
        path.Reverse();

        return path;
    }

    private static int GetDistance(Tile tileA, Tile tileB)
    {
        int dstX = Mathf.Abs((int)tileA.gridPosition.x - (int)tileB.gridPosition.x);
        int dstY = Mathf.Abs((int)tileA.gridPosition.y - (int)tileB.gridPosition.y);

        return dstY + dstX;
    }
}