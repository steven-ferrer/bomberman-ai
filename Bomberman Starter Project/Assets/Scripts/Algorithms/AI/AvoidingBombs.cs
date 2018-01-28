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
        List<Node> safeNodes = new List<Node>(owner.accessibleTiles);
        safeNodes.RemoveAll(n => n.GetDropRangeCount() > 0);
        PathRequestManager.ShortestPath(new ShortestPathRequest(owner.aiNode, safeNodes, FoundShortestPath));
        Debug.Log("Walk to safe node: " + shortestPath.gridX + "," + shortestPath.gridY);
        if (shortestPath != _owner.aiNode)
        {
            _owner.WalkTo(shortestPath, DoneWalking);
        }
        else
        {
            owner.stateMachine.ChangeState(Searching.Instance);
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
            owner.stateMachine.ChangeState(Searching.Instance);
        }
    }

    private void FoundShortestPath(Node shortestPath)
    {
        this.shortestPath = shortestPath;
    }

}
