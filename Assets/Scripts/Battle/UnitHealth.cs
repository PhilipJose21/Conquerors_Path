using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    public UnitSO unitData;
    public int currentHealth;
    public int maxHealth;


    public unitPhase currentUnitPhase;

    void Awake()
    {
        UnitSOContainer container = this.GetComponent<UnitSOContainer>();
        UnitStateMachine stateMachine = this.GetComponent<UnitStateMachine>();
        if (stateMachine != null)
        {
            currentUnitPhase = stateMachine.currentUnitPhase;
        }
        if (container != null)
        {
            unitData = container.unitData;
        }
        maxHealth = unitData != null ? unitData.health : maxHealth;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
        }
    }
}
