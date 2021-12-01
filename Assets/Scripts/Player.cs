using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField] bool lookAtMouse;

    [SerializeField, Tooltip("Total faith the player starts with")]
    int maxFaith = 3;

    [SerializeField]
    int curFaith = 0;
    public int Faith { get { return curFaith; } }

    [SerializeField, Tooltip("Total seconds the player stays in iframe")]
    float iFrameTime = 3f;    

    [SerializeField, Tooltip("How long in seconds to wait after rotating towards the shield/launching direction before rotating to look at mouse again")]
    float rotateToShieldDelay = 0.25f;

    [SerializeField]
    float movementSpeed = 30f;

    [SerializeField]
    float rotationSpeed = 15f;

    [SerializeField, Tooltip("How far to shoot the raycast to hit the 'floor'")]
    float rayCastDistance = 500f;

    [SerializeField, Tooltip("The layer mask for the floor that detects mouse position")]
    LayerMask floorLayerMask;

    [SerializeField, Tooltip("The holder for when the shield is attached to the player")]
    Transform shieldHolderXForm;
    public Transform ShieldHolderXForm { get { return shieldHolderXForm; } }

    [SerializeField, Tooltip("The holder for when the shield is launched from the player")]
    Transform shieldLaunchedXForm;

    [SerializeField, Tooltip("Collider to enable when the shield is attached to block enemy attacks")]
    Collider shieldCollider;
    public Collider ShieldCollider { get { return shieldCollider; } }
    public Transform ShieldLaunchedXForm { get { return shieldLaunchedXForm; } }

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

    [SerializeField]
    LayerMask wallLayerMark;

    [SerializeField, Tooltip("Safe distance form which we can throw the shield without risk of getting stuck in a wall")]
    float minWallDistance = 3f;

    Vector3 inputVector;
    Vector3 rotationVector;

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

    public Vector3 Velocity { get { return Rigidbody.velocity; } }

    IEnumerator currentRoutine = null;
    private float rotationAngle;

    [SerializeField, Tooltip("True: test if the player is too close to a wall and prevent throwing the shield")]
    bool testWallOnThrow;

    Shield Shield { get { return GameManager.instance.Shield; } }

    public bool IsInvincible { get; private set; }

    [SerializeField, Tooltip("Mesh renderer for iFrames")]
    Renderer modelRenderer;

    [SerializeField, Range(0.01f, 0.99f), Tooltip("How low to set the alpha to when blink")]
    float iFrameAlpha = 0.75f;

    MaterialPropertyBlock propertyBlock;

    [SerializeField, Tooltip("Arrow to show where shield will throw")]
    GameObject arrowIcon;

    private void Start()
    {
        curFaith = maxFaith;
        propertyBlock = new MaterialPropertyBlock();
        modelRenderer.GetPropertyBlock(propertyBlock);
    }

    private void Update()
    {
        // Don't do anything
        if (LevelController.instance.State != LevelState.Playing)
            return;

        SetInputVector();
        SetRotationVector();

        // Click to throw/recall shield
        if (Input.GetMouseButtonUp(0))
        {
            if (Shield.IsThrowable)
                ThrowShield();
            else if (Shield.IsRecallable)
                RecallShield();
        }

        Move(inputVector);
        SetAnimatorParameters();

        // Only show arrow when shield is held
        arrowIcon.SetActive(Shield.IsAttached);
    }

    private void SetAnimatorParameters()
    {
        Animator.SetFloat("Speed", inputVector != Vector3.zero ? 1f : 0f);
        Animator.SetBool("HasShield", Shield.IsAttached);
    }

    private void FixedUpdate()
    {
        // Don't do anything
        if (LevelController.instance.State != LevelState.Playing)
            return;

        // Ignore movement/rotation
        if (currentRoutine != null)
            return;

        Rotate(rotationVector);

        // Stop the player from moving
        if (inputVector == Vector3.zero)
            Rigidbody.velocity = Vector3.zero;
    }

    private void SetInputVector()
    {
        inputVector.Set(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        inputVector = Camera.main.transform.TransformDirection(inputVector);
        inputVector.y = 0f;
        inputVector.Normalize();
    }

    public void SetRotationVector()
    {
        if (!lookAtMouse)
        {
            if (inputVector != Vector3.zero)
            {
                //var direction = (Rigidbody.position - inputVector).normalized;
                //rotationVector.Set(direction.x, 0f, direction.z);

                float targetAngle = Mathf.Atan2(inputVector.x, inputVector.z) * Mathf.Rad2Deg;
                rotationAngle = Mathf.LerpAngle(rotationAngle, targetAngle, rotationSpeed * Time.deltaTime);
                rotationVector = Vector3.up * rotationAngle;
            }
        }
        else
        {
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.DrawRay(camRay.origin, camRay.direction * rayCastDistance, Color.green);

            if (Physics.Raycast(camRay, out hit, rayCastDistance, floorLayerMask))
            {
                Vector3 dir = hit.point - Rigidbody.position;
                rotationVector.Set(dir.x, 0f, dir.z);
            }
        }
    }

    /// <summary>
    /// Shield can be thrown only if the direction at which we are throwing the shield towards 
    /// has enough space where the shield won't collided with a wall. 
    ///     - If it's colliding with an enemy we are okay with this?
    /// 
    /// The direction defaults to the current mouse position but if the shield has targets
    /// the direction needs to be the first target's position
    /// </summary>
    void ThrowShield()
    {
        // Default direction
        var direction = transform.forward;
        var shield = GameManager.instance.Shield;
        var target = shield.FirstTarget;

        // Use the direction where the enemy is located
        if (target != null)
            direction = target.transform.position - transform.position;

        if(testWallOnThrow)
        {
            // Verify we have enough space to throw the shield
            var origin = new Vector3(transform.position.x, shieldLaunchedXForm.position.y, transform.position.z);
            var end = origin + transform.forward * minWallDistance;

            // We cannot allow the shield to be thrown if they are too close to the wall
            if (Physics.Linecast(origin, end, wallLayerMark))
                return;
        }        

        // We have enough space to through the shield
        currentRoutine = ThrowShieldRoutine(shield, direction);
        StartCoroutine(currentRoutine);
    }

    IEnumerator ThrowShieldRoutine(Shield shield, Vector3 direction)
    {
        // Rotate to look at the direction        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Rigidbody.MoveRotation(targetRotation);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Throw the shield
        shield.Throw(shieldLaunchedXForm, direction);

        // Now we wait a short time so that we can "see" the player rotated
        // and the shield was launched
        yield return new WaitForSeconds(rotateToShieldDelay);
        currentRoutine = null;
    }

    void RecallShield() => GameManager.instance.Shield.Recall();

    private void Move(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        Vector3 targetPosition = Rigidbody.position + (direction.normalized * movementSpeed) * Time.deltaTime;
        Rigidbody.MovePosition(targetPosition);
        //Rigidbody.velocity = direction.normalized * movementSpeed;

    }

    private void Rotate(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            Rigidbody.angularVelocity = Vector3.zero;
            return;
        }

        if (!lookAtMouse)
            Rigidbody.MoveRotation(Quaternion.Euler(Vector3.up * rotationAngle));
        else if (!Shield.IsAttached)
        {
            // Look at the shield
            direction = Shield.transform.position - Rigidbody.position;
            direction.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Rigidbody.MoveRotation(targetRotation);
        }
        else
        {
            // Follow Mouse
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Rigidbody.MoveRotation(targetRotation);
        }
    }

    public IEnumerator PickUpShieldRoutine(Shield shield)
    {
        // Stop any running routine as this takes priority (for now...until death)
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            yield return new WaitForEndOfFrame();
        }

        currentRoutine = PickUpShieldRoutine(shield);

        // Look at the shield (we should already be looking at it but just in case)
        Vector3 direction = shield.transform.position - Rigidbody.position;
        yield return StartCoroutine(RotateRoutine(direction, rotationSpeed));

        Animator.Play("CatchShield");
        yield return new WaitForEndOfFrame();
        Animator.SetBool("HasShield", true);
        while (Animator.GetCurrentAnimatorStateInfo(0).IsName("CatchShield"))
            yield return new WaitForEndOfFrame();

        // Pick shield up
        shield.AttatchTo(ShieldHolderXForm);
        AudioManager.instance.Play(SFXLibrary.instance.shieldPickup);

        currentRoutine = null;
    }

    /// <summary>
    /// Keeps rotating until facing the given direction
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    public IEnumerator RotateRoutine(Vector3 direction, float speed)
    {
        // We don't want up/down rotation so we remove y
        direction.y = 0f;

        // Look rotation does not like "zero" vector so we default to identity
        var targetRotation = Quaternion.identity;
        if (direction != Vector3.zero)
            targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate
        while (Quaternion.Angle(transform.rotation, targetRotation) >= 0.1f)
        {
            var rot = Quaternion.Lerp(Rigidbody.rotation, targetRotation, speed * Time.fixedDeltaTime);
            Rigidbody.MoveRotation(rot);
            yield return new WaitForFixedUpdate();
        }

        // Snap to rotation
        Rigidbody.MoveRotation(targetRotation);
        yield return new WaitForEndOfFrame();
    }

    public void IncreaseFaith()
    {
        // Only playing the sound when we actually add faith
        if(curFaith < maxFaith)
            AudioManager.instance.Play(SFXLibrary.instance.scoreHeart);

        curFaith = Mathf.Clamp(curFaith + 1, 0, maxFaith);
    }

    public void TakeDamage()
    {
        // IsInvincible or Already dead
        if (IsInvincible || LevelController.instance.PlayerDied)
            return;

        curFaith = Mathf.Clamp(curFaith - 1, 0, maxFaith);

        if(curFaith > 0)
        {
            AudioManager.instance.Play(SFXLibrary.instance.playerHurt);
            StartCoroutine(IFrameRoutine());
        }
    }

    IEnumerator IFrameRoutine()
    {
        IsInvincible = true;
        var alphaOn = false;

        var time = Time.time + iFrameTime;
        while(Time.time < time)
        {
            // Old school blink
            modelRenderer.enabled = alphaOn;
            alphaOn = !alphaOn;
            yield return new WaitForEndOfFrame();
        }

        modelRenderer.enabled = true;
        IsInvincible = false;
    }

    public void InstantDeath()
    {
        curFaith = 0;
        AudioManager.instance.Play(SFXLibrary.instance.playerHurt);
    }

    public void PlayDeathAnimation() => Animator.Play("Death");
    public IEnumerator PlayVictoryAnimationRoutine()
    {
        Animator.Play("Victory");
        yield return new WaitForSeconds(2f);
    }
}
