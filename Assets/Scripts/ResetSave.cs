using UnityEngine;

public class ResetSave : MonoBehaviour
{
    public void ResetSaveButton()
    {
        KingdomSaveManager.Instance?.ResetSaveData();
    }
}
