using UnityEngine;

public class BuildingButtons : MonoBehaviour
{
    public int BuildingIndex; //set building Index in inspector, used to get building data from BuildingSystem
    public BuildingSystem BuildingSystem => FindObjectOfType<BuildingSystem>();

    public void SetBuilding()
    {
        BuildingSystem.buildIndex = BuildingIndex;
        BuildingSystem.TrySelectBuildIndex(BuildingIndex, GetMouseWorldPosition());
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}
