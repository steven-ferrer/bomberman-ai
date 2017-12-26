using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridScript : MonoBehaviour {
	
	public Transform player;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRaduis;

	Node[,] grid;
	float nodeDiameter;
	int gridSizeX,gridSizeY;

	void Start(){
		nodeDiameter = nodeRaduis * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
		CreateGrid ();
	}

	public int MaxSize{
		get{
			return gridSizeX * gridSizeY; 
		}
	}

	void CreateGrid(){
		grid = new Node[gridSizeX,gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++) {
			for (int y = 0; y < gridSizeY; y++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRaduis) + Vector3.forward * (y * nodeDiameter + nodeRaduis);
				bool walkable = !(Physics.CheckSphere (worldPoint, nodeRaduis,unwalkableMask));
				GameObject go = GetDestructibleObjectByPosition (new Vector3 (worldPoint.x, 1, worldPoint.z));
				bool des = (go == null) ? false : true;
				grid [x, y] = new Node (walkable,des, worldPoint,x,y);
			}
		}
	}

	public List<Node> path;
	void OnDrawGizmos(){
		Gizmos.DrawWireCube (transform.position, new Vector3 (gridWorldSize.x, 1, gridWorldSize.y));

		if (grid != null) {
			Node playerNode = NodeFromWorldPoint (player.position);
			foreach (Node n in grid) {
				Gizmos.color = (n.walkable) ? Color.white : Color.red;
				if (n.walkable == false && n.destructible == true) {
					Gizmos.color = Color.blue;
				}
				if (n == playerNode) {
					Gizmos.color = Color.cyan;
				}
				if (path != null) {
					if (path.Contains (n)) {
						Gizmos.color = Color.black;
						//Debug.Log(n.gridX + "," + n.gridY + " = " + n.gCost + "," + n.hCost + ","+n.fCost); 
					}
				}
				Gizmos.DrawCube (n.worldPosition, Vector3.one * (nodeDiameter - .1f));
			}
		}
	}

	public List<Node> GetNeighbours(Node node){
		List<Node> neighbours = new List<Node> ();
		//Debug.Log ("(" + node.gridX + "," + node.gridY + ")");
		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0) 
					continue;

				//Update Neighbours only for left,right,up,down side (Manhattan Distance)
				if ((x == -1 && y == -1) || (x == -1 && y == 1) || (x == 1 && y == -1) || (x == 1 && y == 1))
					continue;
				
				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add (grid [checkX, checkY]);
					//Debug.Log ("("+checkX+","+checkY+") => " +  "("+grid [checkX, checkY].gridX+","+grid [checkX, checkY].gridY+")");
				}
			}
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

	private GameObject GetDestructibleObjectByPosition(Vector3 position){
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag ("Destructible");
		foreach (GameObject go in gameObjects) {
			if (go.transform.position == position) {
				return go;
			}
		}
		return null;
	}
}
