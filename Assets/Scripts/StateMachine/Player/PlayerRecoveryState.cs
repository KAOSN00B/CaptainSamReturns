using UnityEngine;

// Plays the "return to idle" animation after the last hit of a combo, so the sword comes back
// to the ready stance smoothly instead of snapping. It is only there for looks: moving, attacking,
// dodging or jumping cancels it straight away, so it never makes the game feel slower.
public class PlayerRecoveryState : PlayerBaseState
{
    private const float CrossFadeDuration = 0.1f;
    private const int BaseLayer = 0;
    private const float AnimationFinished = 1f;
    private const int FirstAttackIndex = 0;

    private readonly int recoveryHash;

    public PlayerRecoveryState(PlayerStateMachine playerStateMachine, string recoveryAnimationName) : base(playerStateMachine)
    {
        recoveryHash = Animator.StringToHash(recoveryAnimationName);
    }

    public override void Enter()
    {
        playerStateMachine.InputReader.DodgeEvent += OnDodge;
        playerStateMachine.InputReader.JumpEvent += OnJump;

        playerStateMachine.Animator.CrossFadeInFixedTime(recoveryHash, CrossFadeDuration);
    }

    public override void Tick(float deltaTime)
    {
        Move(deltaTime);

        if (playerStateMachine.InputReader.IsAttacking)
        {
            playerStateMachine.SwitchState(new PlayerAttackingState(playerStateMachine, FirstAttackIndex));
            return;
        }

        if (playerStateMachine.InputReader.IsShooting)
        {
            playerStateMachine.SwitchState(new PlayerAimingState(playerStateMachine));   // gun out
            return;
        }

        if (playerStateMachine.InputReader.MovementValue != Vector2.zero || RecoveryFinished())
        {
            ReturnToLocomotion();
        }
    }

    public override void Exit()
    {
        playerStateMachine.InputReader.DodgeEvent -= OnDodge;
        playerStateMachine.InputReader.JumpEvent -= OnJump;
    }

    private bool RecoveryFinished()
    {
        Animator animator = playerStateMachine.Animator;
        if (animator.IsInTransition(BaseLayer)) return false;

        AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(BaseLayer);
        return current.shortNameHash == recoveryHash && current.normalizedTime >= AnimationFinished;
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
