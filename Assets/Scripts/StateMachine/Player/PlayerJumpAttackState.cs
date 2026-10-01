using MoreMountains.Feedbacks;
using UnityEngine;

// Air attack: raise the sword, hold it overhead while plunging down faster, then slam on landing.
// Built like the enemy's attack tell: phases + a frozen pose, then release.
public class PlayerJumpAttackState : PlayerBaseState
{
    public PlayerJumpAttackState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
    {
        attack = playerStateMachine.JumpAttack;
    }

    private enum Phase { Raise, Plunge, Slam }

    private const float NormalAnimSpeed = 1f;
    private const float AnimationFinished = 1f;
    private const int BaseLayer = 0;

    private readonly Attack attack;
    private const float FrozenAnimSpeed = 0f;

    private Phase phase;
    private Vector3 momentum;
    private float hangTimer;
    private float impactHoldTimer;

    public override void Enter()
    {
        playerStateMachine.GunSelector.Unequip();   // the slam is a sword move (before the crossfade)

        phase = Phase.Raise;

        momentum = playerStateMachine.Controller.velocity;
        momentum.y = 0f;

        playerStateMachine.WeaponDamage.SetAttack(attack.Damage, attack.KnockBack,
            attack.CameraShake, attack.HitStopDuration, attack.SwordSwingSFX, attack.PoiseDamage);
        playerStateMachine.Animator.CrossFade(attack.AnimationName, attack.TransitionDuration, BaseLayer, playerStateMachine.JumpAttackStartTime);

        hangTimer = 0f;   // hang first, dive after (see Raise)
    }

    public override void Tick(float deltaTime)
    {
        float normalizedTime = GetNormailzedTime(playerStateMachine.Animator);

        if (phase != Phase.Slam && playerStateMachine.Controller.isGrounded)
        {
            Land();
        }

        switch (phase)
        {
            case Phase.Raise:
                playerStateMachine.ForceReceiver.Hover();   // anticipation: hang at the top while the sword goes up
                SteerInAir(deltaTime);
                if (normalizedTime >= playerStateMachine.JumpAttackHoldPoseTime)
                {
                    playerStateMachine.Animator.speed = playerStateMachine.JumpAttackHoldAnimSpeed;   // freeze with the sword overhead
                }

                hangTimer += deltaTime;
                if (hangTimer >= playerStateMachine.JumpAttackHangTime)
                {
                    phase = Phase.Plunge;
                    playerStateMachine.ForceReceiver.Plunge(playerStateMachine.JumpAttackPlungeSpeed);
                }
                break;

            case Phase.Plunge:
                SteerInAir(deltaTime);
                break;

            case Phase.Slam:
                Move(deltaTime);   // planted on the ground
                if (impactHoldTimer > 0f)
                {
                    impactHoldTimer -= deltaTime;
                    if (impactHoldTimer <= 0f) playerStateMachine.Animator.speed = NormalAnimSpeed;   // let the follow-through play
                    break;
                }
                if (normalizedTime >= AnimationFinished) FinishAttack();
                break;
        }
    }

    public override void Exit()
    {
        playerStateMachine.Animator.speed = NormalAnimSpeed;   // always restore, however we leave
        playerStateMachine.WeaponHandler.DisableWeapon();
    }

    private void SteerInAir(float deltaTime)
    {
        momentum = MovementWhileInAir(momentum, deltaTime);
        Move(momentum, deltaTime);

        FaceMovementDirection(momentum, deltaTime);
    }

    // hit the ground: snap straight to the sword-in-the-ground pose and hold it, so the impact lands on contact
    private void Land()
    {
        phase = Phase.Slam;
        playerStateMachine.Animator.Play(attack.AnimationName, BaseLayer, playerStateMachine.JumpAttackImpactTime);
        playerStateMachine.Animator.speed = FrozenAnimSpeed;
        impactHoldTimer = playerStateMachine.JumpAttackImpactHoldTime;

        MMF_Player slam = playerStateMachine.SlamFeedback != null ? playerStateMachine.SlamFeedback : playerStateMachine.LandFeedback;
        playerStateMachine.PlayMovementFeedback(slam);
        playerStateMachine.SetUsedJump(false);
        playerStateMachine.SetUsedDoubleJump(false);
    }

    private void FinishAttack()
    {
        if (string.IsNullOrEmpty(attack.RecoveryAnimationName))
        {
            ReturnToLocomotion();
            return;
        }

        playerStateMachine.SwitchState(new PlayerRecoveryState(playerStateMachine, attack.RecoveryAnimationName));
    }
}
