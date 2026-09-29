using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float duration = 0.15f;

    private Material flashInstance;
    private Material[][] originalMaterials;
    private Material[][] flashMaterials;
    private Coroutine flashRoutine;

    private void Awake()
    {
        flashInstance = new Material(flashMaterial); // own copy so fading doesn't change the shared asset

        originalMaterials = new Material[renderers.Length][];
        flashMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].sharedMaterials;

            // same materials plus the overlay on the end, so the mesh is drawn again brightened on top
            flashMaterials[i] = new Material[originalMaterials[i].Length + 1];
            originalMaterials[i].CopyTo(flashMaterials[i], 0);
            flashMaterials[i][originalMaterials[i].Length] = flashInstance;
        }
    }

    private void OnEnable()
    {
        health.OnTakeDamage += HandleHit;
        health.OnDeath += HandleHit;
    }

    private void OnDisable()
    {
        health.OnTakeDamage -= HandleHit;
        health.OnDeath -= HandleHit;
    }

    private void HandleHit()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash(flashColor, duration));
    }

    private IEnumerator Flash(Color color, float flashDuration)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterials = flashMaterials[i];
        }

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            // additive: fading to black means adding nothing, so the flash fades out smoothly
            flashInstance.SetColor("_BaseColor", Color.Lerp(color, Color.black, elapsed / flashDuration));
            elapsed += Time.unscaledDeltaTime; // real time, so hit-stop doesn't stretch the flash
            yield return null;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterials = originalMaterials[i];
        }

        flashRoutine = null;
    }

    public void PlayFlash(Color color, float flashDuration)
    {
        if (flashRoutine != null) StopCoroutine (flashRoutine);
        flashRoutine = StartCoroutine(Flash(color, flashDuration));
    }
}
