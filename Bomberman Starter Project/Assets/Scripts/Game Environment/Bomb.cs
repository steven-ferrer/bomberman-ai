using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;

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
      audioSource = GetComponent<AudioSource>();
      Invoke("Explode", timeToExplode);
      GridScript.Update_List.Add(new GridUpdate(GameObjectType.BOMB, transform.position, false));
      DropRange();
	}

  public void DropRange()
  {
      StartCoroutine(UpdateDropRange(Vector3.forward));
      StartCoroutine(UpdateDropRange(Vector3.right));
      StartCoroutine(UpdateDropRange(Vector3.back));
      StartCoroutine(UpdateDropRange(Vector3.left));
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
    GridScript.Update_List.Add(new GridUpdate(GameObjectType.BOMB, transform.position, true));
	}

	private IEnumerator CreateExplosions(Vector3 direction) {
		for (int i = 1; i <= bombRange; i++) { 
			RaycastHit hit; 
			Physics.Raycast (transform.position + new Vector3 (0, .5f, 0), direction, out hit, i, blockMask); 
      
			if (!hit.collider) {
        Vector3 pos = transform.position + (i * direction);
        Instantiate(explosionPrefab, pos, explosionPrefab.transform.rotation);
        GridScript.Update_List.Add(new GridUpdate(GameObjectType.EXPLOSION, pos, true));
			} else {
          if (hit.collider.CompareTag(GameObjectType.DESTRUCTIBLE_WALL.GetTag()))
          {
            Destroy(hit.transform.gameObject);
            GridScript.Update_List.Add(new GridUpdate(GameObjectType.DESTRUCTIBLE_WALL, hit.transform.position, true));
            CheckUpdateDropRange(hit, false);
        }
        yield break;
			}
			yield return new WaitForSeconds (.05f);
		}
	}

  private IEnumerator UpdateDropRange(Vector3 direction)
  {
      for (int i = 1; i <= bombRange; i++)
      {
          RaycastHit hit;
          Physics.Raycast(transform.position + new Vector3(0, .5f, 0), direction, out hit, i, blockMask);
          if (!hit.collider)
          {
              Vector3 pos = transform.position + (i * direction);
              GridScript.Update_List.Add(new GridUpdate(GameObjectType.EXPLOSION, pos, false));
          }
          else
          {
              if (hit.collider.CompareTag(GameObjectType.DESTRUCTIBLE_WALL.GetTag()))
              {
                  CheckUpdateDropRange(hit,true);
                  yield break;
              }
          }
          yield return new WaitForSeconds(.01f);
      }
  }

  static List<Vector3> dropList = new List<Vector3>();
  static List<UpdateExplode> updateExplodeList = new List<UpdateExplode>();
  struct UpdateExplode
  {
      public Bomb bomb;
      public Vector3 destructiblePosition;

      public UpdateExplode(Bomb _bomb, Vector3 _destructiblePosition)
      {
          bomb = _bomb;
          destructiblePosition = _destructiblePosition;
      }
  }
  private void CheckUpdateDropRange(RaycastHit hit,bool isDrop)
  {
    if (isDrop)
    {
        if (dropList.Contains(hit.transform.position))
        {
            updateExplodeList.Add(new UpdateExplode(this, hit.transform.position));
        }
        dropList.Add(hit.transform.position);
    }
    else
    {
        dropList.Remove(hit.transform.position);
        UpdateExplode ue = updateExplodeList.Find(x => x.destructiblePosition == hit.transform.position);
        if (ue.bomb != null)
        {
            GridScript.New_Update_List.Add(ue.bomb.transform.position);
        }
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
