using UnityEngine;
using StateStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ExploringMap : State<AI>
{
    private static ExploringMap _instance;
    private AI owner;
    private const int PLACING_POSITIONS_LIMIT = 6; //how many positions to search where to place the bombs
    public bool placeAnotherBomb = false;

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
        Debug.Log("Start exploring the map...");

        if (placeAnotherBomb == true)
            _owner.StartCoroutine(this.PlaceAnotherBomb());
        else
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
                    Node bombPos = bombPosition;
                    if (((bombPosition.gridX % 2) == 0) || ((bombPosition.gridY % 2) == 0))
                    {
                        bombPos = owner.grid.GetNeighbours(bombPosition).Find(x => x.walkable == true);
                        if (!IsSafeToPlaceTheBomb(bombPos))
                        {
                            bombPos = bombPosition;
                        }
                    }

                    Debug.Log("place bomb to " + bombPos.gridX + "," + bombPos.gridY);
                    owner.visualBombPosition = bombPos;

                    if (bombPos == owner.aiNode)
                    {
                        owner.agent.DropBomb();
                        owner.stateMachine.ChangeState(Searching.Instance);
                        yield break;
                    }

                    owner.WalkTo(bombPos, DoneWalking);
                    success = true;
                    yield break; //for now on it can place only one bomb to destroy the walls
                }
            }
        }
        if(!success)
            owner.stateMachine.ChangeState(Searching.Instance);
    }

    private IEnumerator PlaceAnotherBomb()
    {
        yield return new WaitForSeconds(0.5f);
        bool success = false;
        foreach (Node bombPos in GetBombPositions())
        {
            if (IsSafeToPlaceTheBomb(bombPos))
            {
                Debug.Log("place bomb to " + bombPos.gridX + "," + bombPos.gridY);
                owner.visualBombPosition = bombPos;

                if (bombPos == owner.aiNode)
                {
                    owner.agent.DropBomb();
                    owner.stateMachine.ChangeState(Searching.Instance);
                    yield break;
                }

                owner.WalkTo(bombPos, DoneWalking);
                success = true;
                yield break;
            }
        }
        if (!success)
            owner.stateMachine.ChangeState(Searching.Instance);
    }

    public void DoneWalking(bool success)
    {
        if (success)
        {
            Debug.Log("Done walking!");
            owner.agent.DropBomb();
            owner.visualBombPosition = null;
        }
    }
}
