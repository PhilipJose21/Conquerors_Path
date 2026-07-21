using UnityEngine;
using System.Collections;

public class RotateModel : MonoBehaviour
{
    private Transform modelTransform;

    [Header("Rotation")]
    [Tooltip("Degrees per second the model turns to face its new movement direction.")]
    public float rotationSpeed = 720f;

    [Tooltip("If true, ignores any difference in height (Y) between current and target position, so the model only ever turns around the vertical axis instead of tilting up/down.")]
    public bool onlyRotateOnHorizontalPlane = true;

    [Tooltip("Some models are rigged with their back facing the direction the script treats as 'forward'. Enable this to flip the target rotation 180 degrees for those models, instead of re-rigging or re-exporting them.")]
    public bool invertFacing = false;

    private Coroutine rotateCoroutine;

    void Awake()
    {
        modelTransform = this.transform;
    }

    public void FaceDirection(Vector3 direction)
    {
        if (onlyRotateOnHorizontalPlane)
        {
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.0001f)
        {
            // No meaningful direction (e.g. target == current position) — nothing to face.
            return;
        }

        if (invertFacing)
        {
            direction = -direction;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
        }
        rotateCoroutine = StartCoroutine(RotateTowards(targetRotation));
    }

    // Convenience overload: face toward a world-space point instead of a raw direction.
    public void FacePosition(Vector3 targetPosition)
    {
        FaceDirection(targetPosition - modelTransform.position);
    }

    private IEnumerator RotateTowards(Quaternion targetRotation)
    {
        while (Quaternion.Angle(modelTransform.rotation, targetRotation) > 0.5f)
        {
            modelTransform.rotation = Quaternion.RotateTowards(modelTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }
        modelTransform.rotation = targetRotation;
        rotateCoroutine = null;
    }
}