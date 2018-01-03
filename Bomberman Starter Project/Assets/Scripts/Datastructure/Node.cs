using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node> {

	public bool walkable;
	public bool destructible;
	public Vector3 worldPosition;
	public int gridX, gridY;

	public int gCost;
	public int hCost;
	public Node parent;
	int heapIndex;

	public Node(bool walkable,bool destructible, Vector3 worldPosition,int gridX, int gridY){
		this.walkable = walkable;
		this.destructible = destructible;
		this.worldPosition = worldPosition;
		this.gridX = gridX;
		this.gridY = gridY;
	}
		
	public int fCost{
		get{
			return gCost + hCost;
		}
	}

	public int HeapIndex{
		get{
			return heapIndex;
		}
		set{
			heapIndex = value;
		}
	}

	public int CompareTo(Node nodeToCompare){
		int compare = fCost.CompareTo (nodeToCompare.fCost);
		if (compare == 0) {
			compare = hCost.CompareTo (nodeToCompare.hCost);
		}
		return -compare;
	}
}
