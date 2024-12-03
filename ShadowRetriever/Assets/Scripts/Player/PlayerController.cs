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
    private float targetVelocityX;
    private float targetVelocityY;

    public int currentJumps;
    public int maxJumps;
    private bool isJumping;
    public bool shouldJump;

    public bool hasWallCling;
    public bool hasDoubleJump;

    public bool canMove;

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
    
    public Dictionary<string, List<KeyCode>> controlDict;

    void Start()
    {
        maxJumps = 1;
        sr.flipX = true;
        canMove = true;

        controlDict = new Dictionary<string, List<KeyCode>>
        {
            { "Move Up", new List<KeyCode> { KeyCode.W, KeyCode.UpArrow } },
            { "Move Down", new List<KeyCode> { KeyCode.S, KeyCode.DownArrow } },
            { "Move Left", new List<KeyCode> { KeyCode.A, KeyCode.LeftArrow } },
            { "Move Right", new List<KeyCode> { KeyCode.D, KeyCode.RightArrow } },
            { "Jump", new List<KeyCode> { KeyCode.Space } },
            { "Sprint", new List<KeyCode> { KeyCode.LeftShift } },
            { "Interact", new List<KeyCode> { KeyCode.E } },
            { "Use", new List<KeyCode> { KeyCode.F } },
            { "Ability 1", new List<KeyCode> { KeyCode.Q } },
            { "Reload", new List<KeyCode> { KeyCode.R } },
            { "Open Inventory", new List<KeyCode> { KeyCode.Tab } },
            { "Pause Menu", new List<KeyCode> { KeyCode.Escape } }
        };
    } 

    void Update()
    {
        // Get input
        input.x = (IsAnyKeyHeld("Move Left") ? -1 : 0) + (IsAnyKeyHeld("Move Right") ? 1 : 0);
        input.y = (IsAnyKeyDown("Move Down") ? -1 : 0) + (IsAnyKeyDown("Move Up") ? 1 : 0);
        input.z = IsAnyKeyDown("Jump") ? 1 : 0;
        if(!canMove) input = Vector3.zero;

        if (input.y > 0 || input.z > 0)
        {
            shouldJump = true;
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
        targetVelocityX = speed * input.x;
        float currentVelocityX = rb.velocity.x;
        xVelocity = Mathf.MoveTowards(currentVelocityX, targetVelocityX, accelerationRate * Time.fixedDeltaTime);

        // Apply new x and y velocities
        rb.velocity = new Vector2(xVelocity, yVelocity);

        //Animator horizontal movement
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        // Flip sprite horizontal movement
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

    private void HandleStateTransitions()
    {
        if (currentState is IdleState)
        {
            // Transition from Idle to Running
            if (Mathf.Abs(input.x) > 0.1f)
            {
                SwitchState(new RunningState(this));
            }
            else if (shouldJump && currentJumps < maxJumps)
            {
                SwitchState(new JumpingState(this));
            }
            else if (!PlayerIsOnGround())
            {
                SwitchState(new FallingState(this));
            }
        }
        else if (currentState is RunningState)
        {
            // Transition from Running to Idle
            if (Mathf.Abs(input.x) < 0.1f)
            {
                SwitchState(new IdleState(this));
            }
            else if (!PlayerIsOnGround())
            {
                SwitchState(new FallingState(this));
            }
            // Transition from Running to Jumping
            else if (shouldJump && currentJumps < maxJumps)
            {
                SwitchState(new JumpingState(this));
            }
        }
        else if (currentState is FallingState)
        {
            // Transition from Falling
            if (PlayerIsOnGround())
            {
                SwitchState(new IdleState(this));
            }
            else if (shouldJump && currentJumps < maxJumps && hasDoubleJump)
            {
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
            else if (shouldJump && currentJumps < maxJumps)
            {
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
            if (jumpDuration > jumpDurationThreshold || (AllKeysUp("Move Up") && AllKeysUp("Jump")))
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
            // Transition from Wall Jumping
            if (jumpDuration > jumpDurationThreshold || (AllKeysUp("Move Up") && AllKeysUp("Jump")))
            {
                SwitchState(new FallingState(this));
            }
            if (PlayerIsOnWall() && hasWallCling)
            {
                SwitchState(new WallClingingState(this));
            }
        }
    }

    // Input Helper methods
    private bool IsAnyKeyHeld(string action)
    {
        if (controlDict.TryGetValue(action, out var keys))
        {
            foreach (var key in keys)
            {
                if (Input.GetKey(key))
                    return true;
            }
        }
        return false;
    }

    private bool IsAnyKeyDown(string action)
    {
        if (controlDict.TryGetValue(action, out var keys))
        {
            foreach (var key in keys)
            {
                if (Input.GetKeyDown(key))
                    return true;
            }
        }
        return false;
    }

    private bool AllKeysUp(string action)
    {
        if (controlDict.TryGetValue(action, out var keys))
        {
            foreach (var key in keys)
            {
                if (Input.GetKey(key)) // If any key is still held, return false
                    return false;
            }
            return true; // All keys are up
        }
        return true; // Return true if the action has no mapped keys
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

    public void Jump()
    {
        // Jump physics
        rb.velocity = new Vector2(rb.velocity.x * jump.x, jump.y);
        // SetXVelocity(rb.velocity.x * jump.x);
        // Incremenet jump counter
        currentJumps++;
    }

    public void WallJump()
    {
        // rb.velocity = GetWallDirection() ? new Vector2(-jump.y, jump.y) : new Vector2(jump.y, jump.y);
        int wallDirection = GetWallDirection();
        if (wallDirection == -1)
        {
            rb.velocity = new Vector2(jump.y, jump.y);
        }
        else if (wallDirection == 1)
        {
            rb.velocity = new Vector2(-jump.y, jump.y);
        }
        // Incremenet jump counter
        currentJumps++;
    }

    public void JumpUpdate()
    {
        // Continue jump physics
        rb.AddForce(new Vector2(jump.x, jump.y));
        // Increment jump timer
        IncrementJumpDuration();
    }

    public void WallJumpUpdate()
    {
        // Continue wall jump physics
        int wallDirection = GetWallDirection();
        if (wallDirection == -1)
        {
            // Jump off left wall using opposite of Jump vector (i.e. (xForce=jump.y, yForce=jump.y))
            rb.AddForce(new Vector2(jump.y/2, jump.x/2), ForceMode2D.Impulse);
        }
        if (wallDirection == 1)
        {
            // Jump off right wall
            rb.AddForce(new Vector2(-jump.y/2, jump.x), ForceMode2D.Impulse);
        }
        // Increment jump timer
        IncrementJumpDuration();
    }

    public void EndJump()
    {
        ResetJumpDuration();
        shouldJump = false;
        SetTargetYVelocity(0.0f);
    }

    public void CancelInputOnWall()
    {
        int wallDirection = GetWallDirection();
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

    public void SetTargetYVelocity(float targetYVel)
    {
        targetVelocityY = targetYVel;
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


    // Ground check
    public bool PlayerIsOnGround()
    {
        bool groundCheck1 = Physics2D.Raycast(new Vector2(transform.position.x + widthOffset, transform.position.y - playerHeight + heightOffset), -Vector2.up, rayCastLengthCheck, groundLayer);
        bool groundCheck2 = Physics2D.Raycast(new Vector2(transform.position.x + (playerWidth - groundCheckEdgeOffset ) + widthOffset, transform.position.y - playerHeight + heightOffset), -Vector2.up, rayCastLengthCheck, groundLayer);
        bool groundCheck3 = Physics2D.Raycast(new Vector2(transform.position.x - (playerWidth - groundCheckEdgeOffset ), transform.position.y - playerHeight + heightOffset), -Vector2.up, rayCastLengthCheck, groundLayer);

        return groundCheck1 || groundCheck2 || groundCheck3;
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

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            // Update playerWidth and playerHeight from collider if not playing
            playerWidth = GetComponent<Collider2D>().bounds.extents.x;
            playerHeight = GetComponent<Collider2D>().bounds.extents.y;
        }


        // Draw raycasts for ground check
        Gizmos.color = Color.red;
        // Ground check 1
        Gizmos.DrawLine(new Vector2(transform.position.x + widthOffset, transform.position.y - playerHeight + heightOffset),
                        new Vector2(transform.position.x + widthOffset, transform.position.y - playerHeight + heightOffset - rayCastLengthCheck));
        // Ground check 2
        Gizmos.DrawLine(new Vector2(transform.position.x + (playerWidth - groundCheckEdgeOffset ) + widthOffset, transform.position.y - playerHeight + heightOffset),
                        new Vector2(transform.position.x + (playerWidth - groundCheckEdgeOffset ) + widthOffset, transform.position.y - playerHeight + heightOffset - rayCastLengthCheck));
        // Ground check 3
        Gizmos.DrawLine(new Vector2(transform.position.x - (playerWidth - groundCheckEdgeOffset ) + widthOffset, transform.position.y - playerHeight + heightOffset),
                        new Vector2(transform.position.x - (playerWidth - groundCheckEdgeOffset ) + widthOffset, transform.position.y - playerHeight + heightOffset - rayCastLengthCheck));

        // Draw raycasts for wall check
        Gizmos.color = Color.blue;
        // Left wall 1
        Gizmos.DrawLine(new Vector2(transform.position.x - playerWidth + widthOffset, transform.position.y + heightOffset),
                        new Vector2(transform.position.x - playerWidth + widthOffset - rayCastLengthCheck, transform.position.y + heightOffset));
        // Left wall 2
        Gizmos.DrawLine(new Vector2(transform.position.x - playerWidth + widthOffset, transform.position.y + (playerHeight - groundCheckEdgeOffset) + heightOffset),
                        new Vector2(transform.position.x - playerWidth + widthOffset - rayCastLengthCheck, transform.position.y + (playerHeight - groundCheckEdgeOffset) + heightOffset));
        // Left wall 3
        Gizmos.DrawLine(new Vector2(transform.position.x - playerWidth + widthOffset, transform.position.y - (playerHeight - groundCheckEdgeOffset) + heightOffset),
                        new Vector2(transform.position.x - playerWidth + widthOffset - rayCastLengthCheck, transform.position.y - (playerHeight - groundCheckEdgeOffset) + heightOffset));
        
        // Right wall 1
        Gizmos.DrawLine(new Vector2(transform.position.x + playerWidth + widthOffset, transform.position.y + heightOffset),
                        new Vector2(transform.position.x + playerWidth + widthOffset + rayCastLengthCheck, transform.position.y + heightOffset));
        // Right wall 2
        Gizmos.DrawLine(new Vector2(transform.position.x + playerWidth + widthOffset, transform.position.y + (playerHeight - groundCheckEdgeOffset) + heightOffset),
                        new Vector2(transform.position.x + playerWidth + widthOffset + rayCastLengthCheck, transform.position.y + (playerHeight - groundCheckEdgeOffset) + heightOffset));
        // Right wall 3
        Gizmos.DrawLine(new Vector2(transform.position.x + playerWidth + widthOffset, transform.position.y - (playerHeight - groundCheckEdgeOffset) + heightOffset),
                        new Vector2(transform.position.x + playerWidth + widthOffset + rayCastLengthCheck, transform.position.y - (playerHeight - groundCheckEdgeOffset) + heightOffset));
    }
}
