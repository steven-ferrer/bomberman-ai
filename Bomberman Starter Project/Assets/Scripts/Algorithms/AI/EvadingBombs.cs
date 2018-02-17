using UnityEngine;
using StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EvadingBombs : State<AI>
{
    private static EvadingBombs _instance;

    private AI owner;
    private Node shortestPath;

    private EvadingBombs()
    {
        if (_instance != null)
        {
            return;
        }
        _instance = this;
    }

    public static EvadingBombs Instance
    {
        get
        {
            if (_instance == null)
            {
                new EvadingBombs();
            }
            return _instance;
        }
    }

    public override void EnterState(AI _owner)
    {
        owner = _owner;
        Debug.Log("-EVADING BOMBS STATE-");

        if (_owner.aiNode.GetDropRangeCount() > 0 || _owner.aiNode.isBomb) //In range of bomb
            _owner.StartCoroutine(this.EvadeBombs());
        else
            _owner.stateMachine.ChangeState(Searching.Instance);
    }

    public override void ExitState(AI _owner)
    {

    }

    public override void UpdateState(AI _owner)
    {

    }

    private IEnumerator EvadeBombs()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (Node node in owner.accessibleTiles)
        {
            if (node.GetDropRangeCount() == 0 && !node.isBomb)
            {
                Debug.Log("(Walking to Safe Node " + node.gridX + "," + node.gridY + ")");
                owner.visualSafePosition = node;
                owner.WalkTo(node, DoneWalking);
                break;
            }
        }
    }

    public void DoneWalking(bool success)
    {
        owner.visualSafePosition = null;
        owner.stateMachine.ChangeState(Searching.Instance);
    }

}