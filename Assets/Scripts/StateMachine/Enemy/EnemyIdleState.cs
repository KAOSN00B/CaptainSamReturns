using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(EnemyStateMachine enemyStateMachine) : base(enemyStateMachine) {}

    private readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    private readonly int SpeedHash = Animator.StringToHash("Speed");


    private const float CrossFadeDuration = 0.1f;
    private const float AnimatorDampTime = 0.1f;
    private float remainingIdleTime;

    public override void Enter()
    {
        remainingIdleTime = enemyStateMachine.IdleDuration;
        enemyStateMachine.Animator.CrossFade(LocomotionHash, CrossFadeDuration);
    }

    public override void Exit() 
    {
        enemyStateMachine.Agent.ResetPath(); //clears path no longer chases player
        enemyStateMachine.Agent.velocity = Vector3.zero;    
    }

    public override void Tick(float deltaTime)
    {
        Move(deltaTime);

        if (IsInChaseRange())
        {
            NoticePlayer();
            return;
        }

        enemyStateMachine.Animator.SetFloat(SpeedHash, 0, AnimatorDampTime, deltaTime);

        remainingIdleTime -= deltaTime;

        if (remainingIdleTime <= 0f && enemyStateMachine.WayPoints != null && enemyStateMachine.WayPoints.Length > 0)
        {
            enemyStateMachine.SwitchState(new EnemyPatrolState(enemyStateMachine));
        }
    }





}
