using UnityEngine;
using UnityEngine.UI;

// A screen-space bar built on a Synty HUD slider: snaps to the new value, a white "chunk" trails behind
// on loss, it can pulse a warning colour when low, and it can hide itself when full.
public class HUDBar : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;              // tinted for the normal / low colours
    [SerializeField] private RectTransform delayedFill;    // white chunk behind the fill
    [SerializeField] private CanvasGroup canvasGroup;      // optional: lets the bar fade out

    [Header("Colours")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField, Range(0f, 1f)] private float lowThreshold = 0f;   // 0 = never warn
    [SerializeField] private float lowPulseSpeed = 6f;

    [Header("Chunk")]
    [SerializeField] private float delayedDrainDelay = 0.4f;
    [SerializeField] private float delayedDrainSpeed = 1f;    // bar-widths per second

    [Header("Visibility")]
    [SerializeField] private bool hideWhenFull = false;
    [SerializeField] private float hideDelay = 1.5f;
    [SerializeField] private float fadeSpeed = 4f;

    private const float Full = 1f;
    private const float Visible = 1f;
    private const float Hidden = 0f;

    private float fraction = Full;
    private float delayedFraction = Full;
    private float drainDelayRemaining;
    private float hideTimeRemaining;
    private bool forceVisible;

    private void Awake()
    {
        if (canvasGroup != null && hideWhenFull) canvasGroup.alpha = Hidden;
    }

    public void SetValue(float current, float max)
    {
        float newFraction = max > 0f ? Mathf.Clamp01(current / max) : 0f;

        if (newFraction < fraction) drainDelayRemaining = delayedDrainDelay;   // took a hit: hold the chunk a moment
        if (newFraction > delayedFraction) delayedFraction = newFraction;      // healing: no chunk
        if (newFraction < Full) hideTimeRemaining = hideDelay;

        fraction = newFraction;
        if (slider != null) slider.value = fraction;
    }

    // e.g. show the guard meter while blocking even if it's full
    public void SetForceVisible(bool visible)
    {
        forceVisible = visible;
        if (visible) hideTimeRemaining = hideDelay;
    }

    private void Update()
    {
        // HUD runs on real time so it keeps moving during hit-stop freezes
        float deltaTime = Time.unscaledDeltaTime;

        if (drainDelayRemaining > 0f) drainDelayRemaining -= deltaTime;
        else delayedFraction = Mathf.MoveTowards(delayedFraction, fraction, delayedDrainSpeed * deltaTime);

        if (delayedFill != null)
        {
            Vector2 anchorMax = delayedFill.anchorMax;
            anchorMax.x = delayedFraction;
            delayedFill.anchorMax = anchorMax;
        }

        if (fillImage != null)
        {
            bool low = fraction > 0f && fraction <= lowThreshold;
            float pulse = (Mathf.Sin(Time.unscaledTime * lowPulseSpeed) + 1f) * 0.5f;
            fillImage.color = low ? Color.Lerp(normalColor, lowColor, pulse) : normalColor;
        }

        if (canvasGroup != null && hideWhenFull)
        {
            hideTimeRemaining -= deltaTime;
            bool show = forceVisible || fraction < Full || hideTimeRemaining > 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, show ? Visible : Hidden, fadeSpeed * deltaTime);
        }
    }
}
