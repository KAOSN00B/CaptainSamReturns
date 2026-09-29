using UnityEngine;

public class EnemyImpactState : EnemyBaseState
{
    public EnemyImpactState(EnemyStateMachine enemyStateMachine) : base(enemyStateMachine){}
    private const string StaggerReaction = "Stagger";   // the enemy only reaches this state when its poise breaks

    private const float CrossFadeDuration = 0.1f;

    private float duration = 1.0f;
    private GameObject stunEffect;

    public override void Enter()
    {
        enemyStateMachine.StopFlinch();   // the big stagger replaces the small flinch from the same hit

        string state = HitDirection.StateName(StaggerReaction, enemyStateMachine.transform, enemyStateMachine.Health.LastHitFrom);
        enemyStateMachine.Animator.CrossFadeInFixedTime(state, CrossFadeDuration);

        // dizzy stars circle the head for as long as the stagger lasts - "hit it now!"
        if (enemyStateMachine.StunEffect != null)
            stunEffect = Object.Instantiate(enemyStateMachine.StunEffect, enemyStateMachine.StunEffectPoint);
    }

    public override void Exit()
    {
        if (stunEffect != null) Object.Destroy(stunEffect);
    }

    public override void Tick(float deltaTime)
    {
        Move(deltaTime);
        duration -= deltaTime;

        if (duration <= 0.0f)
        {
            enemyStateMachine.SwitchState(new EnemyIdleState(enemyStateMachine));
        }
    }
}
