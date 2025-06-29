using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;
using System.Linq;


public class CatInfoPanelNew : MonoBehaviour
{
    [Header("Info Panel Settings")]
    [SerializeField] private List<CatInfoCard> _CatCards;

    private CatInfoCollection _CatInfoCollection;

    private void Awake()
    {
        _CatInfoCollection = GetComponent<CatInfoCollection>();
        this.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        PopulateCards();
        gameObject.SetActive(false);
    }

    private void PopulateCards()
    {
        foreach (CatInfoCard card in _CatCards)
        {
            card.FillCard(_CatInfoCollection.GetCatInfo(card.CatType));
        }
    }

    public void ButtonClicked_Close()
    {
        gameObject.SetActive(false);
    }
}