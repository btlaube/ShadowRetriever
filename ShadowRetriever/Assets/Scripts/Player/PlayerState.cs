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
    public abstract PlayerStateEnum GetStateEnum();
}

public class IdleState : PlayerState
{
    public IdleState(PlayerController controller) : base(controller) {}

    public override void Enter()
    {
        Debug.Log("Enter Idle");
        playerAnimator.SetBool("Idle", true);
    }

    public override void Update()
    {
        Debug.Log("Idle");
        
    }

    public override void Exit()
    {
        Debug.Log("Exit Idle");
        playerAnimator.SetBool("Idle", false);
    }
    public override PlayerStateEnum GetStateEnum() => PlayerStateEnum.Idle;
}

public class RunningState : PlayerState
{
    public RunningState(PlayerController controller) : base(controller) {}

    public override void Enter()
    {
        Debug.Log("Enter Running");
        playerAnimator.SetBool("Running", true);
    }

    public override void Update()
    {
        Debug.Log("Running");
        playerController.HorizontalMovement();

    }

    public override void Exit()
    {
        Debug.Log("Exit Running");
        playerAnimator.SetBool("Running", false);
    }
    public override PlayerStateEnum GetStateEnum() => PlayerStateEnum.Running;
}

public class FallingState : PlayerState
{
    public FallingState(PlayerController controller) : base(controller) {}

    public override void Enter()
    {
        Debug.Log("Enter Falling");
        playerAnimator.SetBool("Falling", true);
    }

    public override void Update()
    {
        Debug.Log("Falling");
    }

    public override void Exit()
    {
        Debug.Log("Exit Falling");
        playerAnimator.SetBool("Falling", false);
    }
    public override PlayerStateEnum GetStateEnum() => PlayerStateEnum.Falling;
}

public class JumpingState : PlayerState
{
    public JumpingState(PlayerController controller) : base(controller) {}

    public override void Enter()
    {
        Debug.Log("Enter Jumping");
        playerAnimator.SetBool("Jumping", true);
        playerController.StartJumpTimer();
    }

    public override void Update()
    {
        Debug.Log("Jumping");
        playerController.IncrementJumpTimer();
        playerController.VerticalMovement();
    }

    public override void Exit()
    {
        Debug.Log("Exit Jumping");
        playerAnimator.SetBool("Jumping", false);
        playerController.ResetJumpTimer();
    }
    public override PlayerStateEnum GetStateEnum() => PlayerStateEnum.Jumping;
}

// public class WallClingState : PlayerState
// {
//     public WallClingState(PlayerController controller) : base(controller) {}

//     public override void Enter()
//     {
//         Debug.Log("Enter WallCling");
//         playerAnimator.SetBool("WallCling", true);
//     }

//     public override void Update()
//     {
//         Debug.Log("WallCling");
//         Vector3 input = playerController.GetInput();
//         // Cancel horixontal input for on wall
//         int wallDirection = playerController.GetWallDirection();
//         if (wallDirection == -1)
//         {
//             if (input.x < 0.0f)
//                 playerController.CancelHorizontalInput();
//         }
//         else if (wallDirection == 1)
//         {
//             if (input.x > 0.0f)
//                 playerController.CancelHorizontalInput();
//         }
//     }

//     public override void Exit()
//     {
//         Debug.Log("Exit WallCling");
//         playerAnimator.SetBool("WallCling", false);
//     }
//     public override PlayerStateEnum GetStateEnum() => PlayerStateEnum.WallCling;
// }


// public class WallJumpingState : PlayerState
// {
//     public WallJumpingState(PlayerController controller) : base(controller) {}

//     public override void Enter()
//     {
//         Debug.Log("Enter WallJumping");
//         playerAnimator.SetBool("WallJumping", true);
//     }

//     public override void Update()
//     {
//         Debug.Log("WallJumping");
//     }

//     public override void Exit()
//     {
//         Debug.Log("Exit WallJumping");
//         playerAnimator.SetBool("WallJumping", false);
//     }
//     public override PlayerStateEnum GetStateEnum() => PlayerStateEnum.WallJumping;
// }