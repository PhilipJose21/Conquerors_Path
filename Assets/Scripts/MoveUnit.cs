using UnityEngine;

public class MoveUnit : MonoBehaviour
{
    private UnitSO unitData;

    private static int lastProcessedClickFrame = -1;
    private Camera mainCamera;

    private Material originalMaterial;
    bool rayHit;
    RaycastHit hit;

    [Header("Range (set per-unit or via UnitSO)")]
    public int mobility = 2; // Manhattan (diamond/cross) movement range
    public int attackRange = 1; // Square attack range

    void Awake()
    {
        UnitSOContainer container = this.GetComponent<UnitSOContainer>();
        if (container != null)
        {
            unitData = container.unitData;
        }
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Start()
    {
        
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



    //CLICK LOGIC

    public void Clicked(GameObject obj)
    {
        Debug.Log("Clicked on: " + obj.name);

        // Try to get a MoveUnit component from the clicked object (preferred)
        MoveUnit clickedMove = obj.GetComponent<MoveUnit>();
        if (clickedMove != null)
        {
            if (CellHighlighter.Instance != null)
                CellHighlighter.Instance.ShowHighlightsForUnit(obj, clickedMove.mobility, clickedMove.attackRange);
            return;
        }

        // Fallback: if the clicked object has a UnitSOContainer, you can read ranges from it
        UnitSOContainer container = obj.GetComponent<UnitSOContainer>();
        if (container != null && container.unitData != null)
        {
            // If your UnitSO has explicit fields for mobility/attack, map them here.
            // As a safe fallback, use this object's serialized values.
            if (CellHighlighter.Instance != null)
                CellHighlighter.Instance.ShowHighlightsForUnit(obj, mobility, attackRange);
            return;
        }

        // If nothing found, clear highlights
        if (CellHighlighter.Instance != null)
            CellHighlighter.Instance.ClearHighlights();
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
