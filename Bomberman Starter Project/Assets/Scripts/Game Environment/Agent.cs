using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Agent : MonoBehaviour {

	public GlobalStateManager GlobalManager;
	public GridScript grid;
	public GameObject bombPrefab;
    public float moveSpeed = 5f;
	public int maxBomb = 3;

	private int agentIndex;
	private string agentName;
	private int dropBomb = 0;
	private List<Vector3> dropPositions;
    private Rigidbody rigidBody;
	private Animator animator;

	bool walking = false;

	Vector3 currentPos;
	Vector3 nextPos;

    void Start() {
        rigidBody = GetComponent<Rigidbody>();
		dropPositions = new List<Vector3> ();
        animator = transform.Find("PlayerModel").GetComponent<Animator>();
		agentName = transform.name;
    }

	void Update() {
		if (walking == true) {
			walking = false;
			animator.SetBool ("Walking", walking);
		}

		currentPos = transform.position;

		if (Utility.RoundToInt (currentPos) != Utility.RoundToInt (nextPos)) { 
			grid.UpdateAgentMoves (currentPos,nextPos,transform.gameObject.name);
			nextPos = currentPos;
		}
			
		if (agentName == GameObjectType.PLAYER.ToString ()) {
			UpdateMovement (KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.Return);
			UpdateMovement (KeyCode.W, KeyCode.S, KeyCode.D, KeyCode.A, KeyCode.Space);
		}
    }

	private void UpdateMovement(KeyCode up, KeyCode down, KeyCode right, KeyCode left, KeyCode dropBomb) {
		if (Input.GetKey(up)) { //Up movement
			rigidBody.velocity = new Vector3(-moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
			transform.rotation = Quaternion.Euler(0, 270, 0);
			walking = true;
			animator.SetBool("Walking", walking);
		}

		if (Input.GetKey(down)) { //Down movement
			rigidBody.velocity = new Vector3(moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
			transform.rotation = Quaternion.Euler(0, 90, 0);
			walking = true;
			animator.SetBool("Walking", walking);
		}

		if (Input.GetKey(right)) { //Right movement
			rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, moveSpeed);
			transform.rotation = Quaternion.Euler(0, 0, 0);
			walking = true;
			animator.SetBool("Walking",walking);
		}

		if (Input.GetKey(left)) { //Left movement
			rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, -moveSpeed);
			transform.rotation = Quaternion.Euler(0, 180, 0);
			walking = true;
			animator.SetBool("Walking", walking);
		}

		if (Input.GetKeyDown(dropBomb)) { //Drop bomb
			DropBomb();
		}
    }

    private void DropBomb() {
        if (bombPrefab) { 
			bombPrefab.name = GameObjectType.AGENT.ToString() +":" + agentName;
			checkDropBomb();
			if (dropBomb < maxBomb) {
				Vector3 dropPosition = Utility.RoundToInt(transform.position);
				foreach (Vector3 pos in dropPositions) {
					if (dropPosition == pos) {
						return;
					}
				}
				GridScript.Dropped_Bombs.Add (dropPosition);
				Instantiate (bombPrefab, dropPosition, bombPrefab.transform.rotation);
			}
			dropBomb = 0;
        }
    }

	private void checkDropBomb(){
	 	GameObject[] bombs = null;
		if (bombs == null) {
			bombs = GameObject.FindGameObjectsWithTag (GameObjectType.BOMB.GetTag());
			dropPositions.Clear ();
			foreach (GameObject bomb in bombs) {
				if (bomb.name == GameObjectType.AGENT.ToString() +":" + agentName + "(Clone)") {
					dropBomb++;
					dropPositions.Add (bomb.transform.position);
				}
			}
		}
	}
		
    public void OnTriggerEnter(Collider other) {
		if(other.CompareTag (GameObjectType.EXPLOSION.GetTag())) {
			GlobalManager.PlayerDied (agentName); 
			Destroy (gameObject); 
		} 

    }

}
