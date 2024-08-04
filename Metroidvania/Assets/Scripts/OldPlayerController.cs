using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldPlayerController : MonoBehaviour
{
    public float speed = 14f;
    public float accel = 6f;
    public float jumpSpeed = 8f;
    public float jumpDurationThreshold = 2.25f;
    public float airAccel = 3f;
    public float jump = 14f;

    private Vector2 input;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Animator animator;   
    private float rayCastLengthCheck = 0.025f;
    private float jumpDuration;
    private float width;
    private float height;
    private float widthOffset;
    private float heightOffset;
    private bool isJumping;
    private bool isFalling;
    private bool isOnWall;
    private int maxJumps = 2;
    private int currentJumps = 0;

    [SerializeField] private bool gecko;
    [SerializeField] private bool magnet;

    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius;
    [SerializeField] private bool isDropping;
    private Collider2D currentDropCollider;

    public LayerMask wallLayer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        width = GetComponent<Collider2D>().bounds.extents.x + 0.001f;
        height = GetComponent<Collider2D>().bounds.extents.y + 0.001f;
        widthOffset = GetComponent<Collider2D>().offset.x;
        heightOffset = GetComponent<Collider2D>().offset.y;        
    }

    void Update()
    {
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Jump");

        // Check pass through for one-way platforms
        // if (Input.GetAxis("Vertical") < 0.0f)
        // {
        //     if (!isDropping)
        //     {
        //         // Start dropping through a new platform
        //         Collider2D groundCollider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        //         if (groundCollider != null && groundCollider.gameObject.GetComponent<OneWayCollider>() != null)
        //         {
        //             // Set this platform to drop-through mode
        //             currentDropCollider = groundCollider;
        //             currentDropCollider.gameObject.GetComponent<OneWayCollider>().SwitchOff();
        //             isDropping = true;
        //         }
        //     }
        // }
        // else
        // {
        //     // Reset when not pressing the down button
        //     if (isDropping)
        //     {
        //         // Clear the drop state
        //         if (currentDropCollider != null)
        //         {
        //             currentDropCollider = null;
        //         }
        //         isDropping = false;
        //     }
        // }

        if (input.y >= 1f && currentJumps < maxJumps && jumpDuration < jumpDurationThreshold)
        {
            // Debug.Log($"{jumpDuration}, {input.y}, {currentJumps}, {isJumping}");
            if (!isJumping)
            {
                isJumping = true;
                currentJumps++;
                jumpDuration = 0f;
            }
            jumpDuration += Time.deltaTime;
            // animator.SetBool("IsJumping", true);
            // Debug.Log($"{jumpDuration}, {input.y}, {currentJumps}, {isJumping}");
            // Debug.Log($"");
        }
        else if (input.y < 1 || currentJumps == maxJumps)
        {
            isJumping = false;
            // animator.SetBool("IsJumping", false);
            jumpDuration = 0f;
        }

        if (PlayerIsOnGround() && !isJumping)
        {
            currentJumps = 0; // Reset jumps when on ground
            if (input.y > 0f)
            {
                isJumping = true;
            }
            // animator.SetBool("IsOnWall", false);
        }

        if (jumpDuration > jumpDurationThreshold || currentJumps > maxJumps) input.y = 0f;

        // Control gravity for wall slide
        // if (gecko)
        // {
        //     if (PlayerIsOnWall() && !PlayerIsOnGround())
        //     {
        //         rb.gravityScale = 0.1f;
        //         if (GetWallDirection() == -1)
        //         {
        //             sr.flipX = false;
        //         }
        //         else if (GetWallDirection() == 1)
        //         {
        //             sr.flipX = true;
        //         }
        //     }
        //     else
        //     {
        //         rb.gravityScale = 1f;
        //     }
        // }

        // Control gravity for ceiling cling if Magnet is active
        // if (magnet)
        // {
        //     if (PlayerIsOnCeiling())
        //     {
        //         rb.gravityScale = 0f;
        //     }
        //     else if (!PlayerIsOnWall())
        //     {
        //         rb.gravityScale = 1f;
        //     }
        // }

        // animator.SetFloat("Speed", Mathf.Abs(input.x));

        // if (!PlayerIsOnCeiling() && !PlayerIsOnGround() && !PlayerIsOnWall() && !isJumping)
        // {
        //     animator.SetBool("IsFalling", true);
        // }
        // else
        // {
        //     animator.SetBool("IsFalling", false);
        // }
    }

    void FixedUpdate()
    {
        var acceleration = PlayerIsOnGround() || PlayerIsOnCeiling() ? accel : airAccel;

        var xVelocity = PlayerIsOnGround() && input.x == 0 ? 0f : rb.velocity.x;

        var yVelocity = rb.velocity.y;
        // if (PlayerIsOnGround() && input.y > 0)
        // {
        //     yVelocity = jump;
        // }
        // else if (PlayerIsOnWall() && input.y > 0 && gecko)
        // {
        //     yVelocity = jump;
        // }
        // // else if (PlayerIsOnCeiling() && input.y > 0)
        // // {
        // //     yVelocity = -jump;
        // // }
        // else
        // {
        //     yVelocity = rb.velocity.y;
        // }

        // Controls movement on x-axis
        // Prevents movement on x-axis depending on if player is on a wall to prevent player from clinging to wall
        if (GetWallDirection() == -1)
        {
            if (input.x > 0)
            {
                rb.AddForce(new Vector2(((input.x * speed) - rb.velocity.x) * acceleration, 0));
            }
        }
        else if (GetWallDirection() == 1)
        {
            if (input.x < 0)
            {
                rb.AddForce(new Vector2(((input.x * speed) - rb.velocity.x) * acceleration, 0));
            }
        }
        else
        {
            rb.AddForce(new Vector2(((input.x * speed) - rb.velocity.x) * acceleration, 0));
        }

        

        // Controls movement on x-axis for wall ing
        // if (PlayerIsOnWall() && !PlayerIsOnGround() && input.y == 1 && gecko)
        // {
        //     // rb.velocity = new Vector2(-GetWallDirection() * speed * 0.75f, rb.velocity.y);
        //     xVelocity = -GetWallDirection() * speed * 0.75f;
        //     // animator.SetBool("IsOnWall", false);
        //     // animator.SetBool("IsJumping", true);
        // }
        // else if (!PlayerIsOnWall())
        // {
        //     animator.SetBool("IsOnWall", false);
        //     animator.SetBool("IsJumping", true);
        // }
        // if (PlayerIsOnWall() && !PlayerIsOnGround() && gecko)
        // {
        //     animator.SetBool("IsOnWall", true);
        // }

        rb.velocity = new Vector2(xVelocity, yVelocity);

        // if (isJumping && jumpDuration < jumpDurationThreshold)
        // {
        //     rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        // }
        // else
        // {
        //     // Debug.Log($"{isJumping}, {jumpDuration}");
        //     // isJumping = false;
        //     animator.SetBool("IsJumping", false);
        // }
    }

    public bool PlayerIsOnGround()
    {
        bool groundCheck1 = Physics2D.Raycast(new Vector2(transform.position.x + widthOffset, transform.position.y - height + heightOffset), -Vector2.up, rayCastLengthCheck, groundLayer);
        bool groundCheck2 = Physics2D.Raycast(new Vector2(transform.position.x + (width - 0.001f) + widthOffset, transform.position.y - height + heightOffset), -Vector2.up, rayCastLengthCheck, groundLayer);
        bool groundCheck3 = Physics2D.Raycast(new Vector2(transform.position.x - (width - 0.001f), transform.position.y - height + heightOffset), -Vector2.up, rayCastLengthCheck, groundLayer);

        return groundCheck1 || groundCheck2 || groundCheck3;
    }

    public bool PlayerIsOnWall()
    {
        bool wallOnleft = Physics2D.Raycast(new Vector2(transform.position.x - width + widthOffset, transform.position.y + heightOffset), -Vector2.right, rayCastLengthCheck, wallLayer);
        bool wallOnRight = Physics2D.Raycast(new Vector2(transform.position.x + width + widthOffset, transform.position.y + heightOffset), Vector2.right, rayCastLengthCheck, wallLayer);

        return wallOnleft || wallOnRight;
    }

    public bool PlayerIsOnCeiling()
    {
        bool groundCheck1 = Physics2D.Raycast(new Vector2(transform.position.x + widthOffset, transform.position.y + height + heightOffset), Vector2.up, rayCastLengthCheck);
        bool groundCheck2 = Physics2D.Raycast(new Vector2(transform.position.x + (width - 0.001f) + widthOffset, transform.position.y + height + heightOffset), Vector2.up, rayCastLengthCheck);
        bool groundCheck3 = Physics2D.Raycast(new Vector2(transform.position.x - (width - 0.001f) + widthOffset, transform.position.y + height + heightOffset), Vector2.up, rayCastLengthCheck);

        return groundCheck1 || groundCheck2 || groundCheck3;
    }

    public bool PlayerIsTouchingGroundOrWall()
    {
        return PlayerIsOnGround() || PlayerIsOnWall();
    }

    public int GetWallDirection()
    {
        bool isWallLeft1 = Physics2D.Raycast(new Vector2(transform.position.x - width + widthOffset, transform.position.y + heightOffset), -Vector2.right, rayCastLengthCheck, wallLayer);
        bool isWallLeft2 = Physics2D.Raycast(new Vector2(transform.position.x - width + widthOffset, transform.position.y + (height - 0.001f) + heightOffset), -Vector2.right, rayCastLengthCheck, wallLayer);
        bool isWallLeft3 = Physics2D.Raycast(new Vector2(transform.position.x - width + widthOffset, transform.position.y - (height - 0.001f) + heightOffset), -Vector2.right, rayCastLengthCheck, wallLayer);

        bool isWallRight1 = Physics2D.Raycast(new Vector2(transform.position.x + width + widthOffset, transform.position.y + heightOffset), Vector2.right, rayCastLengthCheck, wallLayer);
        bool isWallRight2 = Physics2D.Raycast(new Vector2(transform.position.x + width + widthOffset, transform.position.y + (height - 0.001f) + heightOffset), Vector2.right, rayCastLengthCheck, wallLayer);
        bool isWallRight3 = Physics2D.Raycast(new Vector2(transform.position.x + width + widthOffset, transform.position.y - (height - 0.001f) + heightOffset), Vector2.right, rayCastLengthCheck, wallLayer);

        if (isWallLeft1 || isWallLeft2 || isWallLeft3)
        {
            return -1;
        }
        else if (isWallRight1 || isWallRight2 || isWallRight3)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public void SpawnPlayer()
    {
        animator.SetTrigger("Spawn");
    }

}
