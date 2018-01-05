using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {
	public GameObject explosionPrefab;
	public LayerMask levelMask;
	public AudioClip soundExplosion;

	public int bombRange = 3;
	public float timeToExplode = 4f;

	private bool exploded = false;
	private bool chainReaction = false;
	private float timeBomb;
	AudioSource audioSource;

	public static List<Vector3> wallTobeDestroy = new List<Vector3>();

	void Start () {
		audioSource = GetComponent<AudioSource> ();
		timeBomb = timeToExplode;
		InvokeRepeating ("CountDown", 1.0f, 1.0f);
		Invoke("Explode", timeToExplode);
	}

	void CountDown(){
		timeBomb = timeBomb - 1f;
	}

	public float getTimeBomb(){
		return timeBomb;
	}

	void Explode(){
		Instantiate(explosionPrefab, transform.position, Quaternion.identity); 
		if (chainReaction == false) {
			//audioSource.PlayOneShot (soundExplosion, 0.7F);
			chainReaction = true;
		}
		StartCoroutine(CreateExplosions(Vector3.forward));
		StartCoroutine(CreateExplosions(Vector3.right));
		StartCoroutine(CreateExplosions(Vector3.back));
		StartCoroutine(CreateExplosions(Vector3.left));  

		GetComponent<MeshRenderer>().enabled = false; 
		exploded = true;
		transform.Find("Collider").gameObject.SetActive(false); 
		Destroy(gameObject, .4f); 
	}

	private IEnumerator CreateExplosions(Vector3 direction) {
		for (int i = 1; i <= bombRange; i++) { 
			RaycastHit hit; 
			Physics.Raycast (transform.position + new Vector3 (0, .5f, 0), direction, out hit, i, levelMask); 

			if (!hit.collider) { 
				Instantiate (explosionPrefab, transform.position + (i * direction), explosionPrefab.transform.rotation); 
			} else {
				if (hit.collider.CompareTag ("Destructible")) {
					wallTobeDestroy.Add (hit.transform.position);
					Destroy (hit.transform.gameObject);
				}
				break; 
			}

			yield return new WaitForSeconds (.05f);
		}
	}

	public void OnTriggerEnter(Collider other){
		if (!exploded && other.CompareTag("Explosion")) { 
			CancelInvoke("Explode");
			chainReaction = true;
			Explode(); 
		}  
	}



}
