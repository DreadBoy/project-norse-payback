using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using EnvInfo = Raycasting.EnvInfo;
using Raycast = Raycasting.Raycast;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Raycasting))]
public class Controller : MonoBehaviour
{
    public float speed = 5f;
    public float jumpSpeed = 0.5f;
    public float gravity = 0.0f;
    public float wallGravity = 1.5f;
    public float terminalVelocity = 0.1f;
    public float wallTerminalVelocity = 0.08f;

    class InputPoll
    {
        public bool aboutToJump = false;
        public float axisHorizontal = 0f;
    }
    InputPoll inputPoll = new InputPoll();

    [HideInInspector]
    public Rigidbody2D rigidbody2D;
    [HideInInspector]
    public Collider2D collider2D;
    [HideInInspector]
    public Raycasting raycasting;
    Animator animator;

    float lastH;
    float verticalSpeed;
    float horizontalSpeed;

    public State state = State.idle;
    Facing facing = Facing.right;

    void Awake()
    {
    }

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        raycasting = GetComponent<Raycasting>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump") && isOnGround())
            inputPoll.aboutToJump = true;
        inputPoll.axisHorizontal = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        float horizontal = rigidbody2D.position.x;
        float vertical = rigidbody2D.position.y;

        //flip sprite if needed
        if (inputPoll.axisHorizontal > 0 && facing == Facing.left || inputPoll.axisHorizontal < 0 && facing == Facing.right)
            Flip();


        //potential horizontal movement
        horizontalSpeed = Time.fixedDeltaTime * speed * (inputPoll.axisHorizontal / Mathf.Abs(inputPoll.axisHorizontal));
        if (float.IsNaN(horizontalSpeed))
            horizontalSpeed = 0;

        //raycast
        EnvInfo envInfo = new EnvInfo();
        float marginVert = 0;
        if (verticalSpeed < 0)
            marginVert = -verticalSpeed;
        raycasting.raycastGround(collider2D.bounds, marginVert, ref envInfo);

        float marginHoz = Mathf.Abs(horizontalSpeed);
        raycasting.raycastForward(collider2D.bounds, facing, marginHoz, ref envInfo);

        //horizontal movement
        //if we are in air or on horizontal surface, just move
        if (!envInfo.inwall)
        {
            horizontal = rigidbody2D.position.x + horizontalSpeed;
        }
        //otherwise reset speed and move to wall
        if (envInfo.inwall)
        {
            horizontalSpeed = 0;
            horizontal = horizontal + (int)facing * envInfo.wallDistance;
        }
        lastH = Mathf.Abs(inputPoll.axisHorizontal);


        //vertical movement
        //falling down and hit a floor in this frame
        if (envInfo.grounded && verticalSpeed < 0)
        {
            //stop falling and move to floor level
            verticalSpeed = 0;
            vertical = vertical - envInfo.groundDistance + raycasting.groundMargin / 2;
        }
        //is about to jump
        else if (inputPoll.aboutToJump)
        {
            verticalSpeed = jumpSpeed;
            vertical = rigidbody2D.position.y + verticalSpeed;
            inputPoll.aboutToJump = false;
        }
        //is still jumping or falling
        else if (!envInfo.grounded)
        {
            var grav = envInfo.inwall ? wallGravity : gravity;
            verticalSpeed -= grav * Time.fixedDeltaTime;
            var term = envInfo.inwall ? wallTerminalVelocity : terminalVelocity;
            if (verticalSpeed < -term)
                verticalSpeed = -term;
            vertical = rigidbody2D.position.y + verticalSpeed;

        }
        //want to move up the slope
        else if (envInfo.grounded && envInfo.onslope && envInfo.facingslope)
        {

        }
        //want to down the slope
        else if (envInfo.grounded && envInfo.onslope && !envInfo.facingslope)
        {

        }
        //animate correct sprite
        if (envInfo.grounded && horizontalSpeed != 0)
            state = State.running;
        else if (envInfo.grounded)
            state = State.idle;
        else if (!envInfo.grounded && envInfo.inwall)
            state = State.wallsliding;
        else if (verticalSpeed > 0)
            state = State.jumping;
        else if (verticalSpeed <= 0)
            state = State.falling;
        animator.SetInteger("State", (int)state);


        rigidbody2D.position = new Vector2(horizontal, vertical);
        //rigidbody2D.MovePosition(new Vector2(horizontal, vertical));
    }

    void Flip()
    {
        facing = facing == Facing.right ? Facing.left : Facing.right;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    bool isOnGround()
    {
        return state == State.idle || state == State.running || state == State.wallsliding;
    }

    public enum State
    {
        idle = 0,
        running = 1,
        jumping = 2,
        falling = 3,
        wallsliding = 4
    }

    public enum Facing
    {
        right = 1,
        left = -1
    }
}
