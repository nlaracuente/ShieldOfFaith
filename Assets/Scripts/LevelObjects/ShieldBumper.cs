using UnityEngine;

public class ShieldBumper : MonoBehaviour, IShieldCollisionHandler
{
    [SerializeField]
    Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            return animator;
        }
    }

    public void OnShieldCollisionEnter(Shield shield)
    {
        Animator.Play("Bounce");
        AudioManager.instance.Play(SFXLibrary.instance.bumper);
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    var shield = collision.gameObject.GetComponent<Shield>();
    //    if (shield != null)
    //    {
    //        Animator.Play("Bounce");
    //        AudioManager.instance.Play(SFXLibrary.instance.bumper);
    //    }
    //}
}
