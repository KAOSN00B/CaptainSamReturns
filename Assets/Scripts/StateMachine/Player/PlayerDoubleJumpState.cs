using UnityEngine;

public class PlayerDoubleJumpState : PlayerBaseState
{
    public PlayerDoubleJumpState(PlayerStateMachine playerStateMachine) : base(playerStateMachine) { }

    private readonly int JumpHash = Animator.StringToHash("DoubleJump");

    private const float CrossFadeDuration = 0.1f;

    private Vector3 momentum;

    public override void Enter()
    {
        playerStateMachine.SetUsedDoubleJump(true);
        playerStateMachine.SetUsedJump(true);   // any jump counts - stops the fall after it from offering a coyote jump
        playerStateMachine.ForceReceiver.DoubleJump(playerStateMachine.JumpForce);

        momentum = playerStateMachine.Controller.velocity;
        momentum.y = 0.0f;

        playerStateMachine.Animator.CrossFadeInFixedTime(JumpHash, CrossFadeDuration);
        playerStateMachine.PlayMovementFeedback(playerStateMachine.DoubleJumpFeedback);
    }

    public override void Exit()
    {
        
    }

    public override void Tick(float deltaTime)
    {

        momentum = MovementWhileInAir(momentum, deltaTime);
        Move(momentum, deltaTime);


        if (playerStateMachine.Controller.velocity.y <= 0.0f)
        {
            playerStateMachine.SwitchState(new PlayerFallingState(playerStateMachine));
            return;
        }

        //if (AttackPressedThisFrame())
        //{
        //    playerStateMachine.SwitchState(new PlayerJumpAttackState(playerStateMachine));
        //    return;
        //}


        if (playerStateMachine.Targeter.CurrentTarget != null)
            FaceTarget();                                  // locked on: keep eyes on the enemy
        else
            FaceMovementDirection(momentum, deltaTime);    // free: turn to face where you're flying
    }

}
