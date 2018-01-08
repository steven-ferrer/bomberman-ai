using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {
	public GameObject explosionPrefab;
	public LayerMask blockMask;
	public AudioClip soundExplosion;

	public int bombRange = 3;
	public float timeToExplode = 4f;

	private bool exploded = false;
	private bool chainReaction = false;
	private float timeBomb;
	AudioSource audioSource;

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
			audioSource.PlayOneShot (soundExplosion, 0.7F);
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
		GridScript.Exploded_Bombs.Add (transform.position);
	}

	private IEnumerator CreateExplosions(Vector3 direction) {
		for (int i = 1; i <= bombRange; i++) { 
			RaycastHit hit; 
			Physics.Raycast (transform.position + new Vector3 (0, .5f, 0), direction, out hit, i, blockMask); 

			if (!hit.collider) { 
				Instantiate (explosionPrefab, transform.position + (i * direction), explosionPrefab.transform.rotation); 
			} else {
				if (hit.collider.CompareTag (GameObjectType.DESTRUCTIBLE_WALL.GetTag())) {
					GridScript.Walls_Destroyed.Add (hit.transform.position);
					Destroy (hit.transform.gameObject);
				}
				break; 
			}
			yield return new WaitForSeconds (.05f);
		}
	}

	public void OnTriggerEnter(Collider other){
		if (!exploded && other.CompareTag(GameObjectType.EXPLOSION.GetTag())) { 
			CancelInvoke("Explode");
			chainReaction = true;
			Explode(); 
		}  
	}
}
