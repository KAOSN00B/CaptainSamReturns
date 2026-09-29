using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyAttackingState : EnemyBaseState
{
    public EnemyAttackingState(EnemyStateMachine enemyStateMachine) : base(enemyStateMachine) { }

    private readonly int AttackHash = Animator.StringToHash("Attack");

    private const float TransitionDuration = 0.1f;
    private const int BaseLayer = 0;
    private const float ClipStart = 0f;
    private float holdTimer;
    private AttackPhase phase;
    

    public override void Enter()
    {
        phase = AttackPhase.Windup;
        enemyStateMachine.WeaponDamage.SetAttack(enemyStateMachine.AttackDamage, enemyStateMachine.AttackKnockBack);
        if (enemyStateMachine.Animator.GetCurrentAnimatorStateInfo(BaseLayer).IsTag("Attack"))
            enemyStateMachine.Animator.Play(AttackHash, BaseLayer, ClipStart);   // combo follow-up: restart the swing from the top
        else
            enemyStateMachine.Animator.CrossFadeInFixedTime(AttackHash, TransitionDuration);
    }

    public override void Exit()
    {
        enemyStateMachine.Animator.speed = 1f; //always restores speed back to 1;
        enemyStateMachine.WeaponHandler.DisableWeapon();
    }

    public override void Tick(float deltaTime)
    {
        float normalizedTime = GetNormailzedTime(enemyStateMachine.Animator);

        switch (phase)
        {
            case AttackPhase.Windup:
                if (normalizedTime >= enemyStateMachine.WindupPoseTime)
                {
                    if (Random.value < enemyStateMachine.QuickAttackChance)
                        ReleaseSwing();   // quick attack: no hold, no flash
                    else
                        StartHold();
                }

                FacePlayer();
                break;

            case AttackPhase.Hold:
                FacePlayer();
                holdTimer -= deltaTime;
                if (holdTimer < 0f)
                {
                    ReleaseSwing();
                }
                break;

            case AttackPhase.Swing:
                if (normalizedTime >= 1.0f)
                {
                    if (Random.value < enemyStateMachine.ComboChance && IsPlayerWithin(enemyStateMachine.AttackRange * enemyStateMachine.ComboRangeMultiplier))
                        enemyStateMachine.SwitchState(new EnemyAttackingState(enemyStateMachine));  // follow-up swing
                    else
                        enemyStateMachine.SwitchState(new EnemyEngageState(enemyStateMachine));     // back off and circle
                }
                break;

        }
        

    }

    private void StartHold()
    {

        enemyStateMachine.HitFlash.PlayFlash(enemyStateMachine.TellColor, enemyStateMachine.TellFlashDuration);

        if (enemyStateMachine.TellSound != null)
        {
            SFXManager.instance.PlaySoundFXClip(enemyStateMachine.TellSound, enemyStateMachine.transform, 1f);
        }

        phase = AttackPhase.Hold;
        holdTimer = Random.Range(enemyStateMachine.MinHoldDuration, enemyStateMachine.MaxHoldDuration);
        enemyStateMachine.Animator.speed = enemyStateMachine.WindupAnimSpeed;
    }

    private void ReleaseSwing()
    {
        phase = AttackPhase.Swing;
        enemyStateMachine.Animator.speed = 1f;
    }



    private enum AttackPhase
    {
        Windup,
        Hold,
        Swing
    }
}

