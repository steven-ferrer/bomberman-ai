using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropRange
{
    private Node parentBomb;
    private float timeToExplode;
    private MonoBehaviour monoBehaviour;

    public DropRange(Node _parentBomb, float _timeToExplode,MonoBehaviour _monoBehaviour)
    {
        parentBomb = _parentBomb;
        timeToExplode = _timeToExplode;
        monoBehaviour = _monoBehaviour;
        monoBehaviour.StartCoroutine(this.StartTimer());
    }

    IEnumerator StartTimer()
    {
        while (true)
        {
            if (this.timeToExplode == 0)
            {
                yield break;
            }
            this.timeToExplode--;
            yield return new WaitForSeconds(1f);
        }
    }

    ~DropRange()
    {
        monoBehaviour.StopCoroutine(this.StartTimer());
    }

    public Node GetParentBomb()
    {
        return parentBomb;
    }

    public float TimeToExplode()
    {
        return timeToExplode;
    }
}

public class Node : IHeapItem<Node>
{

    public bool walkable;
    public bool destructible;
    public Vector3 worldPosition;
    public int gridX, gridY;

    public int gCost;
    public int hCost;
    public Node parent;
    int heapIndex;

    public bool isBomb = false;
    public string agentName = null;
    public bool isOverlap = false;
    public float count;
    private List<DropRange> dropList = new List<DropRange>();

    public void AddDropRange(DropRange dropRange)
    {
        if (!dropList.Contains(dropRange))
        {
            dropList.Add(dropRange);
        }
    }

    public void RemoveDropRange(Node bomb)
    {
        if (dropList.Count > 0)
        {
            dropList.RemoveAll(x => x.GetParentBomb() == bomb);
        }
    }

    public int GetDropRangeCount()
    {
        return dropList.Count;
    }

    public void ClearDropRange()
    {
        dropList.Clear();
    }

    public Node(bool walkable, bool destructible, Vector3 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.destructible = destructible;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}