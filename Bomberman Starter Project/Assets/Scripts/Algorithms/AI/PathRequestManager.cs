using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

public class PathRequestManager : MonoBehaviour {

	Queue<PathResult> results = new Queue<PathResult>();

	static PathRequestManager instance;
	AStar pathfinding;

	void Awake(){
		instance = this;
		pathfinding = GetComponent<AStar> ();
	}

	void Update(){
		if (results.Count > 0) {
			int itemInQueue = results.Count;
			lock (results) {
				for (int i = 0; i < itemInQueue; i++) {
					PathResult result = results.Dequeue ();
					result.callback (result.path, result.success);
				}
			}
		}
	}
		
	public static void RequestPath(PathRequest request){
        ThreadStart threadStart = delegate
        {
            instance.pathfinding.FindPath(request, instance.FinishedProcessingPath);
        };
        threadStart.Invoke();
	}

	public static void ShortestPath(ShortestPathRequest request){
		instance.pathfinding.FindShortestPath (request,instance.FinishedProccessingShortestPath);
	}

	public void FinishedProccessingShortestPath(ShortestPathResult result){
		result.callback (result.shortestPath);
	}

	public void FinishedProcessingPath(PathResult result){
		lock (results) {
			results.Enqueue (result);
		}
	}

}

public struct ShortestPathResult{
	public Node shortestPath;
	public Action<Node> callback;

	public ShortestPathResult (Node shortestPath,Action<Node> callback)
	{
		this.shortestPath = shortestPath;
		this.callback = callback;
	}
}

public struct ShortestPathRequest{
	public Node startNode;
	public List<Node> endNodes;
	public Action<Node> callback;

	public ShortestPathRequest(Node startNode,List<Node> endNodes,Action<Node> callback){
		this.startNode = startNode;
		this.endNodes = endNodes;
		this.callback = callback;
	}
}

public struct PathResult{
	public Vector3[] path;
	public bool success;
	public Action<Vector3[], bool> callback;

	public PathResult (Vector3[] path, bool success, Action<Vector3[], bool> callback)
	{
		this.path = path;
		this.success = success;
		this.callback = callback;
	}
}

public struct PathRequest{
	public Node pathStart;
	public Node pathEnd;
	public Action<Vector3[], bool> callback;

	public PathRequest(Node _start,Node _end, Action<Vector3[], bool> _callback){
		pathStart = _start;
		pathEnd = _end;
		callback = _callback;
	}
}
