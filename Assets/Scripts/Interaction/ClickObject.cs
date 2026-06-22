using UnityEngine;
using UnityEngine.EventSystems;

public class ClickObject : MonoBehaviour
{
    private bool wasPlacingLastFrame = false;
    private static int lastProcessedClickFrame = -1;
    public Camera mainCamera;

    [Header("Hover Scale Settings")]
    public float hoverScaleMultiplier = 1.05f; 
    public float scaleSpeed = 12f; 

    [Header("Bouncy Hop Settings")]
    public float bounceDuration = 0.35f; // Fast, snappy animation speed
    public Vector3 peakBounceScale = new Vector3(0.8f, 1.35f, 0.8f); 

    private GameObject currentlyHoveredObject;
    private Vector3 originalObjectScale;

    // Bounce Animation Loop variables
    private GameObject bouncingObject;
    private Vector3 bounceStartScale;
    private float bounceTimer = -1f;

    bool rayHit;
    RaycastHit hit;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        DetectObjects();

        // 1. Check the building placement state at the start of the frame
        BuildingSystem buildingSystem = FindFirstObjectByType<BuildingSystem>();
        bool isPlacingNow = buildingSystem != null && buildingSystem.isPlacing;

        // 2. Only run hover scaling if we are NOT placing a building
        if (!isPlacingNow)
        {
            HandleHoverScale();
            HandleBouncyHop();
        }
        // 3. Process clicks only on the exact frame they happen, and only if we aren't placing a building (or just stopped placing this frame)
        if (Input.GetMouseButtonDown(0)) 
        {
            if (isPlacingNow || wasPlacingLastFrame)
            {
                // Record state and exit early so the click doesn't bleed into selection
                wasPlacingLastFrame = isPlacingNow;
                return;
            }

            if (rayHit)
            {
                Clicked(hit.collider.gameObject);
            }
        }

        // 4. Record the placement state at the very end of the frame so we can compare against it in the next frame
        wasPlacingLastFrame = isPlacingNow;
    }

    public void Clicked(GameObject obj)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Find the script on this object or its parent wrapper layer
        OpenBuildingUI buildingUI = obj.GetComponentInParent<OpenBuildingUI>();
        if (buildingUI != null)
        {
            // Trigger the animation first so open UI panels can't block it!
            TriggerBounce(buildingUI.gameObject);
            
            
            buildingUI.OpenUI();
        }
    }

    private void TriggerBounce(GameObject obj)
    {
        ResetBouncingObjectState();

        bouncingObject = obj;
        
        // Grab the baseline scale to animate cleanly
        if (bouncingObject == currentlyHoveredObject)
        {
            bounceStartScale = originalObjectScale;
        }
        else
        {
            bounceStartScale = bouncingObject.transform.localScale;
        }

        bounceTimer = 0f;
    }

    private void HandleBouncyHop()
    {
        if (bouncingObject == null || bounceTimer < 0f) return;

        bounceTimer += Time.deltaTime;
        float progress = bounceTimer / bounceDuration;

        if (progress >= 1f)
        {
            ResetBouncingObjectState();
        }
        else
        {
            // Mathematical sine arc peak calculation curve
            float bounceArc = Mathf.Sin(progress * Mathf.PI);

            // Smoothly deform the building's scale structure along the jump timeline arc
            Vector3 targetScale = new Vector3(
                Mathf.Lerp(bounceStartScale.x, bounceStartScale.x * peakBounceScale.x, bounceArc),
                Mathf.Lerp(bounceStartScale.y, bounceStartScale.y * peakBounceScale.y, bounceArc),
                Mathf.Lerp(bounceStartScale.z, bounceStartScale.z * peakBounceScale.z, bounceArc)
            );

            bouncingObject.transform.localScale = targetScale;
        }
    }

    private void ResetBouncingObjectState()
    {
        if (bouncingObject != null)
        {
            if (bouncingObject == currentlyHoveredObject)
            {
                bouncingObject.transform.localScale = originalObjectScale * hoverScaleMultiplier;
            }
            else
            {
                bouncingObject.transform.localScale = bounceStartScale;
            }
        }

        bouncingObject = null;
        bounceTimer = -1f;
    }

    private void HandleHoverScale()
    {
        GameObject newHover = null;

        if (rayHit && !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
        {
            if (hit.collider.gameObject.GetComponentInParent<OpenBuildingUI>() != null)
            {
                newHover = hit.collider.gameObject.GetComponentInParent<OpenBuildingUI>().gameObject;
            }
        }

        if (newHover != currentlyHoveredObject)
        {
            if (currentlyHoveredObject != null && currentlyHoveredObject != bouncingObject)
            {
                currentlyHoveredObject.transform.localScale = originalObjectScale;
            }

            currentlyHoveredObject = newHover;

            if (currentlyHoveredObject != null && currentlyHoveredObject != bouncingObject)
            {
                originalObjectScale = currentlyHoveredObject.transform.localScale;
            }
        }

        if (currentlyHoveredObject != null && currentlyHoveredObject != bouncingObject)
        {
            Vector3 targetScale = originalObjectScale * hoverScaleMultiplier;
            currentlyHoveredObject.transform.localScale = Vector3.Lerp(
                currentlyHoveredObject.transform.localScale, 
                targetScale, 
                Time.deltaTime * scaleSpeed
            );
        }
    }

    public void DetectObjects()
    {
        if (lastProcessedClickFrame == Time.frameCount)
            return;

        lastProcessedClickFrame = Time.frameCount;

        Vector3 mousePos = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        rayHit = Physics.Raycast(ray, out hit);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
    }
}