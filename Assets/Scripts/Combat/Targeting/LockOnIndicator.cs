using TMPro;
using UnityEngine;

public class LockOnIndicator : MonoBehaviour
{
    [SerializeField] private Targeter targeter;
    [SerializeField] private RectTransform marker;
    [SerializeField] private float heightOffset = 1.2f;
    [SerializeField] private float lockOnPopScale = 1.6f;   // marker starts this big and snaps down when you lock on
    [SerializeField] private float popSettleSpeed = 8f;
    [SerializeField] private float spinSpeed = 40f;         // degrees per second, a slow sci-fi rotate (0 = no spin)
    [SerializeField] private TMP_Text distanceLabel;        // optional: shows how far away the target is ("12m")

    private const string MetresSuffix = "m";

    private Camera mainCamera;
    private Target lastTarget;
    private float currentScale = 1f;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        Target target = targeter.CurrentTarget;
        marker.gameObject.SetActive(target != null);

        if (target != lastTarget && target != null) currentScale = lockOnPopScale;   // new lock (or switched target): pop
        lastTarget = target;

        if (target == null) return;

        marker.position = mainCamera.WorldToScreenPoint(target.transform.position + Vector3.up * heightOffset);

        currentScale = Mathf.Lerp(currentScale, 1f, popSettleSpeed * Time.unscaledDeltaTime);
        marker.localScale = Vector3.one * currentScale;
        if (spinSpeed != 0f) marker.Rotate(0f, 0f, -spinSpeed * Time.unscaledDeltaTime);

        if (distanceLabel != null)
        {
            float distance = Vector3.Distance(targeter.transform.position, target.transform.position);
            distanceLabel.text = Mathf.RoundToInt(distance) + MetresSuffix;
        }
    }
}
