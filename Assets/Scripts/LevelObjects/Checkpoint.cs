using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    float X { get { return transform.position.x; } }
    float Z { get { return transform.position.z; } }
    public string ID { get { return $"{LevelController.instance.LevelName}_{X}_{Z}"; } }
    
    [SerializeField, Tooltip("Where the camera will start at on reload")]
    Transform cameraRespawnXForm;
    public Vector3 CameraPosition { get { return cameraRespawnXForm.position; } }

    [SerializeField]
    GameObject flagModel;

    [SerializeField]
    Collider triggerCollider;

    [SerializeField]
    ParticleSystem particles;

    public bool IsChecked { get; set; }
    private void Update()
    {
        flagModel.SetActive(!IsChecked);
        triggerCollider.enabled = !IsChecked;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsChecked && other.CompareTag("Player"))
            OnCheckpointEntered();
    }

    void OnCheckpointEntered()
    {
        IsChecked = true;
        particles.Play();
        AudioManager.instance.Play(SFXLibrary.instance.checkpoint);
        LevelController.instance.Checkpoint = this;
    }
}