using UnityEngine;

public class PlayerImpactState : PlayerBaseState
{
    public PlayerImpactState(PlayerStateMachine playerStateMachine) : base(playerStateMachine){ }
    private const string LightReaction = "Hurt";
    private const string HeavyReaction = "Stagger";

    private const float CrossFadeDuration = 0.1f;
    private float remainingHitStun;

    public override void Enter()
    {
        remainingHitStun = playerStateMachine.HitStunDuration;

        Health health = playerStateMachine.Health;
        string reaction = health.LastDamage >= playerStateMachine.HeavyHitDamage ? HeavyReaction : LightReaction;
        string state = HitDirection.StateName(reaction, playerStateMachine.transform, health.LastHitFrom);

        playerStateMachine.Animator.CrossFadeInFixedTime(state, CrossFadeDuration);
    }

    public override void Exit()
    {
        
    }

    public override void Tick(float deltaTime)
    {

        Move(deltaTime);
        
        remainingHitStun -= deltaTime;

        if (remainingHitStun <= 0.0f)
        {
            ReturnToLocomotion();
        }
    }


}
