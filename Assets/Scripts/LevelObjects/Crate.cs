using System.Collections;
using UnityEngine;

public class Crate : MonoBehaviour, IShieldCollisionHandler, IShieldEnterTriggerHandler
{
    [SerializeField]
    GameObject model;

    [SerializeField]
    ParticleSystem brokenParticles;

    [SerializeField]
    SoundEffect crateSound;

    [SerializeField]
    Collider mainCollider;

    bool destroyed = false;

    IEnumerator DestroyRoutine()
    {
        destroyed = true;

        if (crateSound != null)
            AudioManager.instance.Play(crateSound);

        if (model != null)
            model.SetActive(false);

        mainCollider.enabled = false;
        yield return new WaitForEndOfFrame();

        if (brokenParticles != null)
        {
            brokenParticles.Play();
            yield return new WaitForEndOfFrame();
            while (brokenParticles.isPlaying)
                yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }

    public void OnShieldEnterTrigger(Shield shield) => OnShieldCollisionEnter(shield);
    public void OnShieldCollisionEnter(Shield shield)
    {
        if (!destroyed)
            StartCoroutine(DestroyRoutine());
    }
}
