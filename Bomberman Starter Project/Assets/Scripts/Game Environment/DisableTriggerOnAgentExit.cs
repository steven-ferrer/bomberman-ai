using UnityEngine;
using System.Collections;

/// <summary>
/// This script makes sure that a bomb can be laid down at the agent's feet without causing buggy movement when the agent walks away.
/// It disables the trigger on the collider, essentially making the object solid.
/// </summary>
public class DisableTriggerOnAgentExit : MonoBehaviour {

    public void OnTriggerExit(Collider other) {
		if (other.gameObject.CompareTag(GameObjectType.AGENT.GetTag())) { // When the agents exits the trigger area
            GetComponent<Collider>().isTrigger = false; // Disable the trigger
        }
    }
}
