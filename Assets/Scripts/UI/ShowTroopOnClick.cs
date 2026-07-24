using UnityEngine;

public class ShowTroopOnClick : MonoBehaviour {
    public UnitSO troop;
        public void Show()
    {
        Debug.Log("Troop clicked");
        KingdomUIManager.Instance?.ShowSelectedTroop(troop);
    }
}