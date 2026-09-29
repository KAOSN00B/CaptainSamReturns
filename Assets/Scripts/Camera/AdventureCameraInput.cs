using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Device-specific look sensitivity and cursor capture for the adventure camera.</summary>
[DisallowMultipleComponent]
public sealed class AdventureCameraInput : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController[] orbitInputs;
    [SerializeField, Min(0f)] private float mouseDegreesPerPixel = 0.12f;
    [SerializeField, Min(0f)] private float stickDegreesPerSecond = 150f;
    [SerializeField] private bool invertY;
    private CursorLockMode previousLock;
    private bool previousVisible;

    private void OnEnable()
    {
        previousLock = Cursor.lockState;
        previousVisible = Cursor.visible;
        if (orbitInputs != null)
            foreach (var input in orbitInputs)
                if (input != null)
                    input.ReadControlValueOverride = ReadLook;
        CaptureCursor();
    }

    private void OnDisable()
    {
        if (orbitInputs != null)
            foreach (var input in orbitInputs)
                if (input != null && input.ReadControlValueOverride == ReadLook)
                    input.ReadControlValueOverride = null;
        Cursor.lockState = previousLock;
        Cursor.visible = previousVisible;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Application.isFocused && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            CaptureCursor();
    }

    private static void CaptureCursor()
    {
        if (!Application.isPlaying || !Application.isFocused) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private float ReadLook(InputAction action, IInputAxisOwner.AxisDescriptor.Hints hint,
        UnityEngine.Object context, CinemachineInputAxisController.Reader.ControlValueReader defaultReader)
    {
        if (!Application.isFocused || Time.timeScale == 0f) return 0f;
        bool mouse = action.activeControl != null && action.activeControl.device is Mouse;
        if (mouse && Cursor.lockState != CursorLockMode.Locked) return 0f;
        float value = defaultReader(action, hint, context, null);
        // Cinemachine integrates rates using deltaTime; mouse delta already represents a whole frame.
        value *= mouse ? (Time.deltaTime > 0f ? mouseDegreesPerPixel / Time.deltaTime : 0f)
                       : stickDegreesPerSecond;
        if (hint == IInputAxisOwner.AxisDescriptor.Hints.Y && !invertY) value = -value;
        return value;
    }
}
