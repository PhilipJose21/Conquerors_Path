using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    private RotateModel rotateModel;
    private bool hasActedThisTurn = false;
    public bool isMoving;

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

        rotateModel = this.GetComponent<RotateModel>();
        if (rotateModel == null && unitObject != null)
        {
            rotateModel = unitObject.GetComponentInChildren<RotateModel>();
        }
        if (rotateModel == null)
        {
            rotateModel = this.GetComponentInChildren<RotateModel>();
        }
        if (rotateModel == null)
        {
            // Covers cases where the model/rotation script lives higher up the
            // hierarchy (e.g. on a grandparent "unit root" object) rather than
            // on this object, unitObject, or either of their children.
            rotateModel = this.GetComponentInParent<RotateModel>();
        }
        if (rotateModel == null)
        {
            Debug.LogWarning($"EnemyMovement on '{gameObject.name}': No RotateModel found — model won't rotate when moving.");
        }
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

        // Not in range -> move towards the target up to mobility cells.
        // The battlefield can be made of several BuildingGrid tiles stitched together
        // (e.g. "Plain" + "Plain (1)") to form an irregular shape. Pathfinding therefore
        // can't be locked to one grid's local coordinate frame - it needs to resolve, at
        // every step, which tile actually owns that cell and hop between tiles as needed.
        BuildingGrid[] candidateGrids = (grids != null) ? grids.Where(g => g.isBattleGrid).ToArray() : new BuildingGrid[0];
        if (candidateGrids.Length == 0 && grids != null) candidateGrids = grids;

        var moveTransform = unitObject != null ? unitObject.transform : transform;

        BuildingGrid sourceGrid = FindGridContaining(myPos, candidateGrids);
        if (sourceGrid == null) sourceGrid = FindNearestGrid(myPos, candidateGrids);
        BuildingGrid targetGrid = FindGridContaining(nearest.position, candidateGrids);
        if (targetGrid == null) targetGrid = FindNearestGrid(nearest.position, candidateGrids);

        if (sourceGrid != null && targetGrid != null)
        {
            (int sx, int sy) = sourceGrid.WorldToGridPosition(myPos);
            (int tx, int ty) = targetGrid.WorldToGridPosition(nearest.position);
            GridCell sourceCell = new GridCell(sourceGrid, sx, sy);
            GridCell targetCell = new GridCell(targetGrid, tx, ty);

            // Use the unit's full mobility as the BFS search budget rather than capping it
            // to the straight-line distance to the target. Capping by straight-line distance
            // assumes an unobstructed path; when terrain/units block the direct route, that
            // cap starves the BFS of the extra steps it needs to detour around the obstacle,
            // causing the unit to find no better cell than its current one and stand still
            // (which is what produced the pile-up behind blocked tiles).
            int moveCells = mobility;
            if (moveCells <= 0) return;

            // Build a "true" path-distance map by flood-filling outward from the target,
            // respecting blocked terrain AND grid-tile boundaries (a step off the edge of
            // every known tile is treated as impassable - there's no cell there to land on).
            const int maxPathSearchRadius = 30; // safety cap so this can't run away on huge grids
            (int dx, int dy)[] dirs4 = new (int, int)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
            var distFromTarget = new Dictionary<GridCell, int>();
            {
                var pathQueue = new Queue<(GridCell cell, int dist)>();
                pathQueue.Enqueue((targetCell, 0));
                distFromTarget[targetCell] = 0;
                while (pathQueue.Count > 0)
                {
                    var curr = pathQueue.Dequeue();
                    if (curr.dist >= maxPathSearchRadius) continue;
                    foreach (var dir in dirs4)
                    {
                        GridCell? next = GetNeighborCell(curr.cell, dir.dx, dir.dy, candidateGrids);
                        if (next == null) continue; // off every known tile - impassable
                        if (distFromTarget.ContainsKey(next.Value)) continue;
                        if (IsCellBlockedByTerrain(next.Value.grid, next.Value.x, next.Value.y)) continue;
                        distFromTarget[next.Value] = curr.dist + 1;
                        pathQueue.Enqueue((next.Value, curr.dist + 1));
                    }
                }
            }

            int delta = distFromTarget.TryGetValue(sourceCell, out var deltaDist) ? deltaDist : (maxPathSearchRadius * 2);
            if (delta <= attackRange) return; // already in range

            // BFS to find the reachable landing cell (within this unit's mobility) that has
            // the shortest TRUE path distance to the target, using the map built above.
            GridCell best = sourceCell;
            int minDistToTarget = distFromTarget.TryGetValue(sourceCell, out var startDist) ? startDist : delta;

            var queue = new Queue<(GridCell cell, int dist)>();
            var visited = new HashSet<GridCell>();
            queue.Enqueue((sourceCell, 0));
            visited.Add(sourceCell);

            while (queue.Count > 0)
            {
                var curr = queue.Dequeue();
                GridCell cell = curr.cell;
                int cdist = curr.dist;

                bool terrainBlocksCell = IsCellBlockedByTerrain(cell.grid, cell.x, cell.y);
                bool unitBlocksCell = IsCellOccupiedByUnit(cell.grid, cell.x, cell.y);

                // Only consider this cell as a destination option if it can be safely landed on.
                // Units no longer block traversal, but they still block landing.
                if (!terrainBlocksCell && !unitBlocksCell)
                {
                    int distToTarget = distFromTarget.TryGetValue(cell, out var pathDist)
                        ? pathDist
                        // Cell wasn't covered by the flood fill (outside search radius / isolated) -
                        // heavily deprioritize it rather than treating it as attractive.
                        : (maxPathSearchRadius * 2);
                    if (distToTarget < minDistToTarget)
                    {
                        minDistToTarget = distToTarget;
                        best = cell;
                    }
                }

                if (cdist < moveCells)
                {
                    foreach (var dir in dirs4)
                    {
                        GridCell? next = GetNeighborCell(cell, dir.dx, dir.dy, candidateGrids);
                        if (next == null) continue; // off every known tile
                        if (visited.Contains(next.Value)) continue;
                        visited.Add(next.Value);
                        if (!IsCellBlockedByTerrain(next.Value.grid, next.Value.x, next.Value.y))
                        {
                            queue.Enqueue((next.Value, cdist + 1));
                        }
                    }
                }
            }

            // Compute world center of the chosen landing cell using ITS OWN owning grid's
            // transform/cell size - this is what guarantees the final position is always a
            // real cell center on a real tile, never a blended/incorrect position.
            float cs = best.grid.CellSize;
            Vector3 localCenter = new Vector3((best.x + 0.5f) * cs, 0f, (best.y + 0.5f) * cs);
            Vector3 worldTarget = best.grid.transform.TransformPoint(localCenter);
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
            // Fallback: couldn't resolve a grid for the unit's position or the target's
            // position at all (e.g. no BuildingGrid exists in the scene, or the position is
            // outside every known tile). This should be rare,
            // but if it happens, do NOT snap using raw world X/Z - the grid can be rotated
            // (isometric board), so flooring world coordinates directly produces a position
            // that doesn't line up with any real cell (the off-grid diagonal bug). Since
            // there's no grid transform to work in, just move without snapping at all;
            // an unsnapped-but-correct position is safer than a confidently wrong "snapped" one.
            float moveMax = mobility * approxCell;
            Vector3 dir = (nearest.position - myPos);
            float dist = dir.magnitude;
            float desiredDist = Mathf.Max(0f, dist - maxAttackDist);
            float moveDist = Mathf.Min(moveMax, desiredDist);
            if (moveDist <= 0f) return;
            Vector3 moveTarget = myPos + dir.normalized * moveDist;
            MoveToPosition(moveTarget);
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
            // Same battle-grid-only rule as Act() - never let cell size come from an
            // unrelated (e.g. base-building) grid in the scene.
            var battleGrids = grids.Where(g => g.isBattleGrid).ToArray();
            var candidateGrids = battleGrids.Length > 0 ? battleGrids : grids;

            foreach (var g in candidateGrids)
            {
                if (g.ContainsWorldPosition(moveTransform.position))
                {
                    grid = g;
                    break;
                }
            }
            if (grid == null)
            {
                float bestGridDist = float.MaxValue;
                foreach (var g in candidateGrids)
                {
                    float d = Vector3.Distance(g.transform.position, moveTransform.position);
                    if (d < bestGridDist) { bestGridDist = d; grid = g; }
                }
            }
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

        // Turn the model to face the direction it's about to travel in.
        rotateModel?.FaceDirection(target - moveTransform.position);

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

    private readonly struct GridCell : System.IEquatable<GridCell>
    {
        public readonly BuildingGrid grid;
        public readonly int x;
        public readonly int y;
        public GridCell(BuildingGrid grid, int x, int y) { this.grid = grid; this.x = x; this.y = y; }
        public bool Equals(GridCell other) => grid == other.grid && x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is GridCell other && Equals(other);
        public override int GetHashCode()
        {
            int gridHash = grid != null ? grid.GetInstanceID() : 0;
            return gridHash * 397 ^ (x * 31 + y);
        }
    }

    // Finds whichever grid tile actually contains this world position, if any.
    private BuildingGrid FindGridContaining(Vector3 worldPos, BuildingGrid[] grids)
    {
        if (grids == null) return null;
        foreach (var g in grids)
        {
            if (g != null && g.ContainsWorldPosition(worldPos)) return g;
        }
        return null;
    }

    // Used only when a position falls outside every tile's bounds (e.g. right on a seam);
    // picks the closest tile rather than an arbitrary one so coordinates stay sane.
    private BuildingGrid FindNearestGrid(Vector3 worldPos, BuildingGrid[] grids)
    {
        if (grids == null || grids.Length == 0) return null;
        BuildingGrid best = null;
        float bestDist = float.MaxValue;
        foreach (var g in grids)
        {
            if (g == null) continue;
            float d = Vector3.Distance(g.transform.position, worldPos);
            if (d < bestDist) { bestDist = d; best = g; }
        }
        return best;
    }

    private GridCell? GetNeighborCell(GridCell cell, int dx, int dy, BuildingGrid[] grids)
    {
        if (cell.grid == null) return null;
        float cs = cell.grid.CellSize;
        Vector3 localCenter = new Vector3((cell.x + dx + 0.5f) * cs, 0f, (cell.y + dy + 0.5f) * cs);
        Vector3 worldPos = cell.grid.transform.TransformPoint(localCenter);

        BuildingGrid owner = FindGridContaining(worldPos, grids);
        if (owner == null) return null; // off every known tile - impassable

        (int ox, int oy) = owner.WorldToGridPosition(worldPos);
        return new GridCell(owner, ox, oy);
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