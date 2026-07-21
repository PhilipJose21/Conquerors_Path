using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public Animator animator;
    public AnimationClip attackAnimation; // played for the "Damage" state (unit dealing damage)
    public AnimationClip idleAnimation;   // played for the "Idle" state
    public AnimationClip hurtAnimation;   // played for the "Hurt" state (unit taking damage)
    public AnimationClip moveAnimation;   // played for the "Move" state

    // These must match the Trigger parameter names set up on the Animator Controller.
    private const string IdleTrigger = "Idle";
    private const string MoveTrigger = "Move";
    private const string DamageTrigger = "Damage";
    private const string HurtTrigger = "Hurt";

    public float GetClipLength(unitPhase phase)
    {
        switch (phase)
        {
            case unitPhase.Idle:
                return idleAnimation != null ? idleAnimation.length : 0f;
            case unitPhase.Move:
                return moveAnimation != null ? moveAnimation.length : 0f;
            case unitPhase.Damage:
                return attackAnimation != null ? attackAnimation.length : 0f;
            case unitPhase.Hurt:
                return hurtAnimation != null ? hurtAnimation.length : 0f;
        }
        return 0f;
    }

    public void RestartCurrentState(int layer = 0)
    {
        if (animator == null)
        {
            return;
        }
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layer);
        animator.Play(info.fullPathHash, layer, 0f);
    }

    public void PlayAnimationForState(unitPhase phase)
    {
        if (animator == null)
        {
            return;
        }

        switch (phase)
        {
            case unitPhase.Idle:
                animator.SetTrigger(IdleTrigger);
                break;
            case unitPhase.Move:
                animator.SetTrigger(MoveTrigger);
                break;
            case unitPhase.Damage:
                animator.SetTrigger(DamageTrigger);
                break;
            case unitPhase.Hurt:
                animator.SetTrigger(HurtTrigger);
                break;
        }
    }
}