using UnityEngine;
using System.Collections;
using System;

public class Player : MonoBehaviour {

    //Player parameters
    [Range(1, 2)]
    public int playerNumber = 1;
    public float moveSpeed = 5f;
	public GameObject bombPrefab;

	private const int max_bomb = 3;

    //Cached components
    private Rigidbody rigidBody;
    private Transform myTransform;
    private Animator animator;

    void Start() {
        //Cache the attached components for better performance and less typing
        rigidBody = GetComponent<Rigidbody>();
        myTransform = transform;
        animator = myTransform.Find("PlayerModel").GetComponent<Animator>();
    }

    // Update is called once per frame
	void Update() {
		animator.SetBool("Walking", false);

		switch (playerNumber) {
			case 1:
			UpdateMovement(KeyCode.W,KeyCode.S,KeyCode.D,KeyCode.A,KeyCode.Space);
			break;
			case 2:
			UpdateMovement(KeyCode.UpArrow,KeyCode.DownArrow,KeyCode.RightArrow,KeyCode.LeftArrow,KeyCode.Return);
			break;
		}
    }

	private void UpdateMovement(KeyCode up, KeyCode down, KeyCode right, KeyCode left, KeyCode dropBomb) {
		if (Input.GetKey(up)) { //Up movement
			rigidBody.velocity = new Vector3(-moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
			myTransform.rotation = Quaternion.Euler(0, 270, 0);
			animator.SetBool("Walking", true);
		}

		if (Input.GetKey(down)) { //Down movement
			rigidBody.velocity = new Vector3(moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
			myTransform.rotation = Quaternion.Euler(0, 90, 0);
			animator.SetBool("Walking", true);
		}

		if (Input.GetKey(right)) { //Right movement
			rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, moveSpeed);
			myTransform.rotation = Quaternion.Euler(0, 0, 0);
			animator.SetBool("Walking",true);
		}

		if (Input.GetKey(left)) { //Left movement
			rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, -moveSpeed);
			myTransform.rotation = Quaternion.Euler(0, 180, 0);
			animator.SetBool("Walking", true);
		}

		if (Input.GetKeyDown(dropBomb)) { //Drop bomb
			DropBomb();
		}
    }

    private void DropBomb() {
        if (bombPrefab) { //Check if bomb prefab is assigned first
			Instantiate(bombPrefab, new Vector3(Mathf.RoundToInt(myTransform.position.x), 
				bombPrefab.transform.position.y, Mathf.RoundToInt(myTransform.position.z)),
				bombPrefab.transform.rotation);  
        }
    }
		
    public void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Explosion")) {
            Debug.Log("P" + playerNumber + " hit by explosion!");
        }
    }


}
