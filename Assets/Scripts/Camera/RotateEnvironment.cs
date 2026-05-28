using UnityEngine;

public class RotateEnvironment : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private GameObject environmentParent;
    private Quaternion initialRotation;

    // Cache the initial rotation so we can reset later.
    void Start()
    {
        if (environmentParent != null)
            initialRotation = environmentParent.transform.rotation;
    }

    // Update handles input for rotating and resetting the environment parent.
    void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            RotateEnvironmentObject(true);
        }
        if (Input.GetKey(KeyCode.T))
        {
            RotateEnvironmentObject(false);
        }
        // Right mouse button resets rotation to the cached initial rotation.
        if (Input.GetMouseButton(1))
        {
            if (environmentParent != null)
                environmentParent.transform.rotation = initialRotation; // Reset rotation on right-click
        }
        
    }

    // Rotate the environment parent around Y axis by rotationSpeed.
    public void RotateEnvironmentObject(bool goingRight = true)
    {
        if (environmentParent != null)
        {
            environmentParent.transform.Rotate(Vector3.up, (goingRight ? 1 : -1) * rotationSpeed * Time.deltaTime);
        }
    }
}
