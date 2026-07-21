using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public bool isHidden;

    private readonly HashSet<TerrainHideUnit> activeFogTerrains = new HashSet<TerrainHideUnit>();

    public GameObject unitObject;
    private bool hasActedThisTurn = false;
    private bool isMoving;

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

    private bool IsPlayerHidden(GameObject p)
    {
        if (p == null) return false;
        // Check for MoveUnit on children first, then parents for robustness
        var mv = p.GetComponentInChildren<MoveUnit>();
        if (mv == null) mv = p.GetComponentInParent<MoveUnit>();
        bool hidden = mv != null && mv.isHidden;
        
        return hidden;
    }

    public void EnterFog(TerrainHideUnit terrainHideUnit)
    {
        if (terrainHideUnit == null)
        {
            return;
        }

        activeFogTerrains.Add(terrainHideUnit);
        isHidden = activeFogTerrains.Count > 0;
    }

    public bool ExitFog(TerrainHideUnit terrainHideUnit)
    {
        if (terrainHideUnit == null)
        {
            return isHidden;
        }

        activeFogTerrains.Remove(terrainHideUnit);
        isHidden = activeFogTerrains.Count > 0;
        return isHidden;
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
        }
    }

    void LateUpdate()
    {
        if (stateMachine == null) return;

        if (!isMoving)
        {
            // Don't stomp a one-shot Hurt/Damage reaction that's currently playing —
            // it will revert to Idle on its own once the clip finishes.
            if (stateMachine.currentUnitPhase != unitPhase.Hurt &&
                stateMachine.currentUnitPhase != unitPhase.Damage)
            {
                stateMachine.ChangeState(unitPhase.Idle);
            }
        }
        else
        {
            stateMachine.ChangeState(unitPhase.Move);
        }
    }

    public void Act()
    {
        // Find nearest player unit (prefer unhidden players; if none unhidden, allow hidden)
        GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerUnit");
        if (players == null || players.Length == 0)
        {
            endTurn = true;
            return;
        }

        // Determine if any unhidden players exist
        bool anyUnhidden = false;
        foreach (var p in players) { if (!IsPlayerHidden(p)) { anyUnhidden = true; break; } }

        

        Transform nearest = null;
        float bestDist = float.MaxValue;
        Vector3 myPos = unitObject != null ? unitObject.transform.position : transform.position;
        foreach (var p in players)
        {
            if (anyUnhidden && IsPlayerHidden(p))
            {
                
                continue; // skip hidden when unhidden exist
            }
            float d = Vector3.Distance(myPos, p.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                nearest = p.transform;
            }
        }
        if (nearest == null) return;

        // Compute approximate cell size
        BuildingGrid[] grids = UnityEngine.Object.FindObjectsByType<BuildingGrid>(UnityEngine.FindObjectsSortMode.None);
        float approxCell = 1f;
        if (grids != null && grids.Length > 0) approxCell = grids[0].CellSize;
        // Check if any player is already within attack range from current position.
        float maxAttackDist = (attackRange + 0.5f) * approxCell;
        // Find any players within attack distance (could be multiple)
        Transform inRangeTarget = null;
        float inRangeBest = float.MaxValue;
        foreach (var p in players)
        {
            if (anyUnhidden && IsPlayerHidden(p))
            {
                
                continue; // skip hidden when unhidden exist
            }
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
            // There is at least one player in range -> attempt to attack. If the chosen
            // inRangeTarget yields no valid attack (e.g., filtered out by AttackPlayerUnit),
            // try other in-range players before giving up.
            if (attackerComp == null)
            {
                return;
            }
            if (attackActions <= 0)
            {
                return;
            }

            // First try the primary target
            bool attacked = attackerComp.TryAttackAtPosition(inRangeTarget.position);
            if (attacked) return;

            // If the primary attempt failed, try other in-range players (respect hidden filtering)
            foreach (var p in players)
            {
                if (anyUnhidden && IsPlayerHidden(p)) continue; // respect hidden filtering
                if (p.transform == inRangeTarget) continue;
                float d = Vector3.Distance(myPos, p.transform.position);
                if (d <= maxAttackDist)
                {
                    
                    attacked = attackerComp.TryAttackAtPosition(p.transform.position);
                    if (attacked) return;
                }
            }

            // No valid attack found among in-range players
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

            // Use the unit's full mobility as the BFS search budget rather than capping it
            // to the straight-line distance to the target. Capping by straight-line distance
            // assumes an unobstructed path; when terrain/units block the direct route, that
            // cap starves the BFS of the extra steps it needs to detour around the obstacle,
            // causing the unit to find no better cell than its current one and stand still
            // (which is what produced the pile-up behind blocked tiles).
            int moveCells = mobility;
            if (moveCells <= 0) return;

            int nx = sx;
            int ny = sy;

            float cs = chosenGrid.CellSize;

            // Build a "true" path-distance map by flood-filling outward from the target,
            // respecting blocked terrain. Straight-line (Manhattan) distance can't see
            // obstacles, so a cell that is technically further "as the crow flies" can
            // still be the correct next step if it's on the only route around a blocked
            // patch of tiles. This gives every reachable cell its real walking distance
            // to the target instead of a straight-line guess.
            const int maxPathSearchRadius = 30; // safety cap so this can't run away on huge grids
            var distFromTarget = new Dictionary<(int x, int y), int>();
            {
                var pathQueue = new Queue<(int x, int y, int dist)>();
                pathQueue.Enqueue((tx, ty, 0));
                distFromTarget[(tx, ty)] = 0;
                (int dx, int dy)[] dirs4 = new (int, int)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
                while (pathQueue.Count > 0)
                {
                    var curr = pathQueue.Dequeue();
                    if (curr.dist >= maxPathSearchRadius) continue;
                    foreach (var dir in dirs4)
                    {
                        var next = (curr.x + dir.dx, curr.y + dir.dy);
                        if (distFromTarget.ContainsKey(next)) continue;
                        if (IsCellBlockedByTerrain(chosenGrid, next.Item1, next.Item2)) continue;
                        distFromTarget[next] = curr.dist + 1;
                        pathQueue.Enqueue((next.Item1, next.Item2, curr.dist + 1));
                    }
                }
            }

            // BFS to find the reachable landing cell (within this unit's mobility) that has
            // the shortest TRUE path distance to the target, using the map built above.
            int bestX = sx;
            int bestY = sy;
            int minDistToTarget = distFromTarget.TryGetValue((sx, sy), out var startDist) ? startDist : delta;

            Queue<(int x, int y, int dist)> queue = new Queue<(int x, int y, int dist)>();
            HashSet<(int x, int y)> visited = new HashSet<(int x, int y)>();

            queue.Enqueue((sx, sy, 0));
            visited.Add((sx, sy));

            while (queue.Count > 0)
            {
                var curr = queue.Dequeue();
                int cx = curr.x;
                int cy = curr.y;
                int cdist = curr.dist;

                bool terrainBlocksCell = IsCellBlockedByTerrain(chosenGrid, cx, cy);
                bool unitBlocksCell = IsCellOccupiedByUnit(chosenGrid, cx, cy);

                // Only consider this cell as a destination option if it can be safely landed on.
                // Units no longer block traversal, but they still block landing.
                if (!terrainBlocksCell && !unitBlocksCell)
                {
                    int distToTarget = distFromTarget.TryGetValue((cx, cy), out var pathDist)
                        ? pathDist
                        // Cell wasn't covered by the flood fill (outside search radius / isolated) -
                        // heavily deprioritize it rather than treating it as attractive.
                        : (Mathf.Abs(cx - tx) + Mathf.Abs(cy - ty)) + maxPathSearchRadius;
                    if (distToTarget < minDistToTarget)
                    {
                        minDistToTarget = distToTarget;
                        bestX = cx;
                        bestY = cy;
                    }
                }

                if (cdist < moveCells)
                {
                    (int dx, int dy)[] dirs = new (int, int)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
                    foreach (var dir in dirs)
                    {
                        int nxtX = cx + dir.dx;
                        int nxtY = cy + dir.dy;
                        if (!visited.Contains((nxtX, nxtY)))
                        {
                            visited.Add((nxtX, nxtY));
                            if (!IsCellBlockedByTerrain(chosenGrid, nxtX, nxtY))
                            {
                                queue.Enqueue((nxtX, nxtY, cdist + 1));
                            }
                        }
                    }
                }
            }
            nx = bestX;
            ny = bestY;

            // Compute world center of target cell
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
            yield break;
        }
        if (attackActions <= 0)
        {
            yield break;
        }
        // Recompute distance after movement and only attack if within attackRange
        var moveTransform = unitObject != null ? unitObject.transform : transform;
        Vector3 myPos = moveTransform.position;

        BuildingGrid[] grids = UnityEngine.Object.FindObjectsByType<BuildingGrid>(UnityEngine.FindObjectsSortMode.None);
        float approxCell = 1f;
        if (grids != null && grids.Length > 0) approxCell = grids[0].CellSize;
        float maxAttackDist = (attackRange + 0.5f) * approxCell;
        float dist = Vector3.Distance(myPos, target.position);
        if (dist <= maxAttackDist)
        {
            bool attacked = attacker.TryAttackAtPosition(target.position);
        }
        else
        {
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

        BuildingGrid[] grids = UnityEngine.Object.FindObjectsByType<BuildingGrid>(UnityEngine.FindObjectsSortMode.None);
        BuildingGrid grid = null;
        if (grids != null && grids.Length > 0)
        {
            foreach (var g in grids)
            {
                if (g.ContainsWorldPosition(moveTransform.position))
                {
                    grid = g;
                    break;
                }
            }
            if (grid == null) grid = grids[0];
        }

        // Validate landing spot safety
        float cs = grid != null ? grid.CellSize : 1f;
        Collider[] terrainCols = Physics.OverlapSphere(target, cs * 0.35f);
        foreach (var c in terrainCols)
        {
            var ti = c.GetComponentInParent<TerrainInteraction>();
            if (ti != null && ti.cannotMoveOn)
            {
                return;
            }
        }

        // Calculate if the enemy is already sitting at the target center to prevent wasting actions
        float stopCheckRadius = 0.1f;
        if (grids != null && grids.Length > 0) stopCheckRadius = grids[0].CellSize * 0.4f;

        if (Vector3.Distance(moveTransform.position, target) <= stopCheckRadius)
        {
            return;
        }

        if (moveActions <= 0)
        {
            return;
        }

        // Spend the action point only if actual displacement happens
        moveActions = Mathf.Max(0, moveActions - 1);

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(target));
        isMoving = true;
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
        isMoving = false;
    }

    // Public helper to force this enemy to act immediately (usable from UI button)
    public void ForceActNow()
    {
        Act();
    }

    private bool IsCellBlockedByTerrain(BuildingGrid grid, int gx, int gy)
    {
        if (grid == null) return false;

        // Check against terrain that should block traversal.
        float cs = grid.CellSize;
        Vector3 localCenter = new Vector3((gx + 0.5f) * cs, 0.01f, (gy + 0.5f) * cs);
        Vector3 worldCenter = grid.transform.TransformPoint(localCenter);
        Collider[] terrainCols = Physics.OverlapSphere(worldCenter, cs * 0.35f);
        foreach (var c in terrainCols)
        {
            var ti = c.GetComponentInParent<TerrainInteraction>();
            if (ti != null && (ti.cannotMoveOn || ti.CantWalkThrough()))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsCellOccupiedByUnit(BuildingGrid grid, int gx, int gy)
    {
        if (grid == null) return false;

        float cs = grid.CellSize;
        Vector3 localCenter = new Vector3((gx + 0.5f) * cs, 0.01f, (gy + 0.5f) * cs);
        Vector3 worldCenter = grid.transform.TransformPoint(localCenter);
        Collider[] terrainCols = Physics.OverlapSphere(worldCenter, cs * 0.35f);
        foreach (var c in terrainCols)
        {
            var enemy = c.GetComponentInParent<EnemyMovement>();
            if (enemy != null && enemy != this)
            {
                return true;
            }

            var player = c.GetComponentInParent<MoveUnit>();
            if (player != null)
            {
                return true;
            }
        }

        return false;
    }
}