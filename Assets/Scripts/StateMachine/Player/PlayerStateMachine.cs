using MoreMountains.Feedbacks;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    [Header("Scripts")]
    [field: SerializeField] public InputReader InputReader { get; private set; }
    [field: SerializeField] public CharacterController Controller { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public Targeter Targeter { get; private set; }
    [field: SerializeField] public ForceReceiver ForceReceiver { get; private set; }
    [field: SerializeField] public Attack[] Attacks { get; private set; }
    [field: SerializeField] public WeaponDamage WeaponDamage { get; private set; }
    [field: SerializeField] public WeaponHandler WeaponHandler { get; private set; }
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public Poise Poise { get; private set; }
    [field: SerializeField] public Ragdoll Ragdoll { get; private set; }

    [Header("Movement Settings")]
    [field: SerializeField] public float FreeLookMovementSpeed { get; private set; }
    [field: SerializeField] public float TargetingMovementSpeed { get; private set; }
    [field: SerializeField] public float RotationSmoothValue { get; private set; }
    [field: SerializeField] public float AirMovementSpeed { get; private set; } = 8.0f;
    [field: SerializeField] public float AirAcceleration { get; private set; } = 15.0f;

    [Header("Dodge Settings")]
    [field: SerializeField] public float DodgeDuration { get; private set; }
    [field: SerializeField] public float DodgeDistance { get; private set; }
    [field: SerializeField] public float DodgeCooldown { get; private set; }
    [field: SerializeField] public float DodgeIFramesDuration { get; private set; }

    [Header("Other Settings")]
    [field: SerializeField] public float JumpForce { get; private set; }
    [field: SerializeField] public float HitStunDuration { get; private set; }
    [field: SerializeField] public float HitStunStarting { get; private set; }
    [field: SerializeField] public float AttackDirectionSteering { get; private set; } = 2.0f;
    [field: SerializeField] public int HeavyHitDamage { get; private set; } = 20;  // hits this strong play the big stagger instead of the quick flinch
    [field: SerializeField] public float GuardBreakDuration { get; private set; } = 1.2f;  // how long you're stunned when your guard breaks
    [field: SerializeField] public float LoseTargetInAir { get; private set; } = 0.5f;  
    [field: SerializeField] public float CoyoteTime { get; private set; } = 0.1f;

    [Header("Jump Attack")]
    [field: SerializeField] public Attack JumpAttack { get; private set; }                       // damage, animation etc. for the air plunge
    [field: SerializeField] public float JumpAttackPlungeSpeed { get; private set; } = 10f;      // how fast you drive down
    [field: SerializeField] public float JumpAttackStartTime { get; private set; } = 0.25f;      // skip the slow start of the clip (normalized)
    [field: SerializeField] public float JumpAttackHoldPoseTime { get; private set; } = 0.56f;   // normalized time where the sword is raised overhead, just before the slash
    [field: SerializeField] public float JumpAttackHoldAnimSpeed { get; private set; } = 0.05f;  // animation speed while holding that pose in the air
    [field: SerializeField] public float JumpAttackHangTime { get; private set; } = 0.15f;       // hang at the top with the sword raised before diving (anticipation)
    [field: SerializeField] public float JumpAttackImpactTime { get; private set; } = 0.33f;     // normalized time of the "sword in the ground" pose we snap to on landing
    [field: SerializeField] public float JumpAttackImpactHoldTime { get; private set; } = 0.18f; // freeze on that pose so the impact reads

    [Header("Game Feel (Feel feedbacks)")]
    [field: SerializeField] public MMF_Player JumpFeedback { get; private set; }        // stretch + dust puff on take-off
    [field: SerializeField] public MMF_Player DoubleJumpFeedback { get; private set; }  // stretch + puff for the flip
    [field: SerializeField] public MMF_Player LandFeedback { get; private set; }        // squash + dust on landing
    [field: SerializeField] public MMF_Player HurtFeedback { get; private set; }        // red vignette, chromatic kick, shake when hit
    [field: SerializeField] public float MinAirTimeForLandFeedback { get; private set; } = 0.15f;  // tiny steps don't squash
    [field: SerializeField] public MMF_Player SlamFeedback { get; private set; }        // shockwave ring, boom, camera kick when the jump attack lands
    [field: SerializeField] public float MinLandIntensity { get; private set; } = 0.6f;            // short hop landing strength
    [field: SerializeField] public float MaxLandIntensity { get; private set; } = 1.6f;            // long fall landing strength
    [field: SerializeField] public float AirTimeForMaxLandIntensity { get; private set; } = 1.2f;  // seconds of falling to reach the max



    public Transform MainCameraTransform { get; private set; }
    public float PreviousDodgeTime { get; private set; } = Mathf.NegativeInfinity;
    public bool UsedJump { get; private set; } 
    public bool UsedDoubleJump { get; private set; } 

    private const float FullIntensity = 1f;

    private Transform squashTarget;     // the bone every jump / land / slam squash & stretch scales
    private Vector3 squashRestScale;

    private void Start()
    {
        MainCameraTransform = Camera.main.transform;
        CacheSquashTarget();
        SwitchState(new PlayerFreeLookState(this));    
    }

    private MMF_Player[] MovementFeedbacks() => new[] { JumpFeedback, DoubleJumpFeedback, LandFeedback, SlamFeedback };

    private void CacheSquashTarget()
    {
        foreach (MMF_Player player in MovementFeedbacks())
        {
            if (player == null) continue;
            foreach (MMF_Feedback feedback in player.FeedbacksList)
            {
                if (feedback is MMF_SquashAndStretch squash && squash.SquashAndStretchTarget != null)
                {
                    squashTarget = squash.SquashAndStretchTarget;
                    squashRestScale = squashTarget.localScale;
                    return;
                }
            }
        }
    }

    public void PlayMovementFeedback(MMF_Player feedback) => PlayMovementFeedback(feedback, transform.position, FullIntensity);

    // Jump, land and slam all squash the same bone, and Feel remembers the bone's scale when a squash starts.
    // One starting mid-squash would remember a squashed size and the player shrinks for good,
    // so stop any running squash and put the bone back to rest before playing the next one.
    public void PlayMovementFeedback(MMF_Player feedback, Vector3 position, float intensity)
    {
        if (feedback == null) return;

        foreach (MMF_Player player in MovementFeedbacks())
        {
            if (player == null) continue;
            foreach (MMF_Feedback other in player.FeedbacksList)
            {
                if (other is MMF_SquashAndStretch) other.Stop(position);
            }
        }
        if (squashTarget != null) squashTarget.localScale = squashRestScale;

        feedback.PlayFeedbacks(position, intensity);
    }
    private void OnEnable()
    {
        Health.OnTakeDamage += HandleTakeDamage;
        Health.OnDeath += HandleDeath;
        Poise.OnPoiseBroken += HandleGuardBreak;
    }

    private void OnDisable()
    {
        Health.OnTakeDamage -= HandleTakeDamage;
        Health.OnDeath -= HandleDeath;
        Poise.OnPoiseBroken -= HandleGuardBreak;
    }

    private const string GuardBrokenMessage = "Guard Broken!";

    private void HandleGuardBreak()
    {
        if (Health.isDead) return;

        EventLogUI.Show(GuardBrokenMessage);
        SwitchState(new PlayerGuardBreakState(this));
    }

    private void HandleTakeDamage()
    {
        if (HurtFeedback != null) HurtFeedback.PlayFeedbacks();
        SwitchState(new PlayerImpactState(this));
    }

    private void HandleDeath()
    {
        SwitchState(new PlayerDeathState(this));
    }

    public void SetDodgeTime(float dodgeTime)
    {
        PreviousDodgeTime = dodgeTime;

    }

    public void SetUsedJump(bool usedJump)
    {
        UsedJump = usedJump;
    }

    public void SetUsedDoubleJump(bool usedDoubleJump)
    {
        UsedDoubleJump = usedDoubleJump;
    }






}
