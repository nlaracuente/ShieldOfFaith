using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTransition : MonoBehaviour
{
    [SerializeField]
    float transitionTime = 2f;

    [SerializeField]
    float transitionDelay = 1f;

    [SerializeField]
    RectTransform levelTransitionRect;

    [SerializeField]
    RectTransform levelInfoRect;

    public Vector2 StretchedLeftAnchor(RectTransform rectTransform)
    {
        return new Vector2(-rectTransform.rect.width, 0f);
    }

    public Vector2 StretchedRightAnchor(RectTransform rectTransform)
    {
        return new Vector2(rectTransform.rect.width, 0f);
    }

    public Vector2 StretchedUpAnchor(RectTransform rectTransform)
    {
        return new Vector2(0f, rectTransform.rect.height);
    }

    public Vector2 StretchedDownAnchor(RectTransform rectTransform)
    {
        return new Vector2(0f, -rectTransform.rect.height);
    }

    public Vector2 StretchedCenterAnchor { get { return Vector2.zero; } }

    private void Start()
    {
        if (levelTransitionRect == null)
            levelTransitionRect = GetComponent<RectTransform>();
    }

    //public IEnumerator LevelIntroTransitionRoutine()
    //{
    //    yield return StartCoroutine(TransitionIn());
    //    yield return new WaitForSeconds(transitionDelay);
    //    yield return StartCoroutine(TransitionOut());
    //}

    //IEnumerator TransitionIn()
    //{
    //    yield return StartCoroutine(
    //       TransitionRoutine(
    //           levelInfoRect,
    //           StretchedRightAnchor(levelInfoRect),
    //           CenterAnchor,
    //           transitionTime
    //       )
    //   );
    //}

    //IEnumerator TransitionOut()
    //{
    //    yield return StartCoroutine(
    //       TransitionRoutine(
    //           levelTransitionRect,
    //           CenterAnchor,
    //           StretchedLeftAnchor(levelInfoRect),
    //           transitionTime
    //       )
    //   );
    //}

    public IEnumerator TransitionRoutine(RectTransform target, Vector2 origin, Vector2 destination, float time)
    {
        // pop the target at the desired origin
        target.anchoredPosition = origin;
        yield return new WaitForEndOfFrame();

        // calculate speed
        float distance = Vector2.Distance(origin, destination);
        var speed = distance / time;

        // move
        do
        {
            target.anchoredPosition = Vector2.MoveTowards(target.anchoredPosition, destination, speed * Time.unscaledDeltaTime);
            yield return new WaitForEndOfFrame();
            distance = Vector2.Distance(target.anchoredPosition, destination);
        } while (distance > 0.01f);
    }
}
