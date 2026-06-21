using UnityEngine;

public class TerrainHideUnit : MonoBehaviour
{
    private TerrainSOContainer terrainSOContainer;
    private TerrainSO terrainSO;
    public TerrainSO.ResourceType resourceType;
    public TerrainSO.ResourceType secondaryResourceType;

    private float initialAlpha = 1f; // Store the initial alpha value of the unit's material
    public float hiddenAlpha = 0.3f; // Alpha value when the unit is hidden
    void Awake()
    {
        terrainSOContainer = this.GetComponent<TerrainSOContainer>();

        terrainSO = terrainSOContainer.terrainData;
        resourceType = terrainSO.resourceType;
        secondaryResourceType = terrainSO.secondaryResourceType;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            //looks for PlayerUnit || EnemyUnit in the children and decreases the alpha to make it transparent
        }
    } 
}
