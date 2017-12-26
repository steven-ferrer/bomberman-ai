using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AStar : MonoBehaviour {

	public Transform seeker, target;

	GridScript grid;

	void Awake(){
		grid = GetComponent<GridScript> ();
	}

	void Update(){
		if (Input.GetKey (KeyCode.P)) {
			FindPath (seeker.position, target.position);
		}
	}

	void FindPath(Vector3 startPos, Vector3 targetPos){
		Stopwatch sw = new Stopwatch ();
		sw.Start ();
		Node startNode = grid.NodeFromWorldPoint (startPos);
		Node targetNode = grid.NodeFromWorldPoint (targetPos);
		//Debug.Log ("S => " + "("+startNode.gridX+","+startNode.gridY+")" + " , " + "T => " + "("+targetNode.gridX+","+targetNode.gridY+")");

		Heap<Node> openSet = new Heap<Node> (grid.MaxSize);
		HashSet<Node> closedSet = new HashSet<Node> ();
		openSet.Add (startNode);

		while (openSet.Count > 0) {
			Node currentNode = openSet.RemoveFirst (); 
			closedSet.Add (currentNode);

			if (currentNode == targetNode) {
				sw.Stop ();
				print ("Path found: " + sw.ElapsedMilliseconds + " ms");
				RetracePath (startNode, targetNode);
				return;
			}

			foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
				if (!neighbour.walkable || closedSet.Contains(neighbour) ) {
					continue;
				}

				int newMovementCostToNeighbour = currentNode.gCost + GetManhattanDistance (currentNode, neighbour);
				if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains (neighbour)) {
					neighbour.gCost = newMovementCostToNeighbour;
					neighbour.hCost = GetManhattanDistance (neighbour, targetNode);
					neighbour.parent = currentNode;

					if (!openSet.Contains (neighbour))
						openSet.Add (neighbour);
				}
			}
		}
	}

	void RetracePath(Node startNode,Node endNode){
		List<Node> path = new List<Node> ();
		Node currentNode = endNode;

		while (currentNode != startNode) {
			path.Add (currentNode);
			currentNode = currentNode.parent;
		}
		path.Reverse ();
		grid.path = path;
	}

	int GetManhattanDistance(Node nodeA, Node nodeB){
		int dx = Mathf.Abs (nodeA.gridX - nodeB.gridX);
		int dy = Mathf.Abs (nodeA.gridY - nodeB.gridY);
		return dx + dy;
	}

}
