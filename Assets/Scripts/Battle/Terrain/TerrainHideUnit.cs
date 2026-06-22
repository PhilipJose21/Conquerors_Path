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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Renderer otherRenderer = other.GetComponentInChildren<Renderer>();
            MoveUnit moveUnit = other.GetComponent<MoveUnit>();
            if (otherRenderer != null)
            {
                Material mat = otherRenderer.material; // creates instance if needed
                Color currentColor = mat.color;
                currentColor.a = hiddenAlpha; // Set the alpha to the hidden value
                mat.color = currentColor;
            }
            else
            {
                Debug.Log("No Renderer found on the entered unit to hide.");
            }

            if (moveUnit != null)
            {
                moveUnit.isHidden = true;
            }
        }

        else if (other.CompareTag("Enemy"))
        {
            Renderer otherRenderer = other.GetComponentInChildren<Renderer>();
            
            EnemyMovement enemyMovement = other.GetComponent<EnemyMovement>();
            if (otherRenderer != null)
            {
                Material mat = otherRenderer.material; // creates instance if needed
                Color currentColor = mat.color;
                currentColor.a = 0; // Set the alpha to the hidden value
                mat.color = currentColor;
            }
            
            else
            {
                Debug.Log("No Renderer found on the entered unit to hide.");
            }

            if (enemyMovement != null)
            {
                enemyMovement.isHidden = true;
            }
            Debug.Log("Enemy entered the terrain and is now hidden.");
        }
   }

   private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            Renderer otherRenderer = other.GetComponentInChildren<Renderer>();

            MoveUnit moveUnit = other.GetComponent<MoveUnit>();
            EnemyMovement enemyMovement = other.GetComponent<EnemyMovement>();

            if (otherRenderer != null)
            {
                // Always restore to fully opaque
                Material mat = otherRenderer.material;
                Color currentColor = mat.color;
                currentColor.a = 1f;
                mat.color = currentColor;
            }
            else
            {
                Debug.LogWarning("No Renderer found on the unit to restore visibility.");
            }

            if (moveUnit != null)
            {
                moveUnit.isHidden = false;
            }
            if (enemyMovement != null)
            {
                enemyMovement.isHidden = false;
            }
        }
    }
}

