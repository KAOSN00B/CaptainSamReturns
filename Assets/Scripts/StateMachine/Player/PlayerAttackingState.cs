using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlayerAttackingState : PlayerBaseState
{
    private float previousFrameTime;

    private bool alreadyAppliedForce;

    private Attack attack;

    public PlayerAttackingState(PlayerStateMachine playerStateMachine,int attackIndex) : base(playerStateMachine)
    {
        attack = playerStateMachine.Attacks[attackIndex];
    }

    public override void Enter()
    {
        playerStateMachine.WeaponDamage.SetAttack(attack.Damage, attack.KnockBack,
            attack.CameraShake, attack.HitStopDuration, attack.SwordSwingSFX, attack.PoiseDamage);

        playerStateMachine.Animator.CrossFadeInFixedTime(attack.AnimationName, attack.TransitionDuration);  
    }

    public override void Exit()
    {
        playerStateMachine.WeaponHandler.DisableWeapon();
    }

    private void TryComboAttacking(float normalizeTime)
    {
        
        if (attack.ComboStateIndex == -1) return;

        if (normalizeTime < attack.ComboAttackTime) return;

        playerStateMachine.SwitchState(new PlayerAttackingState(playerStateMachine, attack.ComboStateIndex));
 
    }

    public override void Tick(float deltaTime)
    {
        if (playerStateMachine.Targeter.CurrentTarget != null)
        {
            Move(deltaTime);
            FaceTarget();
        }

        else
        {
            // free: shuffle and steer the swing with the stick, Ratchet & Clank-style
            Vector3 movement = CalculateMovement();
            Move(movement * playerStateMachine.AttackDirectionSteering, deltaTime);
            FaceMovementDirection(movement, deltaTime);
        }

        float normalizeTime = GetNormailzedTime(playerStateMachine.Animator);

        if (normalizeTime >= previousFrameTime && normalizeTime < 1.0f)
        {
            if (normalizeTime >= attack.ForceTime)
            {
                TryApplyForce();
            }

            if (playerStateMachine.InputReader.IsAttacking)
            {
                TryComboAttacking(normalizeTime);

            }
        }
        else
        {
            FinishAttack();
        }
        previousFrameTime = normalizeTime;

        
    }

    // combo ended here: ease the sword back to the ready stance instead of snapping to idle/run
    private void FinishAttack()
    {
        if (string.IsNullOrEmpty(attack.RecoveryAnimationName))
        {
            ReturnToLocomotion();
            return;
        }

        playerStateMachine.SwitchState(new PlayerRecoveryState(playerStateMachine, attack.RecoveryAnimationName));
    }

    private void TryApplyForce()
    {
        if (alreadyAppliedForce) return;

        playerStateMachine.ForceReceiver.AddForce(playerStateMachine.transform.forward * attack.Force);

        alreadyAppliedForce = true;
    }
}
