using System.Collections.Generic;
using UnityEngine;

public class TerrainHideUnit : MonoBehaviour
{
    private TerrainSOContainer terrainSOContainer;
    private TerrainSO terrainSO;
    public TerrainSO.ResourceType resourceType;
    public TerrainSO.ResourceType secondaryResourceType;

    [Range(0, 1)] public float hiddenAlpha = 0.3f; // Alpha value when the unit is hidden

    // Track original alpha per renderer so we can restore it on exit
    private readonly Dictionary<Renderer, float> originalAlphas = new();

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
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            Renderer otherRenderer = other.GetComponentInChildren<Renderer>();
            if (otherRenderer != null)
            {
                Material mat = otherRenderer.material; // creates instance if needed
                Color currentColor = mat.color;
                if (!originalAlphas.ContainsKey(otherRenderer)) originalAlphas[otherRenderer] = currentColor.a;
                currentColor.a = hiddenAlpha; // Set the alpha to the hidden value
                mat.color = currentColor;
            }
            else
            {
                Debug.Log("No Renderer found on the entered unit to hide.");
            }
            Debug.Log("TRIGGERED");
        }
   }

   private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            Renderer otherRenderer = other.GetComponentInChildren<Renderer>();
            if (otherRenderer != null)
            {
                if (originalAlphas.TryGetValue(otherRenderer, out float orig))
                {
                    Material mat = otherRenderer.material;
                    Color currentColor = mat.color;
                    currentColor.a = orig; // Restore the alpha to the original value
                    mat.color = currentColor;
                    originalAlphas.Remove(otherRenderer);
                }
                else
                {
                    // Fallback: restore to fully opaque
                    Material mat = otherRenderer.material;
                    Color currentColor = mat.color;
                    currentColor.a = 1f;
                    mat.color = currentColor;
                }
            }
            else
            {
                Debug.LogWarning("No Renderer found on the unit to restore visibility.");
            }
        }
    }
}

