using UnityEngine;

public enum unitPhase
{
    Idle,
    Selected,
    Moving,
    Attacking,
    Hurt,
    Dying,
    Dead
}

public class UnitStateMachine : MonoBehaviour
{
    
    public unitPhase currentUnitPhase;

    void Start()
    {
        AttackEnemyUnit attackComponent = this.GetComponent<AttackEnemyUnit>();
        UnitHealth healthComponent = this.GetComponent<UnitHealth>();
        MoveUnit moveComponent = this.GetComponent<MoveUnit>();
        if (attackComponent == null || healthComponent == null || moveComponent == null)
        {
            Debug.LogError("UnitStateMachine requires AttackEnemyUnit, UnitHealth, and MoveUnit components.");
            return;
        }
        currentUnitPhase = unitPhase.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentUnitPhase)
        {
            case unitPhase.Idle:
                // Handle idle behavior
                break;
            case unitPhase.Selected:
                // Handle selected behavior
                break;
            case unitPhase.Moving:
                // Handle moving behavior
                break;
            case unitPhase.Attacking:
                // Handle attacking behavior
                break;
            case unitPhase.Hurt:
                // Handle hurt behavior
                break;
            case unitPhase.Dying:
                // Handle dying behavior
                break;
            case unitPhase.Dead:
                // Handle dead behavior
                break;
        }
    }
}
