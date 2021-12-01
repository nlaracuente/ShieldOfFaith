using System.Collections;
using UnityEngine;

public class Torch : MonoBehaviour, IShieldCollisionHandler, IShieldEnterTriggerHandler
{
    [SerializeField]
    ParticleSystem torchParticles;

    [SerializeField]
    GameObject activatedDoor;

    [SerializeField]
    Animator doorAnimation;

    bool activated = false;
    public void OnShieldCollisionEnter(Shield shield)
    {
        if (shield.IsOnFire && !activated)
            StartCoroutine(ActivateRoutine());

        if (activated == true)
            shield.SetOnFire();
    }

    IEnumerator ActivateRoutine()
    {

        activated = true;
        doorAnimation.Play("BoneDoor");
        AudioManager.instance.Play(SFXLibrary.instance.shieldFire);

        if (activatedDoor)
            AudioManager.instance.Play(SFXLibrary.instance.door);

        if (torchParticles != null)
            torchParticles.Play();

        yield return new WaitForSeconds(1);
        Destroy(activatedDoor);
    }

    public void OnShieldEnterTrigger(Shield shield)
    {
        OnShieldCollisionEnter(shield);
    }
}
