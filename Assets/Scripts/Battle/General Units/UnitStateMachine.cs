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

    public void ChangeState(unitPhase newPhase)
    {
        if (currentUnitPhase == newPhase)
        {
            return;
        }

        currentUnitPhase = newPhase;
        animationManager?.PlayAnimationForState(currentUnitPhase);

        if (revertToIdleCoroutine != null)
        {
            StopCoroutine(revertToIdleCoroutine);
            revertToIdleCoroutine = null;
        }
        
        if (newPhase == unitPhase.Hurt || newPhase == unitPhase.Damage)
        {
            float clipLength = animationManager != null ? animationManager.GetClipLength(newPhase) : 0f;
            if (clipLength > 0f)
            {
                revertToIdleCoroutine = StartCoroutine(RevertToIdleAfterDelay(clipLength));
            }
        }
    }

    private IEnumerator RevertToIdleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        revertToIdleCoroutine = null;
        ChangeState(unitPhase.Idle);
    }
}