using System.Collections.Generic;
using UnityEngine;

public class TerrainHideUnit : MonoBehaviour
{
    private TerrainSOContainer terrainSOContainer;
    private TerrainSO terrainSO;
    public TerrainSO.ResourceType resourceType;
    public TerrainSO.ResourceType secondaryResourceType;

    [Range(0, 1)] public float hiddenAlpha = 0.3f; // Alpha value when the unit is hidden

    private readonly HashSet<MoveUnit> containedPlayerUnits = new HashSet<MoveUnit>();
    private readonly HashSet<EnemyMovement> containedEnemyUnits = new HashSet<EnemyMovement>();

    void Awake()
    {
        terrainSOContainer = this.GetComponent<TerrainSOContainer>();
        terrainSO = terrainSOContainer != null ? terrainSOContainer.terrainData : null;
        if (terrainSO != null)
        {
            resourceType = terrainSO.resourceType;
            secondaryResourceType = terrainSO.secondaryResourceType;
        }
    }

    // Walks up the transform hierarchy checking for a tag, since child
    // colliders (joints, sub-meshes, etc.) often aren't tagged themselves.
    private static bool HasTagInParents(Transform t, string tag)
    {
        while (t != null)
        {
            if (t.CompareTag(tag))
            {
                return true;
            }
            t = t.parent;
        }
        return false;
    }

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private static void SetAlpha(Renderer targetRenderer, float alpha)
    {
        if (targetRenderer == null)
        {
            return;
        }

        Material mat = targetRenderer.material;

        // URP Lit/Unlit shaders expose "_BaseColor" instead of the legacy
        // "_Color" property. mat.color only works with "_Color", so on URP
        // materials it silently no-ops. Set whichever property the shader
        // actually has.
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

    // Sets alpha on every renderer found under the unit, not just the first one,
    // so multi-part models (separate joints/meshes) hide fully.
    private static void SetAlphaAll(Component unitRoot, float alpha)
    {
        if (unitRoot == null)
        {
            return;
        }

        Renderer[] renderers = unitRoot.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            SetAlpha(r, alpha);
        }
    }

    // Finds the object carrying the Animator (the actual visible model) and
    // enables/disables it. The model is often a SIBLING of unitRoot (both
    // live under a shared "Wrapper" parent) rather than a child of it, so we
    // search from one level up. includeInactive:true is required so we can
    // find it again to re-enable after it's been disabled.
    private static void SetEnemyModelActive(Component unitRoot, bool active)
    {
        if (unitRoot == null)
        {
            return;
        }

        Transform searchRoot = unitRoot.transform.parent != null ? unitRoot.transform.parent : unitRoot.transform;
        Animator animator = searchRoot.GetComponentInChildren<Animator>(true);
        if (animator != null)
        {
            animator.gameObject.SetActive(active);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[TerrainHideUnit] OnTriggerEnter by '{other.gameObject.name}' (tag: {other.tag})");

        MoveUnit moveUnit = other.GetComponentInParent<MoveUnit>();
        if (moveUnit != null || HasTagInParents(other.transform, "Player") || HasTagInParents(other.transform, "PlayerUnit"))
        {
            Component unitRoot = moveUnit != null ? (Component)moveUnit : other.transform;
            SetAlphaAll(unitRoot, hiddenAlpha);

            if (moveUnit != null)
            {
                moveUnit.EnterFog(this);
                containedPlayerUnits.Add(moveUnit);
            }
        }

        else
        {
            EnemyMovement enemyMovement = other.GetComponentInParent<EnemyMovement>();
            Debug.Log($"[TerrainHideUnit] enemyMovement found: {enemyMovement != null}, HasTagInParents Enemy: {HasTagInParents(other.transform, "Enemy")}, EnemyUnit: {HasTagInParents(other.transform, "EnemyUnit")}");
            if (enemyMovement != null || HasTagInParents(other.transform, "Enemy") || HasTagInParents(other.transform, "EnemyUnit"))
            {
                Component unitRoot = enemyMovement != null ? (Component)enemyMovement : other.transform;
                UnitHealth unitHealth = enemyMovement != null ? enemyMovement.GetComponentInChildren<UnitHealth>() : other.GetComponentInParent<UnitHealth>();

                Transform searchRoot = unitRoot.transform.parent != null ? unitRoot.transform.parent : unitRoot.transform;
                Animator foundAnimator = searchRoot.GetComponentInChildren<Animator>(true);
                Debug.Log($"[TerrainHideUnit] unitRoot: '{unitRoot.gameObject.name}', searchRoot: '{searchRoot.name}', Animator found: {(foundAnimator != null ? foundAnimator.gameObject.name : "NULL")}");
                SetEnemyModelActive(unitRoot, false);

                if (enemyMovement != null)
                {
                    enemyMovement.EnterFog(this);
                    containedEnemyUnits.Add(enemyMovement);
                }
                if (unitHealth != null)
                {
                    unitHealth.SetHealthUIHidden(true);
                }
                Debug.Log("Enemy entered the terrain and is now hidden.");
            }
        }
   }

   private void OnTriggerStay(Collider other)
    {
        MoveUnit moveUnit = other.GetComponentInParent<MoveUnit>();
        if (moveUnit != null || HasTagInParents(other.transform, "Player") || HasTagInParents(other.transform, "PlayerUnit"))
        {
            if (moveUnit != null)
            {
                moveUnit.EnterFog(this);
            }
        }
        else
        {
            EnemyMovement enemyMovement = other.GetComponentInParent<EnemyMovement>();
            if (enemyMovement != null || HasTagInParents(other.transform, "Enemy") || HasTagInParents(other.transform, "EnemyUnit"))
            {
                if (enemyMovement != null)
                {
                    enemyMovement.EnterFog(this);
                }
                UnitHealth unitHealth = enemyMovement != null ? enemyMovement.GetComponentInChildren<UnitHealth>() : other.GetComponentInParent<UnitHealth>();
                if (unitHealth != null)
                {
                    unitHealth.SetHealthUIHidden(true);
                }
            }
        }
    }

   private void OnTriggerExit(Collider other)
    {
        MoveUnit moveUnit = other.GetComponentInParent<MoveUnit>();
        EnemyMovement enemyMovement = other.GetComponentInParent<EnemyMovement>();

        if (moveUnit != null || enemyMovement != null
            || HasTagInParents(other.transform, "Player") || HasTagInParents(other.transform, "PlayerUnit")
            || HasTagInParents(other.transform, "Enemy") || HasTagInParents(other.transform, "EnemyUnit"))
        {
            Component unitRoot = moveUnit != null ? (Component)moveUnit
                : enemyMovement != null ? (Component)enemyMovement
                : other.transform;

            if (moveUnit != null)
            {
                containedPlayerUnits.Remove(moveUnit);
                bool stillHidden = moveUnit.ExitFog(this);
                SetAlphaAll(unitRoot, stillHidden ? hiddenAlpha : 1f);
            }
            if (enemyMovement != null)
            {
                containedEnemyUnits.Remove(enemyMovement);
                bool stillHidden = enemyMovement.ExitFog(this);
                SetEnemyModelActive(unitRoot, !stillHidden);
                UnitHealth unitHealth = enemyMovement.GetComponentInChildren<UnitHealth>();
                if (unitHealth != null)
                {
                    unitHealth.SetHealthUIHidden(stillHidden);
                }
            }
        }
    }

    private void OnDisable()
    {
        Debug.Log($"[TerrainHideUnit] OnDisable called on '{gameObject.name}' at time {Time.time:F2} — revealing {containedEnemyUnits.Count} enemy unit(s)");

        foreach (MoveUnit moveUnit in containedPlayerUnits)
        {
            if (moveUnit == null) continue;
            bool stillHidden = moveUnit.ExitFog(this);
            SetAlphaAll(moveUnit, stillHidden ? hiddenAlpha : 1f);
        }
        containedPlayerUnits.Clear();

        foreach (EnemyMovement enemyMovement in containedEnemyUnits)
        {
            if (enemyMovement == null) continue;
            bool stillHidden = enemyMovement.ExitFog(this);
            SetEnemyModelActive(enemyMovement, !stillHidden);
            UnitHealth unitHealth = enemyMovement.GetComponentInChildren<UnitHealth>();
            if (unitHealth != null)
            {
                unitHealth.SetHealthUIHidden(stillHidden);
            }
        }
        containedEnemyUnits.Clear();
    }
}