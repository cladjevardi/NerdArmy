using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    // Some variables
    public Vector2 gridPosition = Vector2.zero;

    [HideInInspector] public Vector3 moveDestination;
    public float moveSpeed = 10.0f;

    public int movementPerActionPoint;
    public int attackRange;

    [HideInInspector] public bool moving = false;
    [HideInInspector] public bool attacking = false;

    public string playerName = "Dude";
    public float HP;

    //public float attackChance = 0.75f;
    public float defenseReduction;
    public float damageBase;
    //public float damageRollSides = 6; // d6

    public int actionPoints;
    [HideInInspector] public bool hasGone = false;

    // Movement animation
    [HideInInspector] public List<Vector3> positionQueue = new List<Vector3>();
    [HideInInspector] public List<Vector3> positionQueueAI = new List<Vector3>();

    // Health bars
    float barDisplay = 0;
    Vector2 barPos = new Vector2(20f, 40f);
    Vector2 barSize = new Vector2(60f, 20f);
    Texture2D progressBarEmpty;
    Texture2D progressBarFull;

    void Awake()
    {
        moveDestination = transform.position;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    public virtual void TurnUpdate()
    {
        if(!GameManager.instance.coroutine)
        {
            if (!GameManager.instance.aiTurn)
            {
                if (actionPoints <= 0)
                {
                    actionPoints = 1;
                    moving = false;
                    attacking = false;
                    hasGone = true;
                    GameManager.instance.nextTurn();
                }
            }
            else
            {
                if (actionPoints <= 0)
                {
                    actionPoints = 1;
                    moving = false;
                    attacking = false;
                    
                    foreach(Player p in GameManager.instance.players)
                    {
                        p.hasGone = false;
                    }

                    GameManager.instance.nextAITurn();
                }
            }
        }
    }

    public virtual void TurnOnGUI()
    {

    }

    public void OnGUI()
    {
        /*
        barDisplay = HP;
        barPos = new Vector2(20f, 40f);
        barSize = new Vector2(60f, 20f);

        // draw the background:
        GUI.BeginGroup(new Rect(barPos.x, barPos.y, barSize.x, barSize.y));
        GUI.Box(new Rect(barPos.x, barPos.y, barSize.x, barSize.y), progressBarEmpty);
        // draw the filled-in part:
        GUI.BeginGroup(new Rect(0, 0, barSize.x * barDisplay, barSize.y));
        GUI.Box(new Rect(0, 0, barSize.x, barSize.y), progressBarFull);
        GUI.EndGroup();
        GUI.EndGroup();
        */

        // Display HP
        if (HP > 0)
        {
            Vector3 location = Camera.main.WorldToScreenPoint(transform.position) + Vector3.up * 100;
            GUI.color = Color.black;
            GUI.Label(new Rect(location.x - 2.5f, Screen.height - location.y, 30, 20), HP.ToString());
        }
    }

    /*
    void OnMouseDown()
    {
        // Set the current player to mouse select
        int index = GameManager.instance.playersGridPosition.IndexOf(gridPosition);
        GameManager.instance.currentPlayerIndex = index;
        //Debug.Log("Index of current player grid position: " + index);
    }
    */
}
