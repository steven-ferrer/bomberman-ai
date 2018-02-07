using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DropRange
{
    public Node parentBomb;
    public float timeToExplode;
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

    public void StopTimer()
    {
        monoBehaviour.StopCoroutine(this.StartTimer());
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
    public float timeToExplode = 0f;
    public string agentName = null;
    public bool isOverlap = false;
    public float count;
    private List<DropRange> dropList = new List<DropRange>();

    public void AddDropRange(Node _node,float _timeToExplode,MonoBehaviour _monobehaviour)
    {
        if (!dropList.Any(x => x.parentBomb == _node))
        {
            dropList.Add(new DropRange(_node, _timeToExplode, _monobehaviour));
        }
    }

    public void RemoveDropRange(Node bomb)
    {
        if (dropList.Count > 0)
        {
            foreach (DropRange drop in dropList)
            {
                if (drop.parentBomb == bomb)
                {
                    drop.StopTimer();
                    drop.timeToExplode = 0;
                }
            }
            dropList.RemoveAll(x => x.parentBomb == bomb);
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

    public float TimeToExplode()
    {
        if (dropList.Count > 0)
        {
            List<float> countdowns = dropList.Select(x => x.timeToExplode).ToList();
            if (dropList.Count > 1)
                countdowns = countdowns.OrderBy(i => i).ToList();

            return countdowns.First();
        }
        else
            return 0f;
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