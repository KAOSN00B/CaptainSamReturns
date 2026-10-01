using UnityEngine;

// Gun out: face where the camera looks, strafe with the gun's blend tree, shoot at the crosshair.
// Entered by pressing Shoot from sword locomotion; the sword button swaps back and swings.
// (Built from the old lock-on targeting state: same strafe movement, now on AimBlendTree.)
public class PlayerAimingState : PlayerBaseState
{
    public PlayerAimingState(PlayerStateMachine playerStateMachine) : base(playerStateMachine) { }

    private readonly int aimBlendTreeHash = Animator.StringToHash("AimBlendTree");
    private readonly int aimForwardHash = Animator.StringToHash("AimForwardSpeed");
    private readonly int aimRightHash = Animator.StringToHash("AimRightSpeed");

    private const float CrossFadeDuration = 0.1f;
    private const float DampTime = 0.1f;
    private const float MoveDeadZone = 0.1f;
    private const int FirstAttackIndex = 0;

    public override void Enter()
    {
        // Set the locomotion clips before entering their blend tree.
        playerStateMachine.GunSelector.Equip();
        playerStateMachine.PlayerIK.SetAim(true, GetAimPoint());

        playerStateMachine.InputReader.DodgeEvent += OnDodge;
        playerStateMachine.InputReader.JumpEvent += OnJump;
        playerStateMachine.Animator.SetFloat(aimForwardHash, 0f);
        playerStateMachine.Animator.SetFloat(aimRightHash, 0f);
        playerStateMachine.Animator.CrossFadeInFixedTime(aimBlendTreeHash, CrossFadeDuration);
    }

    public override void Tick(float deltaTime)
    {
        if (AttackPressedThisFrame())
        {
            // The attack state equips the sword.
            playerStateMachine.SwitchState(new PlayerAttackingState(playerStateMachine, FirstAttackIndex));
            return;
        }

        if (playerStateMachine.InputReader.IsBlocking)
        {
            playerStateMachine.SwitchState(new PlayerBlockingState(playerStateMachine));
            return;
        }

        if (playerStateMachine.Controller.velocity.y <= 0.0f && !playerStateMachine.Controller.isGrounded)
        {
            playerStateMachine.SwitchState(new PlayerFallingState(playerStateMachine));
            return;
        }

        Vector3 movement = CalculateMovement();
        Move(movement * playerStateMachine.AimMovementSpeed, deltaTime);

        FaceCamera(deltaTime);
        UpdateAnimator(movement, deltaTime);

        Vector3 aimPoint = GetAimPoint();
        playerStateMachine.PlayerIK.SetAim(true, aimPoint);   // the arms point the gun at the crosshair

        // CanShoot waits a moment after drawing the gun; Shoot() returns false when the fire rate says "not yet",
        // so the arms only kick on shots that actually fired.
        if (playerStateMachine.InputReader.IsShooting && playerStateMachine.GunSelector.CanShoot)
        {
            GunScriptableObject gun = playerStateMachine.GunSelector.ActiveGun;
            if (gun.Shoot(aimPoint))
                playerStateMachine.PlayerIK.Kick(gun.ShootConfig.RecoilKickBack, gun.ShootConfig.RecoilKickAngle);
        }
    }

    public override void Exit()
    {
        playerStateMachine.InputReader.DodgeEvent -= OnDodge;
        playerStateMachine.InputReader.JumpEvent -= OnJump;
        playerStateMachine.PlayerIK.SetAim(false, Vector3.zero);
    }

    // turn (yaw only) to where the camera is looking
    private void FaceCamera(float deltaTime)
    {
        Vector3 look = playerStateMachine.MainCameraTransform.forward;
        look.y = 0f;
        if (look.sqrMagnitude < Mathf.Epsilon) return;

        playerStateMachine.transform.rotation = Quaternion.Slerp(playerStateMachine.transform.rotation,
            Quaternion.LookRotation(look), 1f - Mathf.Exp(-deltaTime * playerStateMachine.AimRotationSpeed));
    }

    // what the screen center is on. The ray starts level with the player so it can't hit things
    // between the camera and the player (or the player itself).
    private Vector3 GetAimPoint()
    {
        Transform cam = playerStateMachine.MainCameraTransform;
        float startDistance = Mathf.Max(0f, Vector3.Dot(playerStateMachine.transform.position - cam.position, cam.forward));
        Vector3 origin = cam.position + cam.forward * startDistance;

        if (Physics.Raycast(origin, cam.forward, out RaycastHit hit, playerStateMachine.AimMaxDistance,
            playerStateMachine.AimMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return origin + cam.forward * playerStateMachine.AimMaxDistance;
    }

    private void UpdateAnimator(Vector3 movement, float deltaTime)
    {
        // world-space movement -> the character's own forward (z) and right (x)
        Vector3 local = playerStateMachine.transform.InverseTransformDirection(movement);

        float forward = Mathf.Abs(local.z) < MoveDeadZone ? 0f : Mathf.Clamp(local.z, -1f, 1f);
        float right = Mathf.Abs(local.x) < MoveDeadZone ? 0f : Mathf.Clamp(local.x, -1f, 1f);

        playerStateMachine.Animator.SetFloat(aimForwardHash, forward, DampTime, deltaTime);
        playerStateMachine.Animator.SetFloat(aimRightHash, right, DampTime, deltaTime);
    }

    private void OnDodge()
    {
        TryDodge(CalculateMovement());
    }

    private void OnJump()
    {
        playerStateMachine.SwitchState(new PlayerJumpingState(playerStateMachine));
    }
}
