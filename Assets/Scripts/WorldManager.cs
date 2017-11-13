using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class WorldManager : GameManager
{
    public static WorldManager worldInstance;

    //private int mapNumberOfColumns;
    //private int mapNumberOfRows;

    private void Awake()
    {
        worldInstance = this;
    }
    /*
    public override void loadWorldOne1()
    {
        // MAP
        mapNumberOfColumns = 6;
        mapNumberOfRows = 1;
        map = new List<List<Tile>>();
        for (int i = 0; i < mapNumberOfColumns; i++)
        {
            List<Tile> row = new List<Tile>();
            //Debug.Log("Created: " + "row List");

            // Number of Columns
            for (int j = 0; j < mapNumberOfRows; j++)
            {
                Tile tile = ((GameObject)(Instantiate(TilePrefab, new Vector2(i - Mathf.Floor(mapNumberOfColumns / 2), -j + Mathf.Floor(mapNumberOfRows / 2)), Quaternion.Euler(new Vector3())))).GetComponent<Tile>();
                tile.gridPosition = new Vector2(i, j);

                // Add random impassible terrain
                if (i != 0 && j != 0 && j != mapNumberOfRows - 1 && i != mapNumberOfColumns - 1)
                {
                    if (Random.Range(0, 5) > 3)
                        tile.impassible = true;
                }
                //----------------------------

                row.Add(tile);
            }
            map.Add(row);
        }

        // CHARACTERS
        // Playable characters
        UserPlayer player;

        player = ((GameObject)(Instantiate(UserPlayerPrefab, new Vector2(0 - (mapNumberOfColumns / 2), -0 + (mapNumberOfRows / 2)), Quaternion.Euler(new Vector3())))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(0, 0);
        map[(int)player.gridPosition.x][(int)player.gridPosition.y].occupiedAlly = true;
        player.playerName = "Mang0";

        players.Add(player);
        playersGridPosition.Add(player.gridPosition);

        // Bots
        AIPlayer aiPlayer;

        aiPlayer = ((GameObject)(Instantiate(AIPlayerPrefab, new Vector2((mapNumberOfColumns / 2) - 1, -(0f)), Quaternion.Euler(new Vector3())))).GetComponent<AIPlayer>();
        aiPlayer.gridPosition = new Vector2(5, 0);
        map[(int)aiPlayer.gridPosition.x][(int)aiPlayer.gridPosition.y].occupiedAI = true;
        aiPlayer.playerName = "Bot";

        aiPlayers.Add(aiPlayer);
        aiPlayersGridPosition.Add(aiPlayer.gridPosition);
    }
    public override void loadWorldOne2()
    {
        mapNumberOfColumns = 6;
        mapNumberOfRows = 2;
        map = new List<List<Tile>>();
        for (int i = 0; i < mapNumberOfColumns; i++)
        {
            List<Tile> row = new List<Tile>();
            //Debug.Log("Created: " + "row List");

            // Number of Columns
            for (int j = 0; j < mapNumberOfRows; j++)
            {
                Tile tile = ((GameObject)(Instantiate(TilePrefab, new Vector2(i - Mathf.Floor(mapNumberOfColumns / 2), -j + Mathf.Floor(mapNumberOfRows / 2)), Quaternion.Euler(new Vector3())))).GetComponent<Tile>();
                tile.gridPosition = new Vector2(i, j);

                // Add impassible terrain
                if(tile.gridPosition == new Vector2(3,1))
                    tile.impassible = true;
                //----------------------------

                row.Add(tile);
            }
            map.Add(row);
        }
    }
    */
}