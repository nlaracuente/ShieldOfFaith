using System.Collections;
using UnityEngine;

public class WinSceneController : MonoBehaviour
{
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

    void Start() => StartCoroutine(LoadSceneRoutine());

    bool isRunning = false;
    IEnumerator LoadSceneRoutine()
    {
        yield return StartCoroutine(Fader.FadeInRoutine());
    }

    public void MainMenu()
    {
        if(!isRunning)
        {
            isRunning = true;
            StartCoroutine(TransitionToHubRoutine());
        }
    }
    IEnumerator TransitionToHubRoutine()
    {
        yield return StartCoroutine(Fader.BlackoutRoutine());
        GameManager.instance.LoadScene("Hub");
    }
}
