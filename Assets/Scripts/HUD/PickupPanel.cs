using UnityEngine;
using UnityEngine.UI;

public class PickupPanel : MonoBehaviour
{
    [SerializeField] Text counter;
    Text Counter
    {
        get
        {
            if (counter == null)
                counter = GetComponentInChildren<Text>();
            return counter;
        }
    }
    public int TotalPickups { get; set; }

    private void Update() => Counter.text = $"x{TotalPickups}";
}
