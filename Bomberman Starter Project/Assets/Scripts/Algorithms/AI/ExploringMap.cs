using UnityEngine;
using StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ExploringMap : State<AI>
{
    private static ExploringMap _instance;
    private AI owner;
    private const int PLACING_POSITIONS_LIMIT = 6; //how many positions to search where to place the bombs

    private ExploringMap()
    {
        if (_instance != null)
        {
            return;
        }
        _instance = this;
    }

    public static ExploringMap Instance
    {
        get
        {
            if (_instance == null)
            {
                new ExploringMap();
            }
            return _instance;
        }
    }

    public override void EnterState(AI _owner)
    {
        owner = _owner;
        Debug.Log("-EXPLORING MAP STATE-");

        _owner.StartCoroutine(this.PlaceBomb());
    }

    public override void ExitState(AI _owner)
    {

    }

    public override void UpdateState(AI _owner)
    {
    }

    private bool IsSafeToPlaceTheBomb(Node bombPosition)
    {
        List<Node> bombRange = owner.grid.GetNeighbours(bombPosition, owner.bombScript.bombRange);
        bombRange.Add(bombPosition);

        foreach (Node node in owner.accessibleTiles)
        {
            if (node.isBomb)
                continue;
            if (node.GetDropRangeCount() > 0)
                continue;
            if (bombRange.Contains(node))
                continue;
            
            return true;
        }
        return false;
    }

    private List<Node> GetBombPositions()
    {
        List<Node> bombPositions = new List<Node>();
        foreach (Node node in owner.accessibleTiles)
        {
            if (bombPositions.Count >= PLACING_POSITIONS_LIMIT)
                break;
            if (node.isBomb || node.GetDropRangeCount() > 0)
                continue;
            List<Node> neighbours = owner.grid.GetNeighbours(node, 1, false, false, true);
            if (neighbours.Any(x => (x.walkable == false && x.destructible == true)))
            {
                bombPositions.Add(node);
            }
        }
        return bombPositions;
    }

    private IEnumerator PlaceBomb()
    {
        yield return new WaitForSeconds(0.5f);
        List<Node> bombPositions = GetBombPositions();
        bool success = false;
        if (bombPositions.Count > 0)
        {
            foreach (Node bombPosition in bombPositions)
            {
                if (IsSafeToPlaceTheBomb(bombPosition))
                {
                    Debug.Log("(Placing the bomb at " + bombPosition.gridX + "," + bombPosition.gridY + ")");
                    owner.visualBombPosition = bombPosition;

                    if (bombPosition == owner.aiNode)
                    {
                        DropBomb();
                        yield break;
                    }

                    owner.WalkTo(bombPosition, DoneWalking);
                    success = true;
                    yield break; //for now on it can place only one bomb at a time to destroy the walls
                }
            }
        }
        if(!success)
            owner.stateMachine.ChangeState(Searching.Instance);
    }

    public void DoneWalking(bool success)
    {
        if (success)
            DropBomb();
        else
            owner.stateMachine.ChangeState(Searching.Instance);
    }

    private void DropBomb()
    {
        //Check if there is a danger in path when evading the bomb
        if (CanEvadeTheBombInTime())
        {
            owner.agent.DropBomb();
            owner.visualBombPosition = null;
            owner.stateMachine.ChangeState(EvadingBombs.Instance);
        }else
            owner.stateMachine.ChangeState(Searching.Instance);
    }

    private bool CanEvadeTheBombInTime()
    {
        owner.aiNode = owner.grid.NodeFromWorldPoint(owner.transform.position);
        List<Node> bombRange = owner.grid.GetNeighbours(owner.aiNode, owner.bombScript.bombRange);
        bombRange.Add(owner.aiNode);
        List<Node> safeNodes = new List<Node>();

        foreach (Node node in owner.accessibleTiles)
        {
            if (node.isBomb)
                continue;
            if (node.GetDropRangeCount() > 0)
                continue;
            if (bombRange.Contains(node))
                continue;

            Node[] waypoints = PathRequestManager.GetWaypoints(owner.aiNode, node);
            if (waypoints.Length > 0)
            {
                if (waypoints.Any(x => x.GetTimeToExplode() == 1 || x.GetTimeToExplode() == 0))
                {
                    foreach (Node n in waypoints)
                    {
                        if (n.isBomb || n.GetDropRangeCount() > 0) 
                        {
                            return false;
                        }
                        if (n.GetTimeToExplode() >= 0 && n.GetTimeToExplode() <= 1)
                        {
                            return false;
                        }
                    }
                }

                break;
            }
        }

        return true;
    }


}