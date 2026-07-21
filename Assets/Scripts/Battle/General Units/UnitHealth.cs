using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UnitHealth : MonoBehaviour
{
    public UnitSO unitData;
    public int currentHealth;
    public int maxHealth;

    public Image healthBarFill; // Reference to the UI Image component for the health bar fill

    public unitPhase currentUnitPhase;
    public GameObject playerUnit;
    private bool isHealthUIHidden;
    private UnitStateMachine stateMachine;

    [Header("Death")]
    [Tooltip("How long (in seconds) the unit takes to shrink to nothing before being destroyed.")]
    public float deathScaleDuration = 0.6f;
    private bool isDying;

    void Awake()
    {
        UnitSOContainer container = this.GetComponent<UnitSOContainer>();
        stateMachine = this.GetComponent<UnitStateMachine>();
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

    public void SetHealthUIHidden(bool hidden)
    {
        isHealthUIHidden = hidden;

        if (healthBarFill != null)
        {
            healthBarFill.enabled = !hidden;
        }
    }

    public bool IsHealthUIHidden()
    {
        return isHealthUIHidden;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            stateMachine?.ChangeState(unitPhase.Hurt, forceRetrigger: true);
        }
    }

    public void Die()
    {
        if (isDying)
        {
            return; // already dying — don't start a second death sequence
        }
        isDying = true;
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        stateMachine?.ChangeState(unitPhase.Hurt, forceRetrigger: true, holdState: true);

        Transform deathTransform = playerUnit != null ? playerUnit.transform : transform;
        Vector3 startScale = deathTransform.localScale;
        float elapsed = 0f;

        while (elapsed < deathScaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = deathScaleDuration > 0f ? elapsed / deathScaleDuration : 1f;
            deathTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        deathTransform.localScale = Vector3.zero;
        Destroy(playerUnit);
    }
}