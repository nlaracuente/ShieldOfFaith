using UnityEngine;

public class FirePit : MonoBehaviour, IShieldEnterTriggerHandler, IOnButtonPressedHandler
{
    [SerializeField, Tooltip("True: the fire pit is on and emitting fire")]
    bool isOn = true;
    public bool IsOn { get { return isOn; } }

    [SerializeField]
    ParticleSystem particles;
    ParticleSystem Particles
    {
        get
        {
            if (particles == null)
                particles = GetComponentInChildren<ParticleSystem>();
            return particles;
        }
    }

    [SerializeField, Tooltip("To disable the collider that actually hurts things")]
    Collider hitboxCollider;
    Collider HitboxCollider
    {
        get
        {
            if (hitboxCollider == null)
                hitboxCollider = GetComponent<Collider>();
            return hitboxCollider;
        }
    }

    Player Player { get { return GameManager.instance.Player; } }

    private void Update()
    {
        Particles?.gameObject.SetActive(isOn);
        HitboxCollider.enabled = isOn;
    }

    /// <summary>
    /// TODO: make this into a "on Stay" once the player has iframes
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Enemy":
                var bee = other.GetComponent<Bee>();
                if (bee != null)
                    bee.Die(this);
                break;

            //case "Player":
            //    Player.TakeDamage();
            //    break;
        }
    }

    void OnTriggerStay(Collider other)
    {
          if(other.CompareTag("Player"))
            Player.TakeDamage();
    }

    public void OnShieldEnterTrigger(Shield shield)
    {
        if (isOn)
            shield.SetOnFire();
    }

    public void Toggle() => isOn = !isOn;
}
