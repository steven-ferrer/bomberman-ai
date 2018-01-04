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

	public bool isAgent = false;
	public bool isBomb = false;
	public float timeOfBomb = -1f;

	public bool isBombRange;

	public Node(bool walkable,bool destructible, Vector3 worldPosition,int gridX, int gridY){
		this.walkable = walkable;
		this.destructible = destructible;
		this.worldPosition = worldPosition;
		this.gridX = gridX;
		this.gridY = gridY;
	}

	public void setBomb(bool isBomb){
		this.isBomb = isBomb;
		if (isBomb)
			this.isAgent = false;
	}

	public void setTimeBomb(float timeOfBomb){
		this.timeOfBomb = timeOfBomb;
	}

	public void setAgent(bool isAgent){
		this.isAgent = isAgent;
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
