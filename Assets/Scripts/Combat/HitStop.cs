using System;
using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    [SerializeField] private float stopTimeScale = 0.03f;

    private Coroutine runningCoroutine;

    public void StopRunning(float duration)
    {
        if (duration <= 0) return;

        if (runningCoroutine != null ) {StopCoroutine(runningCoroutine);}

        runningCoroutine = StartCoroutine(StopRoutine(duration));
    }

    private IEnumerator StopRoutine(float duration)
    {
        Time.timeScale = stopTimeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        runningCoroutine = null;

    }
}
