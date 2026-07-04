using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button unitsButton;
    [SerializeField] private Button coinsButton;
    [SerializeField] private Button gemsButton;
    [SerializeField] private GameObject unitsShopPanel;
    [SerializeField] private GameObject coinsShopPanel;
    [SerializeField] private GameObject gemsShopPanel;
    [SerializeField] private Button closeButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(unitsButton != null && coinsButton != null && gemsButton != null)
        {
            unitsButton.onClick.AddListener(OpenUnitsShop);
            coinsButton.onClick.AddListener(OpenCoinsShop);
            gemsButton.onClick.AddListener(OpenGemsShop);
            closeButton.onClick.AddListener(CloseShopPanel);
        }
    }
    public void OpenShopPanel()
    {
        shopPanel.SetActive(true);
    }
    public void CloseShopPanel()
    {
        shopPanel.SetActive(false);
    }
    public void OpenUnitsShop()
    {
        unitsShopPanel.SetActive(true);
        coinsShopPanel.SetActive(false);
        gemsShopPanel.SetActive(false);
    }
    public void OpenCoinsShop()
    {
        unitsShopPanel.SetActive(false);
        coinsShopPanel.SetActive(true);
        gemsShopPanel.SetActive(false);
    }
    public void OpenGemsShop()
    {
        unitsShopPanel.SetActive(false);
        coinsShopPanel.SetActive(false);
        gemsShopPanel.SetActive(true);
    }

}
