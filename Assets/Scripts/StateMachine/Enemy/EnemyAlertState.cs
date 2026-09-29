using UnityEngine;

// The enemy just spotted the player: it stops, pops a "!" over its head, shouts and taunts,
// then goes after them. Gives the player a beat to react (and a chance to punish a showboating enemy).
public class EnemyAlertState : EnemyBaseState
{
    public EnemyAlertState(EnemyStateMachine enemyStateMachine) : base(enemyStateMachine) { }

    private readonly int TauntHash = Animator.StringToHash("Taunt");

    private const float CrossFadeDuration = 0.1f;
    private const float AlertVolume = 1f;

    private float remainingAlertTime;

    public override void Enter()
    {
        remainingAlertTime = enemyStateMachine.AlertDuration;
        enemyStateMachine.isChasingPlayer = true;   // noticed you - won't taunt again until it loses you

        enemyStateMachine.Animator.CrossFadeInFixedTime(TauntHash, CrossFadeDuration);

        if (enemyStateMachine.AlertSound != null)
            SFXManager.instance.PlaySoundFXClip(enemyStateMachine.AlertSound, enemyStateMachine.transform, AlertVolume);

        if (enemyStateMachine.AlertEffect != null)
            Object.Instantiate(enemyStateMachine.AlertEffect, enemyStateMachine.AlertEffectPoint);   // rides along above the head
    }

    public override void Tick(float deltaTime)
    {
        Move(deltaTime);   // keeps gravity and knockback working
        FacePlayer();

        remainingAlertTime -= deltaTime;
        if (remainingAlertTime <= 0f)
        {
            enemyStateMachine.SwitchState(new EnemyChasingState(enemyStateMachine));
        }
    }

    public override void Exit()
    {

    }
}
