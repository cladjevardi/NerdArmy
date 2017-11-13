using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

	public Vector2 gridPosition = Vector2.zero;
	public int movementCost = 1;
	public List<Tile> neighbors = new List<Tile>();

	GameObject movementOptions;
	List<GameObject> movementOptionsList = new List<GameObject>();
	List<Tile> storedMovement = new List<Tile>();
	List<Tile> tempStoredMovement = new List<Tile>();

	//public float fCost;
	public int hCost;
	public int gCost;
	public Tile parent;

	public bool impassible = false;
	public bool occupiedAlly = false;
	public bool occupiedAI = false;
	public bool moveTile = false;
	public bool attackTile = false;
	public bool lastHighlight = false;

	public bool action = false;
	public bool chargerAction = false;


	Color originalColor;

	// Use this for initialization
	void Start ()
	{
		originalColor = transform.GetComponent<SpriteRenderer>().color;
		generateNeighbors();
		if(impassible)
		{
			transform.GetComponent<SpriteRenderer>().color = Color.gray;
		}
	}

	// Find adjacent tiles
	void generateNeighbors()
	{
		neighbors = new List<Tile>();

		//up
		if(gridPosition.y > 0)
		{
			Vector2 n = new Vector2(gridPosition.x, gridPosition.y - 1);
			neighbors.Add(GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
		}
		//down
		if (gridPosition.y < GameManager.instance.mapNumberOfRows - 1)
		{
			Vector2 n = new Vector2(gridPosition.x, gridPosition.y + 1);
			neighbors.Add(GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
		}
		//left
		if (gridPosition.x > 0)
		{
			Vector2 n = new Vector2(gridPosition.x - 1, gridPosition.y);
			neighbors.Add(GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
			//Debug.Log(n);
		}
		//right
		if (gridPosition.x < GameManager.instance.mapNumberOfColumns - 1)
		{
			Vector2 n = new Vector2(gridPosition.x + 1, gridPosition.y);
			neighbors.Add(GameManager.instance.map[(int)Mathf.Round(n.x)][(int)Mathf.Round(n.y)]);
		}
	}

	public Tile northNeighbor()
	{
		if (gridPosition.y > 0)
			return GameManager.instance.map[(int)Mathf.Round(gridPosition.x)][(int)Mathf.Round(gridPosition.y - 1)];
		else
			return null;
	}
	public Tile southNeighbor()
	{
		if (gridPosition.y < GameManager.instance.mapNumberOfRows - 1)
			return GameManager.instance.map[(int)Mathf.Round(gridPosition.x)][(int)Mathf.Round(gridPosition.y + 1)];
		else
			return null;
	}
	public Tile westNeighbor()
	{
		if (gridPosition.x > 0)
			return GameManager.instance.map[(int)Mathf.Round(gridPosition.x - 1)][(int)Mathf.Round(gridPosition.y)];
		else
			return null;
	}
	public Tile eastNeighbor()
	{
		if (gridPosition.x < GameManager.instance.mapNumberOfColumns - 1)
			return GameManager.instance.map[(int)Mathf.Round(gridPosition.x + 1)][(int)Mathf.Round(gridPosition.y)];
		else
			return null;
	}

	// Update is called once per frame
	void Update()
	{
		// If it's the player's turn
		if (!GameManager.instance.aiTurn)
		{
			// Check if they click on a player
			if (Input.GetMouseButtonDown(0))
			{

				// Selects the character that is clicked on
				int i = 0;
				foreach (Player p in GameManager.instance.players)
				{
					if (p.gridPosition == CastRay().gridPosition && !p.hasGone)
					{
						GameManager.instance.currentPlayerIndex = i;
					}
					i++;
				}


				if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].GetType() == GameManager.instance.userPlayerCheck.GetType() &&
				GameManager.instance.players[GameManager.instance.currentPlayerIndex].gridPosition == CastRay().gridPosition)
				{
					//Debug.Log("MC selected");
					GameManager.instance.removeTileHighlights();
					GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking = true;
					GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving = true;
					GameManager.instance.highlightTilesAt(GameManager.instance.players[GameManager.instance.currentPlayerIndex].gridPosition, GameManager.instance.players[GameManager.instance.currentPlayerIndex].movementPerActionPoint, GameManager.instance.players[GameManager.instance.currentPlayerIndex].attackRange);
					GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking = false;
					GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving = false;
					action = true;
				}
				else if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].GetType() == GameManager.instance.chargerPlayerCheck.GetType() &&
					GameManager.instance.players[GameManager.instance.currentPlayerIndex].gridPosition == CastRay().gridPosition)
				{
					//Debug.Log("Charger selected");
					GameManager.instance.removeTileHighlights();
					GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking = true;
					GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving = true;
					GameManager.instance.cardinalDirectionsHighlightTilesAt(GameManager.instance.players[GameManager.instance.currentPlayerIndex].gridPosition, GameManager.instance.players[GameManager.instance.currentPlayerIndex].movementPerActionPoint, GameManager.instance.players[GameManager.instance.currentPlayerIndex].attackRange);
					GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking = false;
					GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving = false;
					chargerAction = true;
				}
			}

			// While they hold, draw a path
			if (action || chargerAction)
			{
				// Store tiles that the player hovers over
				if (storedMovement.Count == 0)
				{
					storedMovement.Add(CastRay());
				}
				else
				{
					bool tileExists = false;

					foreach(Tile t in storedMovement)
					{
						if(GetDistance(storedMovement[0], CastRay()) < storedMovement.Count + 1)
						{

						}
						// If the tile already exists in the list
						if (t.gridPosition == CastRay().gridPosition)
						{
							tileExists = true;
						}
					}
					// Store tile if it is not already in the list and is not impassible
					if(!tileExists && !CastRay().impassible)
					{
						storedMovement.Add(CastRay());
					}
					tileExists = false;
				}

				if (storedMovement.Count > 1)
				{
					tempStoredMovement.Clear();
					tempStoredMovement = TilePathFinder.FindPath(storedMovement[1], storedMovement[storedMovement.Count - 1]);
				}

				// prevent endless objects from spawning while they hold
				foreach (GameObject m in movementOptionsList)
				{
					Destroy(m);
				}

				List<Tile> path = (TilePathFinder.FindPath(GameManager.instance.map[(int)GameManager.instance.players[GameManager.instance.currentPlayerIndex].gridPosition.x][(int)GameManager.instance.players[GameManager.instance.currentPlayerIndex].gridPosition.y], CastRay()));
				//Debug.Log(path.Count);
				//Debug.Log(storedMovement.Count);
				//Debug.Log(path.Count);
				if (true)//path.Count + 1 < storedMovement.Count)
				{
					foreach (Tile t in path)
					{
						t.lastHighlight = false;
						// for debug purposes highlight every tile
						movementOptions = ((GameObject)(Instantiate(GameManager.instance.MovementOptionsPrefab, new Vector2(t.gridPosition.x - (GameManager.instance.mapNumberOfColumns / 2),
								  (GameManager.instance.mapNumberOfRows / 2) - t.gridPosition.y), Quaternion.Euler(new Vector3()))));

						movementOptionsList.Add(movementOptions);
					}
				}
				else
				{
					foreach (Tile t in storedMovement)
					{
						t.lastHighlight = false;
						// for debug purposes highlight every tile
						movementOptions = ((GameObject)(Instantiate(GameManager.instance.MovementOptionsPrefab, new Vector2(t.gridPosition.x - (GameManager.instance.mapNumberOfColumns / 2),
								  (GameManager.instance.mapNumberOfRows / 2) - t.gridPosition.y), Quaternion.Euler(new Vector3()))));

						movementOptionsList.Add(movementOptions);
					}
				}
				/*
				foreach (Tile t in path)
				{
					t.lastHighlight = false;
					//Debug.Log(t.gridPosition);
					// for debug purposes highlight every tile
					movementOptions = ((GameObject)(Instantiate(GameManager.instance.MovementOptionsPrefab, new Vector2(t.gridPosition.x - (GameManager.instance.mapNumberOfColumns / 2),
							  (GameManager.instance.mapNumberOfRows / 2) - t.gridPosition.y), Quaternion.Euler(new Vector3()))));

					movementOptionsList.Add(movementOptions);
					// Highlights with movement option for tiles that you can move to
					
					if(t.moveTile)
					{
						movementOptions = ((GameObject)(Instantiate(GameManager.instance.MovementOptionsPrefab, new Vector2(t.gridPosition.x - (GameManager.instance.mapNumberOfColumns / 2),
							   (GameManager.instance.mapNumberOfRows / 2) - t.gridPosition.y), Quaternion.Euler(new Vector3()))));

						movementOptionsList.Add(movementOptions);
					}
					else if(t.attackTile && t.occupiedAI)
					{
						movementOptions = ((GameObject)(Instantiate(GameManager.instance.MovementOptionsPrefab, new Vector2(t.gridPosition.x - (GameManager.instance.mapNumberOfColumns / 2),
							   (GameManager.instance.mapNumberOfRows / 2) - t.gridPosition.y), Quaternion.Euler(new Vector3()))));
						movementOptions.GetComponent<SpriteRenderer>().color = Color.red;
						movementOptions.GetComponent<SpriteRenderer>().sortingOrder = 0;
						movementOptionsList.Add(movementOptions);
					}
					
				}
				*/

				// When they release, check where it is released
				if (Input.GetMouseButtonUp(0))
				{
					storedMovement.RemoveAt(0);
					if (action)
					{
						if(true)//path.Count < storedMovement.Count)
							GameManager.instance.actionWithCurrentPlayer(CastRay());
						else
							GameManager.instance.choiceActionWithCurrentPlayer(storedMovement);


						if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].actionPoints == 0)
							GameManager.instance.players[GameManager.instance.currentPlayerIndex].hasGone = true;
					}
					else if (chargerAction)
					{
						GameManager.instance.actionWithCurrentCharger(CastRay());
						if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].actionPoints == 0)
							GameManager.instance.players[GameManager.instance.currentPlayerIndex].hasGone = true;
					}
					// Destroy the movement options once the movement is made
					foreach (GameObject m in movementOptionsList)
					{
						Destroy(m);
					}
					storedMovement.Clear();

					GameManager.instance.removeTileHighlights();
					action = false;
					chargerAction = false;
				}
			}
		}
	}

	Tile CastRay()
	{
		//Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition), Vector2.zero;
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
		if (hit.collider != null)
		{
			return hit.transform.gameObject.GetComponent<Tile>();
			//Debug.Log("Target Position: " + hit.collider.gameObject.GetComponent<Tile>().gridPosition);
		}
		else
			return null;
	}

	private int GetDistance(Tile tileA, Tile tileB)
	{
		int dstX = Mathf.Abs((int)tileA.gridPosition.x - (int)tileB.gridPosition.x);
		int dstY = Mathf.Abs((int)tileA.gridPosition.y - (int)tileB.gridPosition.y);

		return dstY + dstX;
	}

	public int fCost()
	{
		return gCost + hCost;
	}

	// The cursor enters a tile
	void OnMouseEnter()
	{
		//---------------------------------DEBUG-----------------------------------------
		//Debug.Log("AI occupied: " + occupiedAI + " Ally occupied: " + occupiedAlly);
		
		//Debug.Log("gCost: " + gCost + "\nhCost: " + hCost);
		//Debug.Log("Tile Grid Position: " + gridPosition);
		//Debug.Log("Player Grid Pos" + GameManager.instance.players[GameManager.instance.currentPlayerIndex].gridPosition);
		//Debug.Log(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y].occupiedAI);
		//-------------------------------------------------------------------------------
	}

	// The cursor leaves a tile
	void OnMouseExit()
	{
		//transform.GetComponent<SpriteRenderer>().color = originalColor;
	}
	
}
