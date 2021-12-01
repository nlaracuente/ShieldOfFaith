using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public abstract class Enemy : MonoBehaviour, IShieldCollisionHandler, IPointerDownHandler, IPointerEnterHandler, IShieldEnterTriggerHandler
{
    [SerializeField, Tooltip("How many seconds until the animation is on the 'attack' frame")]
    protected float timeBeforeAttack = 2.5f;
    public float TimeBeforeAttack { get { return timeBeforeAttack; } }

    [SerializeField, Tooltip("How many seconds between attacks while the player is in range (including the time for the attack to complete)")]
    protected float timeBetweenAttacks = 4f;
    public float AttackCooldownTime { get { return timeBetweenAttacks; } }

    [SerializeField, Tooltip("How many seconds after enemy is considered 'dead' before removing the object")]
    protected float timeBeforeRemovingEnemy = 3f;

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

    [SerializeField]
    new Collider collider;
    protected Collider Collider
    {
        get
        {
            if (collider == null)
                collider = GetComponent<Collider>();
            return collider;
        }
    }

    [SerializeField]
    BeeScoreUI targetMarker;
    protected BeeScoreUI TargetMarker
    {
        get
        {
            if (targetMarker == null)
                targetMarker = GetComponentInChildren<BeeScoreUI>();
            return targetMarker;
        }
    }

    public bool IsDead { get; protected set; }
    protected Player Player { get { return GameManager.instance.Player; } }
    protected Shield Shield { get { return GameManager.instance.Shield; } }

    public bool PlayerInRange { get; private set; }

    private void Start()
    {
        ClearMarker();
        StartCoroutine(EnemeyRoutine());
    }

    protected virtual void Update()
    {
        // Look at player
        if (!IsDead && PlayerInRange)
            transform.LookAt(Player.transform);
    }

    protected abstract IEnumerator EnemeyRoutine();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            PlayerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            PlayerInRange = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsDead && collision.gameObject.CompareTag("Player"))
            Player.TakeDamage();
    }

    public void OnShieldCollisionEnter(Shield shield) => Die();

    private void Die()
    {
        IsDead = true;
        ClearMarker();
        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0))
            OnPointerDown(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsDead || gameObject == null)
            return;

        Shield.AddTarget(this);
    }

    public void SetTargetNumber(int n) => TargetMarker.Text = $"{n}";
    public void SetMarker(string s) => TargetMarker.Text = s;
    public void ClearMarker() => TargetMarker.Clear();

    public void OnShieldEnterTrigger(Shield shield)
    {
        // Enemy was hit by the shield when it was being recalled
        // Since it is kinematic there is no "Collision" so we use triggers
        // making sure it is not currently attached to the player
        if (shield.IsRecalled) Die();
    }
}
