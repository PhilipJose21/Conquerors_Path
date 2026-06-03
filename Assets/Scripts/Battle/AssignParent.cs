using UnityEngine;

public class AssignParent : MonoBehaviour
{
    public GameObject parentObject;
    private MoveUnit moveUnitScript;
    private EnemyMovement enemyUnitScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parentObject = this.gameObject; // Default to self if not set in inspector
        // Try to find components on the parentObject (search children as well)
        moveUnitScript = parentObject.GetComponentInChildren<MoveUnit>();
        enemyUnitScript = parentObject.GetComponentInChildren<EnemyMovement>();

        // Assign unitObject on whichever component exists. Do not return early — assign both if present.
        if (moveUnitScript != null)
        {
            moveUnitScript.unitObject = parentObject;
        }

        if (enemyUnitScript != null)
        {
            enemyUnitScript.unitObject = parentObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
