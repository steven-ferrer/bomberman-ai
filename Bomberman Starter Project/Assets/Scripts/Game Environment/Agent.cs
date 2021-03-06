﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Agent : MonoBehaviour
{

    public GlobalStateManager GlobalManager;
    public GridScript grid;
    public GameObject bombPrefab;
    public float moveSpeed = 5f;
    public int maxBomb = 3;

    private int dropBomb = 0;
    private List<Vector3> dropPositions;
    private Rigidbody rigidBody;
    private Animator animator;

    bool walking = false;

    Vector3 currentPos;
    Vector3 nextPos;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        dropPositions = new List<Vector3>();
        animator = transform.Find("PlayerModel").GetComponent<Animator>();
    }

    void Update()
    {
        if (walking == true)
        {
            walking = false;
            animator.SetBool("Walking", walking);
        }

        currentPos = transform.position;

        if (Utility.RoundToInt(currentPos) != Utility.RoundToInt(nextPos))
        {
            grid.UpdateAgentMoves(currentPos, nextPos, transform.gameObject.name);
            nextPos = currentPos;
        }

        if (transform.name == GameObjectType.PLAYER.ToString())
        {
            UpdateMovement(KeyCode.W, KeyCode.S, KeyCode.D, KeyCode.A, KeyCode.Space);
        }
        else if (transform.name == GameObjectType.AGGRESSIVE_AI.ToString())
        {
            UpdateMovement(KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.Return);
        }
    }

    private void UpdateMovement(KeyCode up, KeyCode down, KeyCode right, KeyCode left, KeyCode dropBomb)
    {
        if (Input.GetKey(up))
        { //Up movement
            rigidBody.velocity = new Vector3(-moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
            transform.rotation = Quaternion.Euler(0, 270, 0);
            walking = true;
            animator.SetBool("Walking", walking);
        }

        if (Input.GetKey(down))
        { //Down movement
            rigidBody.velocity = new Vector3(moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
            transform.rotation = Quaternion.Euler(0, 90, 0);
            walking = true;
            animator.SetBool("Walking", walking);
        }

        if (Input.GetKey(right))
        { //Right movement
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, moveSpeed);
            transform.rotation = Quaternion.Euler(0, 0, 0);
            walking = true;
            animator.SetBool("Walking", walking);
        }

        if (Input.GetKey(left))
        { //Left movement
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, -moveSpeed);
            transform.rotation = Quaternion.Euler(0, 180, 0);
            walking = true;
            animator.SetBool("Walking", walking);
        }

        if (Input.GetKeyDown(dropBomb))
        { //Drop bomb
            DropBomb();
        }
    }

    public void DropBomb()
    {
        if (bombPrefab)
        {
            bombPrefab.name = GameObjectType.AGENT.ToString() + ":" + transform.name;
            checkDropBomb();
            if (dropBomb < maxBomb)
            {
                Vector3 dropPosition = Utility.RoundToInt(transform.position);
                foreach (Vector3 pos in dropPositions)
                {
                    if (dropPosition == pos)
                    {
                        return;
                    }
                }
                GameObject go = Instantiate(bombPrefab, dropPosition, bombPrefab.transform.rotation);
                go.GetComponent<Bomb>().SetGridScript(grid);
            }
            dropBomb = 0;
        }
    }

    private void checkDropBomb()
    {
        GameObject[] bombs = null;
        if (bombs == null)
        {
            bombs = GameObject.FindGameObjectsWithTag(GameObjectType.BOMB.GetTag());
            dropPositions.Clear();
            foreach (GameObject bomb in bombs)
            {
                if (bomb.name == GameObjectType.AGENT.ToString() + ":" + transform.name + "(Clone)")
                {
                    dropBomb++;
                    dropPositions.Add(bomb.transform.position);
                }
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GameObjectType.EXPLOSION.GetTag()))
        {
            Destroy(gameObject);
        }
    }

}
