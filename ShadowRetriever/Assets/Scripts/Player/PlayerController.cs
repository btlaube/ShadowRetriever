using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 14f;
    public float acceleration;
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
        SetState(new GroundedState(this));
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
        var accelerationRate = acceleration;

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
    }

    public Vector3 GetInput()
    {
        return input;
    }

    public float GetJumpDuration()
    {
        return jumpDuration;
    }

    public void SetXVelocity(float xVel)
    {
        xVelocity = xVel;
    }

    public void SetYVelocity(float yVel)
    {
        rb.velocity = new Vector2(rb.velocity.x, yVel);
    }

    public void ResetJumpDuration()
    {
        jumpDuration = 0.0f;
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

    public void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jump.y);
        currentJumps++;
        animator.SetBool("IsJumping", true);
    }

    public void WallJump()
    {
        
        rb.velocity = sr.flipX ? new Vector2(-jump.x, jump.y) : new Vector2(jump.x, jump.y);
        animator.SetBool("IsJumping", true);
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

public abstract class PlayerState
{
    protected PlayerController playerController;

    protected PlayerState(PlayerController controller)
    {
        playerController = controller;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}

public class GroundedState : PlayerState
{
    public GroundedState(PlayerController controller) : base(controller) {}

    public override void Enter()
    {
        playerController.animator.SetBool("IsJumping", false);
        playerController.animator.SetBool("IsFalling", false);
        Debug.Log("Enter Grounded");
        playerController.rb.gravityScale = playerController.regGravityScale;
        playerController.currentJumps = 0;
    }

    public override void Update()
    {
        Debug.Log("Grounded");

        Vector3 input = playerController.GetInput();

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            // Ground Jump
            playerController.Jump();
        }

        if (!playerController.PlayerIsOnGround())
        {
            playerController.SwitchState(new FallingState(playerController));
        }
        // else if (playerController.PlayerIsOnWall())
        // {
        //     if (playerController.hasWallCling)
        //         playerController.SwitchState(new WallClingingState(playerController));
        // }
        // drop through one-way platform
        if (input.y < 0 || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            // Get collider stood on and disable
            // Find all colliders within the circle
            Collider2D[] colliders = Physics2D.OverlapCircleAll(playerController.groundCheck.position, playerController.groundCheckRadius);
            
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
    }

    public override void Exit()
    {
        Debug.Log("Exit Grounded");
    }
}

public class FallingState : PlayerState
{
    public FallingState(PlayerController controller) : base(controller) {}

    public override void Enter()
    {
        playerController.animator.SetBool("IsFalling", true);
        Debug.Log("Enter Falling");
        playerController.rb.gravityScale = playerController.regGravityScale;
        // playerController.SetTargetVelocities(playerController.rb.velocity.x, 0.0f); // Use gravity for y velocity
    }

    public override void Update()
    {
        Debug.Log("Falling");
        Vector3 input = playerController.GetInput();

        // Check Jump
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && playerController.currentJumps < playerController.maxJumps)
        {
            if (playerController.hasDoubleJump)
                // Double Jump
                playerController.Jump();
        }

        if (playerController.PlayerIsOnGround())
        {
            playerController.SwitchState(new GroundedState(playerController));
        }
        else if (playerController.PlayerIsOnWall())
        {
            if (playerController.hasWallCling)
                playerController.SwitchState(new WallClingingState(playerController));
        }
    }

    public override void Exit()
    {
        playerController.animator.SetBool("IsFalling", false);
        Debug.Log("Exit Falling");
    }
}

public class WallClingingState : PlayerState
{
    public WallClingingState(PlayerController controller) : base(controller) {}

    public override void Enter()
    {
        playerController.animator.SetBool("IsOnWall", true);
        playerController.animator.SetBool("IsJumping", false);
        Debug.Log("Enter WallClinging");
        playerController.rb.gravityScale = playerController.wallClingGravityScale;
        playerController.SetYVelocity(0.0f);
        playerController.currentJumps = 0;
    }

    public override void Update()
    {
        Debug.Log("WallClinging");
        Vector3 input = playerController.GetInput();

        int wallDirection = playerController.GetWallDirection();
        // flip sprite on wall
        if (wallDirection == -1)
        {
            playerController.sr.flipX = false;
        }
        else if (wallDirection == 1)
        {
            playerController.sr.flipX = true;
        }

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) && playerController.currentJumps < playerController.maxJumps)
        {
            playerController.WallJump();
        }

        if (playerController.PlayerIsOnGround())
        {
            playerController.SwitchState(new GroundedState(playerController));
        }
        else if (!playerController.PlayerIsOnWall())
        {
            playerController.SwitchState(new FallingState(playerController));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exit WallClinging");
        playerController.animator.SetBool("IsOnWall", false);
    }
}

public class JumpingState : PlayerState
{
    public JumpingState(PlayerController controller) : base(controller) {}

    public override void Enter()
    {
        Debug.Log("Enter Jumping");
        playerController.ResetJumpDuration();

        playerController.rb.AddForce(new Vector2(0.0f, playerController.jump.y), ForceMode2D.Impulse);
        playerController.SetYVelocity(playerController.jump.y);

        playerController.currentJumps++;
    }

    public override void Update()
    {
        Debug.Log("Jumping");
        // playerController.SetTargetVelocities(playerController.rb.velocity.x, playerController.jump.y);
        // playerController.rb.AddForce(new Vector2(0.0f, playerController.jump.y));

        Vector3 input = playerController.GetInput();
        float jumpDuration = playerController.GetJumpDuration();

        // if (playerController.PlayerIsOnGround())
        // {
        //     playerController.SwitchState(new GroundedState(playerController));
        // }
        if (playerController.PlayerIsOnWall())
        {
            if (playerController.hasWallCling)
                playerController.SwitchState(new WallClingingState(playerController));
        }
        else if (jumpDuration > playerController.jumpDurationThreshold || input.z <= 0)
        {
            playerController.SwitchState(new FallingState(playerController));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exit Jumping");
        playerController.SetYVelocity(0.0f);
    }
}

public class WallJumpingState : PlayerState
{
    public WallJumpingState(PlayerController controller) : base(controller) {}

    public override void Enter()
    {
        Debug.Log("Enter WallJumping");
        playerController.ResetJumpDuration();

        int wallDirection = playerController.GetWallDirection();
        if (wallDirection == -1)
        {
            // Jump off left wall using opposite of Jump vector (i.e. (xForce=jump.y, yForce=jump.y))
            playerController.rb.AddForce(new Vector2(playerController.jump.y/2, playerController.jump.x/2), ForceMode2D.Impulse);
            // playerController.rb.AddForce(new Vector2(0.0f, playerController.jump.x));
        }
        if (wallDirection == 1)
        {
            // Jump off right wall
            playerController.rb.AddForce(new Vector2(-playerController.jump.y/2, playerController.jump.x), ForceMode2D.Impulse);
            // playerController.rb.AddForce(new Vector2(0.0f, playerController.jump.x));
        }

        playerController.currentJumps++;
    }

    public override void Update()
    {
        Debug.Log("WallJumping");
        // playerController.SetTargetVelocities(playerController.rb.velocity.x, playerController.jump.y);
        // playerController.rb.AddForce(new Vector2(0.0f, playerController.jump.y));

        Vector3 input = playerController.GetInput();
        float jumpDuration = playerController.GetJumpDuration();

        // if (playerController.PlayerIsOnWall())
        // {
        //     playerController.SwitchState(new WallClingingState(playerController));
        // }
        if (jumpDuration > playerController.jumpDurationThreshold || input.z <= 0)
        {
            playerController.SwitchState(new FallingState(playerController));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exit WallJumping");
        playerController.SetYVelocity(0.0f);
    }
}


