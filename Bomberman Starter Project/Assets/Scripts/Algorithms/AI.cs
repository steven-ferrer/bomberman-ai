using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

	public GridScript grid;
	public float speed = 3;
	private Animator animator;

	Vector3[] path;
	int targetIndex;

	void Start(){
		animator = transform.Find("PlayerModel").GetComponent<Animator>();
		grid.CreateGrid ();

		Node aiNode = grid.NodeFromWorldPoint (transform.position);
		List<Node> neighbours = grid.GetBombExplosionRange (aiNode);

		foreach(Node n in neighbours)
			Debug.Log ("(" + n.gridX + "," + n.gridY + ") => " + "walkable:"+n.walkable+" | destructible:" +n.destructible );  
	}

	void Update(){
		animator.SetBool ("Walking", false);
		//PathRequestManager.RequestPath (new PathRequest (transform.position, target.position, OnPathFound));
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
				animator.SetBool("Walking", true);
			} else{
				transform.rotation = Quaternion.Euler (0, 90, 0); //Down
				animator.SetBool("Walking", true);
			}
		} 

		if (nextPos.y == 1 && nextPos.x == currentPos.x) {
			if (nextPos.z > currentPos.z) {
				transform.rotation = Quaternion.Euler (0, 0, 0); //right
				animator.SetBool("Walking", true);
			} else {
				transform.rotation = Quaternion.Euler(0, 180, 0); //left
				animator.SetBool("Walking", true);
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
	}

}
