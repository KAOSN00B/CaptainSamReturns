using UnityEngine;

// Put this on an Animator state to tell the character's WeaponGrip which grip to use while that state plays.
public class WeaponGripSwitch : StateMachineBehaviour
{
    [SerializeField] private bool useAttackGrip;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        WeaponGrip grip = animator.GetComponentInChildren<WeaponGrip>(true);
        if (grip != null) grip.UseAttackGrip(useAttackGrip);
    }
}
