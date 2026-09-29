using UnityEngine;

public class PlayerDodgingState : PlayerBaseState
{
    private readonly int DodgeHash = Animator.StringToHash("DodgeRoll");
    private const float CrossFadeDuration = 0.1f;

    private readonly Vector3 direction;
    private float remainingDodgeTime;

    public PlayerDodgingState(PlayerStateMachine playerStateMachine, Vector3 direction) : base(playerStateMachine)
    {
        this.direction = direction;
    }

    public override void Enter()
    {
        remainingDodgeTime = playerStateMachine.DodgeDuration;

        playerStateMachine.transform.rotation = Quaternion.LookRotation(direction); // face the roll
        playerStateMachine.Health.SetInvulnerability(true);                         // i-frames on
        playerStateMachine.Animator.CrossFadeInFixedTime(DodgeHash, CrossFadeDuration);
    }

    public override void Tick(float deltaTime)
    {
        Move(direction * playerStateMachine.DodgeDistance / playerStateMachine.DodgeDuration, deltaTime);

        remainingDodgeTime -= deltaTime;
        if (playerStateMachine.DodgeDuration - remainingDodgeTime >= playerStateMachine.DodgeIFramesDuration)
        {
            playerStateMachine.Health.SetInvulnerability(false);
        }

        if (playerStateMachine.InputReader.IsAttacking)
        {
            playerStateMachine.SwitchState(new PlayerAttackingState(playerStateMachine, 0));
            return;
        }

        if (remainingDodgeTime <= 0f)
        {
            ReturnToLocomotion();
        }

        
    }

    public override void Exit()
    {
        playerStateMachine.Health.SetInvulnerability(false);                        // i-frames off
    }
}