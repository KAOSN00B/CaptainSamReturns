using System;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Feedbacks;
using UnityEngine;

// Owns the player's gun and swaps between sword and gun.
//
//   Start():    spawns the gun model (hidden) in the right hand and connects it to the aiming IK.
//   Equip():    gun out  - show gun, hide sword, use the gun's animation clips. Called by PlayerAimingState.
//   Unequip():  sword out - the reverse. Called by the sword attack, slam and block states.
//
// Animations: the Animator keeps ONE controller the whole time. A runtime AnimatorOverrideController sits on top,
// and equipping only swaps which clips it plays (so the swap never restarts the animator).
// The pistol uses Controller_Player's own AimBlendTree clips; another gun (e.g. a rifle) can set GunScriptableObject.AnimatorOverride
// to replace those clips with its own.
[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour
{
    [SerializeField] private GunType Gun;                             // which gun from the list to carry
    [SerializeField] private Transform GunParent;                     // Hand_R/GunSocket: the gun rides on the right hand
    [SerializeField] private List<GunScriptableObject> Guns;
    [SerializeField] private PlayerIK InverseKinematics;
    [SerializeField] private Animator Animator;
    [SerializeField] private GameObject SwordModel;                   // hidden while the gun is out
    [SerializeField, Min(0f)] private float equipReadyTime = 0.15f;   // seconds before a freshly drawn gun can fire (arms coming up)
    [SerializeField] private WeaponHandler swordHandler;              // its hitbox/trail is switched off when the gun comes out
    [SerializeField] private MMF_Player swapFeedback;                 // sparks + sound at the hand whenever the weapon swaps

    [Header("Runtime Filled")]
    public GunScriptableObject ActiveGun;   // a runtime copy of the gun asset (see SpawnGun)

    public bool IsGunEquipped { get; private set; }
    public bool CanShoot => IsGunEquipped && Time.time >= readyAt;
    public event Action<bool> OnEquipChanged;   // true = gun out, false = sword out (the crosshair listens to this)

    // names of the optional grip points on a gun prefab
    private const string LeftHandGripName = "LeftHand";
    private const string LeftElbowGripName = "LeftElbow";

    private AnimatorOverrideController runtimeController;
    private readonly List<KeyValuePair<AnimationClip, AnimationClip>> swordClips = new();   // "no overrides" = the base controller's clips
    private readonly List<KeyValuePair<AnimationClip, AnimationClip>> gunClips = new();     // the gun's replacements (if it has any)
    private float readyAt;

    private void Start()
    {
        if (Animator == null) Animator = GetComponent<Animator>();
        if (InverseKinematics == null) InverseKinematics = GetComponent<PlayerIK>();
        if (Animator == null || Animator.runtimeAnimatorController == null || GunParent == null)
        {
            Debug.LogError("Gun selector needs an Animator, a controller and a gun socket.", this);
            return;
        }

        GunScriptableObject gun = Guns?.Find(candidate => candidate != null && candidate.Type == Gun);
        if (gun == null || gun.ModelPrefab == null || gun.ShootConfig == null || gun.TrailConfig == null)
        {
            Debug.LogError($"Complete the gun asset for {Gun} before equipping it.", this);
            return;
        }

        BuildAnimationSets(gun);
        SpawnGun(gun);
        ConnectArmIK();
    }

    // Prepare the two clip lists the runtime controller switches between.
    private void BuildAnimationSets(GunScriptableObject gun)
    {
        runtimeController = new AnimatorOverrideController(Animator.runtimeAnimatorController);
        runtimeController.name = "Player weapons (runtime)";

        runtimeController.GetOverrides(swordClips);   // every clip mapped to "nothing" = play the base clips
        runtimeController.GetOverrides(gunClips);

        if (gun.AnimatorOverride != null)
        {
            for (int i = 0; i < gunClips.Count; i++)
            {
                AnimationClip baseClip = gunClips[i].Key;
                gunClips[i] = new KeyValuePair<AnimationClip, AnimationClip>(baseClip, gun.AnimatorOverride[baseClip]);
            }
        }

        Animator.runtimeAnimatorController = runtimeController;
    }

    // The gun asset is copied first: Spawn() stores the spawned model and bullet-trail pool on the object,
    // and that runtime state must not end up saved on the shared project asset.
    private void SpawnGun(GunScriptableObject gun)
    {
        ActiveGun = Instantiate(gun);
        ActiveGun.Spawn(GunParent, this);
        ActiveGun.SetVisible(false);                   // the sword is out at the start
        if (SwordModel != null) SwordModel.SetActive(true);
    }

    // Hand the gun's grip points and muzzle to the aiming IK.
    private void ConnectArmIK()
    {
        if (InverseKinematics == null) return;

        Transform[] gunParts = GunParent.GetComponentsInChildren<Transform>(true);
        InverseKinematics.LeftHandIKTarget = gunParts.FirstOrDefault(part => part.name == LeftHandGripName);
        InverseKinematics.LeftElbowIKTarget = gunParts.FirstOrDefault(part => part.name == LeftElbowGripName);
        InverseKinematics.SetWeapon(ActiveGun.Muzzle);
    }

    // Gun out. Call this BEFORE the aiming state crossfades into its animation.
    public void Equip()
    {
        if (IsGunEquipped || ActiveGun == null || runtimeController == null) return;

        if (swordHandler != null) swordHandler.DisableWeapon();   // no stray sword hitbox from an interrupted swing
        runtimeController.ApplyOverrides(gunClips);
        IsGunEquipped = true;
        readyAt = Time.time + equipReadyTime;

        ActiveGun.SetVisible(true);
        if (SwordModel != null) SwordModel.SetActive(false);
        PlaySwapFeedback();
        OnEquipChanged?.Invoke(true);
    }

    // Sword out. Call this BEFORE the sword state crossfades into its animation.
    public void Unequip()
    {
        if (!IsGunEquipped) return;

        if (InverseKinematics != null) InverseKinematics.ResetAim();   // let the sword animation have the arms right away
        runtimeController.ApplyOverrides(swordClips);
        IsGunEquipped = false;

        ActiveGun.SetVisible(false);
        if (SwordModel != null) SwordModel.SetActive(true);
        PlaySwapFeedback();
        OnEquipChanged?.Invoke(false);
    }

    private void PlaySwapFeedback()
    {
        if (swapFeedback != null) swapFeedback.PlayFeedbacks(GunParent.position);
    }

    private void OnDestroy()
    {
        if (ActiveGun != null)
        {
            ActiveGun.Despawn();
            Destroy(ActiveGun);
        }
        if (runtimeController != null) Destroy(runtimeController);
    }
}
