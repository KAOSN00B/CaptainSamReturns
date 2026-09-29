using UnityEngine;
using UnityEngine.AI;

public class EnemyStateMachine : StateMachine
{
    [Header("Enemy Take In")]
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public CharacterController Controller { get; private set; }
    [field: SerializeField] public ForceReceiver ForceReceiver { get; private set; }
    [field: SerializeField] public NavMeshAgent Agent { get; private set; }
    [field: SerializeField] public WeaponDamage WeaponDamage { get; private set; }
    [field: SerializeField] public WeaponHandler WeaponHandler { get; private set; }
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public Poise Poise { get; private set; }
    [field: SerializeField] public Target Target { get; private set; }
    [field: SerializeField] public Ragdoll Ragdoll { get; private set; }
    [field: SerializeField] public AudioClip AlertSound { get; private set; }
    [field: SerializeField] public GameObject StunEffect { get; private set; }        // dizzy stars shown while staggered
    [field: SerializeField] public GameObject AlertEffect { get; private set; }        // dizzy stars shown while staggered
    [field: SerializeField] public Transform StunEffectPoint { get; private set; }
    [field: SerializeField] public Transform Transfrom { get; private set; }
    [field: SerializeField] public Transform StartingPosition { get; private set; }
    [field: SerializeField] public Transform AlertEffectPoint { get; private set; }
    [field: SerializeField] public Transform[] WayPoints { get; private set; }

    [Header("Variables")]
    [field: SerializeField] public float PlayerChasingRange { get; private set; }
    [field: SerializeField] public float MovementSpeed { get; private set; }
    [field: SerializeField] public float AttackRange { get; private set; }
    [field: SerializeField] public float AttackKnockBack { get; private set; }
    [field: SerializeField] public float ChaseDistance { get; private set; }
    [field: SerializeField] public float IdleDuration { get; private set; } = 1.5f;
    [field: SerializeField] public float PatrolSpeed { get; private set; } = 1.0f; 
    [field: SerializeField] public float AlertDuration { get; private set; } = 1.2f; 
    [field: SerializeField] public int AttackDamage { get; private set; }
    [field: SerializeField] public int CurrentWayIndex { get; private set; }
    [field: SerializeField] public bool SkipAlert { get; private set; }  //for scenarios like ambushes or combat rooms

    [Header("Attack Tell")]
    [field: SerializeField] public HitFlash HitFlash { get; private set; }
    [field: SerializeField] public Color TellColor { get; private set; }
    [field: SerializeField] public AudioClip TellSound { get; private set; }
    [field: SerializeField] public float WindupPoseTime { get; private set; } = 0.2f;
    [field: SerializeField] public float MinHoldDuration { get; private set; } = 0.1f;
    [field: SerializeField] public float MaxHoldDuration { get; private set; } = 0.9f;
    [field: SerializeField] public float QuickAttackChance { get; private set; } = 0.3f;  // 30% = no hold at all
    [field: SerializeField] public float TellFlashDuration { get; private set; } = 0.2f;
    [field: SerializeField] public float WindupAnimSpeed { get; private set; } = 0.05f;

    [Header("Enemy Combat")]
    [field: SerializeField] public float MaxAttackTimer { get; private set; } = 5.0f;
    [field: SerializeField] public float MinAttackTimer { get; private set; } = 0.5f;
    [field: SerializeField] public float EngageRange { get; private set; } = 3.5f;   // start circling at this distance
    [field: SerializeField] public float StrafeSpeed { get; private set; } = 1.5f;
    [field: SerializeField] public float MinEngageTime { get; private set; } = 1f;   // random wait before attack
    [field: SerializeField] public float MaxEngageTime { get; private set; } = 3f;
    [field: SerializeField] public float ComboChance { get; private set; } = 0.4f;  // 40% chance to swing again
    [field: SerializeField] public float ComboRangeMultiplier { get; private set; } = 1.5f;  // player must be within AttackRange x this to get comboed
    [field: SerializeField] public float MinCircleFlipTime { get; private set; } = 0.5f;  // random time before switching circle direction
    [field: SerializeField] public float MaxCircleFlipTime { get; private set; } = 1.5f;



    public Health Player {  get; private set; }

    public bool isChasingPlayer = false;

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        Agent.updatePosition = false; //we want full control of navigation and rotation
        Agent.updateRotation = false;

        SwitchState(new EnemyIdleState(this));
    }

    // every hit plays a small flinch on its own Animator layer, on top of whatever the enemy is doing,
    // so it reacts without being interrupted (poise decides when it actually staggers)
    private const string FlinchLayerName = "Flinch";
    private const string FlinchReaction = "Flinch";
    private const string NoFlinchState = "Empty";
    private const float FlinchFadeDuration = 0.05f;

    private void OnEnable()
    {
        Poise.OnPoiseBroken += HandleTakeDamage;
        Health.OnTakeDamage += HandleFlinch;
        Health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        Poise.OnPoiseBroken -= HandleTakeDamage;
        Health.OnTakeDamage -= HandleFlinch;
        Health.OnDeath -= HandleDeath;
    }

    private void HandleFlinch()
    {
        if (Health.isDead) return;

        string state = HitDirection.StateName(FlinchReaction, transform, Health.LastHitFrom);
        Animator.CrossFadeInFixedTime(state, FlinchFadeDuration, Animator.GetLayerIndex(FlinchLayerName));
    }

    public void StopFlinch()
    {
        Animator.Play(NoFlinchState, Animator.GetLayerIndex(FlinchLayerName));
    }

    private void HandleTakeDamage()
    {
        if (Health.isDead) return;

        SwitchState(new EnemyImpactState(this));
    }

    private void HandleDeath()
    {
        SwitchState(new EnemyDeathState(this));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, PlayerChasingRange);

        Gizmos.color = Color.aliceBlue;
        Gizmos.DrawWireSphere(transform.position, AttackRange);

        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, EngageRange);
    }
    public void AdvanceWayPoint()
    {
        CurrentWayIndex = (CurrentWayIndex + 1) % WayPoints.Length;   // 0 → 1 → 2 → back to 0
    }



}
