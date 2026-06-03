using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    public UnitSO unitData;

    [Header("Range (set per-unit or via UnitSO)")]
    public int mobility = 2; // Manhattan (diamond/cross) movement range
    public int attackRange = 1; // Square attack range


    [Header("Movement")]
    public float moveSpeed = 4f;
    public int moveActions = 1;
    public int attackActions = 1;
    public bool canMove = true;

    [Header("States")]
    private Coroutine moveCoroutine;
    private TurnManager turnManager;
    public turnPhase currentTurnPhase;
    public UnitStateMachine stateMachine;
    public bool isPlayerTurn;
    public bool isSelected;
    public bool endTurn;

    public GameObject unitObject;
    private bool hasActedThisTurn = false;

    void Awake()
    {
        UnitSOContainer container = this.GetComponent<UnitSOContainer>();
        turnManager = Object.FindAnyObjectByType<TurnManager>();
        if (container != null)
        {
            unitData = container.unitData;
        }

        mobility = unitData != null ? unitData.mobility : mobility;
        attackRange = unitData != null ? unitData.attackRange : attackRange;
        attackActions = unitData != null ? unitData.attackPoints : attackActions;
        moveActions = unitData != null ? unitData.movePoints : moveActions;
        stateMachine = this.GetComponent<UnitStateMachine>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTurnPhase = turnManager != null ? turnManager.currentTurnPhase : currentTurnPhase;
        // Reset per-turn action flag when leaving enemy turn
        if (currentTurnPhase != turnPhase.EnemyTurn)
        {
            hasActedThisTurn = false;
            return;
        }

        // On enemy turn, perform one action per enemy instance
        if (!hasActedThisTurn)
        {
            hasActedThisTurn = true;
            Act();
        }
    }

    void Act()
    {
        // Find nearest player unit
        Debug.Log("Act");
        GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerUnit");
        if (players == null || players.Length == 0)
        {
            // No player units present to target -> mark this enemy as finished for the turn
            endTurn = true;
            return;
        }

        Transform nearest = null;
        float bestDist = float.MaxValue;
        Vector3 myPos = unitObject != null ? unitObject.transform.position : transform.position;
        foreach (var p in players)
        {
            float d = Vector3.Distance(myPos, p.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                nearest = p.transform;
            }
        }
        if (nearest == null) return;

        // Compute approximate cell size
        BuildingGrid[] grids = FindObjectsOfType<BuildingGrid>();
        float approxCell = 1f;
        if (grids != null && grids.Length > 0) approxCell = grids[0].CellSize;
        // Check if any player is already within attack range from current position.
        float maxAttackDist = (attackRange + 0.5f) * approxCell;
        Debug.Log($"EnemyMovement.Act: nearestDist={bestDist} maxAttackDist={maxAttackDist} attackActions={attackActions}");

        // Find any players within attack distance (could be multiple)
        Transform inRangeTarget = null;
        float inRangeBest = float.MaxValue;
        foreach (var p in players)
        {
            float d = Vector3.Distance(myPos, p.transform.position);
            if (d <= maxAttackDist && d < inRangeBest)
            {
                inRangeBest = d;
                inRangeTarget = p.transform;
            }
        }

        var attackerComp = this.GetComponentInChildren<AttackPlayerUnit>();
        if (inRangeTarget != null)
        {
            // There is at least one player in range -> attack the closest one
            if (attackerComp == null)
            {
                Debug.Log("EnemyMovement.Act: no AttackPlayerUnit component found to perform attack");
                return;
            }
            if (attackActions <= 0)
            {
                Debug.Log("EnemyMovement.Act: no attackActions left");
                return;
            }
            Debug.Log($"EnemyMovement.Act: player in range -> attacking {inRangeTarget.name}");
            bool attacked = attackerComp.TryAttackAtPosition(inRangeTarget.position);
            Debug.Log($"EnemyMovement.Act: immediate attack returned {attacked}");
            if (attacked) return;
            // If attack didn't find a valid target (colliders/components), we stop here.
            return;
        }

        // Not in range -> move towards the target up to mobility cells
        // Prefer grid-aligned movement when a BuildingGrid is available
        BuildingGrid chosenGrid = null;
        if (grids != null && grids.Length > 0)
        {
            // Prefer a grid that contains this unit, otherwise use the first grid
            foreach (var g in grids)
            {
                if (g.ContainsWorldPosition(myPos)) { chosenGrid = g; break; }
            }
            if (chosenGrid == null) chosenGrid = grids[0];
        }

        var moveTransform = unitObject != null ? unitObject.transform : transform;
        if (chosenGrid != null)
        {
            // Convert positions to grid coordinates and move in Manhattan steps
            (int sx, int sy) = chosenGrid.WorldToGridPosition(myPos);
            (int tx, int ty) = chosenGrid.WorldToGridPosition(nearest.position);
            int delta = Mathf.Abs(sx - tx) + Mathf.Abs(sy - ty);
            if (delta <= attackRange) return; // already in range

            int moveCells = Mathf.Min(mobility, Mathf.Max(0, delta - attackRange));
            if (moveCells <= 0) return;

            int nx = sx;
            int ny = sy;
            int remaining = moveCells;
            int dx = tx - sx;
            int stepX = dx > 0 ? 1 : (dx < 0 ? -1 : 0);
            int moveX = Mathf.Min(Mathf.Abs(dx), remaining);
            nx += stepX * moveX;
            remaining -= moveX;

            int dy = ty - sy;
            int stepY = dy > 0 ? 1 : (dy < 0 ? -1 : 0);
            int moveY = Mathf.Min(Mathf.Abs(dy), remaining);
            ny += stepY * moveY;
            remaining -= moveY;

            // Compute world center of target cell
            float cs = chosenGrid.CellSize;
            Vector3 localCenter = new Vector3((nx + 0.5f) * cs, 0f, (ny + 0.5f) * cs);
            Vector3 worldTarget = chosenGrid.transform.TransformPoint(localCenter);
            worldTarget.y = moveTransform.position.y;
            MoveToPosition(worldTarget);
            // After moving, attempt an attack if we will be in range
            if (attackerComp != null && attackActions > 0)
            {
                StartCoroutine(AttemptAttackAfterMove(nearest, attackerComp));
            }
        }
        else
        {
            // Fallback: continuous movement but snap to approx cell centers
            float moveMax = mobility * approxCell;
            Vector3 dir = (nearest.position - myPos);
            float dist = dir.magnitude;
            float desiredDist = Mathf.Max(0f, dist - maxAttackDist);
            float moveDist = Mathf.Min(moveMax, desiredDist);
            if (moveDist <= 0f) return;
            Vector3 moveTarget = myPos + dir.normalized * moveDist;
            // Snap to nearest approx cell center
            float cs = approxCell;
            Vector3 origin = Vector3.zero;
            Vector3 snapped = new Vector3(Mathf.Floor(moveTarget.x / cs) * cs + cs * 0.5f, moveTarget.y, Mathf.Floor(moveTarget.z / cs) * cs + cs * 0.5f);
            MoveToPosition(snapped);
            if (attackerComp != null && attackActions > 0)
            {
                StartCoroutine(AttemptAttackAfterMove(nearest, attackerComp));
            }
        }
    }

    IEnumerator AttemptAttackAfterMove(Transform target, AttackPlayerUnit attacker)
    {
        // wait until movement coroutine finishes
        while (moveCoroutine != null)
            yield return null;

        if (attacker == null)
        {
            Debug.Log("AttemptAttackAfterMove: no attacker available");
            yield break;
        }
        if (attackActions <= 0)
        {
            Debug.Log("AttemptAttackAfterMove: no attackActions left after moving");
            yield break;
        }
        // Recompute distance after movement and only attack if within attackRange
        var moveTransform = unitObject != null ? unitObject.transform : transform;
        Vector3 myPos = moveTransform.position;

        BuildingGrid[] grids = FindObjectsOfType<BuildingGrid>();
        float approxCell = 1f;
        if (grids != null && grids.Length > 0) approxCell = grids[0].CellSize;
        float maxAttackDist = (attackRange + 0.5f) * approxCell;
        float dist = Vector3.Distance(myPos, target.position);
        Debug.Log($"AttemptAttackAfterMove: distAfterMove={dist} maxAttackDist={maxAttackDist}");
        if (dist <= maxAttackDist)
        {
            Debug.Log($"AttemptAttackAfterMove: attempting attack on {target.name} at {target.position}");
            bool attacked = attacker.TryAttackAtPosition(target.position);
            Debug.Log($"AttemptAttackAfterMove: attack returned {attacked}");
        }
        else
        {
            Debug.Log("AttemptAttackAfterMove: target still out of range after moving");
        }
    }

    // Public method to force the enemy to act immediately (callable from editor or other scripts)
    public void ForceAct()
    {
        // Directly invoke Act() ignoring turn-phase and per-turn flags
        Act();
    }

    public void MoveToPosition(Vector3 target)
    {
        var moveTransform = unitObject != null ? unitObject.transform : transform;
        target.y = moveTransform.position.y;
        if (moveActions <= 0)
        {
            Debug.Log("Enemy has no move actions available.");
            return;
        }
        moveActions = Mathf.Max(0, moveActions - 1);

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(target));
    }

    IEnumerator MoveRoutine(Vector3 target)
    {
        float stopSq = 0.001f;
        var moveTransform = unitObject != null ? unitObject.transform : transform;
        while ((moveTransform.position - target).sqrMagnitude > stopSq)
        {
            moveTransform.position = Vector3.MoveTowards(moveTransform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        moveTransform.position = target;
        moveCoroutine = null;
    }

    // Public helper to force this enemy to act immediately (usable from UI button)
    public void ForceActNow()
    {
        Act();
    }
}
