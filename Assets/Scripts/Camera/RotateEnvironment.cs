using UnityEngine;

public class RotateEnvironment : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private GameObject environmentParent;
    private Quaternion initialRotation;

    // Expose whether the environment is currently spinning
    public bool IsRotating { get; private set; }

    void Start()
    {
        if (environmentParent != null)
            initialRotation = environmentParent.transform.rotation;
    }

    void Update()
    {
        // Reset the flag at the start of every frame
        IsRotating = false;

        // Lock environment rotation while any unit is animating movement.
        if (MoveUnit.AnyUnitMoving)
        {
            return;
        }

        if (Input.GetKey(KeyCode.T))
        {
            RotateEnvironmentObject(true);
            IsRotating = true; // Set flag
        }
        if (Input.GetKey(KeyCode.Y))
        {
            RotateEnvironmentObject(false);
            IsRotating = true; // Set flag
        }
        
        // Right mouse button resets rotation to the cached initial rotation.
        if (Input.GetMouseButton(1))
        {
            if (environmentParent != null)
                environmentParent.transform.rotation = initialRotation; 
        }
    }

    public void RotateEnvironmentObject(bool goingRight = true)
    {
        if (environmentParent != null)
        {
            environmentParent.transform.Rotate(Vector3.up, (goingRight ? 1 : -1) * rotationSpeed * Time.deltaTime);
        }
    }
}