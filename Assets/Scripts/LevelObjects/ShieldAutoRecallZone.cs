using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShieldAutoRecallZone : MonoBehaviour, IShieldEnterTriggerHandler
{
    [SerializeField, Tooltip("To ensure the model is disabled during gameplay")]
    MeshRenderer meshRenderer;

    private void Start()
    {
        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }

    public void OnShieldEnterTrigger(Shield shield) => shield.Poof();
}
