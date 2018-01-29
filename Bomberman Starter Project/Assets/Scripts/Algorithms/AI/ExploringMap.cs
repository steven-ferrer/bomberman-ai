using UnityEngine;
using StateStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct BombPosition
{
    public Node bomb;
    public int destructibleNeighbourCount;

    public BombPosition(Node _bomb, int _destructibleNeighbourCount)
    {
        bomb = _bomb;
        destructibleNeighbourCount = _destructibleNeighbourCount;
    }
}

public class ExploringMap : State<AI>
{
    private static ExploringMap _instance;
    private AI owner;
    private Vector3[] path;
    private List<BombPosition> bombPositions = new List<BombPosition>();
    private const int PLACING_POSITIONS_LIMIT = 5; //how many positions to search where to place the bombs

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

    }

    private bool IsSafeToPlaceTheBomb(Node bombPosition)
    {
        List<Node> bombRange = owner.grid.GetNeighbours(bombPosition, owner.bombScript.bombRange);
        bombRange.Add(bombPosition);

        foreach (Node node in owner.accessibleTiles)
        {
            if (node.GetDropRangeCount() > 0)
                continue;
            if (bombRange.Contains(node))
                continue;

            return true;
        }
        return false;
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
    }

    private IEnumerator PlaceBomb()
    {
        yield return new WaitForSeconds(0.5f);
        if (bombPositions.Count > 0)
        {
            bombPositions.RemoveAll(x => !IsSafeToPlaceTheBomb(x.bomb));
            foreach (BombPosition bombPosition in bombPositions)
            {
                Node bombPos = bombPosition.bomb;
                if (((bombPosition.bomb.gridX % 2) == 0) || ((bombPosition.bomb.gridY % 2) == 0))
                {
                    bombPos = owner.grid.GetNeighbours(bombPosition.bomb).Find(x => x.walkable == true);
                    if (!IsSafeToPlaceTheBomb(bombPos))
                    {
                        bombPos = bombPosition.bomb;
                    }
                }

                Debug.Log("place bomb to " + bombPos.gridX + "," + bombPos.gridY);
                owner.visualBombPosition = bombPos;

                if (bombPos == owner.aiNode)
                {
                    owner.agent.DropBomb();
                    owner.StartCoroutine(this.AvoidBomb());
                    yield break;
                }

                owner.WalkTo(bombPos, DoneWalking);
                yield break; //for now on it can place only one bomb to destroy the walls
            }
        }
    }

    public void DoneWalking(bool success)
    {
        if (success)
        {
            Debug.Log("Done walking!");
            owner.agent.DropBomb();
            owner.visualBombPosition = null;
            owner.StartCoroutine(this.AvoidBomb());
        }
    }

    private IEnumerator AvoidBomb()
    {
        yield return new WaitForSeconds(0.1f);
        owner.stateMachine.ChangeState(AvoidingBombs.Instance);
    }


}
