using UnityEngine;

public abstract class State
{
    public abstract void Enter(); // any state 
    public abstract void Tick(float deltaTime); 
    public abstract void Exit();
    protected float GetNormailzedTime(Animator animator)
    {
        AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0); // only using layer 0  in the animator
        AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);

        if (animator.IsInTransition(0) && nextInfo.IsTag("Attack"))
            return nextInfo.normalizedTime;
        else if (!animator.IsInTransition(0) && currentInfo.IsTag("Attack"))
            return currentInfo.normalizedTime;

        else
            return 0;

    }

}
