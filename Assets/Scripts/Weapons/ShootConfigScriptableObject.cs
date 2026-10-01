using UnityEngine;

[CreateAssetMenu(fileName = "Shoot Config", menuName = "Guns/Shoot Configuration", order = 2)]
public class ShootConfigScriptableObject : ScriptableObject
{
    public LayerMask HitMask;                              // what bullets can hit
    public Vector3 Spread = new Vector3(.1f, .1f, .1f);    // random wobble added to each shot's direction (bigger = less accurate)
    public float FireRate = .25f;                          // seconds between shots while the button is held

    [Header("Recoil (arms kick on each shot)")]
    public float RecoilKickBack = 0.05f;   // metres the gun hand jumps back toward the shoulder
    public float RecoilKickAngle = 8f;     // degrees the muzzle flips up
}
