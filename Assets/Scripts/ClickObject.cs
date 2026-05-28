using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class ClickObject : MonoBehaviour
{
    // Ensure only one ClickObject processes a click per frame
    private static int lastProcessedClickFrame = -1;
    public Camera mainCamera;

    private Material originalMaterial;

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
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            if(rayHit)
            {
                Clicked(hit.collider.gameObject);
            }
        }
    }

    public void Clicked(GameObject obj)
    {
        OpenBuildingUI buildingUI = obj.GetComponent<OpenBuildingUI>();
        if (buildingUI != null)
        {
            buildingUI.OpenUI();
        }
    }

    public void HighlightObject(GameObject obj)
    {
        // Example highlight logic: change the object's material color
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material; // Store original material
            renderer.material.color = Color.yellow; // Highlight color
        }
    }

    public void DetectObjects()
    {
        // If another ClickObject already handled this frame's click, skip
        if (lastProcessedClickFrame == Time.frameCount)
            return;

        // Mark this frame as handled so others skip
        lastProcessedClickFrame = Time.frameCount;

        Vector3 mousePos = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        rayHit = Physics.Raycast(ray, out hit);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
    }
}
