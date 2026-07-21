using UnityEngine;
using System.Collections;

public class RotateModel : MonoBehaviour
{
    private GameObject modelTransform;

    [Header("Rotation")]
    [Tooltip("Degrees per second the model turns to face its new movement direction.")]
    public float rotationSpeed = 720f;

    [Tooltip("If true, ignores any difference in height (Y) between current and target position, so the model only ever turns around the vertical axis instead of tilting up/down.")]
    public bool onlyRotateOnHorizontalPlane = true;

    private Coroutine rotateCoroutine;

    void Awake()
    {
        modelTransform = this.gameObject;
    }

    // Call this with the direction the unit is about to move (or was clicked to move) in.
    // Smoothly turns the model to face that direction; safe to call again mid-turn
    // (e.g. a new click before the previous turn finished) since it just restarts
    // the coroutine toward the new target rotation.
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
        FaceDirection(targetPosition - modelTransform.transform.position);
    }

    private IEnumerator RotateTowards(Quaternion targetRotation)
    {
        while (Quaternion.Angle(modelTransform.transform.rotation, targetRotation) > 0.5f)
        {
            modelTransform.transform.rotation = Quaternion.RotateTowards(modelTransform.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }
        modelTransform.transform.rotation = targetRotation;
        rotateCoroutine = null;
    }
}