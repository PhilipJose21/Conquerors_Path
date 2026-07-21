using UnityEngine;
using System.Collections;

// Only these 4 states are currently supported/animated: Idle, Move, Damage, Hurt.
public enum unitPhase
{
    Idle,
    Move,
    Damage,
    Hurt
}

public class UnitStateMachine : MonoBehaviour
{
    public unitPhase currentUnitPhase;
    private AnimationManager animationManager;
    private Coroutine revertToIdleCoroutine;

    void Awake()
    {
        animationManager = this.GetComponent<AnimationManager>();
    }

    void Start()
    {
        currentUnitPhase = unitPhase.Idle;
        animationManager?.PlayAnimationForState(currentUnitPhase);
    }

    public void ChangeState(unitPhase newPhase, bool forceRetrigger = false, bool holdState = false)
    {
        bool isSamePhase = currentUnitPhase == newPhase;

        if (isSamePhase && !forceRetrigger)
        {
            return;
        }

        currentUnitPhase = newPhase;

        if (isSamePhase && forceRetrigger)
        {
            animationManager?.RestartCurrentState();
        }
        else
        {
            animationManager?.PlayAnimationForState(currentUnitPhase);
        }

        // Cancel any pending "revert to idle" from a previous one-shot state.
        if (revertToIdleCoroutine != null)
        {
            StopCoroutine(revertToIdleCoroutine);
            revertToIdleCoroutine = null;
        }

        if (!holdState && (newPhase == unitPhase.Hurt || newPhase == unitPhase.Damage))
        {
            float clipLength = animationManager != null ? animationManager.GetClipLength(newPhase) : 0f;
            if (clipLength > 0f)
            {
                revertToIdleCoroutine = StartCoroutine(RevertToIdleAfterDelay(clipLength));
            }
        }
    }

    public float GetClipLength(unitPhase phase)
    {
        return animationManager != null ? animationManager.GetClipLength(phase) : 0f;
    }

    private IEnumerator RevertToIdleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        revertToIdleCoroutine = null;
        ChangeState(unitPhase.Idle);
    }
}