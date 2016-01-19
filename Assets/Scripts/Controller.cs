using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class Controller : MonoBehaviour
{
    public float speed = 5f;
    public float jumpSpeed = 0.25f;
    public float gravity = 2.5f;
    public float wallGravity = 1.5f;
    public float terminalVelocity = 0.1f;
    public float wallTerminalVelocity = 0.08f;

    class InputPoll
    {
        public bool aboutToJump = false;
        public float axisHorizontal = 0f;
    }
    InputPoll inputPoll = new InputPoll();


    [Range(0, 1)]
    public float groundCheck = 0.1f;
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
        
        //raycast
        var grounded = raycastGround();
        var inwall = raycastForward();

        //horizontal movement
        float delta = 0;
        if (Mathf.Abs(inputPoll.axisHorizontal) > 0 && Mathf.Abs(inputPoll.axisHorizontal) >= lastH && !inwall)
            delta = Time.fixedDeltaTime * speed * (inputPoll.axisHorizontal / Mathf.Abs(inputPoll.axisHorizontal));
        horizontal = rigidbody2D.position.x + delta;
        lastH = Mathf.Abs(inputPoll.axisHorizontal);


        //vertical movement
        if (inputPoll.aboutToJump)
        {
            verticalSpeed = jumpSpeed;
            vertical = rigidbody2D.position.y + verticalSpeed;
            inputPoll.aboutToJump = false;
        }
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
        if (grounded && delta == 0)
            state = State.idle;
        else if (grounded && delta != 0)
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

    bool raycastGround()
    {
        var bounds = collider2D.bounds;
        bool grounded = false;

        for (float i = 0; i <= raycastPrecision; i++)
        {
            Vector2 from = new Vector2(
                bounds.min.x + i / raycastPrecision * 2 * bounds.extents.x,
                bounds.min.y
            );
            Vector2 to = from;
            to.y -= groundCheck;

            if (Physics2D.Linecast(from, to, 1 << LayerMask.NameToLayer("Environment")))
            {
                grounded = true;
                break;
            }
        }

        return grounded;
    }

    bool raycastForward()
    {
        var bounds = collider2D.bounds;
        bool inwall = false;

        for (float i = 0; i <= raycastPrecision; i++)
        {
            Vector2 from = new Vector2(
                facing == Facing.right ? bounds.max.x + raycastMargin : bounds.min.x - raycastMargin,
                bounds.min.y + i / raycastPrecision * 2 * bounds.extents.y
            );
            Vector2 to = from;
            to.x += groundCheck;

            if (Physics2D.Linecast(from, to, 1 << LayerMask.NameToLayer("Environment")))
            {
                inwall = true;
                break;
            }
        }
        return inwall;
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
