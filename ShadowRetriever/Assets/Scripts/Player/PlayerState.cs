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

public class IdleState : PlayerState
{
    public IdleState(PlayerController controller) : base(controller) {}

    public override void Enter()
    {
        Debug.Log("Enter Idle");
        playerController.currentJumps = 0;
    }

    public override void Update()
    {
        Debug.Log("Idle");
        
    }

    public override void Exit()
    {
        Debug.Log("Exit Idle");
    }
}

public class RunningState : PlayerState
{
    public RunningState(PlayerController controller) : base(controller) {}

    public override void Enter()
    {
        Debug.Log("Enter Running");
    }

    public override void Update()
    {
        Debug.Log("Running");
    }

    public override void Exit()
    {
        Debug.Log("Exit Running");
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
        playerAnimator.SetBool("IsJumping", true);
        playerController.Jump();
    }

    public override void Update()
    {
        Debug.Log("Jumping");
        playerController.JumpUpdate();
    }

    public override void Exit()
    {
        Debug.Log("Exit Jumping");
        playerAnimator.SetBool("IsJumping", false);
        playerController.EndJump();
    }
}

public class WallJumpingState : PlayerState
{
    public WallJumpingState(PlayerController controller) : base(controller) {}

    public override void Enter()
    {
        Debug.Log("Enter WallJumping");
        playerAnimator.SetBool("IsJumping", true);
        playerController.Jump();

        // Flip sprite on wall
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

    }

    public override void Update()
    {
        Debug.Log("WallJumping");
    }

    public override void Exit()
    {
        Debug.Log("Exit WallJumping");
    }
}


