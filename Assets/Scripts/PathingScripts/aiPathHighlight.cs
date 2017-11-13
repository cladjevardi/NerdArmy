using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class aiTileHighlight
{

    public aiTileHighlight() { }

    public static List<Tile> aiFindHighlight(Tile originTile, int movementPoints)
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
            if (current.costOfPath > movementPoints + 1)
            {
                continue;
            }

            closed.Add(current.lastTile);

            foreach (Tile t in current.lastTile.neighbors)
            {
                // This might need to be fixed (but I currently can't find bugs with it)
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
                //if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking && t.occupiedAI)
                //{
                //    break;
                //}
            }
        }
        closed.Remove(originTile);
        return closed;
    }
}