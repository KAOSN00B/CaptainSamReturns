using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class EnemyEngageState : EnemyBaseState
{
    public EnemyEngageState(EnemyStateMachine enemyStateMachine) : base(enemyStateMachine) { }

    private readonly int StrafingBlendTreeHash = Animator.StringToHash("StrafingBlendTree");
    private readonly int StrafeForwardHash = Animator.StringToHash("StrafeForward");
    private readonly int StrafeRightHash = Animator.StringToHash("StrafeRight");

    private const float CrossFadeDuration = 0.1f;
    private const float AnimatorDampTime = 0.1f;
    private const float EscapeRangeMultiplier = 1.5f;  
    private const float CircleDistanceRatio = 0.8f;    // circle at 80% of EngageRange

    private float waitTimer;
    private float circleDirection;   // 1 = clockwise, -1 = counter-clockwise
    private float flipTimer;

    public override void Enter()
    {
        waitTimer = Random.Range(enemyStateMachine.MinEngageTime, enemyStateMachine.MaxEngageTime);

        if (Random.value < 0.5f)
            circleDirection = 1f;
        else
            circleDirection = -1f;

        enemyStateMachine.Animator.SetFloat(StrafeForwardHash, 0f);   // no stale values
        enemyStateMachine.Animator.SetFloat(StrafeRightHash, 0f);
        enemyStateMachine.Animator.CrossFade(StrafingBlendTreeHash, CrossFadeDuration);

        flipTimer = RandomFlipTime();
    }

    public override void Exit()
    {

    }

    public override void Tick(float deltaTime)
    {
        FacePlayer();

        if (PlayerGotAway())
        {
            enemyStateMachine.SwitchState(new EnemyChasingState(enemyStateMachine));
            return;
        }

        waitTimer -= deltaTime;

        if (IsPlayerWithin(enemyStateMachine.AttackRange))
            waitTimer = 0f;   // you walked into my reach, so I attack NOW

        if (waitTimer > 0f)
        {
            CirclePlayer(deltaTime);
        }
        else if (IsPlayerWithin(enemyStateMachine.AttackRange))
        {
            enemyStateMachine.SwitchState(new EnemyAttackingState(enemyStateMachine));
        }
        else
        {
            WalkUpToPlayer(deltaTime);
        }
    }

    private bool PlayerGotAway()
    {
        return !IsPlayerWithin(enemyStateMachine.EngageRange * EscapeRangeMultiplier);
    }

    private void CirclePlayer(float deltaTime)
    {
        flipTimer -= deltaTime;
        if (flipTimer <= 0f)
        {
            circleDirection *= -1f;                    // switch direction
            flipTimer = RandomFlipTime();
        }

        Vector3 sideways = Vector3.Cross(Vector3.up, DirectionToPlayer()) * circleDirection;
        Vector3 inOrOut = DirectionToPlayer() * DistanceCorrection();

        Vector3 movement = (sideways + inOrOut).normalized;
        Move(movement * enemyStateMachine.StrafeSpeed, deltaTime);

        UpdateStrafeAnimation(movement, deltaTime);
    }

    private float RandomFlipTime()
    {
        return Random.Range(enemyStateMachine.MinCircleFlipTime, enemyStateMachine.MaxCircleFlipTime);
    }

    private float DistanceCorrection()
    {
        float circleDistance = enemyStateMachine.EngageRange * CircleDistanceRatio;
        float howFarOff = DistanceToPlayer() - circleDistance;

        return Mathf.Clamp(howFarOff, -1f, 1f);
    }

    private void WalkUpToPlayer(float deltaTime)
    {
        Move(DirectionToPlayer() * enemyStateMachine.MovementSpeed, deltaTime);

        UpdateStrafeAnimation(DirectionToPlayer(), deltaTime);
    }

    private void UpdateStrafeAnimation(Vector3 movement, float deltaTime)
    {

        Vector3 local = enemyStateMachine.transform.InverseTransformDirection(movement);

        enemyStateMachine.Animator.SetFloat(StrafeRightHash, local.x, AnimatorDampTime, deltaTime);
        enemyStateMachine.Animator.SetFloat(StrafeForwardHash, local.z, AnimatorDampTime, deltaTime);
    }
}