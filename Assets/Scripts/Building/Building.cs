using UnityEngine;

public class Building : MonoBehaviour
{

    // Represents a placed building instance. Responsible for creating and positioning
    // the visual model and applying the requested rotation.
    public string Name => data != null ? data.Name : "Unknown";
    public int Size => data != null ? data.Size : 0;
    public Sprite Icon => data != null ? data.Icon : null;
    public int CoinCost => data != null ? data.coinCost : 0;
    public int FarmCost => data != null ? data.farmCost : 0;
    public int RockCost => data != null ? data.rockCost : 0;
    public int WoodCost => data != null ? data.woodCost : 0;
    public int GemCost => data != null ? data.gemCost : 0;
    public int EnergyCost => data != null ? data.energyCost : 0;


    private BuildingModel model;
    private BuildingData data;
    [SerializeField] private Vector3 manualOffset;
    [SerializeField] private bool persistThroughLoads;

    public bool HasData => data != null;
    public bool PersistThroughLoads => persistThroughLoads;

    public BuildingData BuildingDataAsset => data;

    /// <summary>
    /// Initialize this Building with the provided data and rotation.
    /// - Instantiates the visual BuildingModel as a child.
    /// - Adjusts the model vertically so its bottom sits on the grid plane.
    /// - Applies any manual offset and the requested rotation.
    /// </summary>
    public void SetUp(BuildingData data, float rotation)
    {
        // Guard against missing data
        if (data == null)
        {
            Debug.LogError("Building.SetUp called with NULL data on " + name);
            return;
        }

        // Save data reference
        this.data = data;

        // Parent the visual model to this Building and reset local transform
        model = Instantiate(data.Model, transform);
        model.transform.localRotation = Quaternion.identity;

        // IMPORTANT: do not blindly overwrite model.transform.localPosition
        // for every model. This script is shared by two genuinely different
        // cases:
        //  - Animated battle units (SkinnedMeshRenderer + Animator): these
        //    have their own hand-tuned local offsets baked in at multiple
        //    levels of their hierarchy, and Renderer.bounds on a skinned mesh
        //    is unreliable (reflects bind pose, not the actual posed model) -
        //    so for these we trust the authored position as-is.
        //  - Static structures like Kingdom buildings (plain MeshRenderer,
        //    no Animator): these never had a hand-tuned offset and always
        //    relied on bounds-based auto-grounding to sit flush on the grid
        //    plane - removing that broke Kingdom building placement.
        // Detect which case this is and behave accordingly.
        bool isAnimatedModel = model.GetComponentInChildren<SkinnedMeshRenderer>() != null;

        if (isAnimatedModel)
        {
            // Trust the prefab's own authored local position; only add the
            // generic and per-unit-type corrections on top.
            model.transform.localPosition += manualOffset + data.ModelOffset;
        }
        else
        {
            // Static building: auto-ground it by lifting the model so its
            // lowest renderer bound sits at local Y = 0.
            model.transform.localPosition = Vector3.zero;
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
            model.transform.localPosition += manualOffset + data.ModelOffset;
        }

        // Apply the requested absolute rotation to the visual model
        model.SetRotation(rotation);
    }
    private void OnMouseDown()
    {
        if (!HasData) return;

        BuildingSystem system = FindFirstObjectByType<BuildingSystem>();
        if (system != null && system.isPlacing)
            return;

        KingdomUIManager.Instance?.ShowObjectInfo(this);
    }
}