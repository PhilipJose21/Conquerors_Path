using UnityEngine;

public class Building : MonoBehaviour
{

    public string Name => data.Name;
    public int CellSize => data.CellSize;


    private BuildingModel model;
    private BuildingData data;

    public void SetUp(BuildingData data, float rotation)
    {
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
            foreach (var r in renderers)
            {
                if (r == null) continue;
                Vector3 localMin = model.transform.InverseTransformPoint(r.bounds.min);
                minLocalY = Mathf.Min(minLocalY, localMin.y);
            }
            if (minLocalY < float.PositiveInfinity)
                model.transform.localPosition = new Vector3(0, -minLocalY, 0);
        }
        model.Rotate(rotation);
    }
}
