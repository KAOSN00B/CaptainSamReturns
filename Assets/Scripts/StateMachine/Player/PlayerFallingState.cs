using UnityEngine;

public class PlayerFallingState : PlayerBaseState
{
    public PlayerFallingState(PlayerStateMachine playerStateMachine) : base(playerStateMachine){}

    private readonly int FallingHash = Animator.StringToHash("Falling");

    private const float CrossFadeDuration = 0.1f;

    private Vector3 momentum;

    private float timeInAir;   // how long we've been falling - drives both coyote time and the lock on gap


    public override void Enter()
    {
        timeInAir = 0f;

        momentum = playerStateMachine.Controller.velocity;
        momentum.y = 0f;

        playerStateMachine.Animator.CrossFadeInFixedTime(FallingHash, CrossFadeDuration);
        playerStateMachine.InputReader.JumpEvent += OnJump;
    }

    public override void Exit()
    {
        playerStateMachine.InputReader.JumpEvent -= OnJump;
    }

    public override void Tick(float deltaTime)
    {
        momentum = MovementWhileInAir(momentum, deltaTime);
        Move(momentum, deltaTime);

        if (playerStateMachine.Controller.isGrounded)
        {
            // a real landing (not just stepping down a curb) gets the squash + dust
            bool realLanding = playerStateMachine.UsedJump || timeInAir >= playerStateMachine.MinAirTimeForLandFeedback;
            if (realLanding && playerStateMachine.LandFeedback != null)
            {
                // longer falls land harder: bigger squash, dust, shake and thud
                float fall = Mathf.Clamp01(timeInAir / playerStateMachine.AirTimeForMaxLandIntensity);
                float intensity = Mathf.Lerp(playerStateMachine.MinLandIntensity, playerStateMachine.MaxLandIntensity, fall);
                playerStateMachine.PlayMovementFeedback(playerStateMachine.LandFeedback, playerStateMachine.transform.position, intensity);
            }

            playerStateMachine.SetUsedJump(false);
            playerStateMachine.SetUsedDoubleJump(false);   // landed: double jump comes back
            ReturnToLocomotion();
            return;
        }

        timeInAir += deltaTime;

        // small gaps keep the lock-on; a real fall off a ledge lets go of the enemy
        if (WalkedOffLedge() && timeInAir >= playerStateMachine.LoseTargetInAir && playerStateMachine.Targeter.CurrentTarget != null)
        {
            playerStateMachine.Targeter.Cancel();
        }

        if (AttackPressedThisFrame())
        {
            playerStateMachine.SwitchState(new PlayerJumpAttackState(playerStateMachine));
            return;
        }


        if (playerStateMachine.Targeter.CurrentTarget != null)
            FaceTarget();                                 
        else
            FaceMovementDirection(momentum, deltaTime);   
    }

    // UsedJump is only set by a real jump, so if it's false we got here by stepping off an edge
    private bool WalkedOffLedge()
    {
        return !playerStateMachine.UsedJump;
    }

    private void OnJump()
    {
        // coyote time: just stepped off a ledge? you still get your normal ground jump
        if (WalkedOffLedge() && timeInAir <= playerStateMachine.CoyoteTime)
        {
            playerStateMachine.SwitchState(new PlayerJumpingState(playerStateMachine));
            return;
        }

        TryDoubleJump();
    }


}
