using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;

public class AI : MonoBehaviour {
	
	public GridScript grid;
	public Bomb bombScript;
	public float speed = 3;

	private Animator animator;
	bool walking = false;	
	bool search = true;

	int targetIndex;
	int countPathFound = -1;

	Vector3[] minWaypoint;

	void Start(){
		animator = transform.Find("PlayerModel").GetComponent<Animator>();
		grid.CreateGrid ();
	}
		
	void Update(){
		if (walking == true) {
			walking = false;
			animator.SetBool ("Walking", walking);	
		}

		Node aiNode = grid.NodeFromWorldPoint (transform.position);
		Search (aiNode);

		if (countPathFound == 0) {
			StopCoroutine ("FollowPath");
			StartCoroutine ("FollowPath");
			countPathFound = -1;
		}
	}
		
	//----------------------------------------SEACHING OBJECTS-----------------------------------------------
	public void Search(Node startNode){
		if (search == true) {
			print ("Searching...");
			List<Node> visitedNodes = GetVisitedNodes (startNode);
			if (visitedNodes.Any (v => v.isBomb == true)) {
				print ("Found Bomb!");
				search = false;
				List<Node> bombs = visitedNodes.Where (b => b.isBomb == true).ToList ();
				AvoidBombs (startNode, bombs, visitedNodes);
			}
		}
	}

	private List<Node> GetVisitedNodes(Node startNode){
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
	//----------------------------------------SEACHING OBJECTS-----------------------------------------------

	//----------------------------------------BEHAVIORS------------------------------------------------------
	private void AvoidBombs(Node aiNode,List<Node> bombs,List<Node> visitedNodes){
		List<Node> rangeOfBombs = new List<Node> (); 

		foreach (Node n in bombs) {
			List<Node> rangeOfBomb = grid.GetNeighbours (n, bombScript.bombRange);
			rangeOfBomb.Add (n);
			rangeOfBombs.AddRange (rangeOfBomb);
		}
		visitedNodes.RemoveAll(x => rangeOfBombs.Contains(x));

		int increasedRange = 1;
		bool doneEvaluate = false;
		List<Node> evaluatedSafeZone = grid.GetNeighbours (aiNode, increasedRange, true);

		//Nothing to hide
		if (visitedNodes.Count < 1) {
			return;
		}

		//Evaluate safe zone and find the shortest path node to hide
		while (doneEvaluate == false) {
			if (increasedRange == 10)
				doneEvaluate = true;
			evaluatedSafeZone.RemoveAll(x => !visitedNodes.Contains(x));
			if (evaluatedSafeZone.Count > 0)
				doneEvaluate = true;
			else {
				evaluatedSafeZone = grid.GetNeighbours (aiNode, ++increasedRange, true, true);
			}
		}

		countPathFound = evaluatedSafeZone.Count;

		//Find path for all safe zone
		foreach(Node n in evaluatedSafeZone)
			PathRequestManager.RequestPath (new PathRequest (transform.position, n.worldPosition, OnPathFound));

	}
	//----------------------------------------BEHAVIORS------------------------------------------------------

	//----------------------------------------PATHFINDING----------------------------------------------------
	public void OnPathFound(Vector3[] newPath,bool pathSuccessful){
		if (pathSuccessful) {
			minWaypoint = newPath;
			if (minWaypoint == null)
				minWaypoint = newPath;
			else {
				if (newPath.Length < minWaypoint.Length)
					minWaypoint = newPath;
			}
			countPathFound--;
		}
	}

	IEnumerator FollowPath(){
		Vector3 currentWaypoint = minWaypoint [0];

		while (true) {
			if (transform.position == currentWaypoint) {
				targetIndex++;
				if (targetIndex >= minWaypoint.Length) {
					search = true;
					yield break;
				}
				currentWaypoint = minWaypoint [targetIndex];
			}
			transform.position = Vector3.MoveTowards (transform.position, currentWaypoint, speed * Time.deltaTime);
			UpdateAnimationMovement (transform.position,currentWaypoint);
			yield return null;
		}
	}

	private void UpdateAnimationMovement(Vector3 currentPos, Vector3 nextPos){
		currentPos.x = Mathf.RoundToInt (currentPos.x);
		currentPos.z = Mathf.RoundToInt (currentPos.z);
		nextPos.x = Mathf.RoundToInt (nextPos.x);
		nextPos.z = Mathf.RoundToInt (nextPos.z);

		if (currentPos == nextPos)
			return;

		if (nextPos.y == 1 && nextPos.z == currentPos.z) {
			if (nextPos.x < currentPos.x) {
				transform.rotation = Quaternion.Euler(0, 270, 0); //Up
				walking = true;
				animator.SetBool("Walking", walking);
			} else{
				transform.rotation = Quaternion.Euler (0, 90, 0); //Down
				walking = true;
				animator.SetBool("Walking", walking);
			}
		} 
		if (nextPos.y == 1 && nextPos.x == currentPos.x) {
			if (nextPos.z > currentPos.z) {
				transform.rotation = Quaternion.Euler (0, 0, 0); //right
				walking = true;
				animator.SetBool("Walking", walking);
			} else {
				transform.rotation = Quaternion.Euler(0, 180, 0); //left
				walking = true;
				animator.SetBool("Walking", walking);
			}
		}
	}

	//----------------------------------------PATHFINDING----------------------------------------------------


	public void OnDrawGizmos(){
//		if (path != null) {
//			for (int i = targetIndex; i < path.Length; i++) {
//				Gizmos.color = Color.black;
//				Gizmos.DrawCube (path [i], Vector3.one);
//			}
//		}
	}

}
