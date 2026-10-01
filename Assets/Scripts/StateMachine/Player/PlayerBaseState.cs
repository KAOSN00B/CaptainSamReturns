using System.Collections;
using UnityEngine;

public abstract class PlayerBaseState : State
{
    protected PlayerStateMachine playerStateMachine;

    private bool attackHeldLastFrame;

    public PlayerBaseState(PlayerStateMachine playerStateMachine)
    {
        this.playerStateMachine = playerStateMachine;
        attackHeldLastFrame = playerStateMachine.InputReader.IsAttacking;   // a button already held coming into this state doesn't count as a press
    }

    // true only on the frame attack goes from released to pressed (IsAttacking alone is "held")
    protected bool AttackPressedThisFrame()
    {
        bool held = playerStateMachine.InputReader.IsAttacking;
        bool pressed = held && !attackHeldLastFrame;
        attackHeldLastFrame = held;
        return pressed;
    }

    protected void Move(float deltaTime)
    {
        Vector3 noMotions = Vector3.zero;
        Move(noMotions, deltaTime);  
    }

    protected void Move(Vector3 motion, float deltaTime)
    {
        playerStateMachine.Controller.Move((motion + 
            playerStateMachine.ForceReceiver.Movement) * deltaTime);
    }


    protected void TryDodge(Vector3 direction)
    {
        if (Time.time - playerStateMachine.PreviousDodgeTime < playerStateMachine.DodgeCooldown) return;

        playerStateMachine.SetDodgeTime(Time.time);

        if (direction == Vector3.zero) direction = -playerStateMachine.transform.forward; // no input = roll back

        playerStateMachine.SwitchState(new PlayerDodgingState(playerStateMachine, direction.normalized));
    }

    protected void ReturnToLocomotion()
    {
        if (playerStateMachine.GunSelector.IsGunEquipped)
        {
            playerStateMachine.SwitchState(new PlayerAimingState(playerStateMachine));
        }
        else
        {
            playerStateMachine.SwitchState(new PlayerFreeLookState(playerStateMachine));    
        }
    }


    protected void FaceMovementDirection(Vector3 movement, float deltaTime)
    {
        if (movement == Vector3.zero) return;
        playerStateMachine.transform.rotation = Quaternion.Lerp(playerStateMachine.transform.rotation,
            Quaternion.LookRotation(movement), deltaTime * playerStateMachine.RotationSmoothValue);

    }

    protected Vector3 CalculateMovement()
    {
        Vector3 forward = playerStateMachine.MainCameraTransform.forward;
        Vector3 right = playerStateMachine.MainCameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return forward * playerStateMachine.InputReader.MovementValue.y +
            right * playerStateMachine.InputReader.MovementValue.x;
    }

    protected Vector3 MovementWhileInAir(Vector3 momentum, float deltaTime)
    {
        Vector3 wantedVelocity = CalculateMovement() * playerStateMachine.AirMovementSpeed;

        return Vector3.MoveTowards(momentum, wantedVelocity, playerStateMachine.AirAcceleration * deltaTime); 

    }

    protected void TryDoubleJump()
    {
        if (playerStateMachine.UsedDoubleJump) return;   

        playerStateMachine.SwitchState(new PlayerDoubleJumpState(playerStateMachine));
    }

}
