using TMPro;
using UnityEngine;
using UnityEngine.UI;

// World-space health bar built from the Synty Sci-Fi Soldier HUD bar.
// Red bar snaps to the new health, a white "damage chunk" trails behind it so you can see how much a hit took,
// it always faces the camera, and it fades in when hit and back out after a moment.
public class HealthBarUI : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Slider healthSlider;          // the Synty red bar
    [SerializeField] private RectTransform delayedFill;    // white chunk behind it
    [SerializeField] private CanvasGroup canvasGroup;      // fades the whole bar
    [SerializeField] private TMP_Text nameLabel;           // optional enemy name

    [Header("Settings")]
    [SerializeField] private string displayName = "";      // leave empty to hide the name
    [SerializeField] private bool alwaysVisible = false;
    [SerializeField] private float showDurationAfterHit = 2.5f;
    [SerializeField] private float fadeSpeed = 4f;          // alpha per second
    [SerializeField] private float delayedDrainDelay = 0.4f; // how long the white chunk hangs before draining
    [SerializeField] private float delayedDrainSpeed = 1f;  // bar-widths per second

    private const float Visible = 1f;
    private const float Hidden = 0f;

    private float healthFraction = 1f;
    private float delayedFraction = 1f;
    private float drainDelayRemaining;
    private float showTimeRemaining;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (nameLabel != null)
        {
            nameLabel.gameObject.SetActive(!string.IsNullOrEmpty(displayName));
            nameLabel.text = displayName;
        }

        if (canvasGroup != null) canvasGroup.alpha = alwaysVisible ? Visible : Hidden;
    }

    public void SetHealth(float current, float max, bool show)
    {
        healthFraction = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        if (healthSlider != null) healthSlider.value = healthFraction;

        if (!show)
        {
            delayedFraction = healthFraction;   // no hit, no chunk
            ApplyDelayedFill();
            return;
        }

        drainDelayRemaining = delayedDrainDelay;
        showTimeRemaining = showDurationAfterHit;
    }

    private void LateUpdate()
    {
        // UI runs on real time so it keeps moving during hit-stop freezes
        float deltaTime = Time.unscaledDeltaTime;

        if (mainCamera != null) transform.rotation = mainCamera.transform.rotation;   // billboard

        if (drainDelayRemaining > 0f) drainDelayRemaining -= deltaTime;
        else delayedFraction = Mathf.MoveTowards(delayedFraction, healthFraction, delayedDrainSpeed * deltaTime);
        ApplyDelayedFill();

        showTimeRemaining -= deltaTime;
        if (canvasGroup != null)
        {
            float wanted = alwaysVisible || showTimeRemaining > 0f ? Visible : Hidden;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, wanted, fadeSpeed * deltaTime);
        }
    }

    private void ApplyDelayedFill()
    {
        if (delayedFill == null) return;

        Vector2 anchorMax = delayedFill.anchorMax;
        anchorMax.x = delayedFraction;
        delayedFill.anchorMax = anchorMax;
    }
}
