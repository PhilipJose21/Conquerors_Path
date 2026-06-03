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
                        mr.sharedMaterial = inAttack ? attackMaterial : moveMaterial;
                        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        mr.receiveShadows = false;
                    }

                    // Add a trigger collider so clicks can be detected on highlight tiles
                    var col = tile.GetComponent<BoxCollider>();
                    if (col == null) col = tile.AddComponent<BoxCollider>();
                    col.isTrigger = true;

                    // Attach tile metadata for click handling
                    var ht = tile.AddComponent<HighlightTile>();
                    // store the actual world position (after parenting) for click handling
                    ht.worldPosition = tile.transform.position;
                    ht.isMove = inMove;
                    ht.isAttack = inAttack;

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
}
