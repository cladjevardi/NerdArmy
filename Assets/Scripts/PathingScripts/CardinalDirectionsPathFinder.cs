/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardinalDirectionsPathFinder : MonoBehaviour {

    public static List<Tile> FindPath(Tile originTile, Tile destinationTile)
    {
        List<Tile> closed = new List<Tile>();
        List<TilePath> open = new List<TilePath>();

        TilePath originPath = new TilePath();
        originPath.addTile(originTile);

        open.Add(originPath);

        while (open.Count > 0)
        {
            TilePath current = open[0];
            open.Remove(open[0]);

            if (closed.Contains(current.lastTile))
            {
                continue;
            }
            if (current.lastTile == destinationTile)
            {
                current.listOfTiles.Remove(originTile);
                return current.listOfTiles;
            }

            closed.Add(current.lastTile);
                
            foreach (Tile t in current.lastTile.neighbors)
            {
                if (t.gridPosition.x == GameManager.instance.players[GameManager.instance.currentPlayerIndex].gridPosition.x)
                {
                    if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving && t.occupiedAI)
                    {
                        continue;
                    }
                    // Don't highlight impassible no matter what
                    if (t.impassible)
                    {
                        continue;
                    }
                    TilePath newTilePath = new TilePath(current);
                    newTilePath.addTile(t);
                    open.Add(newTilePath);
                    // Highlight the opponent's tile and the attack path won't 
                    // extend move distance + attack distance through opponents
                    if (t.occupiedAI)
                    {
                        break;
                    }
                }
            }
            foreach (Tile t in current.lastTile.neighbors)
            {
                if (t.gridPosition.y == GameManager.instance.players[GameManager.instance.currentPlayerIndex].gridPosition.y)
                {
                    if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving && t.occupiedAI)
                    {
                        continue;
                    }
                    // Don't highlight impassible no matter what
                    if (t.impassible)
                    {
                        continue;
                    }
                    TilePath newTilePath = new TilePath(current);
                    newTilePath.addTile(t);
                    open.Add(newTilePath);
                    // Highlight the opponent's tile and the attack path won't 
                    // extend move distance + attack distance through opponents
                    if (t.occupiedAI)
                    {
                        break;
                    }
                }
            }
        }
        return null;
    }
}

*/