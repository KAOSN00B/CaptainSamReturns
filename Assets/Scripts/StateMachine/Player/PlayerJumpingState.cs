using UnityEngine;

public class PlayerJumpingState : PlayerBaseState
{
    public PlayerJumpingState(PlayerStateMachine playerStateMachine) : base(playerStateMachine) { }

    private readonly int JumpHash = Animator.StringToHash("Jump");

    private const float CrossFadeDuration = 0.1f;

    private Vector3 momentum;

    public override void Enter()
    {
        playerStateMachine.SetUsedJump(true);
        playerStateMachine.ForceReceiver.Jump(playerStateMachine.JumpForce);

        momentum = playerStateMachine.Controller.velocity;
        momentum.y = 0.0f;

        playerStateMachine.Animator.CrossFadeInFixedTime(JumpHash, CrossFadeDuration);
        playerStateMachine.PlayMovementFeedback(playerStateMachine.JumpFeedback);

        playerStateMachine.InputReader.JumpEvent += OnJump;
    }

    public override void Exit()
    {
        playerStateMachine.InputReader.JumpEvent -= OnJump;  
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

        if (AttackPressedThisFrame())
        {
            playerStateMachine.SwitchState(new PlayerJumpAttackState(playerStateMachine));
            return;
        }


        FaceMovementDirection(momentum, deltaTime);
    }

    private void OnJump()   
    {
        TryDoubleJump();
    }

}
