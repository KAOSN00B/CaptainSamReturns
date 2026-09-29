using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, Controls.IPlayerActions
{
    public event Action JumpEvent;
    public event Action DodgeEvent;
    public event Action TargetEvent;
    public event Action RemoveTargetEvent;
    public event Action BlockTargetEvent;
    public event Action<int> SwitchTargetEvent;

    public Vector2 MovementValue { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsBlocking { get; private set; }

    private Controls controls;

    private void Start()
    {
        controls = new Controls();
        controls.Player.SetCallbacks(this);

        controls.Player.Enable();
    }

    private void OnDestroy()
    {

        controls?.Player.Disable();

    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        JumpEvent?.Invoke();

    }



    public void OnDodge(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }

        DodgeEvent?.Invoke();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MovementValue = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {

    }

    public void OnTargeting(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }

        TargetEvent?.Invoke();
    }

    public void OnEscapeTargeting(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        RemoveTargetEvent?.Invoke();
    }

    public void OnSwitchTarget(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }

        SwitchTargetEvent?.Invoke(context.ReadValue<float>() > 0 ? 1 : -1);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsAttacking = true;
        }
        else
        {
            IsAttacking = false;
        }

    }

    public void OnBlock(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            IsBlocking = true;
        }
        else
        {
            IsBlocking = false;
        }
    }

}
