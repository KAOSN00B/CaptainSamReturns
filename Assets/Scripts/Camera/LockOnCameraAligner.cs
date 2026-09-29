using Unity.Cinemachine;
using UnityEngine;

public class LockOnCameraAligner : MonoBehaviour
{
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
    [SerializeField] private Targeter targeter;

    private void Update()
    {
        if (targeter.CurrentTarget == null) return;

        Vector3 toTarget = targeter.CurrentTarget.transform.position - targeter.transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.01f) return;

        // "home" angle = behind the player on the player -> target line.
        // The player can orbit freely; recentering on the axis eases the camera back here when they stop.
        orbitalFollow.HorizontalAxis.Center = Mathf.DeltaAngle(0f, Quaternion.LookRotation(toTarget).eulerAngles.y);
    }
}
