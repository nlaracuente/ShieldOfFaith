using System.Collections;
using UnityEngine;
using System.Linq;

/// <summary>
/// What was supposed to only control the camera became the scene controller too
/// I guess this is what happens when you are trying to get things done last minute :P 
/// </summary>
public class HubCamera : Singleton<HubCamera>
{
    [SerializeField]
    GameObject mainMenu;

    [SerializeField]
    Animator animator;
    Animator Animator
    {
        get
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            return animator;
        }

    }

    [SerializeField]
    Fader fader;
    Fader Fader
    {
        get
        {
            if (fader == null)
                fader = FindObjectOfType<Fader>();
            return fader;
        }

    }

    Shield Shield { get { return GameManager.instance.Shield; } }
    Player Player { get { return GameManager.instance.Player; } }

    IEnumerator routine;

    private void Start()
    {
        StartCoroutine(StartRoutine());
    }

    IEnumerator StartRoutine()
    {
        yield return new WaitForEndOfFrame();

        // If the player came from a level then we want them to start on the HUB and on that level's starter
        var levelName = GameManager.instance.LastScene;
        var levelStarter = FindObjectsOfType<LevelStarter>().Where(l => l.LevelName == levelName).FirstOrDefault();

        if (levelStarter != null)
            StartCoroutine(StartOnLevel(levelStarter));
        else
        {
            // Show the main menu
            mainMenu.SetActive(true);
            StartCoroutine(Fader.FadeInRoutine());
        }
    }

    IEnumerator StartOnLevel(LevelStarter levelStarter)
    {
        // Wait a frame for things to load
        while (Player == null) 
            yield return new WaitForEndOfFrame();

        // Set the player, camera, and ui
        Player.transform.position = new Vector3(
            levelStarter.transform.position.x,
            Player.transform.position.y,
            levelStarter.transform.position.z
        );

        Animator.Play("HubCamera");
        mainMenu.SetActive(false);
        Shield.StartAttachedToPlayer();
        yield return new WaitForEndOfFrame();

        // Run the fader
        yield return StartCoroutine(Fader.FadeInRoutine());

        // Allow the player to move
        LevelController.instance.EnablePlayerControls();
    }

    public void StartGame()
    {
        if (routine != null)
            return;

        routine = TransitionToHubRoutine();
        StartCoroutine(routine);
    }

    IEnumerator TransitionToHubRoutine()
    {
        mainMenu.SetActive(false);
        Animator.SetTrigger("ToHub");
        AudioManager.instance.Play(SFXLibrary.instance.hubTransition);
        yield return new WaitForEndOfFrame();

        // Wait for the animation to finish
        while (!Animator.GetCurrentAnimatorStateInfo(0).IsName("HubCamera"))
            yield return new WaitForEndOfFrame();

        // Allow the player to move
        Shield.SetState(ShieldState.Thrown); // so that it can be recalled
        LevelController.instance.EnablePlayerControls();
    }

    public void LoadLevel(string levelName) => StartCoroutine(LoadLevelRoutine(levelName));
    IEnumerator LoadLevelRoutine(string levelName)
    {
        AudioManager.instance.Play(SFXLibrary.instance.hubLevelPlay);

        // Disable controls
        LevelController.instance.EnablePlayerControls(false);

        // Run the fader
        yield return StartCoroutine(Fader.BlackoutRoutine());

        // Load the level
        GameManager.instance.LoadScene(levelName);
    }
}
