using UnityEngine;
using UnityEngine.UI;

public class UnitHealth : MonoBehaviour
{
    public UnitSO unitData;
    public int currentHealth;
    public int maxHealth;

    public Image healthBarFill; // Reference to the UI Image component for the health bar fill

    public unitPhase currentUnitPhase;
    public GameObject playerUnit;

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

    void Update()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Die()
    {
        Destroy(playerUnit);
    }
}
