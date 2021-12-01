using UnityEngine;

public class LevelStarter : MonoBehaviour
{
    [SerializeField, Tooltip("The level this starter loads")]
    string levelName;
    public string LevelName { get { return levelName; } }

    [SerializeField, Tooltip("Level Model's animator so that we can make it rotate")]
    Animator levelModelAnimator;

    [SerializeField, Tooltip("Dot animation")]
    Animator dotModelAnimator;

    [SerializeField, Tooltip("The local UI panel to load the level info on")]
    CollectiblesMenuPanel panel;

    [SerializeField, Tooltip("Canvas to turn off when the level is not actie")]
    GameObject levelCanvas;

    bool isActive = false;

    private void Start()
    {
        HideLevelInfo();
    }

    private void Update()
    {
        if (isActive && Input.GetKeyDown(KeyCode.Space))
            HubCamera.instance.LoadLevel(levelName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            AudioManager.instance.Play(SFXLibrary.instance.hubLevelHover);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
            ShowLevelInfo();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            HideLevelInfo();
    }

    void ShowLevelInfo()
    {
        isActive = true;
        levelModelAnimator.SetBool("IsActive", true);
        dotModelAnimator.SetBool("IsActive", true);

        var data = GameManager.instance.GetLevelSaveData(levelName);
        levelCanvas.gameObject.SetActive(true);
        panel.LevelName = levelName;
        panel.SetFromData(data);
    }

    void HideLevelInfo()
    {
        isActive = false;
        levelModelAnimator.SetBool("IsActive", false);
        dotModelAnimator.SetBool("IsActive", false);
        levelCanvas.gameObject.SetActive(false);
    }
}
