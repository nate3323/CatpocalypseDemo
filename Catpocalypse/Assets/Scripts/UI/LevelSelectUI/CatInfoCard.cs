using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CatInfoCard : MonoBehaviour
{
    [SerializeField] private CatTypes catType;

    [SerializeField] private TextMeshProUGUI _Text_DisplayName;
    [SerializeField] private TextMeshProUGUI _Text_Speed;
    [SerializeField] private TextMeshProUGUI _Text_SpawnRate;
    [SerializeField] private TextMeshProUGUI _Text_HP;
    [SerializeField] private TextMeshProUGUI _Text_PayOnDistraction;
    [SerializeField] private TextMeshProUGUI _Text_Special;
    [SerializeField] private TextMeshProUGUI _Text_Description;
    public UnityEngine.UI.Image _ReferenceImage;
    public UnityEngine.UI.Image _CatIcon;

    public void FillCard(CatInfo catInfo)
    {
        _Text_DisplayName.text = catInfo.DisplayName;
        _Text_Speed.text = catInfo.Speed.ToString();
        _Text_SpawnRate.text = catInfo.SpawnRate.ToString();
        _Text_HP.text = catInfo.HP.ToString();
        _Text_PayOnDistraction.text = catInfo.PayOnDistraction.ToString();
        _Text_Special.text = catInfo.Special.ToString();
        _Text_Description.text = catInfo.Description.ToString();
        _ReferenceImage.sprite = catInfo.CatImage;
        _CatIcon.sprite = catInfo.Icon;
    }

    public CatTypes CatType { get { return catType; }    }
}