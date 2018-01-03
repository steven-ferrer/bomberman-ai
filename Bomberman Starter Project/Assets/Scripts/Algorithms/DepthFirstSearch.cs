using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthFirstSearch : MonoBehaviour {

	GridScript grid;

	void Awake(){
		grid = GetComponent<GridScript> ();
	}

	public List<Node> Search(Node startNode){
		List<Node> visitedNodes = new List<Node> ();
		Stack<Node> stack = new Stack<Node> ();

		stack.Push (startNode); 

		while (stack.Count > 0) {
			Node node = stack.Pop ();

			if (!visitedNodes.Contains (node)) {
				visitedNodes.Add (node);

				List<Node> neighbours = grid.GetNeighbours (node);
				foreach (Node n in neighbours) {
					if(n.isBomb == true)
						Debug.Log (n.isBomb + "("+n.timeOfBomb+") => (" + n.gridX+","+n.gridY+")" );
					if (!visitedNodes.Contains (n)) {
						stack.Push (n);
					}
				}
			}
		}
		return visitedNodes;
	}



}
