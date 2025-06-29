using System;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class SaveSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text timeLabel;
    [SerializeField] private TMP_Text levelsCompleted;
    [SerializeField] private TMP_Text defaultText;

    public void UpdateSaveLabel(PlayerData data)
    {
        if (!defaultText.text.Equals(""))
        {
            defaultText.text = "";
        }
        nameLabel.text = data.name;
        float time = data.time;
        int hours = (int) (time - (time % 3600)) / 3600;
        int minutes = (int) (time - ((time - (hours * 3600)) % 60)) / 60;
        int seconds = (int) time - (hours * 3600) - (minutes * 60);
        timeLabel.text = String.Format("{0}:{1}:{2}", hours, minutes, seconds);
        levelsCompleted.text = String.Format("Current Level: {0}", data.levelsCompleted);
    }
}