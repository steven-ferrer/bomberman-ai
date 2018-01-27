using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class MapGenerator : MonoBehaviour {

	//Prefabs for Map
	public Transform tile1;
	public Transform tile2;
	public Transform destructibleWall;
	public Transform indestructibleWall;
	public Transform outerWall;
  public Vector2 mapSize;
	public int destructibleWallCount = 50;
	
	private List<MeshRenderer> mapRenderers;

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
		
	void Awake(){
		mapRenderers = new  List<MeshRenderer> ();
	}

	void Start(){
    bool visualizeNodes = GetComponent<GridScript>().visualizeNodes;

    if (visualizeNodes)
    {
        GameObject[] destructibleObjects = GameObject.FindGameObjectsWithTag(GameObjectType.DESTRUCTIBLE_WALL.GetTag());
        GameObject[] indestructibleObjects = GameObject.FindGameObjectsWithTag(GameObjectType.INDESTRUCTIBLE_WALL.GetTag());
        GameObject[] floorObjects = GameObject.FindGameObjectsWithTag(GameObjectType.FLOOR.GetTag());

        if (destructibleObjects.Length > 0 && indestructibleObjects.Length > 0 && floorObjects.Length > 0)
        {
            for (int x = 0; x < indestructibleObjects.Length; x++)
                mapRenderers.Add(indestructibleObjects[x].GetComponent<MeshRenderer>());
            for (int x = 0; x < destructibleObjects.Length; x++)
                mapRenderers.Add(destructibleObjects[x].GetComponent<MeshRenderer>());
            for (int x = 0; x < floorObjects.Length; x++)
                mapRenderers.Add(floorObjects[x].GetComponent<MeshRenderer>());

            foreach (MeshRenderer meshRenderer in mapRenderers)
                meshRenderer.enabled = !visualizeNodes;
        }

    }
	}
		
	public void GenerateMap(){
		Stopwatch sw = new Stopwatch ();
		sw.Start ();

		//Initialize Random Destructible Wall Position
		allTileCoords = new List<Coord> ();
		for (int x = 1; x < mapSize.x - 1; x++) {
			for (int y = 1; y < mapSize.y - 1; y++) {

                //disregard 3 tile for all corners (where the start position of agents)
				if ((x < 3 && y < 3) || (x > mapSize.x - 4 && y < 3) || (x < 3 && y > mapSize.y - 4) || (x > mapSize.x - 4 && y > mapSize.y - 4))
					continue;

                //disregard position for indestructible wall
                if (y % 2 == 0 && x % 2 == 0)
                    continue;

				allTileCoords.Add (new Coord (x, y));
			}
		}
		System.Random rng = new System.Random ();
    shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), rng.Next(1, destructibleWallCount)));

		//Check if there is already Generated Map, if there is then destroy it first before generating a new map
        if (transform.Find(GameObjectType.GENERATED_MAP.ToString()))
            DestroyImmediate(transform.Find(GameObjectType.GENERATED_MAP.ToString()).gameObject);
		
		//Holders to organize instantiate prefabs
		Transform hMap = new GameObject (GameObjectType.GENERATED_MAP.ToString()).transform;
		Transform hDestructibleWalls = new GameObject (GameObjectType.DESTRUCTIBLE_WALL.ToString()).transform;
		Transform hIndestructibleWalls = new GameObject (GameObjectType.INDESTRUCTIBLE_WALL.ToString()).transform;
		Transform hOuterWalls = new GameObject (GameObjectType.OUTER_WALL.ToString()).transform;
		Transform hFloors = new GameObject (GameObjectType.FLOOR.ToString()).transform;

		LayerMask layerBlock = LayerMask.NameToLayer (GameObjectType.BLOCKS_LAYER.ToString());

		hMap.parent = transform;

		for (int x = 0; x < mapSize.x; x++) {
			for (int y = 0; y < mapSize.y; y++) {
				Vector3 tilePosition = new Vector3 (-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
				Transform newTile;

				//Floors
				if ((x % 2 == 0 && y % 2 == 0) || (x % 2 == 1 && y % 2 == 1)) 
					newTile = Instantiate (tile1, tilePosition, Quaternion.Euler (Vector3.right * 90)) as Transform;
				else 
				 	newTile = Instantiate (tile2, tilePosition , Quaternion.Euler (Vector3.right * 90)) as Transform;
				
				//Outer Walls
				if (x == 0 || y == 0 || y == mapSize.y - 1 || x == mapSize.x - 1) {
					Transform newWall = Instantiate (outerWall, tilePosition + Vector3.up * 1f, Quaternion.identity) as Transform;
					newWall.parent = hOuterWalls;
					hOuterWalls.parent = hMap;
					newWall.gameObject.layer = layerBlock;
					newWall.gameObject.tag = GameObjectType.INDESTRUCTIBLE_WALL.GetTag();
				}

				//Indestructible Walls
				if ( x > 1 && y > 1 && x < mapSize.x - 2 && y < mapSize.y - 2) {
					if (y % 2 == 0 && x % 2 == 0) {
						Transform newWall = Instantiate (indestructibleWall, tilePosition + Vector3.up * 1f, Quaternion.identity) as Transform;
						newWall.parent = hIndestructibleWalls;
						hIndestructibleWalls.parent = hMap;
						newWall.gameObject.layer = layerBlock;
						newWall.gameObject.tag = GameObjectType.INDESTRUCTIBLE_WALL.GetTag();
					}
				}

				newTile.tag = GameObjectType.FLOOR.GetTag();
				newTile.parent = hFloors;
				hFloors.parent = hMap;
			}
		}

		//Desctructible Wall
    for (int x = 0; x < destructibleWallCount; x++)
    {
			Coord randomCoord = GetRandomCoord ();
			Vector3 wallPosition = CoorToPosition (randomCoord.x, randomCoord.y);
			Transform newWall = Instantiate (destructibleWall, wallPosition + Vector3.up * 1f, Quaternion.identity) as Transform;
			newWall.parent = hDestructibleWalls;
			hDestructibleWalls.parent = hMap;
			newWall.gameObject.layer = layerBlock;
			newWall.gameObject.tag = GameObjectType.DESTRUCTIBLE_WALL.GetTag();
		}

		sw.Stop ();
		print ("Map was successfully generated at " + sw.ElapsedMilliseconds+" ms");
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
