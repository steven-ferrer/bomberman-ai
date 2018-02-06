using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

public struct GridUpdate
{
    public GameObjectType objectType ;
    public bool isDestroy;
    public Vector3 position;

    public GridUpdate(GameObjectType _objectType, Vector3 _position, bool _isDestroy)
    {
        objectType = _objectType;
        position = _position;
        isDestroy = _isDestroy;
    }
}

public class GridScript : MonoBehaviour
{
    const float NODE_RADUIS = 0.5f;
    const float NODE_DIAMETER = NODE_RADUIS * 2;

    public LayerMask unwalkableMask;
    public LayerMask agentCollisionMask;
    public bool visualizeNodes = false; //disabled meshrenderer of walls and tiles to show only Gizmos for GRID (to visualize algorithm)

    private Node[,] grid;
    private int gridSizeX, gridSizeY;
    private Vector2 gridWorldSize;
    private bool isCreated = false;
    public int bombRange { set; private get; }
    public float timeToExplode { set; private get; }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    void Awake()
    {
        gridWorldSize = GetComponent<MapGenerator>().mapSize;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / NODE_DIAMETER);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / NODE_DIAMETER);
        CreateGrid();
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                if (n.walkable == false && n.destructible == true)
                    Gizmos.color = Color.blue;
                if (n.GetDropRangeCount() > 0)
                    Gizmos.color = Color.magenta;
                if (n.isBomb == true)
                    Gizmos.color = Color.grey;
                if (n.agentName != null)
                    Gizmos.color = Color.green;

                Gizmos.DrawCube(n.worldPosition, Vector3.one * (NODE_DIAMETER - .1f));
            }
        }
    }

    public void CreateGrid()
    {
        if (isCreated == false)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            grid = new Node[gridSizeX, gridSizeY];
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
            int index = 0;
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * NODE_DIAMETER + NODE_RADUIS) + Vector3.forward * (y * NODE_DIAMETER + NODE_RADUIS);
                    bool walkable = !(Physics.CheckSphere(worldPoint, NODE_RADUIS, unwalkableMask));
                    GameObject goDes = GetObjectByPosition(new Vector3(worldPoint.x, 1, worldPoint.z), GameObjectType.DESTRUCTIBLE_WALL.GetTag());
                    GameObject goBomb = GetObjectByPosition(new Vector3(worldPoint.x, 1, worldPoint.z), GameObjectType.BOMB.GetTag());
                    GameObject goAgent = GetObjectByPosition(new Vector3(worldPoint.x, 1, worldPoint.z), GameObjectType.AGENT.GetTag());

                    bool isDestructibleWall = (goDes == null) ? false : true;
                    bool isBomb = (goBomb == null) ? false : true;
                    bool isAgent = (goAgent == null) ? false : true;

                    Node n = new Node(walkable, isDestructibleWall, worldPoint, x, y);
                    n.isBomb = isBomb;
                    n.HeapIndex = index++;
                    if (isAgent)
                        n.agentName = goAgent.name;

                    grid[x, y] = n;
                }
            }
            isCreated = true;
            sw.Stop();
            print("Grid was successfully Created at " + sw.ElapsedMilliseconds + " ms");
        }
        else
            print("Grid is already Created");
    }

    public void UpdateBombGridExplode(Vector3 position,Vector3 parent)
    {
        Node node = NodeFromWorldPoint(position);
        Node nodeParent = NodeFromWorldPoint(parent);
        grid[node.gridX, node.gridY].RemoveDropRange(nodeParent);
    }

    private List<Node> gridBombs = new List<Node>();
    IEnumerator UpdateBombGrid(Node bomb)
    {
        while (true)
        {
            if (!gridBombs.Contains(bomb))
                yield break;

            List<Node> range = GetNeighbours(bomb, bombRange);
            foreach(Node n in range)
                grid[n.gridX, n.gridY].AddDropRange(new DropRange(bomb, timeToExplode, this));
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void UpdateGrid(GameObjectType objectType,Vector3 position,bool enable)
    {
        Node node = NodeFromWorldPoint(position);
        if (objectType == GameObjectType.DESTRUCTIBLE_WALL)
        {
            grid[node.gridX, node.gridY].destructible = enable;
            grid[node.gridX, node.gridY].walkable = !enable;
        }
        else if (objectType == GameObjectType.BOMB)
        {
            grid[node.gridX, node.gridY].isBomb = enable;
            grid[node.gridX, node.gridY].walkable = !enable;
            if (enable)
            {
                List<Node> range = GetNeighbours(node, bombRange);
                foreach (Node n in range)
                {
                    grid[n.gridX, n.gridY].AddDropRange(new DropRange(node, timeToExplode, this));
                }

                gridBombs.Add(node);
                StartCoroutine(this.UpdateBombGrid(node));
            }
            else
            {
                if(gridBombs.Count > 0)
                    gridBombs.Remove(node);
            }
        }
    }

    public void UpdateAgentMoves(Vector3 current, Vector3 next, string agentName)
    {
        Node currentNode = NodeFromWorldPoint(Utility.RoundToInt(current));
        Node nextNode = NodeFromWorldPoint(Utility.RoundToInt(next));

        if (next == Vector3.zero)
        {
            grid[currentNode.gridX, currentNode.gridY].agentName = agentName;
        }
        else
        {
            grid[currentNode.gridX, currentNode.gridY].agentName = agentName;
            grid[nextNode.gridX, nextNode.gridY].agentName = null;
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        return GetNeighbours(node, 1, false, false, false);
    }

    public List<Node> GetNeighbours(Node node, int range)
    {
        return GetNeighbours(node, range, false, false, false);
    }

    public List<Node> GetNeighbours(Node node, int range, bool diagonal)
    {
        return GetNeighbours(node, range, diagonal, false, false);
    }

    public List<Node> GetNeighbours(Node node, int range, bool diagonal, bool borderOnly)
    {
        return GetNeighbours(node, range, diagonal, borderOnly, false);
    }

    public List<Node> GetNeighbours(Node node, int range, bool diagonal, bool borderOnly, bool includeWalls)
    {
        List<Node> neighbours = new List<Node>();
        bool yCheck = false;

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                //for evaulation
                if (borderOnly)
                {
                    yCheck = (y == 0 || (y < 0 && y > -range) || (y > 0 && y < range));
                    if ((x < 0 && x > -range) && yCheck)
                        continue;
                    if ((x > 0 && x < range) && yCheck)
                        continue;
                }

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    if (diagonal == false)
                    {
                        if ((x == 0 && (y < 0 || y > 0)) || (y == 0 && (x < 0 || x > 0)))
                        {
                            neighbours.Add(grid[checkX, checkY]);
                        }
                    }
                    else
                    {
                        //for evaulation
                        if (borderOnly)
                        {
                            if (x == 0 && yCheck)
                                continue;
                        }
                        neighbours.Add(grid[checkX, checkY]);
                    }
                }
            }
        }

        //Set directions
        if (diagonal == false && includeWalls == false)
        {
            List<Node>[] directions = new List<Node>[4]; //left,right,up,down
            for (int x = 0; x < directions.GetLength(0); x++)
                directions[x] = new List<Node>();

            foreach (Node n in neighbours)
            {
                if (n.gridX == node.gridX)
                {
                    if (n.gridY < node.gridY)
                    {
                        directions[0].Add(n); //left
                    }
                    else
                    {
                        directions[1].Add(n); //right ,reverse
                    }
                }
                else if (n.gridY == node.gridY)
                {
                    if (n.gridX < node.gridX)
                    {
                        directions[2].Add(n); //up
                    }
                    else
                    {
                        directions[3].Add(n); //down , reverse
                    }
                }
            }

            neighbours.Clear();

            //Eliminate walls (indestructible, destructible, and outerwall)
            for (int x = 0; x < directions.GetLength(0); x++)
            {
                if ((x % 2) == 0)
                    directions[x].Reverse();

                bool isWalk = false;
                for (int y = 0; y < directions[x].Count; y++)
                {

                    if (isWalk)
                        directions[x][y].isOverlap = true;
                    if (!directions[x][y].walkable)
                    {
                        isWalk = true;
                        break;
                    }
                    neighbours.Add(directions[x][y]);
                }
            }

            neighbours.RemoveAll(s => s.isOverlap == true);
        }
        return neighbours;
    }

    public List<Node> GetDestructibleWallNeighbours(Node node, int range = 1)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        List<Node>[] directions = new List<Node>[4]; //left,right,up,down
        for (int x = 0; x < directions.GetLength(0); x++)
            directions[x] = new List<Node>();

        foreach (Node n in neighbours)
        {
            if (n.gridX == node.gridX)
            {
                if (n.gridY < node.gridY)
                {
                    directions[0].Add(n); //left
                }
                else
                {
                    directions[1].Add(n); //right ,reverse
                }
            }
            else if (n.gridY == node.gridY)
            {
                if (n.gridX < node.gridX)
                {
                    directions[2].Add(n); //up
                }
                else
                {
                    directions[3].Add(n); //down , reverse
                }
            }
        }

        neighbours.Clear();

        //Eliminate walls (indestructible, destructible, and outerwall)
        for (int x = 0; x < directions.GetLength(0); x++)
        {
            if ((x % 2) == 0)
                directions[x].Reverse();

            bool isWalk = false;
            for (int y = 0; y < directions[x].Count; y++)
            {
                if (isWalk)
                {
                    directions[x][y].isOverlap = true;
                    neighbours.Add(directions[x][y]);
                    break;
                }
                if (!directions[x][y].walkable || (!directions[x][y].walkable && directions[x][y].destructible))
                {
                    isWalk = true;
                }
                neighbours.Add(directions[x][y]);
            }
        }

        neighbours.RemoveAll(s => s.isOverlap == true);

        return neighbours;
    }

    public List<Node> GetAccessibleTiles(Node startNode)
    {
        return GetAccessibleTiles(startNode, 0);
    }

    public List<Node> GetAccessibleTiles(Node startNode,int limit)
    {
        //BREATH FIRST SEARCH
        List<Node> visitedNodes = new List<Node>();
        Queue<Node> queue = new Queue<Node>();
        visitedNodes.Add(startNode);
        queue.Enqueue(startNode);

        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();
            List<Node> neighbours = GetNeighbours(node);
            foreach (Node n in neighbours)
            {
                if (!visitedNodes.Contains(n))
                {
                    visitedNodes.Add(n);
                    queue.Enqueue(n);
                }
            }
        }
        return visitedNodes;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    private GameObject GetObjectByPosition(Vector3 position, string tag)
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject go in gameObjects)
        {
            if (go.transform.position == position)
            {
                return go;
            }
        }
        return null;
    }


}
