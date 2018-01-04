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

			if (visitedNodes.Any (v => v.isBomb == true)) {
				List<Node> bombs = visitedNodes.Where (b => b.isBomb == true).ToList();
				Debug.Log ("Bomb Found: " +bombs.Count());
				AvoidBombs (startNode, bombs);
				search = false;
			}
		}
	}

	private void AvoidBombs(Node aiNode,List<Node> bombs){
		List<Node> rangeOfBombs = new List<Node> (); 

		foreach (Node n in bombs) {
			List<Node> rangeOfBomb = grid.GetNeighbours (n, bombRange);
			rangeOfBombs.AddRange (rangeOfBomb);
		}
	
//		List<Node> safeZone = GetSearchResult (aiNode);
//		safeZone.RemoveAll(x => rangeOfBomb.Contains(x));

		safeZones = rangeOfBombs;
	}
	

}
