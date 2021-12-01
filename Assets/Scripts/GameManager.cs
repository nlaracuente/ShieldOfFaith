using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField, Tooltip("Level that triggers win")]
    string lastLevelName = "Buzzy Desert";
    public string LastLevelName { get { return lastLevelName; } }

    Dictionary<string, LevelSaveData> levelSaveDatas;
    Dictionary<string, LevelSaveData> LevelSaveDatas
    {
        get
        {
            if (levelSaveDatas == null)
                levelSaveDatas = new Dictionary<string, LevelSaveData>();

            return levelSaveDatas;
        }
    }

    public LevelSaveData GetLevelSaveData(string levelName)
    {
        var defaultData = new LevelSaveData();
        defaultData.scripturesCollected = new bool[totalScripturesPerLevel];

        return LevelSaveDatas.ContainsKey(levelName) ? LevelSaveDatas[levelName] : defaultData;
    }

    public void SetLevelSaveData(string levelName, LevelSaveData data) => LevelSaveDatas[levelName] = data;

    [SerializeField, Tooltip("How many scriptures will the player collect on each level")]
    int totalScripturesPerLevel = 3;

    public bool GameOver { get; set; }
    public bool GamePaused { get; set; }
    public bool IsTransitioning { get; set; }
    public bool NotShowingMessage { get; set; } = true;
    public bool BlockButtons { get { return IsTransitioning && NotShowingMessage; } }

    public int BeesKilled { get; set; }
    public int TotalBees { get; set; }

    public string CurrentSceneName
    {
        get
        {
            return SceneManager.GetActiveScene().name;
        }
    }

    TextBox textBox;
    public TextBox TextBox
    {
        get
        {
            if (textBox == null)
                textBox = FindObjectOfType<TextBox>();
            return textBox;
        }
    }

    Player player;
    public Player Player
    {
        get
        {
            if (player == null)
                player = FindObjectOfType < Player>();
            return player;
        }
    }

    Shield shield;
    public Shield Shield
    {
        get
        {
            if (shield == null)
                shield = FindObjectOfType <Shield>();
            return shield;
        }
    }

    Camera cam;
    Camera Camera
    {
        get
        {
            if (cam == null)
                cam = Camera.main;
            return cam;
        }
    }

    /// <summary>
    /// Tracks the current checkpoint
    /// </summary>
    public Checkpoint Checkpoint 
    { 
        set
        {
            if (value == null)
                CheckpointData = default(CheckpointData);
            else
            {
                var point = value;
                CheckpointData = new CheckpointData(
                    point.ID,
                    BeeEnemyController.instance.BeesKilled,
                    LevelController.instance.LevelSaveData.scripturesCollected,
                    ScoreController.instance.Score
                );
            }
        }
    }

    public CheckpointData CheckpointData { get; protected set; }

    void Start()
    {
        if (gameObject == null)
            return;
        
        // Don't lock our cursor while we are deving ;)
        if(!Application.isEditor)
            Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        if (!Application.isMobilePlatform && Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void ReloadLevel() => ReloadScene();
    
    public Vector3 DirectionNameToVector(Direction direction)
    {
        var vector = Vector3.zero;

        switch (direction)
        {
            case Direction.North:
                vector = Vector3.forward;
                break;
            case Direction.West:
                vector = Vector3.left;
                break;
            case Direction.South:
                vector = Vector3.back;
                break;
            case Direction.East:
                vector = Vector3.right;
                break;
        }

        // Making sure to use the active camera to get a vector that matches the camera's current forward
        return Camera.transform.TransformDirection(vector);
    }

    public string LastScene { get; protected set; }
    public void LoadScene(string levelName)
    {
        LastScene = CurrentSceneName;
        SceneManager.LoadScene(levelName);
    }

    public void TriggerGameOver()
    {
        // Already did
        if (GameOver)
            return;

        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        GameOver = true;
        yield return new WaitForSeconds(1f);
        ReloadScene();
    }

    public string GetSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}
