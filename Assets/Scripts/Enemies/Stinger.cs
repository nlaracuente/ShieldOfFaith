using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Stinger : MonoBehaviour
{
    [SerializeField]
    float speed = 10f;

    Rigidbody rb;
    Rigidbody Rigidbody
    {
        get
        {
            if (gameObject != null && rb == null)
                rb = GetComponent<Rigidbody>();
            return rb;
        }
    }

    Shield Shield { get { return GameManager.instance.Shield; } }
    Player Player { get { return GameManager.instance.Player; } }
    Vector3 firedDirection;
    bool isDisabled = false;

    private void FixedUpdate()
    {
        // Keep moving
        if (!isDisabled)
            SetVelocity(firedDirection);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDisabled)
            return;
        HandleTriggerAndCollision(collision.gameObject.tag);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDisabled)
            return;

        HandleTriggerAndCollision(other.tag);
    }

    void HandleTriggerAndCollision(string tag)
    {
        switch (tag)
        {
            case "Player":
                Player.TakeDamage();
                OnImpactHandler();
                break;

            case "Shield":
                // Only break if the shield is attached to the player
                if (Shield.IsAttached)
                {
                    OnImpactHandler();
                    AudioManager.instance.Play(SFXLibrary.instance.shieldRicochet);
                }
                break;

            case "Wall":
                OnImpactHandler();
                break;
        }
    }

    void SetVelocity(Vector3 direction)
    {
        if (gameObject != null)
            Rigidbody.MovePosition(Rigidbody.position + direction * speed * Time.fixedDeltaTime);
    }

    public void SetState(bool enabled)
    {
        if (enabled)
        {
            isDisabled = false;
            gameObject.SetActive(true);
        }
        else
        {
            isDisabled = true;
            firedDirection = Vector3.zero;
            Rigidbody.velocity = Vector3.zero;
            gameObject.SetActive(false);
        }
    }

    void OnImpactHandler() => DestroyProjectile();
    public void Fired(Vector3 direction) => firedDirection = direction;
    public void DestroyProjectile() => BeeEnemyController.instance.Release(this);
}
