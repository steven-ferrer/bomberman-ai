using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;

public class AStar : MonoBehaviour {

	public GridScript grid;

  public void FindPath(PathRequest request,Action<PathResult> callback)
  {
      Node[] waypoints = PathFinding(request.pathStart, request.pathEnd);
      bool pathSuccess = waypoints.Length > 0;
      callback(new PathResult(waypoints, pathSuccess, request.callback));
  }

  public Node[] PathFinding(Node startNode, Node targetNode)
  {
      Node[] waypoints = new Node[0];
      bool pathSuccess = false;

      if ((startNode.walkable && targetNode.walkable) || (!startNode.walkable && targetNode.walkable))
      {
          Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
          HashSet<Node> closedSet = new HashSet<Node>();
          openSet.Add(startNode);

          while (openSet.Count > 0)
          {
              Node currentNode = openSet.RemoveFirst();
              closedSet.Add(currentNode);

              if (currentNode == targetNode)
              {
                  pathSuccess = true;
                  break;
              }

              foreach (Node neighbour in grid.GetNeighbours(currentNode))
              {
                  if (closedSet.Contains(neighbour))
                  {
                      continue;
                  }
                  int newMovementCostToNeighbour = currentNode.gCost + GetManhattanDistance(currentNode, neighbour);
                  if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                  {
                      neighbour.gCost = newMovementCostToNeighbour;
                      neighbour.hCost = GetManhattanDistance(neighbour, targetNode);
                      neighbour.parent = currentNode;

                      if (!openSet.Contains(neighbour))
                          openSet.Add(neighbour);
                      else
                          openSet.UpdateItem(neighbour);
                  }
              }
          }
      }
      if (pathSuccess)
          waypoints = RetracePath(startNode, targetNode);

      return waypoints;
  }

	private Node[] RetracePath(Node startNode,Node endNode){
		List<Node> path = new List<Node> ();
		Node currentNode = endNode;

		while (currentNode != startNode) {
			path.Add (currentNode);
			currentNode = currentNode.parent;
		}
		path.Reverse();
    return path.ToArray();
	}

	private int GetManhattanDistance(Node nodeA, Node nodeB){
		int dx = Mathf.Abs (nodeA.gridX - nodeB.gridX);
		int dy = Mathf.Abs (nodeA.gridY - nodeB.gridY);
		return dx + dy;
	}

}
