using UnityEngine;

// Your guard took too many hits: the block is knocked open and you're stuck for a moment - the punish window.
public class PlayerGuardBreakState : PlayerBaseState
{
    public PlayerGuardBreakState(PlayerStateMachine playerStateMachine) : base(playerStateMachine) { }

    private readonly int GuardBreakHash = Animator.StringToHash("GuardBreak");

    private const float CrossFadeDuration = 0.1f;
    private float remainingStun;

    public override void Enter()
    {
        remainingStun = playerStateMachine.GuardBreakDuration;
        playerStateMachine.Animator.CrossFadeInFixedTime(GuardBreakHash, CrossFadeDuration);
    }

    public override void Tick(float deltaTime)
    {
        Move(deltaTime);

        remainingStun -= deltaTime;
        if (remainingStun <= 0f)
        {
            ReturnToLocomotion();
        }
    }

    public override void Exit()
    {

    }
}
