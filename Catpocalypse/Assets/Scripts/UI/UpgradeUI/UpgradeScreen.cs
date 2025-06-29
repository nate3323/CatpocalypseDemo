using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeScreen : MonoBehaviour
{    
    [SerializeField]
    private TextMeshProUGUI _scrapText;
    [SerializeField] private UpgradeCard[] _UpgradeCards;

    private void Start()
    {

        ChangeScrapText();
        UpgradeCard.OnUpgrade += OnUpgrade;
        gameObject.SetActive(false);
    }

    public void OnUpgrade(object sender, EventArgs e)
    {
        ChangeScrapText();
    }
    private void ChangeScrapText()
    {
        _scrapText.text = PlayerDataManager.Instance.CurrentData.scrap.ToString();
    }

    public void ReturnToMenu()
    {
        gameObject.SetActive(false);
    }
}
