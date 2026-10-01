using Unity.Cinemachine;
using UnityEngine;

// Over-the-shoulder aim camera while a gun is out, the free-look camera while the sword is out.
// Both cameras share the orbit heading, so equipping changes the framing, not where you're looking.
// Driven by the equipped weapon (not the animation), so dodging or jumping with the gun out keeps the aim camera.
[DefaultExecutionOrder(100)]
public class AimCameraSwitcher : MonoBehaviour
{
    [SerializeField] private PlayerGunSelector gunSelector;
    [SerializeField] private CinemachineCamera aimCamera;
    [SerializeField] private CinemachineCamera freeLookCamera;
    [SerializeField] private int aimingPriority = 20;   // above the free-look camera
    [SerializeField] private int idlePriority = 0;      // below the free-look camera
    [SerializeField] private float aimElevationOffset = -10f;   // degrees: the aim camera sits lower (nearer shoulder height) than free-look

    private bool wasEquipped;
    private CinemachineOrbitalFollow aimOrbit;
    private CinemachineOrbitalFollow freeOrbit;

    private void Start()
    {
        aimOrbit = aimCamera.GetComponent<CinemachineOrbitalFollow>();
        freeOrbit = freeLookCamera.GetComponent<CinemachineOrbitalFollow>();
        Update();
    }

    private void Update()
    {
        if (gunSelector == null || aimCamera == null || aimOrbit == null || freeOrbit == null) return;

        bool equipped = gunSelector.IsGunEquipped;
        if (equipped != wasEquipped)
        {
            // The camera we're leaving hands its orbit position to the camera we're switching to:
            // same heading, and the height shifted by the elevation offset (down when the gun comes out, back up when it goes away).
            CinemachineOrbitalFollow from = equipped ? freeOrbit : aimOrbit;
            CinemachineOrbitalFollow to = equipped ? aimOrbit : freeOrbit;
            float elevation = from.VerticalAxis.Value + (equipped ? aimElevationOffset : -aimElevationOffset);

            to.HorizontalAxis.Value = from.HorizontalAxis.Value;
            to.VerticalAxis.Value = Mathf.Clamp(elevation, to.VerticalAxis.Range.x, to.VerticalAxis.Range.y);
            wasEquipped = equipped;
        }

        // the higher priority camera wins, so raising the aim camera above free-look makes it the live one
        aimCamera.Priority.Enabled = true;
        aimCamera.Priority.Value = equipped ? aimingPriority : idlePriority;
    }
}
