using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Controller : MonoBehaviour
{
    bool facingRight = true;
    bool jump = false;


    public float speed = 2f;
    public float jumpSpeed = 0.2f;
    public float gravity = 9.806f;
    public float terminalVelocity = 50f;


    [Range(0, 1)]
    public float groundCheck = 0.1f;
    bool grounded = false;
    [HideInInspector]
    public Vector2 groundRay;
    float timeSinceFall = 0;
    [HideInInspector]
    public float raycastPrecision = 5;

    [HideInInspector]
    public Rigidbody2D rigidbody2D;
    [HideInInspector]
    public Collider2D collider2D;

    float lastH;
    float verticalSpeed;

    void Awake()
    {
        groundRay = new Vector2(0, -groundCheck);
    }

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();
    }

    void Update()
    {
        //grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Environment"));

        //grounded = Physics2D.Linecast(transform.position, (Vector2)transform.position + groundRay, 1 << LayerMask.NameToLayer("Environment"));

        grounded = false;

        var bounds = collider2D.bounds;
        for (float i = 0; i <= raycastPrecision; i++)
        {
            Vector2 from = new Vector2(
                from.x = bounds.center.x - bounds.extents.x + i / raycastPrecision * 2 * bounds.extents.x,
                bounds.center.y - bounds.extents.y
            );
            Vector2 to = from + groundRay;
            if (Physics2D.Linecast(from, to, 1 << LayerMask.NameToLayer("Environment")))
            {
                grounded = true;
                break;
            }
        }


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
