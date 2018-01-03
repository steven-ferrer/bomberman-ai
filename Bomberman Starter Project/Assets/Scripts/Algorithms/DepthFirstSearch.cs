using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthFirstSearch : MonoBehaviour {

	GridScript grid;

	void Awake(){
		grid = GetComponent<GridScript> ();
	}

	public void Search(Node startNode){
		//Initialize
		HashSet<Node> visitedNodes = new HashSet<Node> ();
		Stack<Node> stack = new Stack<Node> ();

		//Initial node
		stack.Push (startNode); 

		while (stack.Count > 0) {
			Node node = stack.Pop ();

			if (!visitedNodes.Contains (node)) {
				visitedNodes.Add (node);

				List<Node> neighbours = grid.GetNeighbours (node);
				foreach (Node n in neighbours) {
					if (!visitedNodes.Contains (n)) {
						stack.Push (n);
					}
				}
			}
		}


		foreach (Node n in visitedNodes) {
			Debug.Log (n.gridX + "," + n.gridY);
		}
	}




}
