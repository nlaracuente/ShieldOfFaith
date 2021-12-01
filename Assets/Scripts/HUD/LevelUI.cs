using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    [SerializeField, Tooltip("Icons that represent faith in ascending order")]
    List<LevelIconUI> faithIcons;

    [SerializeField, Tooltip("Icons that represent collected scriptures in ascending order")]
    List<LevelIconUI> scriptureIcons;

    [SerializeField, Tooltip("The text that we will increase to indicate bees killed vs total remaining")]
    Text beeCounter;

    Player Player { get { return GameManager.instance.Player; } }


    private void LateUpdate()
    {
        SetFaith();
        SetScriptures();
        SetBeeCounter();
    }

    private void SetScriptures()
    {
        for (int i = 0; i < scriptureIcons.Count; i++)
        {
            var icon = scriptureIcons[i];
            icon.SetStatus(LevelController.instance.HasCollectedScripture(i));
        }
    }

    private void SetFaith()
    {
        for (int i = 0; i < faithIcons.Count; i++)
        {
            var icon = faithIcons[i];
            icon.SetStatus(i + 1 <= Player.Faith);
        }
    }

    private void SetBeeCounter()
    {
        var total = GameManager.instance.TotalBees;
        var current = GameManager.instance.BeesKilled;
        beeCounter.text = $"{current}/{total}";
    }
}
