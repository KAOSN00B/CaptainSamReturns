using UnityEngine;

public class EnemyDeathState : EnemyBaseState
{
    public EnemyDeathState(EnemyStateMachine enemyStateMachine) : base(enemyStateMachine){ }

    private readonly int DeathHash = Animator.StringToHash("Death"); 

    private const float CrossFadeDuration = 0.1f;

    public override void Enter()
    {
        enemyStateMachine.Ragdoll.ToggleRagdoll(true);
        enemyStateMachine.WeaponDamage.gameObject.SetActive(false);

        enemyStateMachine.Animator.CrossFadeInFixedTime(DeathHash, CrossFadeDuration);
    }

    public override void Exit()
    {
       
    }

    public override void Tick(float deltaTime)
    {
        
    }

   
}
