using UnityEngine;

public class EnemyChasingState : EnemyBaseState
{
    public EnemyChasingState(EnemyStateMachine enemyStateMachine) : base(enemyStateMachine) { }

    private readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    private readonly int SpeedHash = Animator.StringToHash("Speed");

    private const float AnimatorDampTime = 0.1f;
    private const float CrossFadeDuration = 0.1f;

    public override void Enter()
    {
        enemyStateMachine.Animator.CrossFade(LocomotionHash, CrossFadeDuration);
        enemyStateMachine.isChasingPlayer = true;   // the alert sound now plays in EnemyAlertState, with the taunt
    }

    public override void Exit()
    {
        
    }

    public override void Tick(float deltaTime)
    {
        if (!IsInChaseRange())
        {
            enemyStateMachine.SwitchState(new EnemyIdleState(enemyStateMachine));
            enemyStateMachine.isChasingPlayer = false;
            return;
        }
        else if (IsPlayerWithin(enemyStateMachine.EngageRange))
        {
            enemyStateMachine.SwitchState(new EnemyEngageState(enemyStateMachine));
            return;
        }

        MoveToPlayer(deltaTime);
        FacePlayer();

        enemyStateMachine.Animator.SetFloat(SpeedHash, 1.0f, AnimatorDampTime, deltaTime);


    }

    private void MoveToPlayer(float deltaTime)
    {
        enemyStateMachine.Agent.nextPosition = enemyStateMachine.transform.position;

        if (enemyStateMachine.Agent.isOnNavMesh)
        {
            enemyStateMachine.Agent.destination = enemyStateMachine.Player.transform.position;
            Move(enemyStateMachine.Agent.desiredVelocity.normalized * enemyStateMachine.MovementSpeed, deltaTime);
        }

        enemyStateMachine.Agent.velocity = enemyStateMachine.Controller.velocity;
    }


}
