using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;
using StateStuff;
using System;

public class AI : MonoBehaviour 
{
    public StateMachine<AI> stateMachine { get; set; }

    public GridScript grid;
    public Bomb bombScript;
    public float speed = 3;

    public Agent agent { private set; get; }

    private Animator animator;
    private bool walking = false;
    private bool doneFollowThePath = false;

    public List<Node> accessibleTiles { set; get; }
    public Node aiNode { set; get; }
    public bool isAvoidingTheBombs { set; get; }

    public Node visualBombPosition;
    public Node visualSafePosition;
    public List<Node> visualNodes;

    Action<bool> callbackWalking;

    public void OnDrawGizmos()
    {
        if (visualBombPosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(visualBombPosition.worldPosition, Vector3.one);
        }
        if (visualSafePosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(visualBombPosition.worldPosition, Vector3.one);
        }
        if (visualNodes != null && visualNodes.Count > 0)
        {
            foreach (Node node in visualNodes)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(node.worldPosition, Vector3.one);
            }
        }
    }

    private void Awake()
    {
        animator = transform.Find("PlayerModel").GetComponent<Animator>();
        agent = GetComponent<Agent>();
    }

    private void Start()
    {
        stateMachine = new StateMachine<AI>(this);
        //Invoke("StartState", 2f);
    }

    private void StartState()
    {
        stateMachine.ChangeState(ExploringMap.Instance);
    }

    private void Update()
    {
        if (walking == true)
        {
            walking = false;
            animator.SetBool("Walking", walking);
        }

        aiNode = grid.NodeFromWorldPoint(transform.position);
        accessibleTiles = grid.GetAccessibleTiles(aiNode);
        CheckIfInDanger();
        stateMachine.Update();
    }

    private void CheckIfInDanger()
    {
        //if (!isAvoidingTheBombs)
        //{
        //    if (aiNode.GetDropRangeCount() > 0 || aiNode.isBomb) //In range of bomb
        //    {
        //        isAvoidingTheBombs = true;
        //        stateMachine.ChangeState(AvoidingBombs.Instance);
        //    }
        //}
        if(aiNode.isBomb)
            Debug.Log(aiNode.gridX + "," + aiNode.gridY + " => " + aiNode.timeToExplode);
        else
            Debug.Log(aiNode.gridX + "," + aiNode.gridY + " => " + aiNode.TimeToExplode());
    }

    public void WalkTo(Node destination,Action<bool> callbackWalking)
    {
        this.callbackWalking = callbackWalking;
        PathRequestManager.RequestPath(new PathRequest(aiNode, destination, OnPathFound));
    }

    private void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            StartCoroutine(FollowThePath(newPath));
            StartCoroutine(DoneFollowThePath());
        }
        else
        {
            Debug.Log("Path not Successful.");
            callbackWalking(false);
        }
    }

    private IEnumerator FollowThePath(Vector3[] path)
    {
        int targetIndex = -1;
        Vector3 currentWaypoint = path[0];
        currentWaypoint.y = 1f;
        doneFollowThePath = false;
        Debug.Log("Start walking...");

        while (true)
        {
            if (transform.position == currentWaypoint)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                {
                    doneFollowThePath = true;
                    yield break;
                }
                path[targetIndex].y = 1f;
                currentWaypoint = path[targetIndex];
            }
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            walking = true;
            UpdateAnimationMovement(transform.position, currentWaypoint);
            yield return null;
        }
    }

    private IEnumerator DoneFollowThePath()
    {
        while (!doneFollowThePath)
            yield return new WaitForSeconds(0.1f);

        yield return new WaitForSeconds(0.1f);
        callbackWalking(true);
    }

    public void UpdateAnimationMovement(Vector3 currentPos, Vector3 nextPos)
    {
        currentPos.x = Mathf.RoundToInt(currentPos.x);
        currentPos.z = Mathf.RoundToInt(currentPos.z);
        nextPos.x = Mathf.RoundToInt(nextPos.x);
        nextPos.z = Mathf.RoundToInt(nextPos.z);

        if (currentPos == nextPos)
            return;

        if (nextPos.y == 1 && nextPos.z == currentPos.z)
        {
            if (nextPos.x < currentPos.x)
            {
                transform.rotation = Quaternion.Euler(0, 270, 0); //Up
                animator.SetBool("Walking", walking);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 90, 0); //Down
                animator.SetBool("Walking", walking);
            }
        }
        if (nextPos.y == 1 && nextPos.x == currentPos.x)
        {
            if (nextPos.z > currentPos.z)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0); //right
                animator.SetBool("Walking", walking);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 180, 0); //left
                animator.SetBool("Walking", walking);
            }
        }
    }


}
