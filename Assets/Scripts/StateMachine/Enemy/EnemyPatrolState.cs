using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : EnemyBaseState
{
    public EnemyPatrolState(EnemyStateMachine enemyStateMachine) : base(enemyStateMachine){}

    private readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    private readonly int SpeedHash = Animator.StringToHash("Speed");

    private const float CrossFadeDuration = 0.1f;
    private const float AnimatorDampTime = 0.1f;
    private const float ArriveDistance = 0.5f;

    public override void Enter()
    {
        enemyStateMachine.Animator.CrossFade(LocomotionHash, CrossFadeDuration);


    }

    public override void Exit()
    {
        enemyStateMachine.Agent.ResetPath();
        enemyStateMachine.Agent.velocity = Vector3.zero;
    }

    public override void Tick(float deltaTime)
    {
        // 1. spotted the player? chasing wins over patrolling
        if (IsInChaseRange())
        {
            NoticePlayer();
            return;
        }

        // 2. nowhere to go
        if (enemyStateMachine.WayPoints == null || enemyStateMachine.WayPoints.Length == 0)
        {
            enemyStateMachine.SwitchState(new EnemyIdleState(enemyStateMachine));
            return;
        }

        Vector3 wayPoint = enemyStateMachine.WayPoints[enemyStateMachine.CurrentWayIndex].position;

        // 3. arrived: pick the next point FIRST, then go idle
        if (HasArrived(wayPoint))
        {
            enemyStateMachine.AdvanceWayPoint();
            enemyStateMachine.SwitchState(new EnemyIdleState(enemyStateMachine));
            return;
        }

        // 4. otherwise keep walking
        NavigateToWayPoint(wayPoint, deltaTime);
    }



    private void NavigateToWayPoint(Vector3 wayPoint, float deltaTime)
    {
        enemyStateMachine.Agent.nextPosition = enemyStateMachine.transform.position;   // keep the agent synced

        if (enemyStateMachine.Agent.isOnNavMesh)
        {
            enemyStateMachine.Agent.destination = wayPoint;

            Vector3 direction = enemyStateMachine.Agent.desiredVelocity;
            direction.y = 0f;
            direction.Normalize();

            Move(direction * enemyStateMachine.PatrolSpeed, deltaTime);

            if (direction != Vector3.zero)   // turn smoothly toward where we're walking
            {
                enemyStateMachine.transform.rotation = Quaternion.Lerp(enemyStateMachine.transform.rotation,
                    Quaternion.LookRotation(direction), deltaTime * 10f);
            }
        }

        enemyStateMachine.Agent.velocity = enemyStateMachine.Controller.velocity;
        enemyStateMachine.Animator.SetFloat(SpeedHash, 0.5f, AnimatorDampTime, deltaTime);   // walk
    }

    private bool HasArrived(Vector3 wayPoint)
    {
        Vector3 toWayPoint = wayPoint - enemyStateMachine.transform.position;
        toWayPoint.y = 0f;   // ignore height, like your old GetVectorDistanceWithoutY
        return toWayPoint.sqrMagnitude <= ArriveDistance * ArriveDistance;
    }




}
