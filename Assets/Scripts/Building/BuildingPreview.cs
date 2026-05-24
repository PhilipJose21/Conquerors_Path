using UnityEngine;
using System.Collections.Generic;

public class BuildingPreview : MonoBehaviour
{
    public enum BuildingPreviewState
    {
        VALID,
        INVALID
    }

    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;
    [SerializeField] private Vector3 manualOffset;

    public BuildingPreviewState State { get; private set; } = BuildingPreviewState.INVALID;
    public BuildingData Data { get; private set; }
    public BuildingModel BuildingModel { get; private set; }
    private List<Renderer> renderers = new();
    private List<Collider> colliders = new();
    public void Setup(BuildingData data)
    {
        Data = data;
        BuildingModel = Instantiate(data.Model, transform);
        BuildingModel.transform.localPosition = Vector3.zero;
        BuildingModel.transform.localRotation = Quaternion.identity;
        renderers.AddRange(BuildingModel.GetComponentsInChildren<Renderer>());
        colliders.AddRange(BuildingModel.GetComponentsInChildren<Collider>());
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
        // Lift model so its bottom sits at the preview origin (grid plane)
        if (renderers.Count > 0)
        {
            float minLocalY = float.PositiveInfinity;
            foreach (var r in renderers)
            {
                if (r == null) continue;
                Vector3 localMin = BuildingModel.transform.InverseTransformPoint(r.bounds.min);
                minLocalY = Mathf.Min(minLocalY, localMin.y);
            }
            if (minLocalY < float.PositiveInfinity)
                BuildingModel.transform.localPosition = new Vector3(0, -minLocalY, 0);
            // Apply any manual offset set in the inspector
            BuildingModel.transform.localPosition += manualOffset;
        }
        SetPreviewMaterial(State);
    }

    public void ChangeState(BuildingPreviewState newState)
    {
        if (newState == State) return;
        State = newState;
        SetPreviewMaterial(State);
    }

    public void Rotate(int rotationStep)
    {
        BuildingModel.Rotate(rotationStep);
    }

    private void SetPreviewMaterial(BuildingPreviewState newState)
    {
        Material previewMat = newState == BuildingPreviewState.VALID ? validMaterial : invalidMaterial;
        foreach (var renderer in renderers)
        {
            Material[] mats = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = previewMat;
            }
            renderer.materials = mats;
        }
    }
}
