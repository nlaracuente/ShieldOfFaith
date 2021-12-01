using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Shield : MonoBehaviour
{
    [SerializeField, Tooltip("State of the shield")]
    ShieldState state = ShieldState.Disabled;
    public ShieldState State { get { return state; } set { state = value; } }

    [SerializeField, Tooltip("Additional attributes like being on fire")]
    ShieldAttribute attribute;
    public ShieldAttribute Attribute { get { return attribute; } set { attribute = value; } }

    [SerializeField, Tooltip("Shield's Model so that we can rotate it instead of the shield object")]
    Transform modelXForm;

    [SerializeField, Tooltip("Set to false if you want the player to pickup the shield first")]
    bool defaultsToAttachedToPlayer = true;

    [SerializeField, Tooltip("Use < 1 to disable it.Total amount of times the shield is allowed to bounce off of something before poofing back to the player")]
    int maximumBounces = 3;
    int currentBounces = 0;

    [SerializeField, Tooltip("How fast it travels when thrown")]
    float thrownSpeed = 15f;

    [SerializeField, Tooltip("How fast it travels when recalled")]
    float recallSpeed = 25f;

    [SerializeField, Tooltip("How fast to rotate the shield when thrown")]
    float throwSpinSpeed = 45f;

    [SerializeField, Tooltip("How fast to rotate the shield when recalled")]
    float recallSpinSpeed = 60f;

    [SerializeField, Tooltip("How close to the player before it can be 'attached' or 'picked up'")]
    float attachingDistance = 1f;
    public float AttachingDistance { get { return attachingDistance; } }

    [SerializeField, Tooltip("Whether to use player selected targets")]
    bool manualTargetEnabled = false;

    [SerializeField, Tooltip("Whether to use auto target")]
    bool autoTargetEnabled = false;

    [SerializeField, Tooltip("How big of radious around the shield when it collides to use to find the next target")]
    float autoTargetRadious = 5f;

    [SerializeField, Tooltip("How big of an angle form the shield's location to the enemy will be allow auto target")]
    float autoTargetMaxAngle = 60f;

    [SerializeField]
    LayerMask enemyLayerMask;

    Rigidbody rb;
    Rigidbody Rigidbody
    {
        get
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();
            return rb;
        }
    }

    Player Player { get { return GameManager.instance.Player; } }

    bool isHeldInPlace;
    public bool IsAttached { get { return State == ShieldState.Attached; } }
    public bool IsThrowable { get { return currentRoutine == null && State == ShieldState.Attached; } }
    public bool IsRecalled { get { return State == ShieldState.Recalled; } }
    public bool IsRecallable { get { return currentRoutine == null && State == ShieldState.Thrown; } }
    public bool IsOnFire { get { return Attribute == ShieldAttribute.Fire; } }

    /// <summary>
    /// Might not need this anymore that we are using "states"
    /// Was using it before to avoid lunching too many coroutines
    /// </summary>
    IEnumerator currentRoutine;

    /// <summary>
    /// Direction the rigidbody was moving last
    /// </summary>
    private Vector3 lastFrameVelocity;

    [SerializeField, Tooltip("Just for debugging so I can see targets")]
    List<Enemy> targets;
    List<Enemy> Targets
    {
        set { targets = value; }
        get
        {
            if (targets == null)
                targets = new List<Enemy>();
            return targets;
        }
    }
    public bool HasTargets { get { return Targets.Count > 0; } }
    public Enemy FirstTarget { get { return Targets.FirstOrDefault(); } }
    GameObject lastCollisionGO;

    [SerializeField]
    TrailRenderer trailRenderer;
    TrailRenderer TrailRenderer
    {
        get
        {
            if (trailRenderer == null)
                trailRenderer = GetComponent<TrailRenderer>();
            return trailRenderer;
        }
    }

    [SerializeField]
    Collider shieldCollider;

    AudioSource spinAudioSrc;
    Quaternion initialShieldRotation;

    ShieldParticleSystem particles;
    ShieldParticleSystem Particles
    {
        get
        {
            if (particles == null)
                particles = GetComponentInChildren<ShieldParticleSystem>();
            return particles;
        }
    }

    void Start()
    {
        if (shieldCollider == null)
            shieldCollider = GetComponent<Collider>();

        if (modelXForm != null)
            initialShieldRotation = modelXForm.localRotation;
        else
            initialShieldRotation = Quaternion.identity;

        if (defaultsToAttachedToPlayer)
            StartAttachedToPlayer();
    }

    public void StartAttachedToPlayer()
    {
        SetState(ShieldState.Attached);
        AttatchTo(Player.ShieldHolderXForm);
        modelXForm.localRotation = initialShieldRotation;
        EnableCollisions(false);
        EnableColliders(true);
    }

    // Consider combining the colliders/collision method if possible
    void EnableCollisions(bool enable)
    {
        Rigidbody.isKinematic = !enable;
        Rigidbody.detectCollisions = enable;
    }

    /// <summary>
    /// When the player collider is enabled the shield collider will be disabled
    /// </summary>
    /// <param name="enablePlayerCollider"></param>
    void EnableColliders(bool enablePlayerCollider)
    {
        Player.ShieldCollider.enabled = enablePlayerCollider;
        shieldCollider.enabled = !enablePlayerCollider;
    }

    /// <summary>
    /// Handles checking if shield can be picked up as well as emitters/particles
    /// and keeping track of the rigidbody's velocity
    /// </summary>
    void Update()
    {
        // Don't do anything
        if (LevelController.instance.State != LevelState.Playing)
            return;

        //ShowShieldAttribute();
        lastFrameVelocity = Rigidbody.velocity;
        TrailRenderer.emitting = State == ShieldState.Thrown || State == ShieldState.Recalled;

        switch (State)
        {
            case ShieldState.Disabled:
            case ShieldState.Thrown:
            case ShieldState.Recalled:
                CheckShieldCanBePickedUp();
                break;

            case ShieldState.Attaching:
            case ShieldState.Attached:            
            case ShieldState.Throwing:
                break;
        }
    }

    /// <summary>
    /// handles movement/rotation
    /// </summary>
    private void FixedUpdate()
    {
        // Don't do anything
        if (LevelController.instance.State != LevelState.Playing)
            return;

        switch (State)
        {
            case ShieldState.Thrown:
            case ShieldState.Recalled:
                Movement();
                break;

            case ShieldState.Attaching:
                Rotate();
                break;
        }
    }

    public void DisableShield()
    {
        if (!IsAttached)
            Particles.Play(ShieldParticle.Poof);

        if (spinAudioSrc != null)
            spinAudioSrc.Stop();

        SetVelocity(Vector3.zero);
        modelXForm.gameObject.SetActive(false);
        SetState(ShieldState.Disabled);
    }

    public void SetState(ShieldState _state) => State = _state;

    void Movement()
    {
        Move();
        Rotate();
    }

    void Move()
    {
        // Defaults to the current direction
        var velocity = Rigidbody.velocity.normalized;            

        // When held in place we want to stop movement but continue rotation
        if (isHeldInPlace)
            velocity = Vector3.zero;

        // Velocity will be in the direction of the player when being recalled
        if (State == ShieldState.Recalled || State == ShieldState.Attaching)
            velocity = (Player.transform.position - transform.position).normalized;

        SetVelocity(velocity);
    }

    void SetVelocity(Vector3 direction)
    {
        var speed = IsRecalled ? recallSpeed : thrownSpeed;

        // Kinematic bodies are not affected by physics which means a change in velocity will NOT move the body
        // Hence us having to affect the transform manually
        if (!Rigidbody.isKinematic)
            Rigidbody.velocity = direction * speed;
        else
            transform.position = Vector3.MoveTowards(transform.position, transform.position + direction, speed * Time.deltaTime);
    }

    void Rotate()
    {
        var speed = IsRecalled ? recallSpinSpeed : throwSpinSpeed;
        var targetRotation = Quaternion.Euler(Vector3.up * speed * Time.fixedDeltaTime);
        modelXForm.rotation *= targetRotation;
    }

    public void Throw(Transform startingXForm, Vector3 direction)
    {
        if(State == ShieldState.Attached && currentRoutine == null)
        {
            currentRoutine = ThrowRoutine(startingXForm, direction);
            StartCoroutine(currentRoutine);
        }
    }

    IEnumerator ThrowRoutine(Transform startingXForm, Vector3 direction)
    {
        // Detach from the player
        SetState(ShieldState.Throwing);

        shieldCollider.enabled = true;
        shieldCollider.isTrigger = false;
        Player.ShieldCollider.enabled = false;
        Rigidbody.isKinematic = false;

        transform.SetParent(startingXForm);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        yield return new WaitForEndOfFrame();

        transform.SetParent(null, true);

        // Re-enable collisions and set the speed
        EnableCollisions(true);
        SetVelocity(direction);
        AudioManager.instance.Play(SFXLibrary.instance.shieldThrow);
        yield return new WaitForEndOfFrame();

        // Loop the spinning sound
        spinAudioSrc = AudioManager.instance.Play(SFXLibrary.instance.shieldSpin);      

        // We need to allow enough distance from the player before the shield can be picked up
        // Yes, even if they are really close to the wall we want to prevent the picked up so that the shield goes through the player
        while (Vector3.Distance(Rigidbody.position, Player.transform.position) < attachingDistance)
        {
            // We cannot rely on FixedUpdate because the "currentRoutine" is not yet null which means that
            // shield could lose speed or potentially stop if it hits and object
            Movement();
            yield return new WaitForFixedUpdate();
        }

        SetState(ShieldState.Thrown);
        currentRoutine = null;
    }

    public void AttatchTo(Transform xform)
    {
        transform.SetParent(xform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Recalls the shield only when it is still in thrown state
    /// </summary>
    public void Recall()
    {
        if(State == ShieldState.Thrown && currentRoutine == null)
        {
            ClearTargets();
            lastCollisionGO = null;            
            AudioManager.instance.Play(SFXLibrary.instance.shieldRecall);

            // Making the spin sound quicker to simular the returning faster
            if (spinAudioSrc != null)
                spinAudioSrc.pitch += .4f;

            // Stop it in its track so that we can change is movement quickly
            SetVelocity(Vector3.zero);

            // Turning it into a trigger to avoid bouncing off of things
            shieldCollider.isTrigger = true;
            SetState(ShieldState.Recalled);
        }        
    }

    void CheckShieldCanBePickedUp()
    {
        var distance = Vector3.Distance(Rigidbody.position, GameManager.instance.Player.transform.position);
        if (distance < attachingDistance)
            Pickup();
    }

    void Pickup()
    {
        switch (State)
        {
            case ShieldState.Disabled:
            case ShieldState.Thrown:
            case ShieldState.Recalled:
                if(currentRoutine == null)
                {
                    currentRoutine = PickupRoutine();
                    StartCoroutine(PickupRoutine());
                }
                break;
        }
    }

    /// <summary>
    /// The expectation here is that when the shield is airborn 
    /// the player is always looking at the shield
    /// </summary>
    /// <returns></returns>
    IEnumerator PickupRoutine()
    {
        isHeldInPlace = false;

        shieldCollider.enabled = false;
        Rigidbody.isKinematic = true; // has to be or else it won't move with the player
        Player.ShieldCollider.enabled = true;
        SetState(ShieldState.Attaching);       

        // Shield state is always reset when caught
        SetAttribute(ShieldAttribute.Normal);

        // Stop all movement and disable collisions since we are picking up the shield
        SetVelocity(Vector3.zero);
        yield return new WaitForEndOfFrame();

        // Move until we are close enough for the player to pickup
        while(Vector3.Distance(transform.position, Player.transform.position) > attachingDistance)
        {
            Move();
            yield return new WaitForFixedUpdate();
        }

        // Shield is close enough for the player to pickup
        //yield return StartCoroutine(Player.PickUpShieldRoutine(this));
        AttatchTo(Player.ShieldHolderXForm);
        AudioManager.instance.Play(SFXLibrary.instance.shieldPickup);

        // Ensuring the model is not attached in a weird wait
        if (modelXForm != null)
            modelXForm.localRotation = initialShieldRotation;

        // Stop the spinning sound
        if(spinAudioSrc != null)
            spinAudioSrc.Stop();
        
        currentRoutine = null;
        SetState(ShieldState.Attached);
    }

    private void ClearTargets()
    {
        foreach (var target in Targets)
        {
            if (target != null)
                target.ClearMarker();
        }

        Targets.Clear();
    }

    private void Bounce(Collision collision, bool playSFX = true)
    {
        Vector3 normal = collision.contacts[0].normal;

        // Default direction to bounce
        Vector3 dir = Vector3.Reflect(lastFrameVelocity.normalized, normal.normalized);

        // Collided with a bumper, we will use it's forward vector and center the shield 
        if (collision.gameObject.GetComponent<ShieldBumper>() != null)
        {
            dir = collision.transform.forward;
            transform.position = new Vector3(
                collision.transform.position.x,
                transform.position.y,
                collision.transform.position.z
            );
        }   

        // If we get a zero value we can use the normal itself as the bounce direction
        // Because the "normal" always points away from the object we collided
        if (dir == Vector3.zero)
            dir = normal;

        // However, if we have a target then our direction should be towards' that target
        // First we need to remove any "null" or "dead" enemies from our list
        // Once we hit an enemy they become "dead" so we should move to the next target...need to confirm
        Targets = Targets.Where(e => e != null && !e.IsDead).ToList();

        // Now we check if we stil have any targets and set our direction toward's them
        var target = Targets.FirstOrDefault();
        if(autoTargetEnabled && target == null)
        {
            // Let's look nearby to see if there are targets we can find
            var colliders = Physics.OverlapSphere(transform.position, autoTargetRadious, enemyLayerMask);

            // TODO: Do we need to use a SortedList using the distance between the shield and the target
            //       as the prirotity where a lesser number means a higher priority
            foreach (var collider in colliders)
            {
                var enemy = collider.GetComponent<Enemy>();
                if (enemy == null || enemy.IsDead)
                    continue;

                // TODO: Also use the direction
                // The angle we want is based on the direction we are moving
                // and the direction we would be heading if we go towards the enemy
                var directionToEnemy = enemy.transform.position - transform.position;

                var angle = Vector3.Angle(dir, directionToEnemy);
                if(angle <= autoTargetMaxAngle)
                {
                    target = enemy;
                    AddAutoTarget(target);
                    break;
                }
            }
        }

        if (target != null)
            dir = target.transform.position - transform.position;

        if(playSFX)
            AudioManager.instance.Play(SFXLibrary.instance.shieldBounce);

        SetVelocity(dir);
    }

    IEnumerator PoofBackToPlayerRouting()
    {
        currentBounces = 0;
        TrailRenderer.emitting = false;
        SetVelocity(Vector3.zero);
        yield return new WaitForEndOfFrame();

        SetAttribute(ShieldAttribute.Normal);
        AttatchTo(Player.ShieldHolderXForm);
        Particles.Play(ShieldParticle.Poof);
        yield return StartCoroutine(PickupRoutine());
    }

    public void HoldInPlace(Transform center)
    {
        switch (State)
        {
            case ShieldState.Thrown:
            case ShieldState.Recalled:
                if (currentRoutine == null && !isHeldInPlace)
                    StartCoroutine(HoldInPlaceRoutine(center));
                break;
        }
    }

    IEnumerator HoldInPlaceRoutine(Transform center)
    {
        isHeldInPlace = true;
        transform.position = new Vector3(
            center.position.x,
            transform.position.y,
            center.position.z
        );
        yield return new WaitForEndOfFrame();

        while(State == ShieldState.Thrown)
            yield return new WaitForEndOfFrame();

        isHeldInPlace = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        var color = new Color(0f, 0f, 1f, .10f);
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, autoTargetRadious);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ignore player
        if (collision.collider.CompareTag("Player"))
            return;

        // Ignore if this is the same collision as last time
        if (collision.gameObject == lastCollisionGO)
            return;

        HandleCollision(collision);
    }

    void HandleCollision(Collision collision)
    {
        switch (State)
        {
            case ShieldState.Thrown:
            case ShieldState.Throwing:
            case ShieldState.Recalled:

                var handler = collision.gameObject.GetComponent<IShieldCollisionHandler>();
                if (handler != null)
                    handler.OnShieldCollisionEnter(this);
                else if (maximumBounces > 0)
                {
                    currentBounces++;
                    if (currentBounces > maximumBounces)
                    {
                        Poof();
                        return;
                    }
                }

                // There's something funky going on with hitting a bee with the shield when the bee's shield collider is set to trigger
                // As in, it does not detect the trigger, but the "collision" always triggers so rather than changing the bee's to "trigger"
                // so that the shield cuts through the bee rather than bounce off of it, we will check in collision if the bee is killable
                // and not bounce
                //var bee = collision.gameObject.GetComponent<Bee>();
                //Debug.Log($"Bee: {bee}");
                //if (bee == null || !bee.IsKillable)
                //    Bounce(collision, handler == null);
                //else
                //    Rigidbody.velocity = lastFrameVelocity;

                Bounce(collision, handler == null);
                lastCollisionGO = collision.gameObject;
                break;
        }
    }

    public void Poof()
    {
        switch (State)
        {
            case ShieldState.Thrown:
            case ShieldState.Throwing:
                if (currentRoutine == null)
                {
                    currentRoutine = PoofBackToPlayerRouting();
                    StartCoroutine(currentRoutine);
                }
                break;
        }      
    }

    private void OnTriggerEnter(Collider other) => HandleTriggerEnter(other);
    void HandleTriggerEnter(Collider other)
    {
        switch (State)
        {
            case ShieldState.Thrown:
            case ShieldState.Throwing:
            case ShieldState.Recalled:
                var handler = other.GetComponent<IShieldEnterTriggerHandler>();

                // Might have to get it from the parent
                if (handler == null)
                    handler = other.GetComponentInParent<IShieldEnterTriggerHandler>();

                if (handler != null)
                    handler.OnShieldEnterTrigger(this);
                break;
        }
    }

    void OnTriggerExit(Collider other) => HandleTriggerExit(other);
    void HandleTriggerExit(Collider other)
    {
        switch (State)
        {
            case ShieldState.Thrown:
            case ShieldState.Throwing:
            case ShieldState.Recalled:
                var handler = other.GetComponent<IShieldExitTriggerHandler>();
                if (handler != null)
                    handler.OnShieldExitTrigger(this);
                break;
        }        
    }

    public void AddTarget(Enemy enemy)
    {
        if (manualTargetEnabled && IsAttached && !Targets.Contains(enemy))
        {
            Targets.Add(enemy);
            enemy.SetTargetNumber(Targets.Count);
        }   
    }

    void AddAutoTarget(Enemy enemy)
    {
        if (!Targets.Contains(enemy))
        {
            Targets.Add(enemy);
            enemy.SetMarker(".");
        }   
    }

    public void SetOnFire() => SetAttribute(ShieldAttribute.Fire);
    void SetAttribute(ShieldAttribute attr)
    {
        // Based on the current attribute we might need to play/stop
        // or trigger some other effects/behaviors
        switch (Attribute)
        {
            // Currently on fire
            case ShieldAttribute.Fire:
                // Shield is no longer on fire
                if (attr != ShieldAttribute.Fire)
                {
                    Particles.Disable(ShieldParticle.Fire);
                    Particles.Play(ShieldParticle.Smoke);
                }
                break;
        }

        // Change the attribute
        Attribute = attr;

        // Now play the effect based on the new attribute
        PlayShieldAttributeEffect(attr);
    }

    void PlayShieldAttributeEffect(ShieldAttribute attr)
    {
        switch (attr)
        {
            case ShieldAttribute.Fire:
                Particles.Play(ShieldParticle.Fire);
                break;
        }
    }
}
