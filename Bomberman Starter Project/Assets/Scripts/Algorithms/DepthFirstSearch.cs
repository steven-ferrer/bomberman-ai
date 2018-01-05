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

	private List<Node> GetSearchResult(Node startNode){
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
		return visitedNodes;
	}


	public void Search(Node startNode){
		if (search == true) {
			List<Node> visitedNodes = GetSearchResult (startNode);

			if (visitedNodes.Any (v => v.isBomb == true)) {
				List<Node> bombs = visitedNodes.Where (b => b.isBomb == true).ToList ();
				AvoidBombs (startNode, bombs,visitedNodes);
				search = false;
			}

		}
	}


	private void AvoidBombs(Node aiNode,List<Node> bombs,List<Node> visitedNodes){
		List<Node> rangeOfBombs = new List<Node> (); 
		rangeOfBombs.Add (aiNode);

		foreach (Node n in bombs) {
			n.walkable = false;
			List<Node> rangeOfBomb = grid.GetNeighbours (n, bombRange);
			rangeOfBomb.Add (n);
			rangeOfBombs.AddRange (rangeOfBomb);
		}

		visitedNodes.RemoveAll(x => rangeOfBombs.Contains(x));
		safeZones = visitedNodes;

		//find path according to waypoints. lowest to highest
		//PathRequestManager.RequestPath (new PathRequest (aiNode.worldPosition, safeZones.First().worldPosition, OnPathFound));
		
	}


	public void OnPathFound(Vector3[] newPath,bool pathSuccessful){
		if (pathSuccessful) {
//			path = newPath;
//			StopCoroutine ("FollowPath");
//			StartCoroutine ("FollowPath");
			Debug.Log ("Path was successfully found");
		} else
			Debug.Log ("Not found");
	}



	

}
