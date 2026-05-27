using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BuildingModel : MonoBehaviour
{
    // Visual representation of a building type. Contains a `wrapper` transform
    // which is rotated to visually orient the model without changing unit world positions.
    [SerializeField] private Transform wrapper;
    public float Rotation => wrapper.localEulerAngles.y;
    private BuildingShapeUnit[] shapeUnits;

    private void Awake()
    {
        // Cache all child units that define the building's occupied cells.
        shapeUnits = GetComponentsInChildren<BuildingShapeUnit>();
    }

    public void Rotate(float rotationStep)
    {
        // Rotate the wrapper transform around Y axis to change visual orientation.
        // `rotationStep` is an angle in degrees.
        wrapper.Rotate(new Vector3(0, rotationStep, 0));
    }

    public void SetRotation(float angle)
    {
        wrapper.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    public List<Vector3> GetAllBuildingPosition()
    {
        // Return world-space positions of each BuildingShapeUnit. These positions are
        // used for placement validation and snapping.
        if (shapeUnits == null || shapeUnits.Length == 0) return new List<Vector3>();
        return shapeUnits.Select(unit => unit.transform.position).ToList();
    }
}
