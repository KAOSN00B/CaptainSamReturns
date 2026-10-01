using UnityEngine;
using UnityEngine.UI;

// A Synty hexagon ring dial (e.g. the Minimal HUD's status icons) used as a meter:
// the ring fills with the value, swaps to the red "low" version when nearly empty,
// and can hide itself when full.
public class HUDDial : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Image normalFill;          // ring fill in the normal dial
    [SerializeField] private Image lowFill;             // ring fill in the red dial
    [SerializeField] private GameObject normalDial;
    [SerializeField] private GameObject lowDial;
    [SerializeField] private CanvasGroup canvasGroup;   // optional: lets the dial fade out

    [Header("Settings")]
    [SerializeField, Range(0f, 1f)] private float lowThreshold = 0.3f;
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private float hideDelay = 1.5f;
    [SerializeField] private float fadeSpeed = 4f;

    private const float Full = 1f;
    private const float Visible = 1f;
    private const float Hidden = 0f;

    private float fraction = Full;
    private float hideTimeRemaining;
    private bool forceVisible;

    private void Awake()
    {
        if (canvasGroup != null && hideWhenFull) canvasGroup.alpha = Hidden;
    }

    public void SetValue(float current, float max)
    {
        fraction = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        if (fraction < Full) hideTimeRemaining = hideDelay;

        if (normalFill != null) normalFill.fillAmount = fraction;
        if (lowFill != null) lowFill.fillAmount = fraction;

        bool low = fraction <= lowThreshold;
        if (normalDial != null) normalDial.SetActive(!low);
        if (lowDial != null) lowDial.SetActive(low);
    }

    public void SetForceVisible(bool visible)
    {
        forceVisible = visible;
        if (visible) hideTimeRemaining = hideDelay;
    }

    private void Update()
    {
        if (canvasGroup == null || !hideWhenFull) return;

        hideTimeRemaining -= Time.unscaledDeltaTime;
        bool show = forceVisible || fraction < Full || hideTimeRemaining > 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, show ? Visible : Hidden, fadeSpeed * Time.unscaledDeltaTime);
    }
}
