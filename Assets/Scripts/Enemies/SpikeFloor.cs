using UnityEngine;

[RequireComponent(typeof(Toggleable))]
public class SpikeFloor : MonoBehaviour
{
    Toggleable toggleable;
    Toggleable Toggleable
    {
        get
        {
            if (toggleable == null)
                toggleable = GetComponent<Toggleable>();
            return toggleable;
        }
    }

    Player Player { get { return GameManager.instance.Player; } }
    private void OnTriggerStay(Collider other)
    {
        // Spikes are up and player is touching them
        if (Toggleable.IsActive && other.CompareTag("Player"))
            Player.TakeDamage();
    }
}
