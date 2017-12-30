using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridScript : MonoBehaviour {
	
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRaduis;

	Node[,] grid;
	float nodeDiameter;
	int gridSizeX,gridSizeY;

	void Awake(){
		nodeDiameter = nodeRaduis * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
	}

	public int MaxSize{
		get{
			return gridSizeX * gridSizeY; 
		}
	}

	public void CreateGrid(){
		grid = new Node[gridSizeX,gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++) {
			for (int y = 0; y < gridSizeY; y++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRaduis) + Vector3.forward * (y * nodeDiameter + nodeRaduis);
				bool walkable = !(Physics.CheckSphere (worldPoint, nodeRaduis,unwalkableMask));
				GameObject go = GetObjectByPosition (new Vector3 (worldPoint.x, 1, worldPoint.z),"Destructible");
				bool des = (go == null) ? false : true;
				grid [x, y] = new Node (walkable,des, worldPoint,x,y);
			}
		}
	}

	public List<Node> GetNeighbours(Node node){
		List<Node> neighbours = new List<Node> ();
		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0) 
					continue;
				
				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					//Update Neighbours only for left,right,up,down side (Manhattan Distance)
					if ((x == 0 && (y < 0 || y > 0)) || (y == 0 && (x < 0 || x > 0))) {
						neighbours.Add (grid [checkX, checkY]);
					}
				}
			}
		}
		return neighbours;
	}

	public List<Node> GetBombExplosionRange(Node bombPosition){
		int range = 3;
		List<Node> explosionRange = new List<Node> ();

		for (int x = -range; x <= range; x++) {
			for (int y = -range; y <= range; y++) {
				if (x == 0 && y == 0)
					continue;

				int checkX = bombPosition.gridX + x;
				int checkY = bombPosition.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					if ((x == 0 && (y < 0 || y > 0)) || (y == 0 && (x < 0 || x > 0))) {
						if (grid [checkX, checkY].walkable)
							explosionRange.Add (grid [checkX, checkY]);
					}
				}
			}
		}
			
		return explosionRange;
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
		Gizmos.DrawWireCube (transform.position, new Vector3 (gridWorldSize.x, 1, gridWorldSize.y));
		if (grid != null) {
			foreach (Node n in grid) {
				Gizmos.color = (n.walkable) ? Color.white : Color.red;
				if (n.walkable == false && n.destructible == true)
					Gizmos.color = Color.blue;
				Gizmos.DrawCube (n.worldPosition, Vector3.one * (nodeDiameter - .1f));
			}
		}
	}

}
