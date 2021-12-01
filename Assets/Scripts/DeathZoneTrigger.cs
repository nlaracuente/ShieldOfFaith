using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeathZoneTrigger : MonoBehaviour
{
    [SerializeField, Tooltip("To ensure the model is disabled during gameplay")]
    MeshRenderer meshRenderer;

    Player Player { get { return GameManager.instance.Player; } }

    private void Start()
    {
        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Player.InstantDeath();
    }
}