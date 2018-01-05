using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Agent : MonoBehaviour {

	public GlobalStateManager GlobalManager;
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
	
		switch (agentName) {
		case "Player":
			UpdateMovement (KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.Return);
			break;
		case "Aggressive AI":
			UpdateMovement (KeyCode.W, KeyCode.S, KeyCode.D, KeyCode.A, KeyCode.Space);
			break;
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
			bombPrefab.name = "Agent:" + agentName;
			checkDropBomb();
			if (dropBomb < maxBomb) {
				Vector3 dropPosition = new Vector3 (Mathf.RoundToInt (transform.position.x), bombPrefab.transform.position.y, Mathf.RoundToInt (transform.position.z));
				foreach (Vector3 pos in dropPositions) {
					if (dropPosition == pos) {
						return;
					}
				}
				Instantiate (bombPrefab, dropPosition, bombPrefab.transform.rotation);
			}
			dropBomb = 0;
        }
    }

	private void checkDropBomb(){
	 	GameObject[] bombs = null;
		if (bombs == null) {
			bombs = GameObject.FindGameObjectsWithTag ("Bomb");
			dropPositions.Clear ();
			foreach (GameObject bomb in bombs) {
				if (bomb.name == "Agent:" + agentName + "(Clone)") {
					dropBomb++;
					dropPositions.Add (bomb.transform.position);
				}
			}
		}
	}
		
    public void OnTriggerEnter(Collider other) {
		if(other.CompareTag ("Explosion")) {
			GlobalManager.PlayerDied (agentName); 
			Destroy (gameObject); 
		} 

    }

}
