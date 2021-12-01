using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody), typeof(BeeMaterial))]
public class Bee : MonoBehaviour, IShieldCollisionHandler, IShieldEnterTriggerHandler, IPointerDownHandler, IPointerEnterHandler
{
    [SerializeField, Tooltip("Default speed to move at")]
    float movementSpeed = 5f;

    [SerializeField, Tooltip("How quickly it rotates to look at the player")]
    float rotationSpeed = 15f;

    [SerializeField, Tooltip("Degrees from the center of the bee to the left and right that the bee can see the player from")]
    float fieldOfView = 60f;

    [SerializeField, Tooltip("How many seconds after enemy is considered 'dead' before removing the object")]
    protected float timeBeforeRemovingEnemy = 3f;

    [SerializeField, Tooltip("True: bee can only be killed by fire")]
    bool deathByFire = false;

    [SerializeField, Tooltip("True: bee can only be killed by ice")]
    bool deathByIce = false;

    [SerializeField]
    Transform stingerSpawnXForm;
    public Transform StingerSpawnXForm { get { return stingerSpawnXForm; } }

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

    bool isDying { get; set; }
    public bool IsDead { get { return isDying || State == BeeState.Dead; } }

    protected Player Player { get { return GameManager.instance.Player; } }
    protected Shield Shield { get { return GameManager.instance.Shield; } }

    [SerializeField, Tooltip("Distance to the player before the bee can become aware of the player")]
    float awarenessDistance = 10f;

    [SerializeField] LayerMask wallLayerMask;
    public bool IsAwareOfPlayer
    {
        get
        {
            return IsPlayerWithinRange(awarenessDistance) && IsPlayerVisible();
        }
    }

    IEnemyIdleHandler idleHandler;
    IEnemyAttackHandler attackHandler;
    IEnemyMovementHandler movementHandler;

    [SerializeField] BeeState currentState = BeeState.Idle;
    public BeeState State { get { return currentState; } protected set { currentState = value; } }

    [SerializeField, Tooltip("Visibile in the editor for debugging")]
    BeeState lastState = BeeState.Idle;

    public bool OnAttackFrame { get; set; }
    public bool OnDashFrame { get; set; }

    Rigidbody rb;
    public Rigidbody Rigidbody
    {
        get
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();
            return rb;
        }
    }

    public bool IsMoving { get { return Rigidbody.velocity.magnitude > movementMagnitude; } }
    [SerializeField, Range(0f, 1f)]
    float movementMagnitude = 0.15f;

    [SerializeField, Tooltip("The collider the shield looks for when hitting the bee")]
    Collider shieldCollider;
    Collider ShieldCollider
    {
        get
        {
            if (shieldCollider == null)
                shieldCollider = GetComponent<Collider>();
            return shieldCollider;
        }
    }

    [SerializeField]
    BeeMaterial beeMaterial;
    BeeMaterial BeeMaterial
    {
        get
        {
            if (beeMaterial == null)
                beeMaterial = GetComponent<BeeMaterial>();
            return beeMaterial;
        }
    }

    [SerializeField]
    ParticleSystem smokeParticles;
    ParticleSystem SmokeParticles
    {
        get
        {
            if (smokeParticles == null)
                smokeParticles = GetComponentInChildren<ParticleSystem>();
            return smokeParticles;
        }
    }

    [SerializeField]
    BeeScoreUI beeScoreUI;
    BeeScoreUI BeeScoreUI
    {
        get
        {
            if (beeScoreUI == null)
                beeScoreUI = GetComponentInChildren<BeeScoreUI>();
            return beeScoreUI;
        }
    }

    public bool IsKillable
    {
        get
        {
            return !deathByFire || Shield.Attribute == ShieldAttribute.Fire;
        }
    }

    /// <summary>
    /// Attempts to return a constant ID for these bees
    /// Tried using the bee's position but maybe physics was moving the bees 
    /// ever so sligthly changing their ID every now and then.
    /// 
    /// So long as we never EVER duplicate the name of the gameobject this 
    /// is the closest to uniqueness we gonna get for now
    /// </summary>
    public string ID { get { return name; } }

    public BeeColor BeeColor 
    { 
        get 
        {
            var color = BeeColor.Yellow;
            if (deathByFire)
                color = BeeColor.Red;
            
            if (deathByIce)
                color= BeeColor.Blue;

            return color;
        } 
    }

    private void Start()
    {
        movementHandler = GetComponent<IEnemyMovementHandler>();
        idleHandler = GetComponent<IEnemyIdleHandler>();
        attackHandler = GetComponent<IEnemyAttackHandler>();

        // Making sure this is up to date
        SetDefaultMaterial();
        StartCoroutine(LogicRoutine());
    }

    [SerializeField] bool setColliderToTrigger;

    void Update()
    {
        // Don't do anything
        if (LevelController.instance.State != LevelState.Playing)
            return;

        // Bee's shield collider will be set to triggered when it is killable
        ShieldCollider.isTrigger = setColliderToTrigger && IsKillable;

        // Can't change the state because it is dying or we are waiting for bonk to finish
        if (IsDead || currentState == BeeState.Bonk)
            return;

        // Trigger the bee to attack since the bee is aware of the player
        if (attackHandler != null && currentState != BeeState.Attack && IsAwareOfPlayer)
            ChangeState(BeeState.Attack);
    }

    public void SetDefaultMaterial()
    {
        if (deathByFire)
            BeeMaterial.SetMaterial(BeeColor.Red);
        else if (deathByIce)
            BeeMaterial.SetMaterial(BeeColor.Blue);
        else
            BeeMaterial.SetMaterial(BeeColor.Yellow);
    }

    IEnumerator LogicRoutine()
    {
        // Wait until the level controller is in gameplay mode to handle the logic
        while(LevelController.instance.State != LevelState.Playing)
            yield return new WaitForEndOfFrame();

        // Enemy always starts in IDLE
        // Enemy switches from idle to move if they have a move handler
        // The attack handler will trigger a change to attack state when it determines it can
        // attacks should always end back to idle
        // Death trumps all other requests 
        while (!isDying)
        {
            switch (State)
            {
                case BeeState.Idle:
                    if (idleHandler != null)
                        yield return StartCoroutine(idleHandler.IdleRoutine());
                    else
                        ChangeState(BeeState.Move);
                    break;

                case BeeState.Move:
                    if (movementHandler != null)
                        yield return StartCoroutine(movementHandler.MoveRoutine());
                    else
                        ChangeState(BeeState.Idle);
                    break;

                case BeeState.Attack:
                    if (attackHandler != null)
                        yield return StartCoroutine(attackHandler.AttackRoutine());
                    else
                        ChangeState(BeeState.Idle);
                    break;

                case BeeState.Bonk:
                    yield return StartCoroutine(BonkRoutine());
                    break;

                case BeeState.Dead:
                    yield return StartCoroutine(DeathRoutine());
                    break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void ChangeState(BeeState _state)
    {
        // We are already on that state so we can ignore it
        if (currentState == _state)
            return;

        // If not already dead then we can change the state
        if (!isDying && currentState != BeeState.Dead)
        {
            lastState = currentState;
            currentState = _state;
        }
    }

    public void SetIsMovingAnimationBool(bool isMoving) => Animator.SetBool("IsMoving", isMoving);

    private void OnTriggerStay(Collider other)
    {
        if (!isDying && other.CompareTag("Player"))
            Player.TakeDamage();
    }

    public void OnShieldCollisionEnter(Shield shield) => HandleShieldCollision(shield);
    public void OnShieldEnterTrigger(Shield shield) => HandleShieldCollision(shield);
    void HandleShieldCollision(Shield shield)
    {
        // Enemy was hit by the shield when it was being recalled
        // Since it is kinematic there is no "Collision" so we use triggers
        // making sure it is not currently attached to the player
        if (!shield.IsAttached)
            Die(shield);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0))
            OnPointerDown(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isDying || gameObject == null)
            return;
    }

    /// <summary>
    /// Shield will trigger death only if the bee can bee killed by it
    /// or the shield needs to be on fire and it is on fire
    /// </summary>
    /// <param name="shield"></param>
    public void Die(Shield shield)
    {
        // Already dead
        if (State == BeeState.Dead)
            return;

        if (!IsKillable)
            ChangeState(BeeState.Bonk);
        else
        {
            ChangeState(BeeState.Dead);
            if (shield.IsOnFire)
                Burn();
        }
    }

    void Burn()
    {
        SmokeParticles.Play();
        BeeMaterial.SetMaterial(BeeColor.Black);
        AudioManager.instance.Play(SFXLibrary.instance.shieldFireOut);
    }

    public void LookAtTarget(Vector3 target)
    {
        var direction = target - transform.position;
        Rotate(direction);
    }

    /// <summary>
    /// If the bee is aware of the player it will instantly look at the player
    /// </summary>
    public void LookAtPlayer(bool forceLook = false)
    {
        if (IsAwareOfPlayer || forceLook)
        {
            var direction = Player.transform.position - transform.position;
            Rotate(direction);
        }
    }

    /// <summary>
    /// If the bee is aware of the player it will smoothly rotate towards the player last seen position
    /// </summary>
    /// <returns></returns>
    public IEnumerator LookAtPlayerRoutine()
    {
        var direction = Player.transform.position - transform.position;
        if(IsAwareOfPlayer)
            yield return StartCoroutine(RotateRoutine(direction, rotationSpeed));
    }

    public bool IsPlayerWithinRange(float range) { return Vector3.Distance(transform.position, Player.transform.position) <= range; }
    public bool IsPlayerVisible() 
    {
        var isVisible = false;

        var origin = transform.position;
        origin.y = 2.5f;

        var target = Player.transform.position;
        target.y = 2.5f;

        // Is the Bee looking in the general direction of there the player is?
        var direction = target - origin;
        var distance = Vector3.Distance(origin, target);
        var angle = Vector3.Angle(transform.forward, direction);

        // Player is within the angle, now we need to see if the view is clear
        if (angle <= fieldOfView)
        {
            // Since we never shoot the ray beyond the player we only care if there's a wall in the way
            // Debug.DrawRay(origin, direction * distance, Color.red);
            isVisible = !Physics.Raycast(origin, direction, distance, wallLayerMask);
        }

        return isVisible; 
    }

    public void Move(Vector3 destination, float speed = 0f)
    {
        if (speed < 1f)
            speed = movementSpeed;

        // We will always ignore "y" 
        destination.y = transform.position.y;

        var position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        if (Rigidbody.isKinematic)
            transform.position = position;
        else
            Rigidbody.MovePosition(position);

        SetIsMovingAnimationBool(true);
    }

    public void StopMoving()
    {
        Rigidbody.velocity = Vector3.zero;
        SetIsMovingAnimationBool(false);
    }

    #region Rotation
    /// <summary>
    /// Instantly rotates to look at the given rotation
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    public void Rotate(Vector3 direction)
    {
        // We don't want up/down rotation so we remove y
        direction.y = 0f;

        // Look rotation does not like "zero" vector so we default to identity
        var targetRotation = Quaternion.identity;
        if (direction != Vector3.zero)
            targetRotation = Quaternion.LookRotation(direction);

        if(!Rigidbody.isKinematic)
            Rigidbody.MoveRotation(targetRotation);
        else
            transform.rotation = targetRotation;
    }

    /// <summary>
    /// Set rotation to the given rotation
    /// </summary>
    /// <param name="rotation"></param>
    public void Rotate(Quaternion rotation)
    {
        if (!Rigidbody.isKinematic)
            Rigidbody.MoveRotation(rotation);
        else
            transform.rotation = rotation;
    }

    /// <summary>
    /// Rotates towards the given direction at the speed given for a single frame
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    public void Rotate(Vector3 direction, float speed)
    {
        // We don't want up/down rotation so we remove y
        direction.y = 0f;

        // Look rotation does not like "zero" vector so we default to identity
        var targetRotation = Quaternion.identity;
        if (direction != Vector3.zero)
            targetRotation = Quaternion.LookRotation(direction);

        var rot = Quaternion.Lerp(Rigidbody.rotation, targetRotation, speed * Time.fixedDeltaTime);

        if (!Rigidbody.isKinematic)
            Rigidbody.MoveRotation(rot);
        else
            transform.rotation = rot;
    }
    #endregion

    /// <summary>
    /// Keeps rotating until facing the given direction
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    public IEnumerator RotateRoutine(Vector3 direction, float speed)
    {
        // This routine will only happens while there is no other routine happening
        var stateBeforeRotate = currentState;

        // We don't want up/down rotation so we remove y
        direction.y = 0f;

        // Look rotation does not like "zero" vector so we default to identity
        var targetRotation = Quaternion.identity;
        if(direction != Vector3.zero)
            targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate
        while (stateBeforeRotate == currentState && Quaternion.Angle(transform.rotation, targetRotation) >= 0.1f)
        {
            var rot = Quaternion.Lerp(Rigidbody.rotation, targetRotation, speed * Time.fixedDeltaTime);
            Rotate(rot);
            yield return new WaitForFixedUpdate();
        }

        // Snap to rotation
        if (stateBeforeRotate == currentState)
        {
            Rotate(targetRotation);
            yield return new WaitForEndOfFrame();
        }   
    }

    /// <summary>
    /// TODO: Allow me to change the color when it is done with multiplier
    /// </summary>
    /// <param name="points"></param>
    public void ShowKillPoints(int points) => BeeScoreUI.Text = $"{points}";

    /// <summary>
    /// A bonk happens while the bee is actively engaged in a different state
    /// Once the animation is done playing and while the bee is not dead
    /// we will return to the last state
    /// </summary>
    /// <returns></returns>
    IEnumerator BonkRoutine()
    {
        // Bee should stop in its track since it this is like a "stun"
        StopMoving();

        // Play the animation and wait a frame for the state info to update
        Animator.Play("Bonk");
        AudioManager.instance.Play(SFXLibrary.instance.shieldHit);
        yield return new WaitForEndOfFrame();

        while (!IsDead && Animator.GetCurrentAnimatorStateInfo(0).IsName("Bonk"))
            yield return new WaitForEndOfFrame();

        if (!IsDead)
        {
            // Safety net
            if (lastState != BeeState.Bonk)
                ChangeState(lastState);
            else
                // Idle is always the safest state to return to when we don't know
                ChangeState(BeeState.Idle); 
        }

        yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// Bee stepped into a firepit that is ON
    /// This is guranteed death
    /// </summary>
    /// <param name="firePit"></param>
    public void Die(FirePit firePit)
    {
        if (firePit.IsOn)
            ChangeState(BeeState.Dead);
    }

    /// <summary>
    /// This is what kills the bee
    /// </summary>
    /// <returns></returns>
    IEnumerator DeathRoutine()
    {
        // Notify all listening party that bee is dead
        isDying = true;

        // Stop Moving! 
        StopMoving();
        Rigidbody.velocity = Vector3.zero;

        // Disable colliders 
        foreach (var collider in GetComponents<Collider>())
            collider.enabled = false;

        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = false;

        // Please death animation and set timer to remove the bee
        Animator.Play("Death");
        AudioManager.instance.Play(SFXLibrary.instance.shieldHit);
        AudioManager.instance.Play(SFXLibrary.instance.beeDie);

        BeeEnemyController.instance.OnBeeKilled(this);
        Destroy(gameObject, timeBeforeRemovingEnemy);
        yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// This is so that we can destroy all bees before a checkpoint
    /// </summary>
    public void DestroyBee() => Destroy(gameObject);
}
