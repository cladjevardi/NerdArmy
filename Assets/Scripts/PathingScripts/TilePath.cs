using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TilePath
{
    public List<Tile> listOfTiles = new List<Tile>();

    public int costOfPath = 0;
    public int attackCostOfPath = 0;
    public int movementCostOfPath = 0;

    public Tile lastTile;

    public TilePath() { }

    public TilePath(TilePath tp)
    {
        listOfTiles = tp.listOfTiles.ToList();
        costOfPath = tp.costOfPath;
        attackCostOfPath = tp.attackCostOfPath;
        movementCostOfPath = tp.movementCostOfPath;
        lastTile = tp.lastTile;
    }

    public void addTile(Tile t)
    {
        costOfPath += t.movementCost;

        if (t.attackTile)
        {
            movementCostOfPath+=10;
            attackCostOfPath++;
        }
        else if (t.moveTile)
            movementCostOfPath++;

        listOfTiles.Add(t);
        lastTile = t;
    }
}
