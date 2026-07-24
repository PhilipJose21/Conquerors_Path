using System.Collections.Generic;
using UnityEngine;

// Attach this to the terrain tile's GameObject (the one with the Renderer and
// the trigger Collider — same object TerrainHideUnit/TerrainDamage live on).
// While one or more units are standing inside this terrain, its own material
// AND every child renderer's material (e.g. mountain/pyramid meshes nested
// underneath) are faded down so the unit underneath stays visible. When the
// last unit leaves, everything returns to its original opacity.
//
// Materials are automatically switched into URP's Transparent/Alpha blend
// mode at Awake (see SetTransparentBlendMode) — you do NOT need to manually
// set each material's Surface Type in the Inspector.
[RequireComponent(typeof(Collider))]
public class TerrainTransparent : MonoBehaviour
{
    [Range(0, 1)] public float occupiedAlpha = 0.35f; // Alpha while a unit is standing inside

    private Renderer[] terrainRenderers;
    // Each material's own starting alpha, since child meshes may not all
    // start at the same opacity.
    private readonly Dictionary<Material, float> originalAlphas = new Dictionary<Material, float>();
    private int unitCount = 0;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    void Awake()
    {
        // includeInactive:true so a child that starts disabled still gets
        // restored correctly if something else enables it later.
        terrainRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in terrainRenderers)
        {
            foreach (Material mat in r.materials)
            {
                if (!originalAlphas.ContainsKey(mat))
                {
                    originalAlphas[mat] = GetAlpha(mat);
                    // Deliberately NOT switching blend mode here. Forcing
                    // every tile's material into Transparent mode up front
                    // (even at alpha=1) causes self-overlapping meshes like
                    // these pyramids to lose correct depth sorting (ZWrite
                    // off), producing a dithered/see-through look on tiles
                    // nothing is even standing on. Blend mode is only
                    // switched when a unit actually occupies the tile, and
                    // switched back when they leave — see UpdateAlpha.
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsUnit(other)) return;
        unitCount++;
        UpdateAlpha();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsUnit(other)) return;
        unitCount = Mathf.Max(0, unitCount - 1);
        UpdateAlpha();
    }

    private void OnDisable()
    {
        unitCount = 0;
        RestoreOriginalAlphas();
    }

    // Same detection style as TerrainHideUnit: prefer the actual movement
    // component, but fall back to walking up parent tags in case a child
    // collider (joint/sub-mesh) isn't itself tagged.
    private static bool IsUnit(Collider other)
    {
        if (other.GetComponentInParent<MoveUnit>() != null) return true;
        if (other.GetComponentInParent<EnemyMovement>() != null) return true;

        Transform t = other.transform;
        while (t != null)
        {
            if (t.CompareTag("Player") || t.CompareTag("PlayerUnit") ||
                t.CompareTag("Enemy") || t.CompareTag("EnemyUnit"))
            {
                return true;
            }
            t = t.parent;
        }
        return false;
    }

    private void UpdateAlpha()
    {
        if (unitCount > 0)
        {
            SetAllAlpha(occupiedAlpha);
        }
        else
        {
            RestoreOriginalAlphas();
        }
    }

    private void SetAllAlpha(float alpha)
    {
        foreach (Renderer r in terrainRenderers)
        {
            if (r == null) continue;
            foreach (Material mat in r.materials)
            {
                SetTransparentBlendMode(mat);
                SetAlpha(mat, alpha);
            }
        }
    }

    private void RestoreOriginalAlphas()
    {
        foreach (Renderer r in terrainRenderers)
        {
            if (r == null) continue;
            foreach (Material mat in r.materials)
            {
                float original = originalAlphas.TryGetValue(mat, out var a) ? a : 1f;
                SetAlpha(mat, original);
                SetOpaqueBlendMode(mat);
            }
        }
    }

    private static void SetAlpha(Material mat, float alpha)
    {
        if (mat.HasProperty(BaseColorId))
        {
            Color c = mat.GetColor(BaseColorId);
            c.a = alpha;
            mat.SetColor(BaseColorId, c);
        }
        else if (mat.HasProperty(ColorId))
        {
            Color c = mat.GetColor(ColorId);
            c.a = alpha;
            mat.SetColor(ColorId, c);
        }
    }

    private static float GetAlpha(Material mat)
    {
        if (mat.HasProperty(BaseColorId)) return mat.GetColor(BaseColorId).a;
        if (mat.HasProperty(ColorId)) return mat.GetColor(ColorId).a;
        return 1f;
    }

    // Replicates what URP's Lit/SimpleLit shader Inspector does when you set
    // Surface Type: Transparent + Blending Mode: Alpha by hand — sets the
    // blend factors, disables ZWrite, moves it into the transparent render
    // queue, and flips the matching shader keyword. Safe to call on a
    // material that's already Transparent; it just re-applies the same
    // values. Materials that don't use URP's _Surface property (a custom
    // shader graph, for example) are left alone.
    private static void SetTransparentBlendMode(Material mat)
    {
        if (!mat.HasProperty("_Surface"))
        {
            return;
        }

        mat.SetFloat("_Surface", 1f); // 0 = Opaque, 1 = Transparent
        mat.SetFloat("_Blend", 0f);   // 0 = Alpha blend mode

        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.SetOverrideTag("RenderType", "Transparent");

        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    // Reverses SetTransparentBlendMode — puts the material back into normal
    // Opaque mode when no unit is standing on the tile, so unoccupied terrain
    // renders with correct depth sorting again instead of staying dithered.
    private static void SetOpaqueBlendMode(Material mat)
    {
        if (!mat.HasProperty("_Surface"))
        {
            return;
        }

        mat.SetFloat("_Surface", 0f); // 0 = Opaque

        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetFloat("_ZWrite", 1f);
        mat.SetOverrideTag("RenderType", "Opaque");

        mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
    }
}