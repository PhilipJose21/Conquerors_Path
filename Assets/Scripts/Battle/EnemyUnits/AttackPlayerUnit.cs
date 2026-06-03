using UnityEngine;

public class AttackPlayerUnit : MonoBehaviour
{
    private UnitSO unitData;
    private EnemyMovement moveComp;
    public int dmg;

    void Awake()
    {
        // Find UnitSOContainer and EnemyMovement on this or parent objects so component can live on child objects
        UnitSOContainer container = this.GetComponentInParent<UnitSOContainer>();
        if (container != null)
        {
            unitData = container.unitData;
            dmg = unitData != null ? unitData.damage : dmg;
        }
        moveComp = this.GetComponentInParent<EnemyMovement>();
    }

    // Try to attack any player at the given world position (e.g., nearest player).
    // Returns true if a player was found and attacked.
    public bool TryAttackAtPosition(Vector3 worldPos)
    {
        float checkRadius = 0.6f;
        int available = moveComp != null ? moveComp.attackActions : 0;
        Debug.Log($"AttackPlayerUnit.TryAttackAtPosition: attempting attack at {worldPos}, available attackActions={available}");
        Collider[] hits = Physics.OverlapSphere(worldPos, checkRadius);
        Debug.Log($"AttackPlayerUnit: Overlap hits={hits.Length}");
        foreach (var h in hits)
        {
            Debug.Log($"  Hit collider: {h.gameObject.name} tag={h.gameObject.tag}");
            // Attempt to find UnitHealth on the collider or its parents
            var health = h.GetComponentInParent<UnitHealth>();
            if (health == null)
            {
                Debug.Log("  No UnitHealth on this hit");
                continue;
            }

            var owner = health.gameObject;
            bool isPlayer = owner.CompareTag("PlayerUnit") || owner.GetComponentInParent<MoveUnit>() != null;
            Debug.Log($"  Found UnitHealth on {owner.name}, isPlayer={isPlayer}");
            if (!isPlayer) continue;

            if (moveComp != null && moveComp.attackActions > 0)
            {
                Debug.Log($"  Attacking player {owner.name} for {dmg} dmg");
                health.TakeDamage(dmg);
                moveComp.attackActions = Mathf.Max(0, moveComp.attackActions - 1);
                Debug.Log($"  Attack succeeded, remaining enemy attackActions={moveComp.attackActions}");
                return true;
            }
            else
            {
                Debug.LogWarning("Enemy tried to attack but has no attack actions left.");
            }
        }
        Debug.Log("AttackPlayerUnit: no valid player target hit");
        return false;
    }

    public void CheckForPlayersInRange(int attackRangeCells, float cellSize)
    {
        float radius = (attackRangeCells + 0.5f) * cellSize;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        Debug.Log($"CheckForPlayersInRange: overlaps={hits.Length} radius={radius} attackActions={(moveComp!=null?moveComp.attackActions:0)}");
        foreach (var h in hits)
        {
            Debug.Log($"  Overlap hit: {h.gameObject.name} tag={h.gameObject.tag}");
            var health = h.GetComponentInParent<UnitHealth>();
            if (health == null)
            {
                Debug.Log("  No UnitHealth on this hit");
                continue;
            }
            var owner = health.gameObject;
            bool isPlayer = owner.CompareTag("PlayerUnit") || owner.GetComponentInParent<MoveUnit>() != null;
            Debug.Log($"  Found UnitHealth on {owner.name}, isPlayer={isPlayer}");
            if (!isPlayer) continue;

            if (moveComp != null && moveComp.attackActions > 0)
            {
                Debug.Log($"  CheckForPlayersInRange attacking {owner.name} for {dmg}");
                health.TakeDamage(dmg);
                moveComp.attackActions = Mathf.Max(0, moveComp.attackActions - 1);
                Debug.Log($"  After attack, enemy attackActions={moveComp.attackActions}");
                return;
            }
            else
            {
                Debug.LogWarning("Enemy tried to attack in CheckForPlayersInRange but has no attack actions left.");
            }
        }
    }
}
