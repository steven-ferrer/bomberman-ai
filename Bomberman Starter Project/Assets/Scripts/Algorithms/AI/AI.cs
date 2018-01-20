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
	int targetIndex;

	Vector3[] path;

	List<Node> accesibleTiles;
	List<Node> visual;
	Node aiNode;
	Node shortestPath;

	bool searching = true;
	bool doneFollowThePath = false;
	bool isPlacingBomb = false;

	Agent agent;

	void Awake(){
		animator = transform.Find("PlayerModel").GetComponent<Animator>();
		agent = GetComponent<Agent> ();
	}
		
	void Start(){
	//	Invoke ("Searching", 2f);
	}

	void Update(){
		if (walking == true) {
			walking = false;
			animator.SetBool ("Walking", walking);	
		}
	}

	public void Searching (){
		accesibleTiles = GetAccesibleTiles ();

		print ("Searching...");
		if (accesibleTiles.Any (v => v.isBomb == true)) {
			print ("Found Bomb!");
		} else if (accesibleTiles.Any (v => v.agentName != null && v.agentName != transform.name)) {
			print ("Found enemy!");
		} else {
			print ("Destroy Mode!");
			Node dropBombPosition = GetWhereToPlaceTheBomb ();
			if (dropBombPosition != null) {
				print ("Place Bomb: " + dropBombPosition.gridX + "," + dropBombPosition.gridY);
				isPlacingBomb = true;
				PathRequestManager.RequestPath(new PathRequest(aiNode,dropBombPosition,OnPathFound));
			}
		}
	}




	//---------- Bombs -----------
	private Node GetWhereToPlaceTheBomb(){
		accesibleTiles = GetAccesibleTiles ();
		foreach (Node n in accesibleTiles) {
			List<Node> neighbours = grid.GetNeighbours (n,1,false,false,true);
			if (neighbours.Any (x => (x.walkable == false && x.destructible == true))) {
				if (!IsSafeToPlaceTheBomb (n)) {
					continue;
				} else
					return n;
			}
		}
		return null;
	}
		
	private bool IsSafeToPlaceTheBomb(Node bomb){
		List<Node> range = grid.GetNeighbours (bomb, bombScript.bombRange);
		range.Add (bomb);
		IEnumerable<Node> safeZones = accesibleTiles.Except (range);
		if (safeZones.Count () > 0)
			return true;
		else
			return false;
	}

	private void AvoidPlacedBomb(Node dropPosition){
		accesibleTiles.RemoveAll (n => n.isDropRange == true);
		PathRequestManager.ShortestPath (new ShortestPathRequest (aiNode, accesibleTiles,FoundShortestPath));
		isPlacingBomb = false;
		print (aiNode.gridX + "," + aiNode.gridY + " => " + shortestPath.gridX + "," + shortestPath.gridY);
		PathRequestManager.RequestPath (new PathRequest (aiNode, shortestPath, OnPathFound));
	}
	//---------- Bombs -----------


	//---------- Path Finding -----------
	public void OnPathFound(Vector3[] newPath,bool pathSuccessful){
		if (pathSuccessful) {
			path = newPath;
			StopCoroutine ("FollowThePath");
			StartCoroutine ("FollowThePath");

			StopCoroutine ("DoneFollowThePath");
			StartCoroutine ("DoneFollowThePath");
		}
	}

	IEnumerator FollowThePath(){
		Vector3 currentWaypoint = path [0];
		currentWaypoint.y = 1f;
		doneFollowThePath = false;

		while (true) {
			if (transform.position == currentWaypoint) {
				targetIndex++;
				if (targetIndex >= path.Length) {
					doneFollowThePath = true;
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

	IEnumerator DoneFollowThePath() {
		while(!doneFollowThePath)       
			yield return new WaitForSeconds(0.1f);

		if (isPlacingBomb) {
			agent.DropBomb ();
			yield return new WaitForSeconds (0.5f);
			AvoidPlacedBomb (grid.NodeFromWorldPoint (transform.position));
		} else {
			yield return new WaitForSeconds (0.5f);
			Searching ();
		}
	}
	//---------- Path Finding -----------


	public void FoundShortestPath(Node shortestPath){
		this.shortestPath = shortestPath;
	}

	private List<Node> GetAccesibleTiles(){
		aiNode = grid.NodeFromWorldPoint (transform.position);
		List<Node> visitedNodes = new List<Node> ();
		Stack<Node> stack = new Stack<Node> ();
		stack.Push (aiNode); 
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
		if (visual != null) {
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






















//		List<Node> safeZones = accesibleTiles.Where (p => p.isDropRange == false).ToList();
//
//		int increasedRange = 1;
//		bool doneEvaluate = false;
//		List<Node> evaluatedSafeZone = grid.GetNeighbours (aiNode, increasedRange, true);
//
//		//Nothing to hide
//		if (accesibleTiles.Count < 1)
//			return;
//
//
//		//Evaluate safe zone and find the shortest path node to hide
//		while (doneEvaluate == false) {
//			evaluatedSafeZone.RemoveAll(x => !safeZones.Contains(x));
//			if (evaluatedSafeZone.Count > 0)
//				doneEvaluate = true;
//			else 
//				evaluatedSafeZone = grid.GetNeighbours (aiNode, ++increasedRange, true, true);
//		}