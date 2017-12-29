using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

	public List<Node> GetNeighbours(Node node,int range){
		List<Node> neighbours = new List<Node> ();
		for (int x = -range; x <= range; x++) {
			for (int y = -range; y <= range; y++) {
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


	private List<Node> explosionRange = new List<Node> ();

	public List<Node> GetBombExplosionRange(Node node){
		int range = 3;
		List<Node> nodeList = GetNeighbours (node,range);
		List<Node> left = new List<Node> ();
		List<Node> right = new List<Node> ();
		List<Node> down = new List<Node> ();
		List<Node> up = new List<Node> ();

		foreach(Node n in nodeList) {
			if (n.gridX == node.gridX) {
				if (n.gridY < node.gridY) {
					left.Add (n);
				} else {
					right.Add (n);
				}
			}
			else if (n.gridY == node.gridY) {
				if (n.gridX < node.gridX) {
					up.Add (n);
				} else {
					down.Add (n);
				}
			}
		}

		setExplosionRange (up, true);
		setExplosionRange (down, false);
		setExplosionRange (left, true);	
		setExplosionRange (right, false);	

		explosionRange.RemoveAll (s => s.walkable == false);

		return explosionRange;
	}

	private void setExplosionRange(List<Node> node,bool isReverse){
		if (isReverse)
			node.Reverse ();
		bool isWalk = false;
		for (int x = 0; x < node.Count; x++) {
			if (isWalk)
				node [x].walkable = false;
			if (!node [x].walkable)
				isWalk = true;

			explosionRange.Add (node [x]);
		}
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
