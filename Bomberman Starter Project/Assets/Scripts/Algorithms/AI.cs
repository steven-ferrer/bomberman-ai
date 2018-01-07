using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class AI : MonoBehaviour {
	public GridScript grid;
	public DepthFirstSearch dfs;
	public float speed = 3;

	bool walking = false;

	private Animator animator;

	Vector3[] path;
	List<Node> allSearch;
	int targetIndex;
	Node aiNode;

	void Start(){
		animator = transform.Find("PlayerModel").GetComponent<Animator>();
		grid.CreateGrid ();
	}
		
	void Update(){
		if (walking == true) {
			walking = false;
			animator.SetBool ("Walking", walking);	
		}

		aiNode = grid.NodeFromWorldPoint (transform.position);
		dfs.Search (aiNode);
		allSearch = dfs.safeZones;

	}

	public void OnPathFound(Vector3[] newPath,bool pathSuccessful){
		if (pathSuccessful) {
			path = newPath;
			StopCoroutine ("FollowPath");
			StartCoroutine ("FollowPath");
		}
	}

	IEnumerator FollowPath(){
		Vector3 currentWaypoint = path [0];
		Vector3 pos = transform.position;

		while (true) {
			if (pos == currentWaypoint) {
				targetIndex++;
				if (targetIndex >= path.Length) {
					yield break;
				}
				currentWaypoint = path [targetIndex];
			}

			transform.position = Vector3.MoveTowards (pos, currentWaypoint, speed * Time.deltaTime);
			UpdateAnimationMovement (pos,currentWaypoint);

			yield return null;
		}
	}

	void UpdateAnimationMovement(Vector3 currentPos, Vector3 nextPos){
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
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube (path [i], Vector3.one);
			}
		}

		if (allSearch != null) {
			foreach (Node n in allSearch) {
				Gizmos.color = Color.cyan;
				Gizmos.DrawCube (n.worldPosition, Vector3.one);
			}
		}
	}

}
