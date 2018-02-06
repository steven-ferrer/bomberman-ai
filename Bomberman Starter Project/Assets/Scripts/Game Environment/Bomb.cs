using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;

public class Bomb : MonoBehaviour
{

    public GameObject explosionPrefab;
    public LayerMask blockMask;
    public AudioClip soundExplosion;

    public int bombRange = 3;
    public float timeToExplode = 4f;

    private bool exploded = false;
    private bool chainReaction = false;
    private AudioSource audioSource;
    private GridScript grid;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Invoke("Explode", timeToExplode);
        grid.UpdateGrid(GameObjectType.BOMB,transform.position, true);
    }

    public void SetGridScript(GridScript grid)
    {
        this.grid = grid;
        this.grid.bombRange = bombRange;
        this.grid.timeToExplode = timeToExplode;
    }

    void Explode()
    {
        grid.UpdateGrid(GameObjectType.BOMB, transform.position, false);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        if (chainReaction == false)
        {
            audioSource.PlayOneShot(soundExplosion, 0.7F);
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

    private IEnumerator CreateExplosions(Vector3 direction)
    {
        for (int i = 1; i <= bombRange; i++)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position + new Vector3(0, .5f, 0), direction, out hit, i, blockMask);

            if (!hit.collider)
            {
                GameObject obj = Instantiate(explosionPrefab, transform.position + (i * direction), explosionPrefab.transform.rotation);
                grid.UpdateBombGridExplode(obj.transform.position,transform.position);
            }
            else
            {
                if (hit.collider.CompareTag(GameObjectType.DESTRUCTIBLE_WALL.GetTag()))
                {
                    Destroy(hit.transform.gameObject);
                    grid.UpdateGrid(GameObjectType.DESTRUCTIBLE_WALL, hit.transform.position,false);
                    yield break;
                }
            }
            yield return new WaitForSeconds(.05f);
        }
    }


    public void OnTriggerEnter(Collider other)
    {
        if (!exploded && other.CompareTag(GameObjectType.EXPLOSION.GetTag()))
        {
            CancelInvoke("Explode");
            chainReaction = true;
            Explode();
        }
    }

}
