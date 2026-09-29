using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] AudioClip hurtSound;
    [SerializeField] ParticleSystem hurtParticles;
    [SerializeField] Collider hurtBox;
    [SerializeField] ParticleSystem blockSparks;   // sprays off the guard when a hit is blocked
    [SerializeField] AudioClip blockSound;

    private int health;
    private bool isInvulnerable;
    private bool isBlocking;
    private Poise guard;   // optional: a Poise on the same object doubles as the guard meter while blocking
    public bool isDead => health == 0;
    public Vector3 LastHitFrom { get; private set; }   // lets hit reactions face the right way
    public int LastDamage { get; private set; }        // lets hit reactions tell light hits from heavy ones

    public event Action OnTakeDamage;
    public event Action OnDeath;


    private void Start()
    {
        health = maxHealth;
        TryGetComponent(out guard);
    }

    public void DealDamage(int damage, Vector3 attackerPosition)
    {
        if (health <= 0) return;

        if (isInvulnerable)
        {
            return;
        }

        // blocked from the front: the guard soaks the hit instead of health (hits from behind or the side still land)
        if (isBlocking && guard != null && HitDirection.DirectionHitFrom(transform, attackerPosition) == HitDirection.Front)
        {
            if (blockSound != null) SFXManager.instance.PlaySoundFXClip(blockSound, transform, 1f);
            SpawnHitEffect(blockSparks, attackerPosition);
            guard.TakePoiseDamage(damage);
            return;
        }

        LastHitFrom = attackerPosition;
        LastDamage = damage;

        health = Mathf.Max(health - damage, 0);

        if (health == 0)
        {
            OnDeath?.Invoke();
            return;
        }

        SFXManager.instance.PlaySoundFXClip(hurtSound, transform, 1f);
        SpawnHitEffect(hurtParticles, attackerPosition);

        OnTakeDamage?.Invoke();
        Debug.Log(health);
    }

    // sprays an effect out from the point of the body closest to the attacker
    private void SpawnHitEffect(ParticleSystem effect, Vector3 attackerPosition)
    {
        if (effect == null || hurtBox == null) return;

        Vector3 hitPoint = hurtBox.ClosestPoint(attackerPosition);
        Vector3 outward = (hitPoint - hurtBox.bounds.center).normalized;
        Quaternion sprayRotation = Quaternion.LookRotation(outward + Vector3.up);

        ParticleSystem newParticle = Instantiate(effect, hitPoint, sprayRotation);
        Destroy(newParticle.gameObject, newParticle.main.duration + newParticle.main.startLifetime.constantMax);
    }

    public void SetBlocking(bool isBlocking)
    {
        this.isBlocking = isBlocking;
    }

    public void SetInvulnerability(bool isInvulnerable)
    {
        this.isInvulnerable = isInvulnerable;
    }




}
