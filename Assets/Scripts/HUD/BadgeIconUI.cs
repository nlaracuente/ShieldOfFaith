using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BadgeIconUI : MonoBehaviour
{
    [SerializeField, Tooltip("Image to show when the badge is earned")]
    Sprite earnedSprite;

    [SerializeField]
    Text counter;
    public Text Counter
    {
        get
        {
            if (counter == null)
                counter = GetComponentInChildren<Text>();
            return counter;
        }
    }
    public string CounterText { set { Counter.text = value; } }

    [SerializeField]
    Image icon;
    public Image Icon
    {
        get
        {
            if (icon == null)
                icon = GetComponentInChildren<Image>();
            return icon;
        }
    }

    [SerializeField]
    Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            return animator;
        }
    }

    private void Start() => CounterText = "";
    public void SetIconToEarned() => Icon.sprite = earnedSprite;
    public void PlayEarnedAnimation() => Animator.Play("Earned");

    public IEnumerator PlayEarnedAnimationRoutine()
    {
        SetIconToEarned();
        PlayEarnedAnimation();
        yield return new WaitForEndOfFrame();
        while(Animator.GetCurrentAnimatorStateInfo(0).IsName("Earned"))
            yield return new WaitForEndOfFrame();
    }
}
