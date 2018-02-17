using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;
using StateMachine;
using System;

public class AI : MonoBehaviour 
{
    public FiniteStateMachine<AI> stateMachine { get; set; }

    public GridScript grid;
    public Bomb bombScript;
    public float speed = 3;

    public Agent agent { private set; get; }

    private Animator animator;
    private bool walking = false;
    private bool doneFollowThePath = false;

    public List<Node> accessibleTiles { set; get; }
    public Node aiNode { set; get; }

    public Node visualBombPosition;
    public Node visualSafePosition;
    public Node[] visualPath;
    int targetIndex;

    private Node source;
    private Node des;

    Action<bool> callbackWalking;

    public void OnDrawGizmos()
    {
        if (visualBombPosition != null)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawCube(visualBombPosition.worldPosition, Vector3.one);
        }
        if (visualSafePosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(visualSafePosition.worldPosition, Vector3.one);
        }
        if (visualPath != null && visualPath.Length > 0)
        {
            for (int i = targetIndex; i < visualPath.Length; i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(visualPath[i].worldPosition, Vector3.one);
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
        stateMachine = new FiniteStateMachine<AI>(this);
        Invoke("StartState", 2f);
    }

    private void StartState()
    {
        stateMachine.ChangeState(Searching.Instance);
    }

    private void Update()
    {
        if (walking == true)
        {
            walking = false;
            animator.SetBool("Walking", walking);
        }
        stateMachine.Update();
    }

    public void WalkTo(Node destination,Action<bool> callbackWalking)
    {
        this.callbackWalking = callbackWalking;
        PathRequestManager.RequestPath(new PathRequest(aiNode, destination, OnPathFound));
        source = aiNode;
        des = destination;
    }

    private void OnPathFound(Node[] newPath, bool pathSuccessful)
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

    private IEnumerator FollowThePath(Node[] path)
    {
        Vector3 currentWaypoint = path[0].worldPosition;
        List<Node> temp = path.ToList();
        temp.Insert(0, source);
        temp.Add(des);
        path = temp.ToArray();

        currentWaypoint.y = 1f;
        targetIndex = 0;
        doneFollowThePath = false;
        visualPath = path;
        
        while (true)
        {
            if (transform.position == currentWaypoint)
            {
                targetIndex++;
                if (targetIndex >= path.Length - 1)
                {
                    doneFollowThePath = true;
                    yield break;
                }
                path[targetIndex].worldPosition.y = 1f;
                currentWaypoint = path[targetIndex].worldPosition;
            }
            //Node node = grid.NodeFromWorldPoint(currentWaypoint);
            //if (node.isBomb || node.GetDropRangeCount() > 0)
            //{
            //    if (node.GetTimeToExplode() >= 0 && node.GetTimeToExplode() <= 1)
            //    {
            //        Debug.Log(node.gridX + "," + node.gridY + " => " + node.GetTimeToExplode());
            //        while (node.isBomb || node.GetDropRangeCount() > 0)
            //        {
            //            yield return null;
            //        }
            //        yield return new WaitForSeconds(0.5f);
            //    }
            //}
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
        visualPath = null;
        callbackWalking(true);
        yield break;
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