using Unity.Cinemachine;
using UnityEngine;

public class CameraStartPosition : MonoBehaviour
{
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
    [SerializeField] private Transform player;
    [SerializeField] private float startHeight = 12f; // vertical orbit angle, in degrees

    private void Start()
    {
        orbitalFollow.HorizontalAxis.Value = Mathf.DeltaAngle(0f, player.eulerAngles.y); // directly behind the player
        orbitalFollow.VerticalAxis.Value = startHeight;
    }
}
