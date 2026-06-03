using UnityEngine;

public class AttackPlayerUnit : MonoBehaviour
{
    private UnitSO unitData;
    private EnemyMovement moveComp;
    private int attackPoints;
    public int dmg;

    void Awake()
    {
        UnitSOContainer container = this.GetComponent<UnitSOContainer>();
        if (container != null)
        {
            unitData = container.unitData;
            moveComp = this.GetComponent<EnemyMovement>();
            attackPoints = moveComp != null ? moveComp.attackActions : 0;
        }
    }

    // Try to attack any player at the given world position (e.g., nearest player).
    // Returns true if a player was found and attacked.
    public bool TryAttackAtPosition(Vector3 worldPos)
    {
        float checkRadius = 0.6f;
        Collider[] hits = Physics.OverlapSphere(worldPos, checkRadius);
        foreach (var h in hits)
        {
            if (h.CompareTag("PlayerUnit") && attackPoints > 0)
            {
                var health = h.GetComponent<UnitHealth>();
                if (health != null)
                {
                    health.TakeDamage(dmg);
                    attackPoints--;
                    return true;
                }
            }
        }
        return false;
    }

    public void CheckForPlayersInRange(int attackRangeCells, float cellSize)
    {
        float radius = (attackRangeCells + 0.5f) * cellSize;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var h in hits)
        {
            if (h.CompareTag("PlayerUnit") && attackPoints > 0)
            {
                h.GetComponent<UnitHealth>()?.TakeDamage(dmg);
                attackPoints--;
                return;
            }
        }
    }
}
