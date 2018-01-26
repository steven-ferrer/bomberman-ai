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
        _owner.aiNode = _owner.grid.NodeFromWorldPoint(_owner.transform.position);
        _owner.accessibleTiles = _owner.grid.GetAccessibleTiles(_owner.aiNode);
        if (_owner.accessibleTiles.Any(v => (v.isBomb == true)))
        {
            if (_owner.stateMachine.currentState != AvoidingBombs.Instance)
            {
                Debug.Log("Bomb found.");
                _owner.stateMachine.ChangeState(AvoidingBombs.Instance);
            }
        }
        else if (_owner.accessibleTiles.Any(v => v.agentName != null && v.agentName != _owner.transform.name))
        {

        }
        else
        {
            if (_owner.stateMachine.currentState != ExploringMap.Instance)
            {
                Debug.Log("No enemy or bomb found.");
                _owner.stateMachine.ChangeState(ExploringMap.Instance);
            }
        }
    }
}
