using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public enum PlayerStateEnum
    {
        Idle,
        Running,
        Falling,
        Jumping,
        WallCling,
        WallJumping
    }

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

    // Input vector (horizontal input, vertical input, jump input)
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
    private PlayerStateEnum currentStateEnum;

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
    }

    void Start()
    {
        maxJumps = 1;
        // TODO: Remove the need for flipping sprite??
        sr.flipX = true;

        // Initial state
        SetState(new IdleState(this));
    }

    void Update()
    {
        // Get input
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        input.z = Input.GetAxis("Jump");
    }

    void FixedUpdate()
    {
        // **State Transition Logic**
        HandleStateTransitions(); // NEW: Centralized state transition handling

        // Sprite flip
        if (rb.velocity.x > 0.0f)
        {
            sr.flipX = false;
        }
        if (rb.velocity.x < 0.0f)
        {
            sr.flipX = true;
        }

        // drop through one-way platform
        if (input.y < 0)
        {
            // Get collider stood on and disable
            // Find all colliders within the circle
            Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
            
            foreach (Collider2D collider in colliders)
            {
                // Get the OneWayCollider component from the collider
                OneWayCollider oneWayCollider = collider.GetComponent<OneWayCollider>();
                
                // Check if the component is not null and call SwitchOff()
                if (oneWayCollider != null)
                {
                    oneWayCollider.SwitchOff();
                }
            }
        }

        //Update state
        currentState.Update();
    }

    // Helper functions
    public Vector3 GetInput()
    {
        return input;
    }

    public void CancelHorizontalInput()
    {
        input.x = 0.0f;
    }

    public Animator GetAnimator()
    {
        return animator;
    }

    public void HorizontalMovement()
    {
        var accelerationRate = acceleration;
        xVelocity = rb.velocity.x;
        // Smoothly apply horizontal movment to xVelocity towards the target velocity
        float targetVelocityX = speed * input.x;
        float currentVelocityX = rb.velocity.x;       
        xVelocity = Mathf.MoveTowards(currentVelocityX, targetVelocityX, accelerationRate * Time.fixedDeltaTime);
        rb.velocity = new Vector2(xVelocity, rb.velocity.y);
    }

    public void VerticalMovement()
    {
        var accelerationRate = acceleration;
        yVelocity = rb.velocity.y;
        // Smoothly apply horizontal movment to xVelocity towards the target velocity
        float targetVelocityY = jump.y;
        float currentVelocityY = rb.velocity.y;       
        yVelocity = Mathf.MoveTowards(currentVelocityY, targetVelocityY, accelerationRate * Time.fixedDeltaTime);
        rb.velocity = new Vector2(rb.velocity.x, yVelocity);
    }

    public void SpawnPlayer()
    {
        animator.SetTrigger("Spawn");
    }

    public void StartJumpTimer()
    {
        jumpDuration = 0.0f;
    }

    public void IncrementJumpTimer()
    {
        jumpDuration += Time.fixedDeltaTime;
    }

    public void ResetJumpTimer()
    {
        jumpDuration = 0.0f;
    }

    // **NEW: Centralized State Transition Logic**
    private void HandleStateTransitions()
    {
        if (currentStateEnum == PlayerStateEnum.Idle)
        {
            // Transition from Idle
                // To Running
            if (input.x != 0)
            {
                SwitchState(new RunningState(this));
            }
                // To Falling
            else if (!PlayerIsOnGround())
            {
                SwitchState(new FallingState(this));
            }
            //     // To Jumping
            // else if ((input.y > 0 || input.z > 0) && CanJump())
            // {
            //     SwitchState(new JumpingState(this));
            //     Jump();
            // }
        }
        else if (currentStateEnum == PlayerStateEnum.Running)
        {
            // Transition from Running
                // To Idle
            if (input.x == 0.0f)
            {
                SwitchState(new IdleState(this));
            }
            // To Falling
            else if (!PlayerIsOnGround())
            {
                SwitchState(new FallingState(this));
            }
                // To jumping
            else if ((input.y > 0 || input.z > 0))
            {
                SwitchState(new JumpingState(this));
            }
        }
        else if (currentStateEnum == PlayerStateEnum.Falling)
        {
            // Transition from Falling
                // To idle
            if (PlayerIsOnGround())
            {
                SwitchState(new IdleState(this));
            }
                // To jumping
            // else if ((input.y > 0 || input.z > 0) && hasDoubleJump && CanJump())
            // {
            //     SwitchState(new JumpingState(this));
            //     Jump();
            // }
                // To wallcling
            // else if (PlayerIsOnWall() && hasWallCling)
            // {
            //     SwitchState(new WallClingState(this));
            // }
        }
        else if (currentStateEnum == PlayerStateEnum.Jumping)
        {
            // Transition from Jumping
                // To WallCling
            // if (PlayerIsOnWall() && hasWallCling)
            // {
            //     SwitchState(new WallClingState(this));
            // }
                // To Falling
            if ((input.y == 0 && input.z == 0) || jumpDuration >= jumpDurationThreshold)
            {
                SwitchState(new FallingState(this));
            }
        }
        // else if (currentStateEnum == PlayerStateEnum.WallCling)
        // {
        //     // Transition from WallCling
        //         // To Idle
        //     if (PlayerIsOnGround())
        //     {
        //         SwitchState(new IdleState(this));
        //     }
        //         // To Wall Jump
        //     else if (input.y > 0 || input.z > 0)
        //     {
        //         SwitchState(new WallJumpingState(this));
        //         Jump();
        //     }
        //         // To Falling
        //     else if (!PlayerIsOnWall())
        //     {
        //         SwitchState(new FallingState(this));
        //     }
        // }
        // else if (currentStateEnum == PlayerStateEnum.WallJumping)
        // {
        //     // Transition from WallJumping
        //         // To WallCling
        //     if (PlayerIsOnWall() && hasWallCling)
        //     {
        //         SwitchState(new WallClingState(this));
        //     }
        //         // To Falling
        //     else if ((input.y > 0 && input.z > 0))
        //     {
        //         SwitchState(new FallingState(this));
        //     }
        // }
    }

    // **UPDATED: SwitchState ensures the state enum is synced**
    public void SwitchState(PlayerState newState)
    {
        SetState(newState);
        SetStateEnum(newState.GetStateEnum());
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

    public void SetStateEnum(PlayerStateEnum newStateEnum)
    {
        currentStateEnum = newStateEnum;
    }

    // Collisions
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
                maxJumps = 2;
                break;
            default:
                break;
        }
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

