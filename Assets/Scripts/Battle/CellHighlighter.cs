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
            Debug.Log("CellHighlighter: clicked same unit again - clearing highlights.");
            ClearHighlights();
            return;
        }

        ClearHighlights();
        if (unit == null) return;

        // Find a BuildingGrid that contains the unit
        BuildingGrid[] grids = FindObjectsOfType<BuildingGrid>();
        BuildingGrid grid = null;
        foreach (var g in grids)
        {
            if (g.ContainsWorldPosition(unit.transform.position))
            {
                grid = g;
                break;
            }
        }

        if (grid == null)
        {
            Debug.LogWarning("CellHighlighter: No BuildingGrid found that contains the unit.");
            return;
        }

        float cellSize = grid.CellSize;
        (int cx, int cy) = grid.WorldToGridPosition(unit.transform.position);
        int maxRange = Mathf.Max(mobility, attackRange);

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
