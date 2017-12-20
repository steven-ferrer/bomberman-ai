using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {

	public bool walkable;
	public bool destructible;
	public Vector3 worldPosition;

	public Node(bool walkable,bool destructible, Vector3 worldPosition){
		this.walkable = walkable;
		this.destructible = destructible;
		this.worldPosition = worldPosition;
	}
	
}
