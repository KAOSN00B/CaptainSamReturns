using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private GameObject weaponLogic;
    [SerializeField] private TrailRenderer swingTrail;
    [SerializeField] AudioClip weaponHitSound;

    public void EnableWeapon()
    {
        weaponLogic.SetActive(true);
        swingTrail.Clear();
        swingTrail.emitting = true;
        SFXManager.instance.PlaySoundFXClip(weaponHitSound, transform, 1f);


    }

    public void DisableWeapon()
    {
        weaponLogic.SetActive(false);
        swingTrail.emitting = false;
    }

}
