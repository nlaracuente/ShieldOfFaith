using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text;

public class LevelController : Singleton<LevelController>
{
    [SerializeField]
    string levelName;
    public string LevelName { get { return levelName; } }

    [SerializeField]
    AudioClip music;

    [SerializeField]
    LevelState state;
    public LevelState State { get { return state; } }

    [SerializeField, Tooltip("How long to take to slide the menu in/out")]
    float transitionTime = 1f;

    [SerializeField, Tooltip("How long after the menus slide before transitioning again")]
    float transitionDelay = .5f;

    [SerializeField,
     Tooltip("How many seconds to wait after the transition into the level is done before the camera begins moving")]
    float cameraStartMoveDelay = .5f;

    [SerializeField, Tooltip("How many seconds to count down total bees remaining during end sequence")]
    float beeCounterTime = 3f;

    Player Player { get { return GameManager.instance.Player; } }
    Shield Shield { get { return GameManager.instance.Shield; } }
    public bool PlayerDied { get { return Player.Faith <= 0; } }
    public bool LevelCompleted { set; protected get; }
    public bool IsGameplayOver { get { return PlayerDied || LevelCompleted; } }

    LevelSaveData saveData;
    public LevelSaveData LevelSaveData { get { return saveData; } }

    CollectiblesMenuPanel collectiblesMenu;
    CollectiblesMenuPanel CollectiblesMenu
    {
        get
        {
            if (collectiblesMenu == null)
                collectiblesMenu = FindObjectOfType<CollectiblesMenuPanel>();
            return collectiblesMenu;
        }
    }

    LevelTransition levelTransition;
    LevelTransition LevelTransition
    {
        get
        {
            if (levelTransition == null)
                levelTransition = FindObjectOfType<LevelTransition>();
            return levelTransition;
        }
    }

    CameraRigController cameraRig;
    CameraRigController CameraRig
    {
        get
        {
            if (cameraRig == null)
                cameraRig = FindObjectOfType<CameraRigController>();
            return cameraRig;
        }
    }

    public bool InGamePlayMode { get; protected set; }
    public bool MenuIsOpened { get; protected set;  }

    IEnumerator menuRoutine = null;

    public Checkpoint Checkpoint { set { GameManager.instance.Checkpoint = value; } }

    [SerializeField, Tooltip("A verse split into 3 lines each representing a specific scripture pickup")]
    string[] verses;

    void Start()
    {
        // For the main menu we just need to start the music
        if (levelName == "MainMenu")            
            AudioManager.instance.PlayMusic(music);
        else
            StartCoroutine(LoadLevelRoutine());
    }

    void Update()
    {
        if (levelName == "MainMenu")
            return;

        if (Input.GetKeyDown(KeyCode.Space) && menuRoutine == null)
        {
            if (State == LevelState.Playing)
                menuRoutine = OpenMenuRoutine();
            else if (State == LevelState.Paused)
                menuRoutine = CloseMenuRoutine();

            if (menuRoutine != null)
                StartCoroutine(menuRoutine);
        }
    }

    public void EnablePlayerControls(bool enabled = true)
    {
        state = enabled ? LevelState.Playing : LevelState.Paused;
    }

    public bool HasCollectedScripture(int num)
    {
        var scrtipures = saveData.scripturesCollected;
        if (num >= 0 && num < scrtipures.Length)
            return scrtipures[num];

        return false;
    }

    public void ScriptureCollected(ScripturePickup scripture)
    {
        var num = scripture.ScriptureNumber - 1; // zero base counting
        if (num >= 0 && num < saveData.scripturesCollected.Length)
            saveData.scripturesCollected[num] = true;
    }

    IEnumerator OpenMenuRoutine()
    {
        // Pause the game including audio/visuals
        state = LevelState.Paused;
        Time.timeScale = 0;
        AudioManager.instance.PauseSFXs();

        // Stops gameplay logic while the menu is opened
        MenuIsOpened = true;

        // Play the menu open transition
        yield return StartCoroutine(MenuController.instance.OpenMenuRoutine(transitionTime));
        
        // Allows the menu to be closed again
        menuRoutine = null;
    }

    IEnumerator CloseMenuRoutine()
    {
        // Play the menu close transition
        yield return StartCoroutine(MenuController.instance.CloseMenuRoutine(transitionTime));
        
        // Resume audio/visuals and gameplay
        Time.timeScale = 1;
        AudioManager.instance.ResumeSFXs();
        state = LevelState.Playing;
        MenuIsOpened = false;

        // Allows the menu to be opened again
        menuRoutine = null;
    }

    IEnumerator LoadLevelRoutine()
    {
        state = LevelState.Loading;

        // In case this was reload we want to ensure time is running again
        Time.timeScale = 1;

        // Get the level info and set level specific defaults
        saveData = GameManager.instance.GetLevelSaveData(levelName);
        LevelCompleted = false;
        MenuIsOpened = false;
        GameManager.instance.GameOver = false;
        GameManager.instance.BeesKilled = 0;
        yield return new WaitForEndOfFrame();

        // Now that we've allow a frame for all objects to be created
        // Let's finished setting up the level info
        GameManager.instance.TotalBees = BeeEnemyController.instance.TotalBees;
        CollectiblesMenu.LevelName = levelName;
        CollectiblesMenu.HighScore = saveData.highScore;
        yield return new WaitForEndOfFrame();

        // When we have a checkpoint that means we want to restart at that position
        // So we need to setup the player and camera position at the checkpoint
        // Remove everything all bees before the checkpoint to prevent double counting a bee
        // Reset bee counter to whatever it was when the checkpoint was touched
        Checkpoint checkpoint = null;
        var checkpointData = GameManager.instance.CheckpointData;
        if(checkpointData.hasData)
        {
            checkpoint = FindObjectsOfType<Checkpoint>().Where(c => c.ID == checkpointData.checkpointId).FirstOrDefault();
            if(checkpoint != null)
            {
                // Before we move the player we want to disable the checkpoint so that the player doesn't touch it again
                checkpoint.IsChecked = true;

                // Restore saved values 
                // Making closes on the list/array to avoid creating a reference
                // that then modifies the cached data
                GameManager.instance.BeesKilled = checkpointData.beesKilled.Count;
                BeeEnemyController.instance.BeesKilled = new List<string>(checkpointData.beesKilled);
                Array.Copy(checkpointData.scripturesCollected, saveData.scripturesCollected, checkpointData.scripturesCollected.Length);
                ScoreController.instance.SetScore(checkpointData.totalScore);
                yield return new WaitForEndOfFrame();

                // Update bees/scriptures already killed/collected or not longer in range
                BeeEnemyController.instance.OnLevelStartFromCheckpoint(checkpoint);
                yield return new WaitForEndOfFrame();

                // Position the player/camera at the checkpoint
                // Doing the same with the shield until we make it so that it can start attached
                Shield.transform.position = new Vector3(
                    checkpoint.transform.position.x,
                    Shield.transform.position.y,
                    checkpoint.transform.position.z
                );

                Player.transform.position = new Vector3(
                    checkpoint.transform.position.x,
                    Player.transform.position.y,
                    checkpoint.transform.position.z
                );

                // Since the camera only moves on the "X" position
                // We only want to know the X value
                // NOTE: be nice to have a solution regardless of which direction the camera moves
                CameraRig.transform.position = new Vector3(
                    checkpoint.CameraPosition.x,
                    CameraRig.transform.position.y,
                    CameraRig.transform.position.z
                );
            }
        }

        // Now that we have all the info about the level
        // Let's tell all the collectibles if they have been picked up already
        // TODO: Change this to invoke a message to any object that cares if the level has been loaded
        foreach (var scripture in FindObjectsOfType<ScripturePickup>())
            scripture.AlreadyCollected = HasCollectedScripture(scripture.ScriptureNumber - 1); // 0 based counting

        // Set the badges for when we load the menu
        MenuController.instance.SetBadges(saveData.allBeesKilled, saveData.allScripturesCollected, saveData.levelCompleted);

        // Play the level music
        AudioManager.instance.PlayMusic(music);

        if (checkpoint != null)
            yield return StartCoroutine(MenuController.instance.LevelRestartTransition(transitionDelay));
        else
            yield return StartCoroutine(MenuController.instance.LevelStartTransition(transitionTime, transitionDelay));

        // Start gameplay routine
        StartCoroutine(GameplayRoutine());
    }

    IEnumerator GameplayRoutine()
    {
        state = LevelState.Playing;

        // We don't want the camera to immedeatly start moving
        // to allow time for the player to gain control of their character
        // and get a feel for the gameplay before the camera beings moving
        yield return new WaitForSeconds(cameraStartMoveDelay);
        CameraRig.MoveCamera = true;

        while (!IsGameplayOver)
        {
            // Wait for pause menu
            while (MenuIsOpened)
                yield return new WaitForEndOfFrame();

            yield return new WaitForEndOfFrame();
        }

        if (PlayerDied)
            StartCoroutine(GameOverRoutine());
        else
            StartCoroutine(LevelCompletedRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        state = LevelState.GameOver;
        CameraRig.MoveCamera = false;
        Shield.DisableShield();
        AudioManager.instance.StopSFXs();
        yield return new WaitForEndOfFrame();

        // While the death animation plays we want to play the game over transition
        Player.PlayDeathAnimation();
        var deathSrc = AudioManager.instance.Play(SFXLibrary.instance.playerDie);
        yield return StartCoroutine(MenuController.instance.GameOverTransition(transitionTime));

        deathSrc.Stop();

        RestartLevel(false);
        yield return new WaitForEndOfFrame();
    }

    IEnumerator LevelCompletedRoutine()
    {
        state = LevelState.Completed;

        // Snap the camera in place in case the player reached the ending before the camera did
        CameraRig.MoveCamera = false;
        CameraRig.SnapToEnd();

        // Remove shield and play celebration
        // While making the "Level Completed" text drop
        Shield.DisableShield();
        AudioManager.instance.StopSFXs();
        AudioManager.instance.PlayMusic(MusicLibrary.instance.victoryMusic);
        StartCoroutine(MenuController.instance.LevelCompletedRoutine(transitionTime));
        yield return StartCoroutine(Player.PlayVictoryAnimationRoutine());

        // Save the high score when it is higher than the previous
        var newHighScore = saveData.highScore < ScoreController.instance.Score;
        if (newHighScore)
            saveData.highScore = ScoreController.instance.Score;

        // Build the verse
        var secondsToRead = 0f;
        var scripture = new StringBuilder();
        for (int i = 0; i < verses.Length; i++)
        {
            var verse = verses[i];
            if (!HasCollectedScripture(i))
                verse = new string('_', verse.Length);
            else
                secondsToRead++; // increase the time to read since we have a line to read

            scripture.AppendLine(verse);
        }

        // Transition to results
        yield return StartCoroutine(MenuController.instance.ShowResultsRoutine(transitionTime, beeCounterTime, newHighScore, scripture.ToString()));

        // Clear checkpoints since the level is completed
        GameManager.instance.Checkpoint = null;

        // Permanently store the results
        var killedAllBees = BeeEnemyController.instance.BeesKilled.Count >= GameManager.instance.TotalBees;
        saveData.allBeesKilled = saveData.allBeesKilled ? true : killedAllBees;
        saveData.allScripturesCollected = saveData.allScripturesCollected ? true : !saveData.scripturesCollected.Where(s => !s).Any();
        saveData.levelCompleted = true;

        GameManager.instance.SetLevelSaveData(levelName, saveData);

        // Give some time to read the scripture
        if (secondsToRead < 1) secondsToRead = 1; // Put a little wait
        yield return new WaitForSeconds(secondsToRead);

        // Fade to Hub
        if (levelName == GameManager.instance.LastLevelName)
            yield return StartCoroutine(TransitionToScene("Win")); // Game won
        else
            yield return StartCoroutine(TransitionToScene("Hub"));
    }

    public void RestartLevel(bool withTransition = true)
    {
        // Wait until the menu has finished transitioning
        if (menuRoutine != null)
            return;

        if (!withTransition)
            GameManager.instance.ReloadLevel();
        else
        {
            // Want the fader to do its thing
            var scene = GameManager.instance.GetSceneName();
            StartCoroutine(TransitionToScene(scene));
        }            
    }

    public void ExitLevel()
    {
        // Wait until the menu has finished transitioning
        if (menuRoutine != null)
            return;

        StartCoroutine(TransitionToScene("Hub"));
    }

    IEnumerator TransitionToScene(string scene)
    {
        state = LevelState.Transitioning;
        Shield.DisableShield();
        AudioManager.instance.StopSFXs();
        yield return new WaitForEndOfFrame();

        // Make sure this didn't come from a menu which would've paused everything
        Time.timeScale = 1f;

        // Fade to Hub
        yield return StartCoroutine(MenuController.instance.FadeOutRotuine());
        GameManager.instance.LoadScene(scene);
    }
}
