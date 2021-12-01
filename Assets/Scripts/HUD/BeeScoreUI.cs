using UnityEngine;
using UnityEngine.UI;

public class BeeScoreUI : MonoBehaviour
{
    [SerializeField]
    Animator scoreAnimator;

    [SerializeField]
    Text counter;
    Text Counter
    {
        get
        {
            if (counter == null)
                counter = GetComponentInChildren<Text>();
            return counter;
        }
    }

    public string Text 
    { 
        set 
        { 
            if(Counter != null)
                Counter.text = value;

            scoreAnimator.Play("BeeScore");
        } 
    }

    private void Start() => Clear();
    public void Clear() => Counter.text = "";
}
