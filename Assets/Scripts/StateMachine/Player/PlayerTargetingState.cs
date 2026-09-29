using UnityEngine;

public class PlayerTargetingState : PlayerBaseState
{
    public PlayerTargetingState(PlayerStateMachine playerStateMachine) : base(playerStateMachine) { }
    private readonly int targetingBlendTreeHash = Animator.StringToHash("TargetingBlendTree"); //convert string to int for better speed.
    private readonly int targetingForwardHash = Animator.StringToHash("TargetingForwardSpeed");
    private readonly int targetingRightHash = Animator.StringToHash("TargetingRightSpeed");

    private const float CrossFadeDuration = 0.1f;
    private const float dampTime = 0.1f;

    public override void Enter()
    {
        playerStateMachine.InputReader.RemoveTargetEvent += OnCancel;
        playerStateMachine.InputReader.TargetEvent += OnCancel;
        playerStateMachine.InputReader.SwitchTargetEvent += playerStateMachine.Targeter.SwitchTarget;

        playerStateMachine.InputReader.DodgeEvent += OnDodge;
        playerStateMachine.InputReader.JumpEvent += OnJump;
        playerStateMachine.Animator.SetFloat(targetingForwardHash, 0f);
        playerStateMachine.Animator.SetFloat(targetingRightHash, 0f);
        playerStateMachine.Animator.CrossFadeInFixedTime(targetingBlendTreeHash, CrossFadeDuration);
    }


    public override void Tick(float deltaTime)
    {
        if (playerStateMachine.InputReader.IsAttacking)
        {
            playerStateMachine.SwitchState(new PlayerAttackingState(playerStateMachine, 0));
            return;
        }

        if (playerStateMachine.Targeter.CurrentTarget == null)
        {
            playerStateMachine.SwitchState(new PlayerFreeLookState(playerStateMachine));
            return;
        }

        if (playerStateMachine.InputReader.IsBlocking)
        {
            playerStateMachine.SwitchState(new PlayerBlockingState(playerStateMachine));
            return;
        }


        // stepped off an edge: Falling keeps the lock-on through small gaps and decides when to let go
        if (playerStateMachine.Controller.velocity.y <= 0.0f && !playerStateMachine.Controller.isGrounded)
        {
            playerStateMachine.SwitchState(new PlayerFallingState(playerStateMachine));
            return;
        }

        Vector3 movement = CalculateMovement();

        Move(movement * playerStateMachine.TargetingMovementSpeed, deltaTime);

        UpdateAnimator(movement, deltaTime);

        FaceTarget();



    }

    public override void Exit()
    {
        playerStateMachine.InputReader.RemoveTargetEvent -= OnCancel;
        playerStateMachine.InputReader.TargetEvent -= OnCancel;
        playerStateMachine.InputReader.SwitchTargetEvent -= playerStateMachine.Targeter.SwitchTarget;
        playerStateMachine.InputReader.DodgeEvent -= OnDodge;
        playerStateMachine.InputReader.JumpEvent -= OnJump;
    }


    private void OnCancel()
    {
        playerStateMachine.Targeter.Cancel();

        playerStateMachine.SwitchState(new PlayerFreeLookState(playerStateMachine));
    }

    private void UpdateAnimator(Vector3 movement, float deltaTime)
    {
        // world-space movement -> the character's own forward (z) and right (x)
        Vector3 local = playerStateMachine.transform.InverseTransformDirection(movement);

        if (Mathf.Abs(local.z) < 0.1f)
        {
            playerStateMachine.Animator.SetFloat(targetingForwardHash, 0, dampTime, deltaTime);
        }
        else
        {
            float value = local.z > 0 ? 1f : -1f;
            playerStateMachine.Animator.SetFloat(targetingForwardHash, value, dampTime, deltaTime);
        }

        if (Mathf.Abs(local.x) < 0.1f)
        {
            playerStateMachine.Animator.SetFloat(targetingRightHash, 0, dampTime, deltaTime);
        }
        else
        {
            float value = local.x > 0 ? 1f : -1f;
            playerStateMachine.Animator.SetFloat(targetingRightHash, value, dampTime, deltaTime);
        }
    }

    private void OnDodge()
    {
        TryDodge(CalculateMovement());
    }

    private void OnJump()
    {
        playerStateMachine.SwitchState(new PlayerJumpingState(playerStateMachine));
    }
}
