using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;

public class DepthFirstSearch : MonoBehaviour {

	GridScript grid;
	public Bomb bombScript;

	private int bombRange;
	Node bombFound;
	public List<Node> safeZones;

	public bool search = true;

	void Awake(){
		grid = GetComponent<GridScript> ();
		bombRange = bombScript.bombRange;
	}

	public void Search(Node startNode){
		if (search == true) {
			List<Node> visitedNodes = new List<Node> ();
			Stack<Node> stack = new Stack<Node> ();

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
			safeZones = visitedNodes;
		}
	}

	private void AvoidBombs(Node aiNode,Node bomb){
		//Debug.Log ("AI: (" + aiNode.gridX + "," + aiNode.gridY + " => bomb: (" + bomb.gridX + "," + bomb.gridY); 
		List<Node> rangeOfBomb = grid.GetNeighbours (bomb, bombRange,false);
		rangeOfBomb.Add (aiNode);
		//Debug.Log ("range bomb: " + rangeOfBomb.Count);
	
//		List<Node> safeZone = GetSearchResult (aiNode);
//		safeZone.RemoveAll(x => rangeOfBomb.Contains(x));
//
//		safeZones = safeZone;
	}
	

}
