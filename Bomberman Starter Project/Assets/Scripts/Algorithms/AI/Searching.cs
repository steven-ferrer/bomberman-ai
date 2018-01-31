using UnityEngine;
using StateStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Searching : State<AI>
{
    private static Searching _instance;

    private AI owner;

    private Searching()
    {
        if (_instance != null)
        {
            return;
        }
        _instance = this;
    }

    public static Searching Instance
    {
        get
        {
            if (_instance == null)
            {
                new Searching();
            }
            return _instance;
        }
    }

    public override void EnterState(AI _owner)
    {
        Debug.Log("Start searching...");
    }

    public override void ExitState(AI _owner)
    {
        Debug.Log("Exit searching...");
    }

    public override void UpdateState(AI _owner)
    {
        if (!_owner.isAvoidingTheBombs)
        {
            if (_owner.accessibleTiles.Any(v => v.agentName != null && v.agentName != _owner.transform.name))
            {
                Debug.Log("Enemy found.");
            }
            else if (_owner.accessibleTiles.Any(v => v.isBomb == true))
            {
                //Search for another destructible wall to destroy
                ExploringMap.Instance.placeAnotherBomb = true;
                _owner.stateMachine.ChangeState(ExploringMap.Instance);
            }
            else
            {
                if (_owner.stateMachine.currentState != ExploringMap.Instance)
                {
                    Debug.Log("No enemy or bomb found.");
                    ExploringMap.Instance.placeAnotherBomb = false;
                    _owner.stateMachine.ChangeState(ExploringMap.Instance);
                }
            }
        }
    }
}
