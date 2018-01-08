using UnityEngine;
using System.Collections;

/// <summary>
/// Small script for easily destroying an object after a while
/// </summary>
public class DestroySelf : MonoBehaviour {
    public float Delay = 0.55f; //Delay in seconds before destroying the gameobject

    void Start() {
        Destroy(gameObject, Delay);
    }

}
