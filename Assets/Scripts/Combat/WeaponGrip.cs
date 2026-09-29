using UnityEngine;

// Lets a weapon change how it sits in the hand: one grip while moving/circling, another while swinging.
// The Synty sword animations are made for a longsword held blade-up; for a knife that looks wrong in
// the swing, so the attack grip turns the blade to lead toward the target. The switch happens over a
// fraction of a second, like the character re-gripping.
public class WeaponGrip : MonoBehaviour
{
    [SerializeField] private Vector3 guardGrip;              // local rotation while moving / circling
    [SerializeField] private Vector3 attackGrip;             // local rotation while attacking
    [SerializeField] private float regripDegreesPerSecond = 720f;

    private Quaternion targetGrip;

    private void Awake()
    {
        targetGrip = Quaternion.Euler(guardGrip);
    }

    public void UseAttackGrip(bool attacking)
    {
        targetGrip = Quaternion.Euler(attacking ? attackGrip : guardGrip);
    }

    private void LateUpdate()
    {
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetGrip, regripDegreesPerSecond * Time.deltaTime);
    }
}
