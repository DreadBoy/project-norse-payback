using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Controller : MonoBehaviour
{
    public bool facingRight = true;         // For determining which way the player is currently facing.
    public bool jump = false;               // Condition for whether the player should jump.


    public float speed = 2f;             // The fastest the player can travel in the x axis.
    public float jumpSpeed = 0.2f;         // Amount of force added when the player jumps.
    public float gravity = 9.806f;
    public float terminalVelocity = 50f;


    public Transform groundCheck;          // A position marking where to check if the player is grounded.
    public bool grounded = false;          // Whether or not the player is grounded.
    Vector3 groundRay;
    float timeSinceFall = 0;

    Rigidbody2D rigidbody2D;

    float lastH;
    float verticalSpeed;

    void Awake()
    {
        // Setting up references.
        if (groundCheck == null)
            groundCheck = transform.Find("groundCheck");
        groundRay = new Vector3(0, (groundCheck.position - transform.position).y);
    }

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Environment"));

        grounded = Physics2D.Linecast(transform.position, transform.position + groundRay, 1 << LayerMask.NameToLayer("Environment"));
        if (!grounded)
            timeSinceFall += Time.deltaTime;

        if (Input.GetButtonDown("Jump") && grounded)
            jump = true;
    }


    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float horizontal = rigidbody2D.position.x;
        float vertical = rigidbody2D.position.y;

        if (Mathf.Abs(h) > 0 && Mathf.Abs(h) >= lastH)
            horizontal = rigidbody2D.position.x + Time.fixedDeltaTime * speed * (h / Mathf.Abs(h));

        if (h > 0 && !facingRight || h < 0 && facingRight)
            Flip();

        if (!grounded)
        {
            verticalSpeed -= gravity * Time.fixedDeltaTime;
            if (verticalSpeed < -terminalVelocity)
                verticalSpeed = -terminalVelocity;
            vertical = rigidbody2D.position.y + verticalSpeed;
        }
        else if (jump)
        {
            verticalSpeed = jumpSpeed;
            vertical = rigidbody2D.position.y + verticalSpeed;
            jump = false;
            timeSinceFall = 0;
        }

        rigidbody2D.MovePosition(new Vector2(horizontal, vertical));
        lastH = Mathf.Abs(h);
    }


    void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
