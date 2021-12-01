using System.Collections;
using UnityEngine;

public class ScripturePickup : MonoBehaviour
{
    [SerializeField, Tooltip("Which scripture this is: 1, 2, or 3")]
    int scriptureNumber = 0;
    public int ScriptureNumber { get { return scriptureNumber; } }

    [SerializeField, Tooltip("How quickly to move to the player")]
    float speed = 1f;

    [SerializeField, Tooltip("Material to use to indicate is has been previously collected")]
    Material alreadyCollectedMaterial;

    [SerializeField]
    Renderer scriptureRenderer;

    [SerializeField]
    new Collider collider;
    Collider Collider
    {
        get
        {
            if (collider == null)
                collider = GetComponent<Collider>();
            return collider;
        }
    }

    Vector3 PlayerPosition 
    { 
        get 
        { 
            var player = GameManager.instance.Player;
            return new Vector3(
                player.transform.position.x,
                transform.position.y,
                player.transform.position.z
            );
        } 
    }

    bool alreadyCollected;
    bool routineRunning;

    public bool AlreadyCollected
    {
        set
        {
            alreadyCollected = value;
            if (scriptureRenderer == null)
                scriptureRenderer = GetComponentInChildren<Renderer>();

            if(alreadyCollected)
                scriptureRenderer.material = alreadyCollectedMaterial;
        }
    }

    public void OnPickedUp()
    {
        Collider.enabled = false;
        if(!routineRunning)
        {
            routineRunning = true;
            StartCoroutine(PickupRoutine());
        }
    }

    IEnumerator PickupRoutine()
    {
        AudioManager.instance.Play(SFXLibrary.instance.scripture);
        while (Vector3.Distance(transform.position, PlayerPosition) > 0.1f)
        {
            var position = Vector3.MoveTowards(transform.position, PlayerPosition, speed * Time.deltaTime);
            transform.position = position;
            yield return new WaitForEndOfFrame();
        }

        if(!alreadyCollected)
            LevelController.instance.ScriptureCollected(this);

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (routineRunning)
            return;

        if (other.CompareTag("Player") || other.CompareTag("Shield"))           
            OnPickedUp();
    }
}
