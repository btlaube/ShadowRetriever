using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 14f;
    public float acceleration;
    public float airAcceleration;
    public Vector2 jump;
    public float jumpDurationThreshold;
    public float groundCheckEdgeOffset;
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayer;
    public LayerMask groundWallLayer;
    public float regGravityScale;
    public float wallClingGravityScale;

    private Vector3 input;
    [SerializeField] private float jumpDuration;

    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer sr;

    // Determine player size (by collider)
    private float playerWidth;
    private float playerHeight;
    private float widthOffset;
    private float heightOffset;
    private float rayCastLengthCheck = 0.025f;

    private float xVelocity;
    private float yVelocity;
    private float targetXVel;
    private float targetYVel;

    public int currentJumps;
    public int maxJumps;
    private bool isJumping;

    public bool hasWallCling;
    public bool hasDoubleJump;

    // State
    private PlayerState currentState;

    void Awake()
    {
        // Get reference to private components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // Define playerWidth and playerHeight and offsets from collider
        playerWidth = GetComponent<Collider2D>().bounds.extents.x ;
        playerHeight = GetComponent<Collider2D>().bounds.extents.y;
        widthOffset = GetComponent<Collider2D>().offset.x;
        heightOffset = GetComponent<Collider2D>().offset.y;

        // Initial state
        SetState(new IdleState(this));
    }

    void Start()
    {
        maxJumps = 1;
        sr.flipX = true;
    }

    void Update()
    {
        // Debug.Log(currentState is GroundedState);
        // Get input
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        input.z = Input.GetAxis("Jump");

        // Cancel horixontal input for on wall
        int wallDirection = GetWallDirection();
        // flip sprite on wall
        if (wallDirection == -1)
        {
            if (input.x < 0.0f)
                input.x = 0.0f;
        }
        else if (wallDirection == 1)
        {
            if (input.x > 0.0f)
                input.x = 0.0f;
        }


        //Update state
        currentState.Update();
    }

    void FixedUpdate()
    {
        // Use input for physics calculations
        // Ground or air acceleration
        var accelerationRate = PlayerIsOnGround() ? acceleration : airAcceleration;

        // Get rigidbody current x and y velocities
        xVelocity = rb.velocity.x;
        yVelocity = rb.velocity.y;

        // Apply physics to x and y velocites
    
        // Smoothly apply horizontal movment to xVelocity towards the target velocity
        float targetVelocityX = speed * input.x;
        float currentVelocityX = rb.velocity.x;       
        xVelocity = Mathf.MoveTowards(currentVelocityX, targetVelocityX, accelerationRate * Time.fixedDeltaTime);        

        // Apply new x and y velocities
        rb.velocity = new Vector2(xVelocity, yVelocity);

        //Animator and sprite flip
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        // sr.flipX = rb.velocity.x < 0.0f;
        if (rb.velocity.x > 0.0f)
        {
            sr.flipX = false;
        }
        if (rb.velocity.x < 0.0f)
        {
            sr.flipX = true;
        }

        HandleStateTransitions();
    }

    public Vector3 GetInput()
    {
        return input;
    }

    public Animator GetAnimator()
    {
        return animator;
    }

    public float GetJumpDuration()
    {
        return jumpDuration;
    }

    public void IncrementJumpDuration()
    {
        jumpDuration += Time.fixedDeltaTime;
    }

    public void ResetJumpDuration()
    {
        jumpDuration = 0.0f;
    }

    public void SetXVelocity(float xVel)
    {
        xVelocity = xVel;
    }

    public void SetYVelocity(float yVel)
    {
        rb.velocity = new Vector2(rb.velocity.x, yVel);
    }


    public void SetState(PlayerState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;
        currentState.Enter();
    }

    public void SwitchState(PlayerState newState)
    {
        SetState(newState);
    }

    public void SpawnPlayer()
    {
        animator.SetTrigger("Spawn");
    }

    private void HandleStateTransitions()
    {
        if (currentState is IdleState)
        {
            // Transition from Idle to Running
            if (Mathf.Abs(input.x) > 0.1f)
            {
                SwitchState(new RunningState(this));
            }
            else if (!PlayerIsOnGround())
            {
                SwitchState(new FallingState(this));
            }
            else if ((input.y > 0 || input.z > 0) && currentJumps < maxJumps)
            {
                Jump();
                SwitchState(new JumpingState(this));
            }
        }
        else if (currentState is RunningState)
        {
            // Transition from Running to Idle
            if (Mathf.Abs(input.x) < 0.1f)
            {
                SwitchState(new IdleState(this));
            }
            // Transition from Running to Jumping
            else if ((input.y > 0 || input.z > 0) && currentJumps < maxJumps)
            {
                SwitchState(new JumpingState(this));
                Jump();
            }
        }
        else if (currentState is FallingState)
        {
            // Transition from Falling
            // Check Jump
            if (PlayerIsOnGround())
            {
                SwitchState(new IdleState(this));
            }
            else if ((input.y > 0 || input.z > 0) && currentJumps < maxJumps && hasDoubleJump)
            {
                Jump();
                SwitchState(new JumpingState(this));
            }
            else if (PlayerIsOnWall() && hasWallCling)
            {
                SwitchState(new WallClingingState(this));
            }
        }
        else if (currentState is WallClingingState)
        {
            // Transition from WallCling
            if (PlayerIsOnGround())
            {
                SwitchState(new IdleState(this));
            }
            // Transition from WallCling to WallJumping
            else if (input.y > 0 || input.z > 0)
            {
                WallJump();
                SwitchState(new WallJumpingState(this));
            }
            else if (!PlayerIsOnWall())
            {
                SwitchState(new FallingState(this));
            }
        }
        else if (currentState is JumpingState)
        {
            // Transition from Jumping
            if (jumpDuration > jumpDurationThreshold || (input.z == 0 && input.y == 0))
            {
                SwitchState(new FallingState(this));
            }
            if (PlayerIsOnWall() && hasWallCling)
            {
                SwitchState(new WallClingingState(this));
            }
        }
        else if (currentState is WallJumpingState)
        {
            // Transition from Jumping
            if (rb.velocity.y < 0)
            {
                SwitchState(new FallingState(this));
            }
            else if ((input.y > 0 || input.z > 0) && hasDoubleJump)
            {
                Jump();
                SwitchState(new JumpingState(this));
            }
            else if (PlayerIsOnWall() && hasWallCling)
            {
                SwitchState(new WallClingingState(this));
            }
        }
    }

    // Ground check
    public bool PlayerIsOnGround()
    {
        bool groundCheck1 = Physics2D.Raycast(new Vector2(transform.position.x + widthOffset, transform.position.y - playerHeight + heightOffset), -Vector2.up, rayCastLengthCheck, groundLayer);
        bool groundCheck2 = Physics2D.Raycast(new Vector2(transform.position.x + (playerWidth - groundCheckEdgeOffset ) + widthOffset, transform.position.y - playerHeight + heightOffset), -Vector2.up, rayCastLengthCheck, groundLayer);
        bool groundCheck3 = Physics2D.Raycast(new Vector2(transform.position.x - (playerWidth - groundCheckEdgeOffset ), transform.position.y - playerHeight + heightOffset), -Vector2.up, rayCastLengthCheck, groundLayer);

        return groundCheck1;
    }

    public bool PlayerIsOnWall()
    {
        bool wallOnleft = Physics2D.Raycast(new Vector2(transform.position.x - playerWidth + widthOffset, transform.position.y + heightOffset), -Vector2.right, rayCastLengthCheck, groundWallLayer);
        bool wallOnRight = Physics2D.Raycast(new Vector2(transform.position.x + playerWidth + widthOffset, transform.position.y + heightOffset), Vector2.right, rayCastLengthCheck, groundWallLayer);

        return wallOnleft || wallOnRight;
    }

    public int GetWallDirection()
    {
        bool isWallLeft1 = Physics2D.Raycast(new Vector2(transform.position.x - playerWidth + widthOffset, transform.position.y + heightOffset), -Vector2.right, rayCastLengthCheck, groundWallLayer);
        bool isWallLeft2 = Physics2D.Raycast(new Vector2(transform.position.x - playerWidth + widthOffset, transform.position.y + (playerHeight - groundCheckEdgeOffset) + heightOffset), -Vector2.right, rayCastLengthCheck, groundWallLayer);
        bool isWallLeft3 = Physics2D.Raycast(new Vector2(transform.position.x - playerWidth + widthOffset, transform.position.y - (playerHeight - groundCheckEdgeOffset) + heightOffset), -Vector2.right, rayCastLengthCheck, groundWallLayer);

        bool isWallRight1 = Physics2D.Raycast(new Vector2(transform.position.x + playerWidth + widthOffset, transform.position.y + heightOffset), Vector2.right, rayCastLengthCheck, groundWallLayer);
        bool isWallRight2 = Physics2D.Raycast(new Vector2(transform.position.x + playerWidth + widthOffset, transform.position.y + (playerHeight - groundCheckEdgeOffset) + heightOffset), Vector2.right, rayCastLengthCheck, groundWallLayer);
        bool isWallRight3 = Physics2D.Raycast(new Vector2(transform.position.x + playerWidth + widthOffset, transform.position.y - (playerHeight - groundCheckEdgeOffset) + heightOffset), Vector2.right, rayCastLengthCheck, groundWallLayer);

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

    public void UnlockAbility(int ability)
    {
        switch (ability)
        {
            case 1:
                hasWallCling = true;
                break;
            case 2:
                hasDoubleJump = true;
                maxJumps++;
                break;
            default:
                break;
        }
    }

    public void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x * jump.x, jump.y);
        currentJumps++;
        // animator.SetBool("IsJumping", true);
    }

    public void WallJump()
    {
        
        rb.velocity = sr.flipX ? new Vector2(-jump.x, jump.y) : new Vector2(jump.x, jump.y);
        // animator.SetBool("IsJumping", true);
        currentJumps++;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            // Update playerWidth and playerHeight from collider if not playing
            playerWidth = GetComponent<Collider2D>().bounds.extents.x;
            playerHeight = GetComponent<Collider2D>().bounds.extents.y;
        }

        Gizmos.color = Color.red;

        // Draw raycasts for ground check
        Gizmos.DrawLine(new Vector2(transform.position.x + widthOffset, transform.position.y - playerHeight + heightOffset),
                        new Vector2(transform.position.x + widthOffset, transform.position.y - playerHeight + heightOffset - rayCastLengthCheck));
        Gizmos.DrawLine(new Vector2(transform.position.x + (playerWidth - groundCheckEdgeOffset ) + widthOffset, transform.position.y - playerHeight + heightOffset),
                        new Vector2(transform.position.x + (playerWidth - groundCheckEdgeOffset ) + widthOffset, transform.position.y - playerHeight + heightOffset - rayCastLengthCheck));
        Gizmos.DrawLine(new Vector2(transform.position.x - (playerWidth - groundCheckEdgeOffset ) + widthOffset, transform.position.y - playerHeight + heightOffset),
                        new Vector2(transform.position.x - (playerWidth - groundCheckEdgeOffset ) + widthOffset, transform.position.y - playerHeight + heightOffset - rayCastLengthCheck));

        // Draw raycasts for wall check
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector2(transform.position.x - playerWidth + widthOffset, transform.position.y + heightOffset),
                        new Vector2(transform.position.x - playerWidth + widthOffset - rayCastLengthCheck, transform.position.y + heightOffset));
        Gizmos.DrawLine(new Vector2(transform.position.x + playerWidth + widthOffset, transform.position.y + heightOffset),
                        new Vector2(transform.position.x + playerWidth + widthOffset + rayCastLengthCheck, transform.position.y + heightOffset));
    }
}
