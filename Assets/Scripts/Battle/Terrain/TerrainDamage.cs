using UnityEngine;

public class TerrainDamage : MonoBehaviour
{
    public TerrainSOContainer terrainDataContainer;
    public TerrainSO terrainData;
    public int hazardDamage;
    void Awake()
    {
        terrainDataContainer = GetComponent<TerrainSOContainer>();
        if (terrainDataContainer != null && terrainDataContainer.terrainData != null)
        {
            terrainData = terrainDataContainer.terrainData;
            hazardDamage = terrainData.terrainHazardDamage;
        }
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerUnit") || other.CompareTag("EnemyUnit"))
        {
            UnitHealth unitHealth = other.GetComponentInChildren<UnitHealth>();
            if (unitHealth != null)
            {
                unitHealth.TakeDamage(hazardDamage);
                Debug.Log($"Unit {other.name} took {hazardDamage} damage from terrain hazard.");
            }
            Debug.Log("TRIGGERED");
        }
    }

    
}
