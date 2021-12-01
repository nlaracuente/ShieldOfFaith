using System.Collections;
using UnityEngine;

public class HiveRed : MonoBehaviour, IShieldCollisionHandler, IShieldEnterTriggerHandler
{
    [SerializeField]
    GameObject hiveModel;

    [SerializeField]
    ParticleSystem brokenParticles;

    [SerializeField]
    Collider mainCollider;

    [SerializeField]
    float timeBeforeDestroying = 1.5f;

    [SerializeField]
    Animator animator;
    Animator Animator
    {
        get
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            return animator;
        }
    }

    bool destroyed = false;
    public void OnShieldCollisionEnter(Shield shield)
    {

        if (!shield.IsOnFire)
            StartCoroutine(HitRoutine());

        else if (!destroyed)
            StartCoroutine(DestroyRoutine());
    }

    IEnumerator HitRoutine()
    {
        Animator.Play("Hit");
        AudioManager.instance.Play(SFXLibrary.instance.hiveHit);
        yield return new WaitForEndOfFrame();

        while (Animator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            yield return new WaitForEndOfFrame();
    }

    IEnumerator DestroyRoutine()
    {
        destroyed = true;
        AudioManager.instance.Play(SFXLibrary.instance.hiveDestroy);
        yield return new WaitForEndOfFrame();

        if (hiveModel != null)
            hiveModel.SetActive(false);

        mainCollider.enabled = false;

        if (brokenParticles != null)
            brokenParticles.Play();

        Destroy(gameObject, timeBeforeDestroying);
    }

    public void OnShieldEnterTrigger(Shield shield)
    {
        OnShieldCollisionEnter(shield);
    }
}
