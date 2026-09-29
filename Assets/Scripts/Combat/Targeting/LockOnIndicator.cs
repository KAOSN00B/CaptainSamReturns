using UnityEngine;

public class LockOnIndicator : MonoBehaviour
{
    [SerializeField] private Targeter targeter;
    [SerializeField] private RectTransform marker;
    [SerializeField] private float heightOffset = 1.2f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        Target target = targeter.CurrentTarget;
        marker.gameObject.SetActive(target != null);

        if (target == null) return;

        marker.position = mainCamera.WorldToScreenPoint(target.transform.position + Vector3.up * heightOffset);
    }
}
