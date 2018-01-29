using UnityEngine;
using StateStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AvoidingBombs : State<AI>
{
    private static AvoidingBombs _instance;
    
    private AI owner;
    private Node shortestPath;

    private AvoidingBombs()
    {
        if (_instance != null)
        {
            return;
        }
        _instance = this;
    }

    public static AvoidingBombs Instance
    {
        get
        {
            if (_instance == null)
            {
                new AvoidingBombs();
            }
            return _instance;
        }
    }

    public override void EnterState(AI _owner)
    {
        owner = _owner;
        Debug.Log("Avoiding the bombs");
        foreach (Node node in _owner.accessibleTiles)
        {
            if (node.GetDropRangeCount() == 0 && !node.isBomb)
            {
                Debug.Log("Walk to safe node: " + node.gridX + "," + node.gridY);
                _owner.visualSafePosition = node;
                _owner.WalkTo(node, DoneWalking);
                break;
            }
        }
    }

    public override void ExitState(AI _owner)
    {

    }

    public override void UpdateState(AI _owner)
    {

    }

    public void DoneWalking(bool success)
    {
        if (success)
        {
            Debug.Log("Done walking!");
            owner.visualSafePosition = null;
            owner.isAvoidingTheBombs = false;
        }
    }

    private void FoundShortestPath(Node shortestPath)
    {
        this.shortestPath = shortestPath;
    }

}




