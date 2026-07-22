using System.Collections.Generic;
using UnityEngine;

public class CellHighlighter : MonoBehaviour
{
    public static CellHighlighter Instance { get; private set; }

    public Material moveMaterial;
    public Material attackMaterial;

    private List<GameObject> tiles = new List<GameObject>();
    private GameObject currentUnit;
    public GameObject CurrentUnit => currentUnit;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (moveMaterial == null)
        {
            moveMaterial = new Material(Shader.Find("Standard"));
            moveMaterial.color = new Color(0f, 0.5f, 1f, 0.45f);
            SetupMaterialTransparent(moveMaterial);
        }
        if (attackMaterial == null)
        {
            attackMaterial = new Material(Shader.Find("Standard"));
            attackMaterial.color = new Color(1f, 0f, 0f, 0.45f);
            SetupMaterialTransparent(attackMaterial);
        }
    }

    void SetupMaterialTransparent(Material m)
    {
        if (m == null) return;
        m.SetFloat("_Mode", 3);
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 3000;
    }

    public void ClearHighlights()
    {
        // Ensure any selected unit is deselected when highlights are cleared
        if (currentUnit != null)
        {
            var mu = currentUnit.GetComponent<MoveUnit>();
            if (mu != null) mu.isSelected = false;
        }

        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            var t = tiles[i];
            if (t != null) Destroy(t);
        }
        tiles.Clear();
        currentUnit = null;
    }

    // Shows highlights for the provided unit GameObject.
    // mobility: Manhattan distance (diamond / cross)
    // attackRange: square radius (all dx,dy where |dx|<=attackRange && |dy|<=attackRange)
    public void ShowHighlightsForUnit(GameObject unit, int mobility, int attackRange)
    {
        // Toggle: if clicking the same unit again, clear and return
        if (unit != null && currentUnit == unit)
        {
            ClearHighlights();
            return;
        }

        ClearHighlights();
        if (unit == null) return;

        // Find BuildingGrids in the scene. Prefer the grid that contains the unit,
        // but if none contains the unit, allow highlighting on any grid where
        // the computed cells fall inside that grid. This supports units placed
        // between multiple separate grids.
        BuildingGrid[] grids = UnityEngine.Object.FindObjectsByType<BuildingGrid>(UnityEngine.FindObjectsSortMode.None);
        if (grids == null || grids.Length == 0)
        {
            Debug.LogWarning("CellHighlighter: No BuildingGrid instances found in scene.");
            return;
        }

        HashSet<TerrainInteraction> clearedFogTerrains = new HashSet<TerrainInteraction>();

        // If a grid contains the unit, include that grid. Also include any other
        // grids that have at least one cell within the unit's mobility/attack range
        // so highlights span adjacent grids. If no grid contains the unit, use all.
        List<BuildingGrid> gridsToUse = new List<BuildingGrid>();
        BuildingGrid containing = null;
        foreach (var g in grids)
        {
            if (g.ContainsWorldPosition(unit.transform.position))
            {
                containing = g;
                break;
            }
        }
        int maxRange = Mathf.Max(mobility, attackRange);
        if (containing == null)
        {
            gridsToUse.AddRange(grids);
        }
        else
        {
            gridsToUse.Add(containing);
            // Include other grids that intersect the set of candidate cells
            foreach (var g in grids)
            {
                if (g == containing) continue;
                (int gx, int gy) = g.WorldToGridPosition(unit.transform.position);
                bool added = false;
                for (int dx = -maxRange; dx <= maxRange && !added; dx++)
                {
                    for (int dy = -maxRange; dy <= maxRange; dy++)
                    {
                        bool inMove = Mathf.Abs(dx) + Mathf.Abs(dy) <= mobility;
                        bool inAttack = Mathf.Abs(dx) <= attackRange && Mathf.Abs(dy) <= attackRange;
                        if (!inMove && !inAttack) continue;
                        int x = gx + dx;
                        int y = gy + dy;
                        Vector3 localCenter = new Vector3((x + 0.5f) * g.CellSize, 0.01f, (y + 0.5f) * g.CellSize);
                        Vector3 worldPos = g.transform.TransformPoint(localCenter);
                        if (g.ContainsWorldPosition(worldPos))
                        {
                            gridsToUse.Add(g);
                            added = true;
                            break;
                        }
                    }
                }
            }
        }

        // reuse maxRange computed above
        // Tracks every world position a tile has already been created at, so that
        // if two grids overlap in world space (see gridsToUse above) we never
        // create a second HighlightTile stacked on the same cell. Without this,
        // a stray tile from a secondary grid — computed relative to THAT grid's
        // coordinates — could land on top of the correct tile with a different
        // isMove/isAttack combo, and whichever one the raycast happens to hit
        // would win, making clicks behave inconsistently with what's shown.
        HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();

        foreach (var grid in gridsToUse)
        {
            float cellSize = grid.CellSize;
            (int cx, int cy) = grid.WorldToGridPosition(unit.transform.position);

            for (int dx = -maxRange; dx <= maxRange; dx++)
            {
                for (int dy = -maxRange; dy <= maxRange; dy++)
                {
                    bool inMove = Mathf.Abs(dx) + Mathf.Abs(dy) <= mobility;
                    bool inAttack = Mathf.Abs(dx) <= attackRange && Mathf.Abs(dy) <= attackRange;
                    if (!inMove && !inAttack) continue;

                    int x = cx + dx;
                    int y = cy + dy;

                    Vector3 localCenter = new Vector3((x + 0.5f) * cellSize, 0.01f, (y + 0.5f) * cellSize);
                    Vector3 worldPos = grid.transform.TransformPoint(localCenter);

                    // skip if outside grid bounds
                    if (!grid.ContainsWorldPosition(worldPos)) continue;

                    // Round to guard against float precision differences between grids
                    // landing "the same" position a hair apart and slipping past the check.
                    Vector3 dedupeKey = new Vector3(
                        Mathf.Round(worldPos.x * 100f) / 100f,
                        Mathf.Round(worldPos.y * 100f) / 100f,
                        Mathf.Round(worldPos.z * 100f) / 100f);
                    if (!occupiedPositions.Add(dedupeKey)) continue; // already have a tile here

                    bool overlap = inMove && inAttack;

                    GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = "HighlightTile";
                    // Parent the highlight tile to the grid so it follows grid position/rotation
                    tile.transform.SetParent(grid.transform, false);
                    tile.transform.localPosition = localCenter;
                    tile.transform.localRotation = Quaternion.identity;
                    tile.transform.localScale = new Vector3(cellSize, 0.02f, cellSize);

                    var mr = tile.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        if (overlap)
                        {
                            // Both movable AND attackable — hide the plain full-cell
                            // renderer and show a diagonal-split visual instead (below)
                            // so it clearly reads as "both", not just attack.
                            mr.enabled = false;
                        }
                        else
                        {
                            mr.sharedMaterial = inAttack ? attackMaterial : moveMaterial;
                            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                            mr.receiveShadows = false;
                        }
                    }

                    // Add a trigger collider so clicks can be detected on highlight tiles.
                    // Stays full-cell-sized even for overlap tiles so the whole cell is
                    // clickable no matter which half is visually showing.
                    var col = tile.GetComponent<BoxCollider>();
                    if (col == null) col = tile.AddComponent<BoxCollider>();
                    col.isTrigger = true;

                    // Attach tile metadata for click handling
                    var ht = tile.AddComponent<HighlightTile>();
                    // worldPosition is now a live property (tile.transform.position),
                    // so it always stays correct even if the environment rotates later.
                    ht.isMove = inMove;
                    ht.isAttack = inAttack;

                    if (overlap)
                    {
                        CreateSplitVisual(grid.transform, localCenter, cellSize);
                    }

                    TryClearFogTerrainAt(worldPos, cellSize * 0.35f, clearedFogTerrains);

                    tiles.Add(tile);
                }
            }
        }
        currentUnit = unit;
        // NOTE: do not auto-attack when highlights are shown — require the player to
        // select their unit and click the highlighted tile to perform an attack.
    }

    // Move the currently selected unit (if any) to the given world position.
    public bool MoveCurrentUnitTo(Vector3 worldPos)
    {
        if (currentUnit == null) return false;

        // Face the model toward the actual clicked cell right away, independent of
        // how MoveToPosition ends up pathing around obstacles. Checks the unit's own
        // object first, then its children, since the visual model with RotateModel
        // often lives on a separate child object from the unit's logic root.
        var rotateModel = currentUnit.GetComponent<RotateModel>();
        if (rotateModel == null)
        {
            rotateModel = currentUnit.GetComponentInChildren<RotateModel>();
        }
        if (rotateModel == null)
        {
            // Covers cases where the model/rotation script lives higher up the
            // hierarchy (e.g. on a grandparent "unit root" object) rather than
            // on the unit's logic object or its children.
            rotateModel = currentUnit.GetComponentInParent<RotateModel>();
        }
        if (rotateModel != null)
        {
            rotateModel.FacePosition(worldPos);
        }
        else
        {
            Debug.LogWarning($"CellHighlighter: No RotateModel found on '{currentUnit.name}' or its children — model won't rotate to face the clicked cell.");
        }

        var mu = currentUnit.GetComponent<MoveUnit>();
        if (mu != null)
        {
            mu.MoveToPosition(worldPos);
            return true;
        }
        // Fallback: teleport
        Vector3 tp = worldPos;
        tp.y = currentUnit.transform.position.y;
        currentUnit.transform.position = tp;
        return true;
    }

    // Creates two purely-visual triangular halves (no colliders) so a cell that
    // is both within movement range AND attack range clearly reads as "both" —
    // one diagonal half tinted with moveMaterial, the other with attackMaterial.
    // Split runs along the bottom-left -> top-right diagonal of the cell (in the
    // grid's local X/Z space), so it reads correctly from angled/isometric cameras.
    private void CreateSplitVisual(Transform gridTransform, Vector3 localCenter, float cellSize)
    {
        GameObject moveTri = CreateDiagonalHalf(gridTransform, localCenter, cellSize, moveMaterial, true, "HighlightTile_MoveHalf");
        GameObject attackTri = CreateDiagonalHalf(gridTransform, localCenter, cellSize, attackMaterial, false, "HighlightTile_AttackHalf");

        tiles.Add(moveTri);
        tiles.Add(attackTri);
    }

    // upperLeft = true builds the triangle on the bottom-left/top-left/top-right side
    // of the diagonal; upperLeft = false builds the bottom-left/bottom-right/top-right side.
    // Together the two triangles exactly cover the cell.
    private GameObject CreateDiagonalHalf(Transform gridTransform, Vector3 localCenter, float cellSize, Material material, bool upperLeft, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(gridTransform, false);
        go.transform.localPosition = localCenter;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        float half = cellSize * 0.5f;
        const float y = 0.015f; // sits just above the (hidden) base tile to avoid z-fighting

        Vector3 topLeft = new Vector3(-half, y, half);
        Vector3 bottomLeft = new Vector3(-half, y, -half);
        Vector3 bottomRight = new Vector3(half, y, -half);
        Vector3 topRight = new Vector3(half, y, half);

        Vector3[] vertices = upperLeft
            ? new[] { topLeft, bottomLeft, topRight }
            : new[] { bottomLeft, bottomRight, topRight };

        // Two triangles sharing the same 3 vertices with opposite winding, so the
        // flat plane renders no matter which side the camera looks from.
        int[] triangles = { 0, 1, 2, 0, 2, 1 };
        Vector3[] normals = { Vector3.up, Vector3.up, Vector3.up };
        Vector2[] uvs = new Vector2[3];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / cellSize + 0.5f, vertices[i].z / cellSize + 0.5f);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.RecalculateBounds();

        var mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = material;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        return go;
    }

    private void TryClearFogTerrainAt(Vector3 worldPos, float radius, HashSet<TerrainInteraction> clearedFogTerrains)
    {
        Collider[] hits = Physics.OverlapSphere(worldPos, radius);
        foreach (var hit in hits)
        {
            TerrainInteraction terrainInteraction = hit.GetComponentInParent<TerrainInteraction>();
            if (terrainInteraction == null || !terrainInteraction.IsFogTerrain())
            {
                continue;
            }

            if (clearedFogTerrains.Contains(terrainInteraction))
            {
                continue;
            }


            clearedFogTerrains.Add(terrainInteraction);
            Destroy(terrainInteraction.gameObject);
        }
    }
}