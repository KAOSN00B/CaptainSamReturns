using UnityEngine;

public class PlayerFreeLookState : PlayerBaseState
{
    public PlayerFreeLookState(PlayerStateMachine playerStateMachine) : base(playerStateMachine) { }
    private readonly int freeLookSpeedHash = Animator.StringToHash("FreeLookSpeed"); //convert string to int for better speed.
    private readonly int freeLookBlendTreeHash = Animator.StringToHash("FreeLookBlendTree");

    private const float AnimatorDampTime = 0.1f;
    private const float CrossFadeDuration = 0.1f;


    public override void Enter()
    {
        playerStateMachine.InputReader.JumpEvent += OnJump;
        playerStateMachine.InputReader.DodgeEvent += OnDodge;

        playerStateMachine.Animator.SetFloat(freeLookSpeedHash, 0f); // clear the stale run value left from before the attack
        playerStateMachine.Animator.CrossFadeInFixedTime(freeLookBlendTreeHash, CrossFadeDuration);
    }


    public override void Tick(float deltaTime)
    {
        if (playerStateMachine.InputReader.IsAttacking)
        {
            playerStateMachine.SwitchState(new PlayerAttackingState(playerStateMachine, 0));
            return;
        }

        if (playerStateMachine.InputReader.IsBlocking)
        {
            playerStateMachine.SwitchState(new PlayerBlockingState(playerStateMachine));
            return;
        }

        if (playerStateMachine.InputReader.IsShooting)
        {
            playerStateMachine.SwitchState(new PlayerAimingState(playerStateMachine));   // gun out
            return;
        }



        Vector3 movement = CalculateMovement();

        Move((movement * playerStateMachine.FreeLookMovementSpeed), deltaTime);

        if (playerStateMachine.InputReader.MovementValue == Vector2.zero)
        {
            // 0 is move value
            playerStateMachine.Animator.SetFloat(freeLookSpeedHash, 0f, AnimatorDampTime, deltaTime);
            return;
        }


        if (playerStateMachine.Controller.velocity.y <= 0.0f && !playerStateMachine.Controller.isGrounded)
        {
            playerStateMachine.SwitchState(new PlayerFallingState(playerStateMachine));
            return;
        }


        //1 is move value
        playerStateMachine.Animator.SetFloat(freeLookSpeedHash, 1f, AnimatorDampTime, deltaTime);
        FaceMovementDirection(movement, deltaTime);

    }

    public override void Exit()
    {
        playerStateMachine.InputReader.JumpEvent -= OnJump;
        playerStateMachine.InputReader.DodgeEvent -= OnDodge;
    }


    private void OnJump()
    {
        playerStateMachine.SwitchState(new PlayerJumpingState(playerStateMachine));
    }

    private void OnDodge()
    {
        TryDodge(CalculateMovement());
    }



}
