using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

	public GameObject explosionPrefab;
	public LayerMask levelMask;

	void Start () {
		Invoke("Explode", 3f);
	}

	void Update () {
		
	}

	void Explode(){
		Instantiate(explosionPrefab, transform.position, Quaternion.identity); 

		StartCoroutine(CreateExplosions(Vector3.forward));
		StartCoroutine(CreateExplosions(Vector3.right));
		StartCoroutine(CreateExplosions(Vector3.back));
		StartCoroutine(CreateExplosions(Vector3.left));  

		GetComponent<MeshRenderer>().enabled = false; 
		transform.Find("Collider").gameObject.SetActive(false); 
		Destroy(gameObject, .3f); 
	}

	private IEnumerator CreateExplosions(Vector3 direction) {
		for (int i = 1; i < 3; i++) { 
			RaycastHit hit; 
			Physics.Raycast (transform.position + new Vector3 (0, .5f, 0), direction, out hit, i, levelMask); 

			if (!hit.collider) { 
				Instantiate (explosionPrefab, transform.position + (i * direction), explosionPrefab.transform.rotation); 
			} else { 
				break; 
			}

			yield return new WaitForSeconds (.05f);
		}
	}



}
