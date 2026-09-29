using System;
using UnityEngine;

public class Poise : MonoBehaviour
{
    [SerializeField] private int maxPoise = 50;
    [SerializeField] private float regenDelay = 2f;
    [SerializeField] private float regenPerSecond = 25f;

    private float currentPoise;
    private float timeSinceLastHit;

    public event Action OnPoiseBroken;

    private void Start()
    {
        currentPoise = maxPoise;
    }

    private void Update()
    {
        timeSinceLastHit += Time.deltaTime;
        if (timeSinceLastHit >= regenDelay)
        {

            currentPoise = Mathf.MoveTowards(currentPoise, 
                maxPoise, regenPerSecond * Time.deltaTime);
            
        }
    }

    public void TakePoiseDamage(int amount)
    {
        currentPoise -= amount;
        timeSinceLastHit = 0;

        if (currentPoise <= 0)
        {
            currentPoise = maxPoise;
            OnPoiseBroken?.Invoke();
        }

    }
}
