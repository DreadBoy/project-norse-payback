using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class Controller : MonoBehaviour
{
    public float speed = 5f;
    public float jumpSpeed = 0.25f;
    public float gravity = 2.5f;
    public float terminalVelocity = 0.1f;

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
    public float raycastMargin = 0.1f;

    [HideInInspector]
    public Rigidbody2D rigidbody2D;
    [HideInInspector]
    public Collider2D collider2D;
    Animator animator;

    float lastH;
    float verticalSpeed;

    State state = State.grounded;
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
        if (Input.GetButtonDown("Jump") && state == State.grounded)
            inputPoll.aboutToJump = true;
        inputPoll.axisHorizontal = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        float horizontal = rigidbody2D.position.x;
        float vertical = rigidbody2D.position.y;
        var bounds = collider2D.bounds;

        if (inputPoll.axisHorizontal > 0 && facing == Facing.left || inputPoll.axisHorizontal < 0 && facing == Facing.right)
            Flip();

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

        if (Mathf.Abs(inputPoll.axisHorizontal) > 0 && Mathf.Abs(inputPoll.axisHorizontal) >= lastH && !inwall)
        {
            var delta = Time.fixedDeltaTime * speed * (inputPoll.axisHorizontal / Mathf.Abs(inputPoll.axisHorizontal));
            horizontal = rigidbody2D.position.x + delta;
        }


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

        if (grounded)
            state = State.grounded;
        if (!grounded)
        {
            verticalSpeed -= gravity * Time.fixedDeltaTime;
            if (verticalSpeed < -terminalVelocity)
                verticalSpeed = -terminalVelocity;
            vertical = rigidbody2D.position.y + verticalSpeed;

            state = verticalSpeed > 0 ? State.jumping : State.falling;
        }
        
        if (inputPoll.aboutToJump)
        {
            verticalSpeed = jumpSpeed;
            vertical = rigidbody2D.position.y + verticalSpeed;
            inputPoll.aboutToJump = false;
        }

        animator.SetBool("Run", horizontal != rigidbody2D.position.x);


        rigidbody2D.MovePosition(new Vector2(horizontal, vertical));
        lastH = Mathf.Abs(inputPoll.axisHorizontal);
    }


    void Flip()
    {
        facing = facing == Facing.right ? Facing.left : Facing.right;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public enum State
    {
        grounded,
        jumping,
        falling,
        wallsliding
    }

    public enum Facing
    {
        right = 1,
        left = -1
    }
}
