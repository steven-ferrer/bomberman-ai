using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

public class GridScript : MonoBehaviour {
	
	public LayerMask unwalkableMask;
	public LayerMask playerCollisionMask;
	public Vector2 gridWorldSize;
	public float nodeRaduis;

	Node[,] grid;
	float nodeDiameter;
	int gridSizeX,gridSizeY;

	private bool isCreated = false;

	void Awake(){
		nodeDiameter = nodeRaduis * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
	}

	void Update(){
		if (isCreated == true) {
			UpdateWalls ();
			UpdateBombs ();
		}
	}

	public int MaxSize{
		get{
			return gridSizeX * gridSizeY; 
		}
	}

	public bool IsCreated(){
		return isCreated;
	}

	public void CreateGrid(){
		if (isCreated == false) {
			Stopwatch sw = new Stopwatch ();
			sw.Start ();

			grid = new Node[gridSizeX, gridSizeY];
			Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
			int index = 0;
			for (int x = 0; x < gridSizeX; x++) {
				for (int y = 0; y < gridSizeY; y++) {
					Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRaduis) + Vector3.forward * (y * nodeDiameter + nodeRaduis);
					bool walkable = !(Physics.CheckSphere (worldPoint, nodeRaduis, unwalkableMask));
					GameObject goDes = GetObjectByPosition (new Vector3 (worldPoint.x, 1, worldPoint.z), "Destructible");

					bool isDestructibleWall = (goDes == null) ? false : true;

					Node n = new Node (walkable, isDestructibleWall, worldPoint, x, y);
					n.HeapIndex = index++;
					grid [x, y] = n;
				}
			}
			isCreated = true;
			print ("Grid was successfully Created at " + sw.ElapsedMilliseconds+" ms");
		}else
			print ("Grid is already Created");
	}

	private void UpdateWalls(){
		if (Bomb.wallTobeDestroy.Count > 0) {
			foreach (Vector3 pos in Bomb.wallTobeDestroy) {
				Node wall = NodeFromWorldPoint (pos);
				grid [wall.gridX, wall.gridY].walkable = true;
				grid [wall.gridX, wall.gridY].destructible = false;
			}
			Bomb.wallTobeDestroy.Clear ();
		}
	}

	private void UpdateBombs(){
		if (Bomb.droppedBombs.Count > 0) {
			foreach (Vector3 pos in Bomb.droppedBombs) {
				Node wall = NodeFromWorldPoint (pos);
				grid [wall.gridX, wall.gridY].setBomb (true);
			}
			Bomb.droppedBombs.Clear ();
		}

		if (Bomb.explodedBombs.Count > 0) {
			foreach (Vector3 pos in Bomb.explodedBombs) {
				Node wall = NodeFromWorldPoint (pos);
				grid [wall.gridX, wall.gridY].setBomb (false);
			}
			Bomb.explodedBombs.Clear ();
		}
	}

	public void UpdateAgentMoves(Vector3 current, Vector3 next){
		Node currentNode = NodeFromWorldPoint (Utility.RoundToInt(current));
		Node nextNode = NodeFromWorldPoint (Utility.RoundToInt(next));

		if (next == Vector3.zero) {
			grid [currentNode.gridX, currentNode.gridY].setAgent (true);
		} else {
			grid [currentNode.gridX, currentNode.gridY].setAgent (true);
			grid [nextNode.gridX, nextNode.gridY].setAgent (false);
		}
	}

	public static void DestroyDestructible(Vector3 position){
		print (position.ToString ());
	}

	public List<Node> GetNeighbours(Node node){
		return GetNeighbours (node, 1, false);
	}

	public List<Node> GetNeighbours(Node node, int range){
		return GetNeighbours (node, range, false);
	}

	public List<Node> GetNeighbours(Node node,int range,bool diagonal){
		List<Node> neighbours = new List<Node> ();

		for (int x = -range; x <= range; x++) {
			for (int y = -range; y <= range; y++) {
				if (x == 0 && y == 0)
						continue;
				
				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					if (diagonal == false) {
						if ((x == 0 && (y < 0 || y > 0)) || (y == 0 && (x < 0 || x > 0))) {
							neighbours.Add (grid [checkX, checkY]);
						}
					}else
						neighbours.Add (grid [checkX, checkY]);
				}
			}
		} 
			
		//Set directions
		if (diagonal == false) {
			List<Node>[] directions = new List<Node>[4]; //left,right,up,down
			for (int x = 0; x < directions.GetLength (0); x++)
				directions [x] = new List<Node> ();
		
			foreach (Node n in neighbours) {
				if (n.gridX == node.gridX) {
					if (n.gridY < node.gridY) {
						directions [0].Add (n); //left
					} else {
						directions [1].Add (n); //right ,reverse
					}
				} else if (n.gridY == node.gridY) {
					if (n.gridX < node.gridX) {
						directions [2].Add (n); //up
					} else {
						directions [3].Add (n); //down , reverse
					}
				}
			}

			neighbours.Clear ();

			//Eliminate walls (indestructible, destructible, and outerwall)
			for (int x = 0; x < directions.GetLength (0); x++) {
				if ((x % 2) == 0)
					directions [x].Reverse ();

				bool isWalk = false;
				for (int y = 0; y < directions [x].Count; y++) {
				
					if (isWalk)
						directions [x] [y].walkable = false;
					if (!directions [x] [y].walkable)
						isWalk = true;

					neighbours.Add (directions [x] [y]);
				}
			}

			neighbours.RemoveAll (s => s.walkable == false);
		}
		return neighbours;
	}
		
	public Node NodeFromWorldPoint(Vector3 worldPosition){
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01 (percentX);
		percentY = Mathf.Clamp01 (percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

		return grid [x, y];
	}

	private GameObject GetObjectByPosition(Vector3 position,string tag){
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag (tag);
		foreach (GameObject go in gameObjects) {
			if (go.transform.position == position) {
				return go;
			}
		}
		return null;
	}

	void OnDrawGizmos(){
		Gizmos.DrawWireCube (transform.position, new Vector3 (gridWorldSize.x, 2, gridWorldSize.y));
		if (grid != null) {
			foreach (Node n in grid) {
				Gizmos.color = (n.walkable) ? Color.white : Color.red;
				if (n.walkable == false && n.destructible == true)
					Gizmos.color = Color.blue;
				if (n.isBomb == true)
					Gizmos.color = Color.grey;
				if (n.isAgent == true)
					Gizmos.color = Color.green;
				
				Gizmos.DrawCube (n.worldPosition, Vector3.one * (nodeDiameter - .1f) );
			}
		}
	}
}
