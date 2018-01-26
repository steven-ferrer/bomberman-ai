﻿using UnityEngine;
using StateStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ExploringMap : State<AI>
{
    private static ExploringMap _instance;

    private AI owner;
    private Vector3[] path;

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
        _owner.StartCoroutine(this.PlaceBomb());
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
            owner.agent.DropBomb();
            owner.StartCoroutine(this.Search());
        }
    }

    private IEnumerator PlaceBomb()
    {
        yield return new WaitForSeconds(0.5f);
        List<Node> bombPositions = GetBombPositions();
        if (bombPositions.Count > 0)
        {
            foreach (Node bombPosition in bombPositions)
            {
                if (IsSafeToPlaceTheBomb(bombPosition))
                {
                    if (bombPosition == owner.aiNode)
                    {
                        owner.agent.DropBomb();
                        owner.StartCoroutine(this.Search());
                        yield break;
                    }

                    Debug.Log("place bomb to " + bombPosition.gridX + "," + bombPosition.gridY);
                    owner.WalkTo(bombPosition, DoneWalking);
                    yield break;
                }
            }
        }
    }

    private IEnumerator Search()
    {
        yield return new WaitForSeconds(0.1f);
        owner.stateMachine.ChangeState(Searching.Instance);
    }

    private List<Node> GetBombPositions()
    {
        List<Node> bombPositions = new List<Node>();
        foreach (Node node in owner.accessibleTiles)
        {
            List<Node> neighbours = owner.grid.GetNeighbours(node, 1, false, false, true);
            if (neighbours.Any(x => (x.walkable == false && x.destructible == true)))
            {
                bombPositions.Add(node);
            }
        }
        return bombPositions;
    }

    private bool IsSafeToPlaceTheBomb(Node bombPosition)
    {
        List<Node> bombRange = owner.grid.GetNeighbours(bombPosition, owner.bombScript.bombRange);
        List<Node> safeNodes = new List<Node>();
        bombRange.Add(bombPosition);

        foreach (Node node in owner.accessibleTiles)
        {
            if (node.isDropRange)
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

}
