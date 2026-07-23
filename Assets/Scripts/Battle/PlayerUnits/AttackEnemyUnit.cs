using UnityEngine;

public class AttackEnemyUnit : MonoBehaviour
{
    private UnitSO unitData;
    private MoveUnit moveUnit;
    private UnitStateMachine stateMachine;
    private RotateModel rotateModel;
    public int dmg;

    void Awake()
    {
        UnitSOContainer container = this.GetComponent<UnitSOContainer>();
        stateMachine = this.GetComponent<UnitStateMachine>();

        rotateModel = this.GetComponent<RotateModel>();
        if (rotateModel == null) rotateModel = this.GetComponentInChildren<RotateModel>();
        if (rotateModel == null) rotateModel = this.GetComponentInParent<RotateModel>();

        if (container != null)
        {
            unitData = container.unitData;
            moveUnit = this.GetComponent<MoveUnit>();
            dmg = unitData != null ? unitData.damage : dmg;
        }
    }    

    private string GetDisplayName(GameObject go)
    {
        if (go == null) return "Unit";
        var container = go.GetComponentInParent<UnitSOContainer>();
        if (container != null && container.unitData != null)
        {
            return container.unitData.unitName;
        }
        return go.name;
    }

    public bool TryAttackAtPosition(Vector3 worldPos)
    {
        float checkRadius = 0.6f;
        Collider[] hits = Physics.OverlapSphere(worldPos, checkRadius);
        foreach (var h in hits)
        {
            var health = h.GetComponentInParent<UnitHealth>();
            if (health != null)
            {
                var owner = health.gameObject;
                bool isEnemy = owner.CompareTag("EnemyUnit") || owner.GetComponentInParent<EnemyMovement>() != null;
                if (!isEnemy) continue;

                bool defenderImmune = false;
                Collider[] terrainHits = Physics.OverlapSphere(owner.transform.position, 0.2f);
                foreach (var th in terrainHits)
                {
                    var ti = th.GetComponentInParent<TerrainInteraction>();
                    if (ti != null && ti.IsAttackRangeImmune()) { defenderImmune = true; break; }
                }

                Vector3 attackerPos = moveUnit != null ? moveUnit.transform.position : transform.position;
                if (defenderImmune)
                {
                    BuildingGrid grid = BuildingGridManager.Instance.FindGridAtPosition(owner.transform.position);
                    bool adjacent = false;
                    if (grid != null)
                    {
                        (int ax, int ay) = grid.WorldToGridPosition(attackerPos);
                        (int dx, int dy) = grid.WorldToGridPosition(owner.transform.position);
                        adjacent = (Mathf.Abs(ax - dx) + Mathf.Abs(ay - dy)) == 1;
                    }
                    else
                    {
                        adjacent = Vector3.Distance(attackerPos, owner.transform.position) <= BuildingSystem.CellSize * 1.5f;
                    }

                    if (!adjacent) return false;
                }

                if (moveUnit != null && moveUnit.attackActions > 0)
                {
                    moveUnit.attackActions = Mathf.Max(0, moveUnit.attackActions - 1);

                    if (rotateModel != null)
                    {
                        rotateModel.FacePosition(owner.transform.position, () =>
                        {
                            health.TakeDamage(dmg);
                            stateMachine?.ChangeState(unitPhase.Damage);
                            // Show Blue Text (isPlayer = true)
                            ActionText.Instance?.ShowAttackText(GetDisplayName(moveUnit != null ? moveUnit.gameObject : gameObject), GetDisplayName(owner), dmg, owner.transform.position, true);
                        });
                    }
                    else
                    {
                        health.TakeDamage(dmg);
                        stateMachine?.ChangeState(unitPhase.Damage);
                        // Show Blue Text (isPlayer = true)
                        ActionText.Instance?.ShowAttackText(GetDisplayName(moveUnit != null ? moveUnit.gameObject : gameObject), GetDisplayName(owner), dmg, owner.transform.position, true);
                    }
                }

                CellHighlighter.Instance?.ClearHighlights();
                return true;
            }
        }
        return false;
    }

    public void CheckForEnemiesInRange(int attackRangeCells, float cellSize)
    {
        float radius = (attackRangeCells + 0.5f) * cellSize;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var h in hits)
        {
            var health = h.GetComponentInParent<UnitHealth>();
            if (health == null) continue;
            var owner = health.gameObject;
            bool isEnemy = owner.CompareTag("EnemyUnit") || owner.GetComponentInParent<EnemyMovement>() != null;
            if (!isEnemy) continue;

            if (moveUnit != null && moveUnit.attackActions > 0)
            {
                moveUnit.attackActions = Mathf.Max(0, moveUnit.attackActions - 1);
                CellHighlighter.Instance?.ClearHighlights();

                if (rotateModel != null)
                {
                    rotateModel.FacePosition(owner.transform.position, () =>
                    {
                        health.TakeDamage(dmg);
                        stateMachine?.ChangeState(unitPhase.Damage);
                        // Show Blue Text (isPlayer = true)
                        ActionText.Instance?.ShowAttackText(GetDisplayName(moveUnit != null ? moveUnit.gameObject : gameObject), GetDisplayName(owner), dmg, owner.transform.position, true);
                    });
                }
                else
                {
                    health.TakeDamage(dmg);
                    stateMachine?.ChangeState(unitPhase.Damage);
                    // Show Blue Text (isPlayer = true)
                    ActionText.Instance?.ShowAttackText(GetDisplayName(moveUnit != null ? moveUnit.gameObject : gameObject), GetDisplayName(owner), dmg, owner.transform.position, true);
                }
                return;
            }
        }
    }
}