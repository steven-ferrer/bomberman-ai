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

	Vector3[] path;

	List<Node> accesibleTiles;
	List<Node> visual;
	Node aiNode;
	Node shortestPath;

	Agent agent;

	void Awake(){
		animator = transform.Find("PlayerModel").GetComponent<Animator>();
		agent = GetComponent<Agent> ();
	}
		
	void Update(){
		if (walking == true) {
			walking = false;
			animator.SetBool ("Walking", walking);	
		}

		Searching ();
	}

	private void Searching (){
		if (search == true) {
			aiNode = grid.NodeFromWorldPoint (transform.position);
			accesibleTiles = GetAccesibleTiles (aiNode);
			print ("Searching...");
			visual = accesibleTiles;
			if (accesibleTiles.Any (v => v.isBomb == true)) {
				print ("Found bomb!");
				search = false;
			} else if (accesibleTiles.Any (v => v.agentName != null && v.agentName != transform.name)) {
				print ("Found enemy!");
				search = false;
			} else {
				if (Input.GetKey (KeyCode.F)) {
					print ("Destroy Destructible Walls");
					DestroyDestructibleWall (GetNearestDestructibleWall ());
					search = false;
				}
			}
		}
	}

	public void FoundShortestPath(Node shortestPath){
		this.shortestPath = shortestPath;
	}

	private List<Node> GetAccesibleTiles(Node startNode){
		List<Node> visitedNodes = new List<Node> ();
		Stack<Node> stack = new Stack<Node> ();
		stack.Push (startNode); 
		while (stack.Count > 0) {
			Node node = stack.Pop ();

			if (!visitedNodes.Contains (node)) {
				visitedNodes.Add (node);

				List<Node> neighbours = grid.GetNeighbours (node);
				foreach (Node n in neighbours) {
					if (!visitedNodes.Contains (n))
						stack.Push (n);
				}
			}
		}
		return visitedNodes;
	}
		
	private Node GetNearestDestructibleWall(){
		bool doneEvaluate = false;
		int increasedRange = 1;
		List<Node> nearDestructibleWall = grid.GetNeighbours (aiNode, increasedRange, true);

		//Evaluate nearest destructible wall
		while (doneEvaluate == false) {
			nearDestructibleWall.RemoveAll(x => x.destructible == false);
			if (nearDestructibleWall.Count > 0)
				doneEvaluate = true;
			else 
				nearDestructibleWall = grid.GetNeighbours (aiNode, ++increasedRange, true, true);
		}
		return nearDestructibleWall.Last ();
	}

	private void DestroyDestructibleWall(Node destructibleWall){
		List<Node> neighbours = grid.GetNeighbours (destructibleWall);
		List<Node> bombDropPosition = accesibleTiles.Intersect (neighbours).ToList();

		PathRequestManager.ShortestPath (new ShortestPathRequest (aiNode, bombDropPosition, FoundShortestPath));
		PathRequestManager.RequestPath (new PathRequest (aiNode, shortestPath, OnPathFound));
	}

	private void AvoidBombs(){
		List<Node> safeZones = accesibleTiles.Where (p => p.isDropRange == false).ToList();

		int increasedRange = 1;
		bool doneEvaluate = false;
		List<Node> evaluatedSafeZone = grid.GetNeighbours (aiNode, increasedRange, true);

		//Nothing to hide
		if (accesibleTiles.Count < 1)
			return;
		

		//Evaluate safe zone and find the shortest path node to hide
		while (doneEvaluate == false) {
			evaluatedSafeZone.RemoveAll(x => !safeZones.Contains(x));
			if (evaluatedSafeZone.Count > 0)
				doneEvaluate = true;
			else 
				evaluatedSafeZone = grid.GetNeighbours (aiNode, ++increasedRange, true, true);
		}


//		countPathFound = evaluatedSafeZone.Count;
//
//		//Find path for all safe zone
//		foreach(Node n in evaluatedSafeZone)
//			PathRequestManager.RequestPath (new PathRequest (transform.position, n.worldPosition, OnPathFound));
//
	}
		
	public void OnPathFound(Vector3[] newPath,bool pathSuccessful){
		if (pathSuccessful) {
			path = newPath;
			StopCoroutine("FollowPath");
			StartCoroutine ("FollowPath");
		}
	}

	IEnumerator FollowPath(){
		Vector3 currentWaypoint = path [0];
		currentWaypoint.y = 1f;

		while (true) {
			if (transform.position == currentWaypoint) {
				targetIndex++;
				if (targetIndex >= path.Length) {
					agent.DropBomb ();	
					yield break;
				}
				path [targetIndex].y = 1f;
				currentWaypoint = path [targetIndex];
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

	public void OnDrawGizmos(){


		if (visual != null && search == true) {
			foreach (Node n in visual) {
				Gizmos.color = Color.cyan;
				Gizmos.DrawCube (n.worldPosition, Vector3.one * (grid.nodeDiameter - .1f));
			}
		}

		if (path != null) {
			for (int i = targetIndex; i < path.Length; i++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube (path[i], Vector3.one * (grid.nodeDiameter - .1f));
			}
		}

	}

}
