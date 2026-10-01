using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponDamage : MonoBehaviour
{

    [SerializeField] private Collider myCollider;
    [SerializeField] private int myDamage = 10;
    [SerializeField] private CinemachineImpulseSource impluseSource;
    [SerializeField] private ParticleSystem swordSparks;
    [SerializeField] private Collider attackLogicCollider;
    [SerializeField] private MMF_Player hitFeedback;   // hit-confirm punch (chromatic kick, impact sound) - player sword only




    private int damage = 10;
    private int poise = 1;
    private float knockBack;
    private float cameraShake;
    private float hitStopDuration;
    private AudioClip hitClip;

    private const float AlwaysFreeze = 0f;   // Feel skips a freeze if time scale is below this; 0 = never skip

    private List<Collider> alreadyCollidedWith = new List<Collider>();

    private void OnEnable()
    {
        alreadyCollidedWith.Clear();
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == myCollider) return;

        if (alreadyCollidedWith.Contains(other)) return;

        alreadyCollidedWith.Add(other);

        if (other.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(damage, attackLogicCollider.bounds.center);

            if (hitClip != null) SFXManager.instance.PlaySoundFXClip(hitClip, transform, .5f);

            if (cameraShake > 0) impluseSource.GenerateImpulse(cameraShake);

            // Feel's MMTimeManager owns time scale now (freeze frames, slow-mo), so ask it for the hit stop
            if (hitStopDuration > 0f) MMFreezeFrameEvent.Trigger(hitStopDuration, AlwaysFreeze);

            if (hitFeedback != null) hitFeedback.PlayFeedbacks(attackLogicCollider.ClosestPoint(other.bounds.center));

        }

        if (other.TryGetComponent<Poise>(out Poise targetPoise))
        {   
            targetPoise.TakePoiseDamage(poise);
        }

        if (swordSparks != null && !other.isTrigger)
        {
            // play the sword sparks upwards
            Vector3 hitPoint = attackLogicCollider.ClosestPoint(other.bounds.center);
            Vector3 outward = (hitPoint - other.bounds.center).normalized;
            Quaternion sprayRotation = Quaternion.LookRotation(outward + Vector3.up);

            // play sparks
            ParticleSystem newParticle = Instantiate(swordSparks, hitPoint, sprayRotation);
            Destroy(newParticle.gameObject, newParticle.main.duration + newParticle.main.startLifetime.constantMax);
        }

        if (other.TryGetComponent<ForceReceiver>(out ForceReceiver forceReceiver))
        {
            Vector3 direction = (other.transform.position - myCollider.transform.position).normalized;
            forceReceiver.AddForce(direction * knockBack);
        }
    }

    public void SetAttack(int damage, float knockBack, float cameraShake = 0f, float hitStopDuration = 0f, AudioClip hitclip = null, int poise = 0)
    {
       this.damage = damage;
       this.knockBack = knockBack;
       this.cameraShake = cameraShake;
       this.hitStopDuration = hitStopDuration;
       this.hitClip = hitclip;
       this.poise = poise;

    }
}
