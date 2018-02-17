using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{

    Queue<PathResult> results = new Queue<PathResult>();

    static PathRequestManager instance;
    AStar pathfinding;

    void Awake()
    {
        instance = this;
        pathfinding = GetComponent<AStar>();
    }

    void Update()
    {
        if (results.Count > 0)
        {
            int itemInQueue = results.Count;
            lock (results)
            {
                for (int i = 0; i < itemInQueue; i++)
                {
                    PathResult result = results.Dequeue();
                    result.callback(result.path, result.success);
                }
            }
        }
    }

    public static void RequestPath(PathRequest request)
    {
        ThreadStart threadStart = delegate
        {
            instance.pathfinding.FindPath(request, instance.FinishedProcessingPath);
        };
        threadStart.Invoke();
    }

    public static Node[] GetWaypoints(Node startNode, Node endNode)
    {
        return instance.pathfinding.PathFinding(startNode, endNode);
    }

    public void FinishedProcessingPath(PathResult result)
    {
        lock (results)
        {
            results.Enqueue(result);
        }
    }

}

public struct PathResult{
	public Node[] path;
	public bool success;
	public Action<Node[], bool> callback;

	public PathResult (Node[] path, bool success, Action<Node[], bool> callback)
	{
		this.path = path;
		this.success = success;
		this.callback = callback;
	}
}

public struct PathRequest
{
    public Node pathStart;
    public Node pathEnd;
    public Action<Node[], bool> callback;

    public PathRequest(Node _start, Node _end, Action<Node[], bool> _callback)
    {
        pathStart = _start;
        pathEnd = _end;
        callback = _callback;
    }
}
