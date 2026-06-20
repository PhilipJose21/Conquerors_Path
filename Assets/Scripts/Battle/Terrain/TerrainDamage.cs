using UnityEngine;

public class TerrainDamage : MonoBehaviour
{
    private TerrainSOContainer terrainSOContainer;
    private TerrainSO terrainSO;
    public int damageAmount;
 
    // Update is called once per frame
    void Awake()
    {
        terrainSOContainer = this.GetComponent<TerrainSOContainer>();
        terrainSO = terrainSOContainer.terrainData;
        damageAmount = terrainSO.terrainDamage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            UnitHealth unitHealth = other.GetComponent<UnitHealth>();
            if (unitHealth != null)
            {
                unitHealth.TakeDamage(damageAmount);
            }
        }
    }
}
