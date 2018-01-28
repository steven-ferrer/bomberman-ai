using UnityEngine;
using StateStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ExploringMap : State<AI>
{
    private static ExploringMap _instance;

    private AI owner;
    private Vector3[] path;

    private List<BombPosition> bombPositions = new List<BombPosition>();

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
        Debug.Log("Start exploring the map...");

        InitializeBombPositions();
        _owner.StartCoroutine(this.PlaceBomb());

    }

    public override void ExitState(AI _owner)
    {

    }

    public override void UpdateState(AI _owner)
    {
        //_owner.aiNode = _owner.grid.NodeFromWorldPoint(_owner.transform.position);
        //_owner.accessibleTiles = _owner.grid.GetAccessibleTiles(_owner.aiNode);
    } 

    public void DoneWalking(bool success)
    {
        if (success)
        {
            Debug.Log("Done walking!");
            owner.agent.DropBomb();
            //owner.StartCoroutine(this.Search());
        }
    }

    private IEnumerator PlaceBomb()
    {
        yield return new WaitForSeconds(0.5f);
        if (bombPositions.Count > 0)
        {
            foreach (BombPosition bombPosition in bombPositions)
            {
                if (IsSafeToPlaceTheBomb(bombPosition.bomb))
                {
                    if (bombPosition.bomb == owner.aiNode)
                    {
                        owner.agent.DropBomb();
                        owner.StartCoroutine(this.Search());
                        yield break;
                    }

                    Debug.Log("place bomb to " + bombPosition.bomb.gridX + "," + bombPosition.bomb.gridY);
                    owner.WalkTo(bombPosition.bomb, DoneWalking);
                    yield break; //for now in it can place only one bomb to destruct the walls
                }
            }
        }
    }

    private IEnumerator Search()
    {
        yield return new WaitForSeconds(0.1f);
        owner.stateMachine.ChangeState(Searching.Instance);
    }

    private void InitializeBombPositions()
    {
        bombPositions.Clear();
        foreach (Node node in owner.accessibleTiles)
        {
            if (bombPositions.Count >= PLACING_POSITIONS_LIMIT)
                break;
            List<Node> neighbours = owner.grid.GetNeighbours(node, 1, false, false, true);
            if (neighbours.Any(x => (x.walkable == false && x.destructible == true)))
            {
                int count = 0;
                foreach (Node n in neighbours)
                {
                    if (n.destructible)
                        count++;
                }
                bombPositions.Add(new BombPosition(node, count));
            }
        }
        bombPositions = bombPositions.OrderByDescending(o => o.destructibleNeighbourCount).ToList();
    }

    private Node GetOptimizedPosition(Node bombPosition)
    {

        return null;
    }

    private bool IsSafeToPlaceTheBomb(Node bombPosition)
    {
        List<Node> bombRange = owner.grid.GetNeighbours(bombPosition, owner.bombScript.bombRange);
        List<Node> safeNodes = new List<Node>();
        bombRange.Add(bombPosition);

        foreach (Node node in owner.accessibleTiles)
        {
            if (node.GetDropRangeCount() > 0)
                continue;
            if (bombRange.Contains(node))
                continue;

            safeNodes.Add(node);
        }

        if (safeNodes.Count > 0)
            return true;
        else
            return false;
    }

    private struct BombPosition
    {
        public Node bomb;
        public int destructibleNeighbourCount;

        public BombPosition(Node _bomb, int _destructibleNeighbourCount)
        {
            bomb = _bomb;
            destructibleNeighbourCount = _destructibleNeighbourCount;
        }
    }

}
