using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserPlayer : Player {

    //public float moveSpeed = 10.0f;
    Color originalColor;

    // Use this for initialization
    void Start () {

        originalColor = transform.GetComponent<SpriteRenderer>().color;

    }
	
	// Update is called once per frame
	public override void Update () {
        // is the current player referenced in the game manager
        
        if (GameManager.instance.players[GameManager.instance.currentPlayerIndex] == this)
        {
            transform.GetComponent<SpriteRenderer>().color = Color.green;
        }
        else if(hasGone)
        {
            transform.GetComponent<SpriteRenderer>().color = Color.gray;
        }
        else
        {
            transform.GetComponent<SpriteRenderer>().color = originalColor;
        }
        

        // Player is dead
        if(HP <= 0)
        {
            // Player rotates on death
            transform.rotation = Quaternion.Euler(new Vector3(180f, 0, 0));
            transform.GetComponent<SpriteRenderer>().sortingOrder = 0;
            // Turns red   
            transform.GetComponent<SpriteRenderer>().color = Color.red;
        }
        
    }

    public override void TurnUpdate()
    {
        // Highlight available tiles to move to
        if (positionQueue.Count > 0)
        {
            if (Vector3.Distance(positionQueue[0], transform.position) > 0.1f)
            {
                transform.position += (positionQueue[0] - transform.position).normalized * moveSpeed * Time.deltaTime;

                // Once they reach the destination
                if (Vector3.Distance(positionQueue[0], transform.position) <= 0.1f)
                {
                    transform.position = positionQueue[0];
                    positionQueue.RemoveAt(0);
                    if(positionQueue.Count == 0)
                    {
                        actionPoints--;
                    }
                }
            }
        }
        base.TurnUpdate();
    }

    public override void TurnOnGUI()
    {
        float buttonHeight = 50;
        float buttonWidth = 150;

        Rect buttonRect = new Rect(0, Screen.height - buttonHeight * 4, buttonWidth, buttonHeight);

        // Action button
        // Move button
        /*
        if (GUI.Button(buttonRect, "Action"))
        {
            GameManager.instance.removeTileHighlights();
            attacking = true;
            GameManager.instance.highlightTilesAt(gridPosition, Color.red, movementPerActionPoint + attackRange);
            attacking = false;
            moving = true;
            GameManager.instance.highlightTilesAt(gridPosition, Color.blue, movementPerActionPoint);
            moving = false;
        }
        */
        /*
        // Move button
        if (GUI.Button(buttonRect, "Move"))
        {
            if(!moving)
            {
                GameManager.instance.removeTileHighlights();
                moving = true;
                attacking = false;
                GameManager.instance.highlightTilesAt(gridPosition, Color.blue, movementPerActionPoint);
            }
            else
            {
                moving = false;
                attacking = false;
                GameManager.instance.removeTileHighlights();
            }
        }


        // Attack button
        buttonRect = new Rect(0, Screen.height - buttonHeight * 2, buttonWidth, buttonHeight); // 2 because of # button
        if (GUI.Button(buttonRect, "Attack"))
        {
            if (!attacking)
            {
                GameManager.instance.removeTileHighlights();
                moving = false;
                attacking = true;
                GameManager.instance.highlightTilesAt(gridPosition, Color.red, attackRange);
            }
            else
            {
                moving = false;
                attacking = false;
                GameManager.instance.removeTileHighlights();
            }
        }
        */

        // End turn button
        buttonRect = new Rect(0, Screen.height - buttonHeight * 1, buttonWidth, buttonHeight); // 1 because of # button
        if (GUI.Button(buttonRect, "End Turn"))
        {
            GameManager.instance.removeTileHighlights();
            actionPoints = 1;
            moving = false;
            attacking = false;
            hasGone = true;
            GameManager.instance.nextTurn();
        }

        // Restart Mission button
        buttonRect = new Rect(Screen.width - buttonWidth, Screen.height - buttonHeight, buttonWidth, buttonHeight);
        if (GUI.Button(buttonRect, "Restart Mission"))
        {
            GameManager.instance.removeTileHighlights();
            actionPoints = 1;
            moving = false;
            attacking = false;
            hasGone = false;
            GameManager.instance.restartMission();
        }

        base.TurnOnGUI();
    }
}
