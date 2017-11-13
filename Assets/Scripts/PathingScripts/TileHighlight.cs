using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TileHighlight {

    public TileHighlight() { }

    private List<Tile> validMoves;

    public static List<Tile> FindHighlight(Tile originTile, int movementPoints, int attackRange)
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
            if (current.costOfPath > movementPoints + attackRange + 1)
            {
                continue;
            }

            closed.Add(current.lastTile);
            //Debug.Log(originTile.gridPosition);
            TilePath newTilePath;

            foreach (Tile t in current.lastTile.neighbors)
            {
                if (current.costOfPath < movementPoints + 1)
                {
                    // north east fix
                    if (current.lastTile.northNeighbor() != null && current.lastTile.eastNeighbor() != null)
                    {
                        if (current.lastTile.northNeighbor().occupiedAI && current.lastTile.eastNeighbor().occupiedAI)
                        {
                            Tile temp = GameManager.instance.map[(int)Mathf.Round(current.lastTile.gridPosition.x + 1)]
                                [(int)Mathf.Round(current.lastTile.gridPosition.y - 1)];
                            temp.attackTile = true;
                            closed.Add(temp);
                        }
                    }
                    // north west fix
                    if (current.lastTile.northNeighbor() != null && current.lastTile.westNeighbor() != null)
                    {
                        if (current.lastTile.northNeighbor().occupiedAI && current.lastTile.westNeighbor().occupiedAI)
                        {
                            Tile temp = GameManager.instance.map[(int)Mathf.Round(current.lastTile.gridPosition.x - 1)]
                                [(int)Mathf.Round(current.lastTile.gridPosition.y - 1)];
                            temp.attackTile = true;
                            closed.Add(temp);
                        }
                    }
                    // south west fix
                    if (current.lastTile.southNeighbor() != null && current.lastTile.westNeighbor() != null)
                    {
                        if (current.lastTile.southNeighbor().occupiedAI && current.lastTile.westNeighbor().occupiedAI)
                        {
                            Tile temp = GameManager.instance.map[(int)Mathf.Round(current.lastTile.gridPosition.x - 1)]
                                [(int)Mathf.Round(current.lastTile.gridPosition.y + 1)];
                            temp.attackTile = true;
                            closed.Add(temp);
                        }
                    }
                    // south east fix
                    if (current.lastTile.southNeighbor() != null && current.lastTile.eastNeighbor() != null)
                    {
                        if (current.lastTile.southNeighbor().occupiedAI && current.lastTile.eastNeighbor().occupiedAI)
                        {
                            Tile temp = GameManager.instance.map[(int)Mathf.Round(current.lastTile.gridPosition.x + 1)]
                                [(int)Mathf.Round(current.lastTile.gridPosition.y + 1)];
                            temp.attackTile = true;
                            closed.Add(temp);
                        }
                    }

                    // Don't highlight impassible no matter what
                    if (t.impassible)
                    {
                        newTilePath = new TilePath(current);
                        t.attackTile = true;
                        newTilePath.addTile(t);
                        open.Add(newTilePath);
                        continue;
                    }
                    // Add attack highlight if AI occupied and within range
                    if (t.occupiedAI)
                    {
                        newTilePath = new TilePath(current);
                        t.attackTile = true;
                        newTilePath.addTile(t);
                        open.Add(newTilePath);
                        continue;
                    }
                    if (current.attackCostOfPath < 1)
                    {
                        newTilePath = new TilePath(current);
                        t.moveTile = true;
                        newTilePath.addTile(t);
                        open.Add(newTilePath);
                        continue;
                    }
                    /*
                    if (current.attackCostOfPath >= 1)
                    {
                        newTilePath = new TilePath(current);
                        t.attackTile = true;
                        newTilePath.addTile(t);
                        open.Add(newTilePath);
                        continue;
                    }
                    */
                    
                }
                else
                {
                    // Don't highlight impassible or designated move tiles
                    if (t.impassible) //|| t.moveTile)
                    {
                        //newTilePath = new TilePath(current);
                        //t.attackTile = true;
                        //newTilePath.addTile(t);
                        //open.Add(newTilePath);
                        continue;
                    }
                    if (current.attackCostOfPath<attackRange)
                    {
                        newTilePath = new TilePath(current);
                        t.attackTile = true;
                        newTilePath.addTile(t);
                        open.Add(newTilePath);
                        continue;
                    }
                }
            }
        }
        closed.Remove(originTile);
        return closed;
    }
}

/* 
        List<Tile> closed = new List<Tile>();
        List<TilePath> open = new List<TilePath>();

        TilePath originPath = new TilePath();
        originPath.addTile(originTile);

        open.Add(originPath);

        while(open.Count > 0)
        {
            TilePath current = open[0];
            open.Remove(open[0]);

            if(closed.Contains(current.lastTile))
            {
                continue;
            }
            if(current.costOfPath > movementPoints + attackRange + 1)
            {
                continue;
            }

            closed.Add(current.lastTile);
            //Debug.Log(originTile.gridPosition);
            TilePath newTilePath;

            foreach (Tile t in current.lastTile.neighbors)
            {
                if (current.costOfPath < movementPoints + 1)
                {
                    // Don't highlight impassible no matter what
                    if (t.impassible)
                    {
                        continue;
                    }
                    // If the current attackcostofpath > 0 then there should be no movement path
                    if(!t.occupiedAI && current.attackCostOfPath == 0)
                    {
                        newTilePath = new TilePath(current);
                        t.moveTile = true;
                        newTilePath.addTile(t);
                        open.Add(newTilePath);
                        continue;
                    }
                    // Add attack highlight if AI occupied and within range
                    if(t.occupiedAI || (current.attackCostOfPath < attackRange))
                    {
                        newTilePath = new TilePath(current);
                        t.attackTile = true;
                        newTilePath.addTile(t);
                        open.Add(newTilePath);
                        continue;
                    }
                    newTilePath = new TilePath(current);
                    t.moveTile = true;
                    newTilePath.addTile(t);
                    open.Add(newTilePath);
                }

                else if(current.costOfPath < movementPoints + attackRange + 1)
                {
                    //closed.Remove(originTile);

                    // Don't highlight impassible or designated move tiles
                    if (t.impassible || t.moveTile)
                    {
                        continue;
                    }
                    if(current.movementCostOfPath > 0)
                    {
                        newTilePath = new TilePath(current);
                        t.attackTile = true;
                        newTilePath.addTile(t);
                        open.Add(newTilePath);
                        continue;
                    }
                    if(current.attackCostOfPath < attackRange)
                    {
                        newTilePath = new TilePath(current);
                        t.attackTile = true;
                        newTilePath.addTile(t);
                        open.Add(newTilePath);
                        continue;
                    }
                }
                
            }
        }
        closed.Remove(originTile);
        return closed;
    }
*/
