using UnityEngine;

public abstract class EnemyBaseState : State
{
    protected EnemyStateMachine enemyStateMachine;


    public EnemyBaseState(EnemyStateMachine enemyStateMachine)
    {
        this.enemyStateMachine = enemyStateMachine;
    }

    protected void Move(float deltaTime)
    {
        Vector3 noMotions = Vector3.zero;
        Move(noMotions, deltaTime);
    }

    protected void Move(Vector3 motion, float deltaTime)
    {
        enemyStateMachine.Controller.Move((motion +
            enemyStateMachine.ForceReceiver.Movement) * deltaTime);
    }

    protected bool IsInChaseRange()
    {

        if (enemyStateMachine.Player.isDead) return false;

        //square magnitute calculations for better performance rather then just magnitutde
        float playerDistanceSquare = (enemyStateMachine.Player.transform.position
            - enemyStateMachine.transform.position).sqrMagnitude;

        return playerDistanceSquare <= enemyStateMachine.PlayerChasingRange * enemyStateMachine.PlayerChasingRange;

    }

    protected void FacePlayer()
    {
        if (enemyStateMachine.Player == null) { return; }

        Vector3 lookPos = enemyStateMachine.Player.transform.position -
            enemyStateMachine.transform.position;
        lookPos.y = 0f;

        enemyStateMachine.transform.rotation = Quaternion.LookRotation(lookPos);
    }


    protected bool IsPlayerWithin(float range)
    {
        if (enemyStateMachine.Player.isDead) return false;

        float playerDistanceSquare = (enemyStateMachine.Player.transform.position
            - enemyStateMachine.transform.position).sqrMagnitude;

        return playerDistanceSquare <= range * range;  //range squared
    }

    // the player just came into range: taunt first, unless this is an ambush or we were already after them
    protected void NoticePlayer()
    {
        if (enemyStateMachine.SkipAlert || enemyStateMachine.isChasingPlayer)
            enemyStateMachine.SwitchState(new EnemyChasingState(enemyStateMachine));
        else
            enemyStateMachine.SwitchState(new EnemyAlertState(enemyStateMachine));
    }

    // distance float and vectors both ignore height with toplayer.y
    protected Vector3 DirectionToPlayer()
    {
        Vector3 toPlayer = enemyStateMachine.Player.transform.position - enemyStateMachine.transform.position;
        toPlayer.y = 0f;
        return toPlayer.normalized;
    }

    protected float DistanceToPlayer()
    {
        Vector3 toPlayer = enemyStateMachine.Player.transform.position - enemyStateMachine.transform.position;
        toPlayer.y = 0f;
        return toPlayer.magnitude;
    }





}
