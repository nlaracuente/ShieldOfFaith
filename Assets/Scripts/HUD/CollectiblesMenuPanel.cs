using UnityEngine;
using UnityEngine.UI;

public class CollectiblesMenuPanel : MonoBehaviour
{
    [SerializeField]
    Text levelName;

    [SerializeField]
    Text highScore;

    [SerializeField]
    Image beesIcon;

    [SerializeField]
    Sprite allBeesKilledSprite;
    [SerializeField]
    Sprite defaultBeesSprite;

    [SerializeField]
    Image scriptureIcon;

    [SerializeField]
    Sprite allScripturesCollectedSprite;
    [SerializeField]
    Sprite defaultScripturesSprite;

    [SerializeField]
    Image completionIcon;

    [SerializeField]
    Sprite levelCompletedSprite;
    [SerializeField]
    Sprite defaultCompletionSprite;

    // Start is called before the first frame update
    void Start()
    {
        defaultBeesSprite = beesIcon.sprite;
        defaultScripturesSprite = scriptureIcon.sprite;
        defaultCompletionSprite = completionIcon.sprite;
    }

    public void SetFromData(LevelSaveData data)
    {
        AllBeesKilled = data.allBeesKilled;
        AllScripturesCollected = data.allScripturesCollected;
        LevelCompleted = data.levelCompleted;
        SetHighScore(data.highScore);
    }

    public bool AllBeesKilled
    {
        set
        {
            beesIcon.sprite = value ? allBeesKilledSprite : defaultBeesSprite;
        }
    }

    public bool AllScripturesCollected
    {
        set
        {
            scriptureIcon.sprite = value ? allScripturesCollectedSprite : defaultScripturesSprite;
        }
    }

    public bool LevelCompleted
    {
        set
        {
            completionIcon.sprite = value ? levelCompletedSprite : defaultCompletionSprite;
        }
    }

    public string LevelName
    {
        set
        {
            levelName.text = value;
        }
    }

    public void SetHighScore(int score) => highScore.text = $"High Score: {score.ToString($"D4")}";

    public int HighScore
    {
        set
        {
            // This is so bad but it is the quickest way to goal right now :P
            highScore.text = $"High Score: {ScoreController.instance.ScoreText}";
        }
    }
}
