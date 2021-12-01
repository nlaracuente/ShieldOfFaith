using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    [SerializeField]
    ParticleSystem winParticles;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (winParticles != null)
            winParticles.Play();

        LevelController.instance.LevelCompleted = true;
    }
}
