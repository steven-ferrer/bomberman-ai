using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {

	public bool walkable;
	public bool destructible;
	public Vector3 worldPosition;
	public int gridX, gridY;

	public int gCost;
	public int hCost;
	public Node parent;

	public int fCost{
		get{
			return gCost + hCost;
		}
	}

	public Node(bool walkable,bool destructible, Vector3 worldPosition,int gridX, int gridY){
		this.walkable = walkable;
		this.destructible = destructible;
		this.worldPosition = worldPosition;
		this.gridX = gridX;
		this.gridY = gridY;
	}
	
}
