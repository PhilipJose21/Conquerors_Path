using UnityEngine;

public class ShowTroopOnClick : MonoBehaviour {
    public TroopData troop;
        public void Show()
    {
        Debug.Log("Troop clicked");
        KingdomUIManager.Instance?.ShowSelectedTroop(troop);
    }
}