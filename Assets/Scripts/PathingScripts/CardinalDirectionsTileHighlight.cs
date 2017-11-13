using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CardinalDirectionsTileHighlight : MonoBehaviour {

    public CardinalDirectionsTileHighlight() { }

    public static List<Tile> FindCardinalDirectionHighlight(Tile originTile, int moveDistance, int attackRange)
    {
        List<Tile> path = new List<Tile>();

        Tile t;
        
        // NORTH PATH
        for (int i = 1; i < moveDistance + attackRange + 1; i++)
        {
            //check north if there are tiles
            if (originTile.gridPosition.y - i < 0)
            {
                continue;
            }
            else
            {
                // Don't highlight impassible tiles
                if (GameManager.instance.map[(int)originTile.gridPosition.x][(int)originTile.gridPosition.y - i].impassible)
                {
                    break;
                }
                // Check if charger can move
                if (i < moveDistance + 1 && !(GameManager.instance.map[(int)originTile.gridPosition.x][(int)originTile.gridPosition.y - i].occupiedAI))
                {
                    Vector2 n = new Vector2(originTile.gridPosition.x, originTile.gridPosition.y - i);
                    t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                    t.moveTile = true;
                    path.Add(t);
                    continue;
                }
                // Check attack range + move distance
                if (i < attackRange + moveDistance + 1)
                {
                    Vector2 n = new Vector2(originTile.gridPosition.x, originTile.gridPosition.y - i);
                    t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                    t.attackTile = true;
                    path.Add(t);

                    // If no AI is encountered then continue;
                    if (!(GameManager.instance.map[(int)originTile.gridPosition.x][(int)originTile.gridPosition.y - i].occupiedAI))
                        continue;
                    // If AI is encountered then break;
                    else
                        break;
                }
            }
        }

        // NORTH WEST
        for (int i = 1; i < moveDistance + attackRange; i++)
        {
            //check north or west if there are tiles
            if (originTile.gridPosition.y - 1 < 0 || originTile.gridPosition.x - i < 0)
            {
                continue;
            }
            else
            {
                if (GameManager.instance.map[(int)originTile.gridPosition.x][(int)originTile.gridPosition.y - 1].moveTile)
                {
                    // Check attack range + move distance
                    if (i < attackRange + moveDistance)
                    {
                        Vector2 n = new Vector2(originTile.gridPosition.x - i, originTile.gridPosition.y - 1);
                        t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                        t.attackTile = true;
                        path.Add(t);

                        // If not AI and impassible then continue;
                        if (!(GameManager.instance.map[(int)originTile.gridPosition.x - i][(int)originTile.gridPosition.y - 1].occupiedAI) &&
                            !(GameManager.instance.map[(int)originTile.gridPosition.x - i][(int)originTile.gridPosition.y - 1].impassible))
                            continue;
                        // If AI is encountered then break;
                        else
                            break;
                    }
                }
            }
        }
        // NORTH EAST
        for (int i = 1; i < moveDistance + attackRange; i++)
        {
            //check north or east if there are tiles
            if (originTile.gridPosition.y - 1 < 0 || !(originTile.gridPosition.x + i < GameManager.instance.mapNumberOfColumns))
            {
                continue;
            }
            else
            {
                if(GameManager.instance.map[(int)originTile.gridPosition.x][(int)originTile.gridPosition.y - 1].moveTile)
                {
                    // Check attack range + move distance
                    if (i < attackRange + moveDistance)
                    {
                        Vector2 n = new Vector2(originTile.gridPosition.x + i, originTile.gridPosition.y - 1);
                        t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                        t.attackTile = true;
                        path.Add(t);

                        // If not AI and impassible then continue;
                        if (!(GameManager.instance.map[(int)originTile.gridPosition.x + i][(int)originTile.gridPosition.y - 1].occupiedAI) &&
                            !(GameManager.instance.map[(int)originTile.gridPosition.x + i][(int)originTile.gridPosition.y - 1].impassible))
                            continue;
                        // If AI is encountered then break;
                        else
                            break;
                    }
                }
            }
        }

        // SOUTH PATH
        for (int i = 1; i < moveDistance + attackRange + 1; i++)
        {
            //check south if there are tiles
            if (!(originTile.gridPosition.y + i < GameManager.instance.mapNumberOfRows))
            {
                continue;
            }
            else
            {
                // Don't highlight impassible tiles
                if (GameManager.instance.map[(int)originTile.gridPosition.x][(int)originTile.gridPosition.y + i].impassible)
                {
                    break;
                }
                // Check if charger can move
                if (i < moveDistance + 1 && !(GameManager.instance.map[(int)originTile.gridPosition.x][(int)originTile.gridPosition.y + i].occupiedAI))
                {
                    Vector2 n = new Vector2(originTile.gridPosition.x, originTile.gridPosition.y + i);
                    t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                    t.moveTile = true;
                    path.Add(t);
                    continue;
                }

                // Check attack range + move distance
                if (i < attackRange + moveDistance + 1)
                {
                    Vector2 n = new Vector2(originTile.gridPosition.x, originTile.gridPosition.y + i);
                    t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                    t.attackTile = true;
                    path.Add(t);

                    // If no AI is encountered then continue;
                    if (!(GameManager.instance.map[(int)originTile.gridPosition.x][(int)originTile.gridPosition.y + i].occupiedAI))
                        continue;
                    // If AI is encountered then break;
                    else
                        break;
                }
            }
        }

        // SOUTH WEST
        for (int i = 1; i < moveDistance + attackRange; i++)
        {
            //check south or west if there are tiles
            if (!(originTile.gridPosition.y + 1 < GameManager.instance.mapNumberOfRows) || originTile.gridPosition.x - i < 0)
            {
                continue;
            }
            else
            {
                if (GameManager.instance.map[(int)originTile.gridPosition.x][(int)originTile.gridPosition.y + 1].moveTile)
                {
                    // Check attack range + move distance
                    if (i < attackRange + moveDistance)
                    {
                        Vector2 n = new Vector2(originTile.gridPosition.x - i, originTile.gridPosition.y + 1);
                        t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                        t.attackTile = true;
                        path.Add(t);

                        // If not AI and impassible then continue;
                        if (!(GameManager.instance.map[(int)originTile.gridPosition.x - i][(int)originTile.gridPosition.y + 1].occupiedAI) &&
                            !(GameManager.instance.map[(int)originTile.gridPosition.x - i][(int)originTile.gridPosition.y + 1].impassible))
                            continue;
                        // If AI is encountered then break;
                        else
                            break;
                    }
                }
            }
        }
        // SOUTH EAST
        for (int i = 1; i < moveDistance + attackRange; i++)
        {
            //check south or east if there are tiles
            if (!(originTile.gridPosition.y + 1 < GameManager.instance.mapNumberOfRows) || !(originTile.gridPosition.x + i < GameManager.instance.mapNumberOfColumns))
            {
                continue;
            }
            else
            {
                if (GameManager.instance.map[(int)originTile.gridPosition.x][(int)originTile.gridPosition.y + 1].moveTile)
                {
                    // Check attack range + move distance
                    if (i < attackRange + moveDistance)
                    {
                        Vector2 n = new Vector2(originTile.gridPosition.x + i, originTile.gridPosition.y + 1);
                        t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                        t.attackTile = true;
                        path.Add(t);

                        // If not AI and impassible then continue;
                        if (!(GameManager.instance.map[(int)originTile.gridPosition.x + i][(int)originTile.gridPosition.y + 1].occupiedAI) &&
                            !(GameManager.instance.map[(int)originTile.gridPosition.x + i][(int)originTile.gridPosition.y + 1].impassible))
                            continue;
                        // If AI is encountered then break;
                        else
                            break;
                    }
                }
            }
        }

        // WEST PATH
        for (int i = 1; i < moveDistance + attackRange + 1; i++)
        {
            //check west if there are tiles
            if (!(originTile.gridPosition.x - i > -1))
            {
                continue;
            }
            else
            {
                // Don't highlight impassible tiles
                if (GameManager.instance.map[(int)originTile.gridPosition.x - i][(int)originTile.gridPosition.y].impassible)
                {
                    break;
                }

                // Check if charger can move
                if (i < moveDistance + 1 &&
                    !(GameManager.instance.map[(int)originTile.gridPosition.x - i][(int)originTile.gridPosition.y].occupiedAI))
                {
                    Vector2 n = new Vector2(originTile.gridPosition.x - i, originTile.gridPosition.y);
                    t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                    t.moveTile = true;
                    path.Add(t);
                    continue;
                }

                // Check attack range + move distance
                if (i < attackRange + moveDistance + 1)
                {
                    Vector2 n = new Vector2(originTile.gridPosition.x - i, originTile.gridPosition.y);
                    t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                    t.attackTile = true;
                    path.Add(t);

                    // If no AI is encountered then continue;
                    if (!(GameManager.instance.map[(int)originTile.gridPosition.x - i][(int)originTile.gridPosition.y].occupiedAI))
                        continue;
                    // If AI is encountered then break;
                    else
                        break;
                }
            }
        }
        // WEST SOUTH
        for (int i = 1; i < moveDistance + attackRange; i++)
        {
            //check south or west if there are tiles
            if (!(originTile.gridPosition.y + i < GameManager.instance.mapNumberOfRows) || originTile.gridPosition.x - 1 < 0)
            {
                continue;
            }
            else
            {
                if (GameManager.instance.map[(int)originTile.gridPosition.x - 1][(int)originTile.gridPosition.y].moveTile)
                {
                    // Check attack range + move distance
                    if (i < attackRange + moveDistance)
                    {
                        Vector2 n = new Vector2(originTile.gridPosition.x - 1, originTile.gridPosition.y + i);
                        t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                        t.attackTile = true;
                        path.Add(t);

                        // If not AI and impassible then continue;
                        if (!(GameManager.instance.map[(int)originTile.gridPosition.x - 1][(int)originTile.gridPosition.y + i].occupiedAI) &&
                            !(GameManager.instance.map[(int)originTile.gridPosition.x - 1][(int)originTile.gridPosition.y + i].impassible))
                            continue;
                        // If AI is encountered then break;
                        else
                            break;
                    }
                }
            }
        }

        // WEST NORTH
        for (int i = 1; i < moveDistance + attackRange; i++)
        {
            //check south or west if there are tiles
            if (originTile.gridPosition.y - i < 0 || originTile.gridPosition.x - 1 < 0)
            {
                continue;
            }
            else
            {
                if (GameManager.instance.map[(int)originTile.gridPosition.x - 1][(int)originTile.gridPosition.y].moveTile)
                {
                    // Check attack range + move distance
                    if (i < attackRange + moveDistance)
                    {
                        Vector2 n = new Vector2(originTile.gridPosition.x - 1, originTile.gridPosition.y - i);
                        t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                        t.attackTile = true;
                        path.Add(t);

                        // If not AI and impassible then continue;
                        if (!(GameManager.instance.map[(int)originTile.gridPosition.x - 1][(int)originTile.gridPosition.y - i].occupiedAI) &&
                            !(GameManager.instance.map[(int)originTile.gridPosition.x - 1][(int)originTile.gridPosition.y - i].impassible))
                            continue;
                        // If AI is encountered then break;
                        else
                            break;
                    }
                }
            }
        }

        // EAST PATH
        for (int i = 1; i < moveDistance + attackRange + 1; i++)
        {
            //check east if there are tiles
            if (!(originTile.gridPosition.x + i < GameManager.instance.mapNumberOfColumns))
            {
                continue;
            }
            else
            {
                // Don't highlight impassible tiles
                if (GameManager.instance.map[(int)originTile.gridPosition.x + i][(int)originTile.gridPosition.y].impassible)
                {
                    break;
                }

                // Check if charger can move
                if (i < moveDistance + 1 &&
                    !(GameManager.instance.map[(int)originTile.gridPosition.x + i][(int)originTile.gridPosition.y].occupiedAI))
                {
                    Vector2 n = new Vector2(originTile.gridPosition.x + i, originTile.gridPosition.y);
                    t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                    t.moveTile = true;
                    path.Add(t);
                    continue;
                }
                
                // Check attack range + move distance
                if (i < attackRange + moveDistance + 1)
                {
                    Vector2 n = new Vector2(originTile.gridPosition.x + i, originTile.gridPosition.y);
                    t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                    t.attackTile = true;
                    path.Add(t);

                    // If no AI is encountered then continue;
                    if (!(GameManager.instance.map[(int)originTile.gridPosition.x + i][(int)originTile.gridPosition.y].occupiedAI))
                        continue;
                    // If AI is encountered then break;
                    else
                        break;
                }
            }
        }

        // EAST SOUTH
        for (int i = 1; i < moveDistance + attackRange; i++)
        {
            //check south or west if there are tiles
            if (!(originTile.gridPosition.y + i < GameManager.instance.mapNumberOfRows) || !(originTile.gridPosition.x + i < GameManager.instance.mapNumberOfColumns))
            {
                continue;
            }
            else
            {
                if (GameManager.instance.map[(int)originTile.gridPosition.x + 1][(int)originTile.gridPosition.y].moveTile)
                {
                    // Check attack range + move distance
                    if (i < attackRange + moveDistance)
                    {
                        Vector2 n = new Vector2(originTile.gridPosition.x + 1, originTile.gridPosition.y + i);
                        t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                        t.attackTile = true;
                        path.Add(t);

                        // If not AI and impassible then continue;
                        if (!(GameManager.instance.map[(int)originTile.gridPosition.x + 1][(int)originTile.gridPosition.y + i].occupiedAI) &&
                            !(GameManager.instance.map[(int)originTile.gridPosition.x + 1][(int)originTile.gridPosition.y + i].impassible))
                            continue;
                        // If AI is encountered then break;
                        else
                            break;
                    }
                }
            }
        }

        // EAST NORTH
        for (int i = 1; i < moveDistance + attackRange; i++)
        {
            //check south or west if there are tiles
            if (originTile.gridPosition.y - i < 0 || !(originTile.gridPosition.x + i < GameManager.instance.mapNumberOfColumns))
            {
                continue;
            }
            else
            {
                if (GameManager.instance.map[(int)originTile.gridPosition.x + 1][(int)originTile.gridPosition.y].moveTile)
                {
                    // Check attack range + move distance
                    if (i < attackRange + moveDistance)
                    {
                        Vector2 n = new Vector2(originTile.gridPosition.x + 1, originTile.gridPosition.y - i);
                        t = GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)];
                        t.attackTile = true;
                        path.Add(t);

                        // If not AI and impassible then continue;
                        if (!(GameManager.instance.map[(int)originTile.gridPosition.x + 1][(int)originTile.gridPosition.y - i].occupiedAI) &&
                            !(GameManager.instance.map[(int)originTile.gridPosition.x + 1][(int)originTile.gridPosition.y - i].impassible))
                            continue;
                        // If AI is encountered then break;
                        else
                            break;
                    }
                }
            }
        }

        //path.Remove(originTile);
        return path;
    }
}