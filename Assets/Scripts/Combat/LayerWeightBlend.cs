using UnityEngine;

// Put this on an Animator state to fade another Animator layer in or out while that state plays.
// Used on the enemy so its "GuardArm" layer (knife pointed at the player) is on while it moves or
// circles, and off while it attacks or staggers - no state machine code needed.
public class LayerWeightBlend : StateMachineBehaviour
{
    [SerializeField] private string layerName = "GuardArm";
    [SerializeField, Range(0f, 1f)] private float targetWeight = 1f;
    [SerializeField] private float blendSpeed = 8f;   // weight change per second

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int layer = animator.GetLayerIndex(layerName);
        if (layer < 0) return;

        float weight = Mathf.MoveTowards(animator.GetLayerWeight(layer), targetWeight, blendSpeed * Time.deltaTime);
        animator.SetLayerWeight(layer, weight);
    }
}
