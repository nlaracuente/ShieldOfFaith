using System.Collections;
using UnityEngine;

public class Hive : MonoBehaviour, IShieldCollisionHandler, IShieldEnterTriggerHandler
{
    [SerializeField, Tooltip("Bee to spawn when destroyed")]
    Bee beePrefab;

    [SerializeField, Tooltip("True when the hive requires fire to be destroyed")]
    bool requiresFire = false;

    [SerializeField, Tooltip("Distance to the player before the hive starts attacking")]
    float awarenessDistance = 20f;

    [SerializeField, Tooltip("How many seconds after shooting stingers before firing again")]
    float stingerSpawnInterval = .25f;

    [SerializeField]
    GameObject hiveModel;

    [SerializeField]
    ParticleSystem particles;

    [SerializeField]
    Collider hiveCollider;

    [SerializeField]
    Collider playerCollider;

    [SerializeField]
    Transform stingersXForm;
    Transform[] stingerSpawnPoints;

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

    Shield Shield { get { return GameManager.instance.Shield; } }
    Player Player { get { return GameManager.instance.Player; } }
    public bool IsPlayerWithinRange(float range) { return Vector3.Distance(transform.position, Player.transform.position) <= range; }

    void Start()
    {
        stingerSpawnPoints = new Transform[stingersXForm.childCount];
        for (int i = 0; i < stingersXForm.childCount; i++)
            stingerSpawnPoints[i] = stingersXForm.GetChild(i);

        StartCoroutine(InitializeRoutine());
    }

    IEnumerator InitializeRoutine()
    {
        while (LevelController.instance.State != LevelState.Playing)
            yield return new WaitForEndOfFrame();

        StartCoroutine(SpawnStingersRoutine());
    }

    private void Update()
    {
        if (LevelController.instance.State != LevelState.Playing)
            return;

        // To avoid the shield going under the beehive when it is recalled
        // We need to temporarily change the collider to be a trigger
        hiveCollider.isTrigger = Shield.IsRecalled;
    }

    IEnumerator SpawnStingersRoutine()
    {
        while(!destroyed)
        {
            while (LevelController.instance.State != LevelState.Playing)
                yield return new WaitForEndOfFrame();

            // When not playing the "hit" animation then shoot stingers
            if(!Animator.GetCurrentAnimatorStateInfo(0).IsName("Hit") && IsPlayerWithinRange(awarenessDistance))
            {
                Animator.Play("Spawn");
                AudioManager.instance.Play(SFXLibrary.instance.hiveAttack);
                foreach (var xForm in stingerSpawnPoints)
                {
                    var stinger = BeeEnemyController.instance.GetStinger(xForm);
                    stinger.transform.forward = xForm.forward;
                    stinger.Fired(xForm.forward);
                }

                yield return new WaitForSeconds(stingerSpawnInterval);
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void OnShieldEnterTrigger(Shield shield) => OnShieldCollisionEnter(shield);
    public void OnShieldCollisionEnter(Shield shield)
    {
        if (!destroyed)
        {
            if(!requiresFire || shield.IsOnFire)
                StartCoroutine(DestroyRoutine());
            else
            {
                Animator.Play("Hit");
                AudioManager.instance.Play(SFXLibrary.instance.hiveHit);
            }   
        }
    }

    IEnumerator DestroyRoutine()
    {
        destroyed = true;        
        AudioManager.instance.Play(SFXLibrary.instance.hiveDestroy);
       
        if (hiveModel != null)
            hiveModel.SetActive(false);

        // Disable the main collider and the player one
        // Yes I could loop through all of them but being lazy
        hiveCollider.enabled = false;
        playerCollider.enabled = false;

        // Quick/Dirty way to solve for the bee getting killed so quickly
        // wait a few frames for the shield to bounce away before spawing it
        // unless this is a recall
        if (!Shield.IsRecalled)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }

        // Spawn the bee
        if (beePrefab != null)
        {             
            var bee = Instantiate(beePrefab).GetComponent<Bee>();
            bee.transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                transform.position.z            
            );
            yield return new WaitForEndOfFrame();
            
            // When the shield hit the hive on a recall we want to instantly kill the bee
            if (Shield.IsRecalled)
                bee.Die(Shield);
        }

        if (particles != null)
        {
            particles.Play();
            yield return new WaitForEndOfFrame();
            while(particles.isPlaying)
                yield return new WaitForEndOfFrame();
        }
        
        Destroy(gameObject);
    }
}
