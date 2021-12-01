using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class MenuController : Singleton<MenuController>
{
    [Header("Containers")]
    [SerializeField] RectTransform background;
    [SerializeField] RectTransform levelTitle;
    [SerializeField] RectTransform collectibles;
    [SerializeField] RectTransform buttons;
    [SerializeField] RectTransform gameOver;
    [SerializeField] RectTransform levelCompleted;
    [SerializeField] RectTransform verse;

    [Header("Interactibles")]
    [SerializeField] Fader fader;
    [SerializeField] Text verseText;
    [SerializeField] Text scoreText;
    [SerializeField] BadgeIconUI beesBadge;
    [SerializeField] BadgeIconUI scriptureBadge;
    [SerializeField] BadgeIconUI levelCompletedBadge;

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

    /// <summary>
    /// Restart the level from the beginning so we want to clear any checkpoints
    /// </summary>
    public void RestartLevel()
    {
        LevelController.instance.Checkpoint = null;
        LevelController.instance.RestartLevel(true); // runs the fader
    }
    public void ExitLevel() => LevelController.instance.ExitLevel();

    /// <summary>
    /// Hide all gameover and button related stuff
    /// </summary>
    private void Start()
    {
        buttons.gameObject.SetActive(false);
        gameOver.gameObject.SetActive(false);
        levelCompleted.gameObject.SetActive(false);
        verse.gameObject.SetActive(false);
    }

    public void SetBadges(bool beesEarned, bool scriptureEarned, bool levelCompletedEarned)
    {
        if (beesEarned)
            beesBadge.SetIconToEarned();

        if (scriptureEarned)
            scriptureBadge.SetIconToEarned();

        if (levelCompletedEarned)
            levelCompletedBadge.SetIconToEarned();
    }

    public IEnumerator LevelStartTransition(float transitionTime, float transitionDelay = 1f)
    {
        // Ensure the backgrounds are in the middle
        background.anchoredPosition = LevelTransition.StretchedCenterAnchor;

        // Disable the fader
        fader.gameObject.SetActive(true);
        fader.Clear();
        yield return new WaitForEndOfFrame();

        // Slide the level title/collectible info from the right side to the center
        StartCoroutine(TransitionRightToMiddle(levelTitle, transitionTime));
        StartCoroutine(TransitionRightToMiddle(collectibles, transitionTime));
        yield return new WaitForSeconds(transitionTime);

        // Wait before we transition out
        yield return new WaitForSeconds(transitionDelay);

        // Slide everything (including bgs) out ot the left
        StartCoroutine(TransitionMiddleToLeft(levelTitle, transitionTime));
        StartCoroutine(TransitionMiddleToLeft(collectibles, transitionTime));
        StartCoroutine(TransitionMiddleToLeft(background, transitionTime));
        yield return new WaitForSeconds(transitionTime);
    }

    public IEnumerator LevelRestartTransition(float transitionDelay = 1f)
    {
        // Get the items we don't need out of the way
        background.anchoredPosition = LevelTransition.StretchedLeftAnchor(background);
        collectibles.anchoredPosition = LevelTransition.StretchedLeftAnchor(collectibles);
        levelTitle.anchoredPosition = LevelTransition.StretchedLeftAnchor(levelTitle);

        // Enable all we need and ensure the fader is in position
        fader.gameObject.SetActive(true);
        gameOver.gameObject.SetActive(false);

        // Wait before we transition 
        yield return new WaitForSeconds(transitionDelay);
        yield return StartCoroutine(fader.FadeInRoutine());
    }

    public IEnumerator OpenMenuRoutine(float transitionTime)
    {
        // Make sure the fader is not in the way or else we cannot click anything
        fader.gameObject.SetActive(false);

        // Slide the level title/collectible info from the right side to the center
        StartCoroutine(TransitionDownToMiddle(background, transitionTime));
        StartCoroutine(TransitionDownToMiddle(levelTitle, transitionTime));
        StartCoroutine(TransitionDownToMiddle(collectibles, transitionTime));

        // Ensure the buttons are visible
        buttons.gameObject.SetActive(true);

        // Since opening the menu causes Time.timescale = 0
        // We simply use one of the transitions to wait since "WaitForSeconds" will not run
        yield return StartCoroutine(TransitionDownToMiddle(buttons, transitionTime));
    }

    public IEnumerator CloseMenuRoutine(float transitionTime)
    {
        // Slide the level title/collectible info from the right side to the center
        StartCoroutine(TransitionMiddleToDown(background, transitionTime));
        StartCoroutine(TransitionMiddleToDown(levelTitle, transitionTime));
        StartCoroutine(TransitionMiddleToDown(collectibles, transitionTime));

        // Since opening the menu causes Time.timescale = 0
        // We simply use one of the transitions to wait since "WaitForSeconds" will not run
        yield return StartCoroutine(TransitionMiddleToDown(buttons, transitionTime));
    }

    public IEnumerator GameOverTransition(float transitionTime)
    {
        // Enable all we need and ensure the fader is in position
        fader.gameObject.SetActive(true);
        gameOver.gameObject.SetActive(true);

        // The dim is faster than the text so we only need to track the text
        StartCoroutine(TransitionUpToMiddle(gameOver, transitionTime));        

        // Since opening the menu causes Time.timescale = 0
        // We simply use one of the transitions to wait since "WaitForSeconds" will not run
        yield return StartCoroutine(fader.DimRoutine());

        // Finish fading out
        yield return StartCoroutine(fader.BlackoutRoutine());
    }

    public IEnumerator LevelCompletedRoutine(float transitionTime)
    {
        levelCompleted.gameObject.SetActive(true);

        // Since opening the menu causes Time.timescale = 0
        // We simply use one of the transitions to wait since "WaitForSeconds" will not run
        // Show the level completed
        yield return StartCoroutine(TransitionDownToMiddle(levelCompleted, transitionTime));
    }

    public IEnumerator ShowResultsRoutine(float transitionTime, float counterTime = 3f, bool newHighScore = false, string scripture = "")
    {
        // Scroll in the BG and collectibles
        StartCoroutine(TransitionLeftToMiddle(background, transitionTime));
        yield return StartCoroutine(TransitionLeftToMiddle(collectibles, transitionTime));

        // Show the new high score if we got one
        if (newHighScore && scoreText != null)
            scoreText.text = $"New! High Score: {ScoreController.instance.ScoreText}";

        // Show the results for each one, one by one
        // We will skip the badges the player already has

        // Level Completed Badge
        if (!LevelController.instance.LevelSaveData.levelCompleted)
        {
            AudioManager.instance.Play(SFXLibrary.instance.badge);
            yield return StartCoroutine(levelCompletedBadge.PlayEarnedAnimationRoutine());
        }

        // Bees Badge
        if (!LevelController.instance.LevelSaveData.allBeesKilled)
        {
            var total  = GameManager.instance.TotalBees;
            var killed = BeeEnemyController.instance.BeesKilled.Count;
            var remaining = total - killed;

            // Tick the counter down until it reaches remainder
            var speed = counterTime / total;

            var counter = total;
            while(counter >= remaining)
            {
                beesBadge.CounterText = $"{counter--}";
                yield return new WaitForSeconds(speed);
            }                

            // Badge earned
            if (remaining == 0)
            {
                // Clear the text since there are none remaining
                beesBadge.CounterText = "";
                AudioManager.instance.Play(SFXLibrary.instance.badge);
                yield return StartCoroutine(beesBadge.PlayEarnedAnimationRoutine());
            }   
        }

        // Scripture Badge
        if (!LevelController.instance.LevelSaveData.allScripturesCollected)
        {
            var missed = LevelController.instance.LevelSaveData.scripturesCollected.Where(s => !s).Any();
            // Badge earned
            if (!missed)
            {
                AudioManager.instance.Play(SFXLibrary.instance.badge);
                yield return StartCoroutine(scriptureBadge.PlayEarnedAnimationRoutine());
                yield return new WaitForSeconds(1f);
            }   
        }

        // Show the verse
        verseText.text = scripture;
        verse.gameObject.SetActive(true);
    }

    public IEnumerator FadeOutRotuine()
    {
        // Enable all we need and ensure the fader is in position
        fader.gameObject.SetActive(true);

        // Finish fading out
        yield return StartCoroutine(fader.BlackoutRoutine());
    }

    IEnumerator TransitionRightToMiddle(RectTransform rect, float time)
    {
        yield return StartCoroutine(
            LevelTransition.TransitionRoutine(
                rect,
                LevelTransition.StretchedRightAnchor(rect),
                LevelTransition.StretchedCenterAnchor,
                time
            )
        );
    }

    IEnumerator TransitionLeftToMiddle(RectTransform rect, float time)
    {
        yield return StartCoroutine(
            LevelTransition.TransitionRoutine(
                rect,
                LevelTransition.StretchedLeftAnchor(rect),
                LevelTransition.StretchedCenterAnchor,
                time
            )
        );
    }

    IEnumerator TransitionMiddleToLeft(RectTransform rect, float time)
    {
        yield return StartCoroutine(
            LevelTransition.TransitionRoutine(
                rect,
                LevelTransition.StretchedCenterAnchor,
                LevelTransition.StretchedLeftAnchor(rect),
                time
            )
        );
    }

    IEnumerator TransitionDownToMiddle(RectTransform rect, float time)
    {
        yield return StartCoroutine(
            LevelTransition.TransitionRoutine(
                rect,
                LevelTransition.StretchedDownAnchor(rect),
                LevelTransition.StretchedCenterAnchor,                
                time
            )
        );
    }

    IEnumerator TransitionMiddleToDown(RectTransform rect, float time)
    {
        yield return StartCoroutine(
            LevelTransition.TransitionRoutine(
                rect,                
                LevelTransition.StretchedCenterAnchor,
                LevelTransition.StretchedDownAnchor(rect),
                time
            )
        );
    }

    IEnumerator TransitionUpToMiddle(RectTransform rect, float time)
    {
        yield return StartCoroutine(
            LevelTransition.TransitionRoutine(
                rect,                
                LevelTransition.StretchedUpAnchor(rect),
                LevelTransition.StretchedCenterAnchor,
                time
            )
        );
    }
}
