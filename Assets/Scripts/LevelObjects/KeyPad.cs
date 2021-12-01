using UnityEngine;
using System.Linq;

public class KeyPad : MonoBehaviour
{
    [SerializeField]
    GameObject keyGO;

    [SerializeField]
    Enemy[] enemies;

    bool keySpawned = false;
    public bool KeyCollected { get; protected set; } = false;

    private void Start()
    {
        enemies = FindObjectsOfType<Enemy>();
        keyGO.SetActive(false);
    }

    private void Update()
    {
        if (keySpawned) return;
        if (enemies.Where(e => e != null).Any()) return;

        keySpawned = true;
        keyGO.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!KeyCollected && keySpawned && other.CompareTag("Player"))
            CollectKey();
    }

    void CollectKey()
    {
        KeyCollected = true;
        keyGO.SetActive(false);
    }
}
