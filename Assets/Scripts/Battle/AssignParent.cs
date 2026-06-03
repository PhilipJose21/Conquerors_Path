using UnityEngine;

public class AssignParent : MonoBehaviour
{
    public GameObject parentObject;
    private MoveUnit moveUnitScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parentObject = this.gameObject;
        moveUnitScript = parentObject.GetComponentInChildren<MoveUnit>();
        moveUnitScript.unitObject = parentObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
