using UnityEngine;

public class TerrainHarvest : MonoBehaviour
{
    private TerrainSOContainer terrainSOContainer;
    private TerrainSO terrainSO;
    public TerrainSO.ResourceType resourceType;
    public TerrainSO.ResourceType secondaryResourceType;
 
    // Update is called once per frame
    void Awake()
    {
        terrainSOContainer = this.GetComponent<TerrainSOContainer>();
        terrainSO = terrainSOContainer.terrainData;
        resourceType = terrainSO.resourceType;
        secondaryResourceType = terrainSO.secondaryResourceType;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
