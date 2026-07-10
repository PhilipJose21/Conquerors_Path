using System.Collections.Generic;
using UnityEngine;

public class TerrainHideUnit : MonoBehaviour
{
    private TerrainSOContainer terrainSOContainer;
    private TerrainSO terrainSO;
    public TerrainSO.ResourceType resourceType;
    public TerrainSO.ResourceType secondaryResourceType;

    [Range(0, 1)] public float hiddenAlpha = 0.3f; // Alpha value when the unit is hidden

    // No longer track original alpha; always restore to fully opaque on exit

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

    private static void SetAlpha(Renderer targetRenderer, float alpha)
    {
        if (targetRenderer == null)
        {
            return;
        }

        Material mat = targetRenderer.material;
        Color currentColor = mat.color;
        currentColor.a = alpha;
        mat.color = currentColor;
    }

    private static Renderer GetUnitRenderer(Component unitRoot)
    {
        return unitRoot != null ? unitRoot.GetComponentInChildren<Renderer>() : null;
    }

    private void OnTriggerEnter(Collider other)
    {
        MoveUnit moveUnit = other.GetComponentInParent<MoveUnit>();
        if (moveUnit != null || other.CompareTag("Player") || other.CompareTag("PlayerUnit"))
        {
            Renderer otherRenderer = GetUnitRenderer(moveUnit != null ? moveUnit : other.GetComponentInParent<MoveUnit>());
            SetAlpha(otherRenderer, hiddenAlpha);

            if (moveUnit != null)
            {
                moveUnit.EnterFog(this);
            }
        }

        else
        {
            EnemyMovement enemyMovement = other.GetComponentInParent<EnemyMovement>();
            if (enemyMovement != null || other.CompareTag("Enemy") || other.CompareTag("EnemyUnit"))
            {
                Renderer otherRenderer = GetUnitRenderer(enemyMovement != null ? enemyMovement : other.GetComponentInParent<EnemyMovement>());
                UnitHealth unitHealth = enemyMovement != null ? enemyMovement.GetComponentInChildren<UnitHealth>() : other.GetComponentInChildren<UnitHealth>();
                SetAlpha(otherRenderer, 0f);

                if (enemyMovement != null)
                {
                    enemyMovement.EnterFog(this);
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
        if (moveUnit != null || other.CompareTag("Player") || other.CompareTag("PlayerUnit"))
        {
            if (moveUnit != null)
            {
                moveUnit.EnterFog(this);
            }
        }
        else
        {
            EnemyMovement enemyMovement = other.GetComponentInParent<EnemyMovement>();
            if (enemyMovement != null || other.CompareTag("Enemy") || other.CompareTag("EnemyUnit"))
            {
                if (enemyMovement != null)
                {
                    enemyMovement.EnterFog(this);
                }
                UnitHealth unitHealth = enemyMovement != null ? enemyMovement.GetComponentInChildren<UnitHealth>() : other.GetComponentInChildren<UnitHealth>();
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

        if (moveUnit != null || enemyMovement != null || other.CompareTag("Player") || other.CompareTag("PlayerUnit") || other.CompareTag("Enemy") || other.CompareTag("EnemyUnit"))
        {
            Renderer otherRenderer = GetUnitRenderer(moveUnit != null ? moveUnit : enemyMovement);
            if (moveUnit != null)
            {
                bool stillHidden = moveUnit.ExitFog(this);
                SetAlpha(otherRenderer, stillHidden ? hiddenAlpha : 1f);
            }
            if (enemyMovement != null)
            {
                bool stillHidden = enemyMovement.ExitFog(this);
                SetAlpha(otherRenderer, stillHidden ? 0f : 1f);
                UnitHealth unitHealth = enemyMovement.GetComponentInChildren<UnitHealth>();
                if (unitHealth != null)
                {
                    unitHealth.SetHealthUIHidden(stillHidden);
                }
            }
        }
    }
}

