using UnityEngine;
using UnityEngine.EventSystems;

public class ClickObject : MonoBehaviour
{
    private static int lastProcessedClickFrame = -1;
    public Camera mainCamera;

    [Header("Hover Scale Settings")]
    public float hoverScaleMultiplier = 1.05f; // Scales up by 5%
    public float scaleSpeed = 12f; // Controls how snappy the swell transition is

    private GameObject currentlyHoveredObject;
    private Vector3 originalObjectScale;

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
        HandleHoverScale();

        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            if (rayHit)
            {
                Clicked(hit.collider.gameObject);
            }
        }
    }

    public void Clicked(GameObject obj)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
        if (KingdomUIManager.Instance != null && KingdomUIManager.Instance.IsObjectInfoOpen)
            return;

        OpenBuildingUI buildingUI = obj.GetComponent<OpenBuildingUI>();
        if (buildingUI != null)
        {
            buildingUI.OpenUI();
        }
    }

    private void HandleHoverScale()
    {
        GameObject newHover = null;

        // Only process hover if we hit an object and the mouse isn't blocked by UI panels
        if (rayHit && !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
        {
            // Only hover items that have the OpenBuildingUI click component attached
            if (hit.collider.gameObject.GetComponent<OpenBuildingUI>() != null)
            {
                newHover = hit.collider.gameObject;
            }
        }

        // Focus Swap Check: Did the mouse move to a brand new object?
        if (newHover != currentlyHoveredObject)
        {
            // Instantly force the old object back to its exact baseline size
            if (currentlyHoveredObject != null)
            {
                currentlyHoveredObject.transform.localScale = originalObjectScale;
            }

            currentlyHoveredObject = newHover;

            // Remember the baseline size of the new object before scaling it
            if (currentlyHoveredObject != null)
            {
                originalObjectScale = currentlyHoveredObject.transform.localScale;
            }
        }

        // Smoothly lerp towards the target swollen size frame-by-frame
        if (currentlyHoveredObject != null)
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