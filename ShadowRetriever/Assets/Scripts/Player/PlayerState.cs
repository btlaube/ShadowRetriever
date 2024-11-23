using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController playerController;
    protected Animator playerAnimator;

    protected PlayerState(PlayerController controller)
    {
        playerController = controller;
        playerAnimator = playerController.GetAnimator();
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
        playerAnimator.SetTrigger("IsJumping");
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
        playerAnimator.SetBool("IsFalling", true);
        Debug.Log("Enter Falling");
        playerController.rb.gravityScale = playerController.regGravityScale;
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
        playerAnimator.SetBool("IsFalling", false);
        Debug.Log("Exit Falling");
    }
}

public class WallClingingState : PlayerState
{
    public WallClingingState(PlayerController controller) : base(controller) {}

    public override void Enter()
    {
        playerAnimator.SetBool("IsOnWall", true);
        playerAnimator.SetBool("IsJumping", false);
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
        playerAnimator.SetBool("IsOnWall", false);
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

        Vector3 input = playerController.GetInput();
        float jumpDuration = playerController.GetJumpDuration();

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
        }
        if (wallDirection == 1)
        {
            // Jump off right wall
            playerController.rb.AddForce(new Vector2(-playerController.jump.y/2, playerController.jump.x), ForceMode2D.Impulse);
        }

        playerController.currentJumps++;
    }

    public override void Update()
    {
        Debug.Log("WallJumping");

        Vector3 input = playerController.GetInput();
        float jumpDuration = playerController.GetJumpDuration();

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


