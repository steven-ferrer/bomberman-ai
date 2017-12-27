using UnityEngine;
using System.Collections;

public class GlobalStateManager : MonoBehaviour {

	private int deadAgents = 0;
	private string deadAgentName = null;

    public void PlayerDied(string agentName) {
		deadAgents++; 

		if (deadAgents == 1) { 
			deadAgentName = agentName; 
			Invoke("CheckPlayersDeath", .3f); 
		}  
    }

	void CheckPlayersDeath() {
		if (deadAgents == 1) { 
			if (deadAgentName == "Aggressive AI") { 
				Debug.Log("Player is the winner!");
			} else { 
				Debug.Log("Aggressive AI is the winner!");
			}
		} else { 
			Debug.Log("The game ended in a draw!");
		}
	}  
}
