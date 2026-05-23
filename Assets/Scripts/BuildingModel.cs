using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BuildingModel : MonoBehaviour
{
    [SerializeField] private Transform wrapper;
    public float Rotation => wrapper.transform.eulerAngles.y;
    private BuildingShapeUnit[] shapeUnits;

    private void Awake()
    {
        shapeUnits = GetComponentsInChildren<BuildingShapeUnit>();
    }

    public void Rotate(float rotationStep)
    {
        wrapper.Rotate(new Vector3(0, rotationStep, 0));
    }

    public List<Vector3> GetAllBuildingPosition()
    {
        if (shapeUnits == null || shapeUnits.Length == 0) return new List<Vector3>();
        return shapeUnits.Select(unit => unit.transform.position).ToList();
    }
}
