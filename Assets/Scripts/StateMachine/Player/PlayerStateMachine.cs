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



    public Transform MainCameraTransform { get; private set; }
    public float PreviousDodgeTime { get; private set; } = Mathf.NegativeInfinity;
    public bool UsedJump { get; private set; } 
    public bool UsedDoubleJump { get; private set; } 

    private void Start()
    {
        MainCameraTransform = Camera.main.transform;
        SwitchState(new PlayerFreeLookState(this));    
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

    private void HandleGuardBreak()
    {
        if (Health.isDead) return;

        SwitchState(new PlayerGuardBreakState(this));
    }

    private void HandleTakeDamage()
    {
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
