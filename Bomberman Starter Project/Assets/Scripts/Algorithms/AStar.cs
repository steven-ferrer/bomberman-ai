using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;

public class AStar : MonoBehaviour {

	public GridScript grid;

	public void FindPath(PathRequest request,Action<PathResult> callback){
		Stopwatch sw = new Stopwatch ();
		sw.Start ();

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

		Node startNode = request.pathStart;
		Node targetNode = request.pathEnd;

		if ((startNode.walkable && targetNode.walkable) || (!startNode.walkable && targetNode.walkable)) {
			Heap<Node> openSet = new Heap<Node> (grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node> ();
			openSet.Add (startNode);

			while (openSet.Count > 0) {
				Node currentNode = openSet.RemoveFirst (); 
				closedSet.Add (currentNode);

				if (currentNode == targetNode) {
					sw.Stop ();
					//print ("Path found: " + sw.ElapsedMilliseconds + " ms");
					pathSuccess = true;
					break;
				}

				foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
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
		
	public void FindShortestPath(ShortestPathRequest request,Action<ShortestPathResult> callback){
		Vector3[] waypoints = new Vector3[0];
		int minWaypoint = -1;
		Node shortestPath = null;

		Node startNode = request.startNode;

		foreach (Node endNode in request.endNodes) {
			if (startNode.walkable && endNode.walkable) {
				Heap<Node> openSet = new Heap<Node> (grid.MaxSize);
				HashSet<Node> closedSet = new HashSet<Node> ();
				openSet.Add (startNode);

				while (openSet.Count > 0) {
					Node currentNode = openSet.RemoveFirst (); 
					closedSet.Add (currentNode);

					if (currentNode == endNode) {
						waypoints = RetracePath (startNode, endNode);

						if (shortestPath == null) {
							shortestPath = endNode;
							minWaypoint = waypoints.Length;
						}
						else {
							if (waypoints.Length < minWaypoint) {
								minWaypoint = waypoints.Length;
								shortestPath = endNode;
							}
						}
					}

					foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
						if (closedSet.Contains (neighbour)) {
							continue;
						}

						int newMovementCostToNeighbour = currentNode.gCost + GetManhattanDistance (currentNode, neighbour);
						if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains (neighbour)) {
							neighbour.gCost = newMovementCostToNeighbour;
							neighbour.hCost = GetManhattanDistance (neighbour, endNode);
							neighbour.parent = currentNode;

							if (!openSet.Contains (neighbour))
								openSet.Add (neighbour);
							else
								openSet.UpdateItem (neighbour);
						}
					}
				}
			}
		}
		callback (new ShortestPathResult (shortestPath, request.callback));
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
