using UnityEngine;

public class BeeAnimatorListener : MonoBehaviour
{
    [SerializeField]
    Bee bee;
    Bee Bee
    {
        get
        {
            if (bee == null)
                bee = GetComponentInParent<Bee>();

            return bee;
        }
    }

    public void OnAttackFrame() => Bee.OnAttackFrame = true;
    public void OnDashFrame() => Bee.OnDashFrame = true;
    public void PlayChargeAttackSFX() => AudioManager.instance.Play(SFXLibrary.instance.beeAttack);
}
