using UnityEngine;

public class AssignParent : MonoBehaviour
{
    public GameObject parentObject;
    private MoveUnit moveUnitScript;
    private EnemyMovement enemyUnitScript;
    private UnitHealth healthScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parentObject = this.gameObject; // Default to self if not set in inspector
        // Try to find components on the parentObject (search children as well)
        moveUnitScript = parentObject.GetComponentInChildren<MoveUnit>();
        enemyUnitScript = parentObject.GetComponentInChildren<EnemyMovement>();
        healthScript = parentObject.GetComponentInChildren<UnitHealth>();
        // Assign unitObject on whichever component exists. Do not return early — assign both if present.
        if (moveUnitScript != null)
        {
            moveUnitScript.unitObject = parentObject;
            
        }

        if (enemyUnitScript != null)
        {
            enemyUnitScript.unitObject = parentObject;
        }

        if (healthScript != null)
        {
            healthScript.playerUnit = parentObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
