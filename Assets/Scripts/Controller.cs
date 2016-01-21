using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
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

    [Range(0, 0.1f)]
    public float groundMargin = 0.01f;
    [HideInInspector]
    public float raycastPrecision = 5;
    [HideInInspector]
    public float raycastMargin = 0.01f;

    [HideInInspector]
    public Rigidbody2D rigidbody2D;
    [HideInInspector]
    public Collider2D collider2D;
    Animator animator;

    float lastH;
    float verticalSpeed;
    float horizontalSpeed;

    public State state = State.idle;
    Facing facing = Facing.right;

    //Scene Editor info
    public struct Raycast
    {
        public Vector2 from;
        public Vector2 to;
    };
    [HideInInspector]
    public List<Raycast> RaycastHits = new List<Raycast>();


    void Awake()
    {
    }

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
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

        RaycastHits.Clear();
        List<Raycast> rays;
        //raycast
        var grounded = raycastGround(verticalSpeed < 0 ? Mathf.Abs(verticalSpeed) : groundMargin, out rays);
        RaycastHits.AddRange(rays);
        var inwall = raycastForward(horizontalSpeed, out rays);
        RaycastHits.AddRange(rays);

        //horizontal movement
        horizontalSpeed = 0;
        if (Mathf.Abs(inputPoll.axisHorizontal) > 0 && Mathf.Abs(inputPoll.axisHorizontal) >= lastH && !inwall)
            horizontalSpeed = Time.fixedDeltaTime * speed * (inputPoll.axisHorizontal / Mathf.Abs(inputPoll.axisHorizontal));
        horizontal = rigidbody2D.position.x + horizontalSpeed;
        lastH = Mathf.Abs(inputPoll.axisHorizontal);


        //vertical movement
        //falling down and hit a floor in this frame
        if(grounded && verticalSpeed < 0)
        {
            //stop falling and move to floor level
            verticalSpeed = 0;
            vertical -= grounded.distance - groundMargin / 2;
        }
        //is about to jump
        if (inputPoll.aboutToJump)
        {
            verticalSpeed = jumpSpeed;
            vertical = rigidbody2D.position.y + verticalSpeed;
            inputPoll.aboutToJump = false;
        }
        //is still jumping or falling
        else if (!grounded)
        {
            var grav = inwall ? wallGravity : gravity;
            verticalSpeed -= grav * Time.fixedDeltaTime;
            var term = inwall ? wallTerminalVelocity : terminalVelocity;
            if (verticalSpeed < -term)
                verticalSpeed = -term;
            vertical = rigidbody2D.position.y + verticalSpeed;

        }

        //animate correct sprite
        if (grounded && horizontalSpeed == 0)
            state = State.idle;
        else if (grounded && horizontalSpeed != 0)
            state = State.running;
        else if (!grounded && inwall)
            state = State.wallsliding;
        else if (verticalSpeed > 0)
            state = State.jumping;
        else if (verticalSpeed <= 0)
            state = State.falling;
        animator.SetInteger("State", (int)state);


        rigidbody2D.MovePosition(new Vector2(horizontal, vertical));
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

    bool raycastGround(float margin)
    {
        List<Raycast> rays = new List<Raycast>();
        return raycastGround(margin, out rays);
    }
    RaycastHit2D raycastGround(float margin, out List<Raycast> rays)
    {
        var bounds = collider2D.bounds;
        bool grounded = false;

        rays = new List<Raycast>();
        RaycastHit2D rayHit = new RaycastHit2D();

        for (float i = 0; i <= raycastPrecision; i++)
        {
            Vector2 from = new Vector2(
                bounds.min.x + i / raycastPrecision * 2 * bounds.extents.x,
                bounds.min.y
            );
            Vector2 to = from;
            to.y -= margin;
            
            rays.Add(new Raycast() { from = from, to = to });
            rayHit = Physics2D.Raycast(from, to - from, (to - from).magnitude, 1 << LayerMask.NameToLayer("Environment"));
            if (rayHit)
            {
                grounded = true;
                break;
            }
        }

        return grounded ? rayHit : new RaycastHit2D();
    }


    bool raycastForward(float margin)
    {
        List<Raycast> rays = new List<Raycast>();
        return raycastForward(margin, out rays);
    }
    RaycastHit2D raycastForward(float margin, out List<Raycast> rays)
    {
        var bounds = collider2D.bounds;
        bool inwall = false;

        rays = new List<Raycast>();
        RaycastHit2D rayHit = new RaycastHit2D();

        for (float i = 0; i <= raycastPrecision; i++)
        {
            Vector2 from = new Vector2(
                facing == Facing.right ? bounds.max.x + raycastMargin : bounds.min.x - raycastMargin,
                bounds.min.y + i / raycastPrecision * 2 * bounds.extents.y
            );
            Vector2 to = from;
            to.x += margin;

            rays.Add(new Raycast() { from = from, to = to });
            rayHit = Physics2D.Raycast(from, to - from, (to -from).magnitude, 1 << LayerMask.NameToLayer("Environment"));
            if (rayHit)
            {
                inwall = true;
                break;
            }
        }
        return inwall ? rayHit : new RaycastHit2D();
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
