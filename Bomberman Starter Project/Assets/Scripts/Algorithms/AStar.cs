using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;

public class AStar : MonoBehaviour {

	GridScript grid;

	void Awake(){
		grid = GetComponent<GridScript> ();
	}

	public void FindPath(PathRequest request,Action<PathResult> callback){
		Stopwatch sw = new Stopwatch ();
		sw.Start ();

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

		Node startNode = grid.NodeFromWorldPoint (request.pathStart);
		Node targetNode = grid.NodeFromWorldPoint (request.pathEnd);

		if (startNode.walkable && targetNode.walkable) {
			Heap<Node> openSet = new Heap<Node> (grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node> ();
			openSet.Add (startNode);

			while (openSet.Count > 0) {
				Node currentNode = openSet.RemoveFirst (); 
				closedSet.Add (currentNode);

				if (currentNode == targetNode) {
					sw.Stop ();
					print ("Path found: " + sw.ElapsedMilliseconds + " ms");
					pathSuccess = true;
					break;
				}

				foreach (Node neighbour in grid.GetNeighbours(currentNode,1)) {
					if (closedSet.Contains (neighbour)) {
						continue;
					}

					int newMovementCostToNeighbour = currentNode.gCost + GetManhattanDistance (currentNode, neighbour);
					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains (neighbour)) {
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.hCost = GetManhattanDistance (neighbour, targetNode);
						neighbour.parent = currentNode;

						if (!openSet.Contains (neighbour))
							openSet.Add (neighbour);
						else
							openSet.UpdateItem (neighbour);
					}
				}
			}
		}
		if (pathSuccess) {
			waypoints = RetracePath (startNode, targetNode);
			pathSuccess = waypoints.Length > 0;
		}
		callback(new PathResult(waypoints,pathSuccess,request.callback));
	}

	Vector3[] RetracePath(Node startNode,Node endNode){
		List<Node> path = new List<Node> ();
		Node currentNode = endNode;

		while (currentNode != startNode) {
			path.Add (currentNode);
			currentNode = currentNode.parent;
		}

		path.Reverse();
		List<Vector3> waypoints = new List<Vector3> ();
		for (int i = 0; i < path.Count; i++) {
			//instead of floor 0 y axis, the y axis of Agent is 1
			path[i].worldPosition.y = 1.0f;
			waypoints.Add (path[i].worldPosition);
		}

		return waypoints.ToArray();
	}

	int GetManhattanDistance(Node nodeA, Node nodeB){
		int dx = Mathf.Abs (nodeA.gridX - nodeB.gridX);
		int dy = Mathf.Abs (nodeA.gridY - nodeB.gridY);
		return dx + dy;
	}

}
