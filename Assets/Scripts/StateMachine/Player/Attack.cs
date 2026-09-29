using System;
using UnityEngine;

[Serializable]
public class Attack
{
    [field: SerializeField] public string AnimationName { get; private set; }
    [field: SerializeField] public string RecoveryAnimationName { get; private set; }  // "return to idle" state played when the combo ends here (blank = none)
    [field: SerializeField] public float TransitionDuration { get; private set; }
    [field: SerializeField] public int ComboStateIndex { get; private set; } = -1;
    [field: SerializeField] public int PoiseDamage { get; private set; } = 1; 
    [field: SerializeField] public float ComboAttackTime { get; private set; }
    [field: SerializeField] public float Force { get; private set; }
    [field: SerializeField] public float ForceTime { get; private set; }
    [field: SerializeField] public float KnockBack { get; private set; }
    [field: SerializeField] public int Damage { get; private set; }
    [field: SerializeField] public float CameraShake { get; private set; }
    [field: SerializeField] public float HitStopDuration { get; private set; }
    [field: SerializeField] public AudioClip SwordSwingSFX { get; private set; }

}

