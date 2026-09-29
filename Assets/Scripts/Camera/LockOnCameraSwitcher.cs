using Unity.Cinemachine;
using UnityEngine;

// Picks the lock-on camera whenever the player has a target, no matter what animation is playing,
// so dodging, attacking or getting hit while locked on never swaps the camera.
public class LockOnCameraSwitcher : MonoBehaviour
{
    [SerializeField] private Targeter targeter;
    [SerializeField] private CinemachineCamera targetingCamera;
    [SerializeField] private int lockedOnPriority = 20;  // above the free-look camera
    [SerializeField] private int freeLookPriority = 0;   // below the free-look camera

    private void Update()
    {
        bool isLockedOn = targeter.CurrentTarget != null;

        targetingCamera.Priority.Enabled = true;
        targetingCamera.Priority.Value = isLockedOn ? lockedOnPriority : freeLookPriority;
    }
}
