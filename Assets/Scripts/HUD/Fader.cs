using System.Collections;
using UnityEngine;

public class Fader : MonoBehaviour
{
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

    public void Clear() => Animator.Play("Clear");
    public IEnumerator DimRoutine()
    {
        Animator.Play("Dim");
        yield return new WaitForEndOfFrame();

        while (Animator.GetCurrentAnimatorStateInfo(0).IsName("Dim"))
            yield return new WaitForEndOfFrame();
    }

    public IEnumerator BlackoutRoutine()
    {
        // if not already dimmed then fir dim
        if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Dimmed"))
            yield return StartCoroutine(DimRoutine());

        Animator.Play("Blackout");
        yield return new WaitForEndOfFrame();

        while (Animator.GetCurrentAnimatorStateInfo(0).IsName("Blackout"))
            yield return new WaitForEndOfFrame();
    }

    public IEnumerator FadeInRoutine()
    {
        Animator.Play("FadeIn");
        yield return new WaitForEndOfFrame();

        while (Animator.GetCurrentAnimatorStateInfo(0).IsName("FadeIn"))
            yield return new WaitForEndOfFrame();
    }
}
