using UnityEngine;

public class Building : MonoBehaviour
{

    // Represents a placed building instance. Responsible for creating and positioning
    // the visual model and applying the requested rotation.
    public string Description => data.Description;
    public int Cost => data.Cost;


    private BuildingModel model;
    private BuildingData data;
    [SerializeField] private Vector3 manualOffset;

    /// <summary>
    /// Initialize this Building with the provided data and rotation.
    /// - Instantiates the visual BuildingModel as a child.
    /// - Adjusts the model vertically so its bottom sits on the grid plane.
    /// - Applies any manual offset and the requested rotation.
    /// </summary>
    public void SetUp(BuildingData data, float rotation)
    {
        // Save data reference
        this.data = data;
        // Parent the visual model to this Building and reset local transform
        model = Instantiate(data.Model, transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        // Lift model so its bottom sits at the Building origin (grid plane)
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            float minLocalY = float.PositiveInfinity;
            // Find the lowest renderer bound relative to the model so we can offset upward
            foreach (var r in renderers)
            {
                if (r == null) continue;
                // Convert renderer bounds min (world) into the model's local space
                Vector3 localMin = model.transform.InverseTransformPoint(r.bounds.min);
                minLocalY = Mathf.Min(minLocalY, localMin.y);
            }
            // Move the model up so its lowest point sits at y = 0 (building origin)
            if (minLocalY < float.PositiveInfinity)
                model.transform.localPosition = new Vector3(0, -minLocalY, 0);
            // Apply any manual offset set in the inspector
            model.transform.localPosition += manualOffset;
        }
        // Apply the requested absolute rotation to the visual model
        model.SetRotation(rotation);
    }
}
