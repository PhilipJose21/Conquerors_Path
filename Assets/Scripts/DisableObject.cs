using UnityEngine;

public class DisableObject : MonoBehaviour
{
    public GameObject objectToDisable;

    public void DisableGameObject()
    {
        objectToDisable.SetActive(false);
    }
}
