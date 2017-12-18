using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

	public Transform tile1;
	public Transform tile2;
	public Vector2 mapSize;
	public Transform destructibleWall;
	public Transform indestructibleWall;
	public Transform outerWall;
	public int wallCount = 50;
	public GameObject[] players;

	List<Coord> allTileCoords;
	Queue<Coord> shuffledTileCoords;

	public struct Coord{
		public int x;
		public int y;

		public Coord(int _x, int _y){
			x = _x;
			y = _y;
		}
	}

	public void initializePlayerPosition(){
		Vector3[] position = { 
			new Vector3 { x = 4, y = 1, z = 8 }, 
			new Vector3 { x = 4, y = 1, z = -8 }, 
			new Vector3 { x = -4, y = 1, z = 8 }, 
			new Vector3 { x = -4, y = 1, z = -8 } 
		};

		System.Random random = new System.Random ();
		Vector3[] cornerPositions = Utility.ShuffleArray (position, random.Next(1,10));

		for (int x = 0; x < players.Length; x++) {
			players [x].transform.position = cornerPositions [x];
			if (cornerPositions [x].x == 4) {
				players [x].transform.localEulerAngles = new Vector3 (0, -90, 0);
			} else {
				players [x].transform.localEulerAngles = new Vector3 (0,90,0);
			}
		}
	}

	public void GenerateMap(){
		
		//Initialize Destructible Walls
		allTileCoords = new List<Coord> ();
		for (int x = 1; x < mapSize.x - 1; x++) {
			for (int y = 1; y < mapSize.y - 1; y++) {
				if ((x < 3 && y < 3) || (x > mapSize.x - 4 && y < 3) || (x < 3 && y > mapSize.y - 4) || (x > mapSize.x - 4 && y > mapSize.y - 4))
					continue;
				allTileCoords.Add (new Coord (x, y));
			}
		}
		System.Random rng = new System.Random ();
		shuffledTileCoords = new Queue<Coord> (Utility.ShuffleArray (allTileCoords.ToArray (), rng.Next(1,100)));
		//----------------------------------------------------------------------------------------------------------------------------------------//


		//Holders Name
		string strMap = "Generated Map";

		//LayerName
		string layerName = "Blocks";

		if (transform.Find (strMap)) {
			DestroyImmediate (transform.Find (strMap).gameObject);
		}

		//Holders
		Transform mapHolder = new GameObject (strMap).transform;
		Transform transIndes = new GameObject ("Indestructible Walls").transform;
		Transform transDes = new GameObject ("Destructible Walls").transform;
		Transform transWall = new GameObject ("Outer Walls").transform;
		Transform transFloor = new GameObject ("Floors").transform;

		LayerMask layerBlock = LayerMask.NameToLayer (layerName);

		mapHolder.parent = transform;

		for (int x = 0; x < mapSize.x; x++) {
			for (int y = 0; y < mapSize.y; y++) {
				Vector3 tilePosition = new Vector3 (-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
				Transform newTile;

				//Floors
				if ((x % 2 == 0 && y % 2 == 0) || (x % 2 == 1 && y % 2 == 1)) {
					newTile = Instantiate (tile1, tilePosition, Quaternion.Euler (Vector3.right * 90)) as Transform;
				} else {
				 	newTile = Instantiate (tile2, tilePosition , Quaternion.Euler (Vector3.right * 90)) as Transform;
				}

				//Outer Walls
				if (x == 0 || y == 0 || y == mapSize.y - 1 || x == mapSize.x - 1) {
					Transform newWall = Instantiate (outerWall, tilePosition + Vector3.up * 1f, Quaternion.identity) as Transform;
					newWall.parent = transWall;
					transWall.parent = mapHolder;
					newWall.gameObject.layer = layerBlock;
				}

				//Indestructible Walls
				if ( x > 1 && y > 1 && x < mapSize.x - 2 && y < mapSize.y - 2) {
					if (y % 2 == 0 && x % 2 == 0) {
						Transform newWall = Instantiate (indestructibleWall, tilePosition + Vector3.up * 1f, Quaternion.identity) as Transform;
						newWall.parent = transIndes;
						transIndes.parent = mapHolder;
						newWall.gameObject.layer = layerBlock;
					}
				}

				newTile.parent = transFloor;
				transFloor.parent = mapHolder;
			}
		}

		//Desctructible Wall
		for (int x = 0; x < wallCount; x++) {
			Coord randomCoord = GetRandomCoord ();
			Vector3 wallPosition = CoorToPosition (randomCoord.x, randomCoord.y);
			Transform newWall = Instantiate (destructibleWall, wallPosition + Vector3.up * 1f, Quaternion.identity) as Transform;
			newWall.parent = transDes;
			transDes.parent = mapHolder;
			newWall.gameObject.layer = layerBlock;
		}
	}

	Vector3 CoorToPosition(int x, int y){
		return new Vector3 (-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
	}

	public Coord GetRandomCoord(){
		Coord randomCoord = shuffledTileCoords.Dequeue ();
		shuffledTileCoords.Enqueue (randomCoord);
		return randomCoord;
	}
		
}
