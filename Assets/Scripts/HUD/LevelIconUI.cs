using UnityEngine;
using UnityEngine.UI;

public class LevelIconUI : MonoBehaviour
{
    [SerializeField]
    Sprite notEnabledSprite;

    [SerializeField]
    Sprite enabledSprite;

    [SerializeField]
    Image image;

    public void SetStatus(bool enabled) => image.sprite = enabled ? enabledSprite : notEnabledSprite;
}
