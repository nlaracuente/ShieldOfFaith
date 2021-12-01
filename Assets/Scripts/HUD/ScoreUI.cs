using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    

    [SerializeField]
    Text counter;

    private void Start()
    {
        if (counter == null)
            counter = GetComponent<Text>();
    }

    private void Update() => counter.text = ScoreController.instance.ScoreText;
}
