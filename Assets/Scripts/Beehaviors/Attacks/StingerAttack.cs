using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When the player is within stringer attack range, it attacks the player
/// </summary>
public class StingerAttack : MonoBehaviour, IEnemyAttackHandler
{
    [SerializeField, Tooltip("How many seconds after the stinger is fired before trying to attack again")]
    protected float attackCoolDown = 1f;

    Bee bee;
    public Bee Bee
    {
        get
        {
            if (bee == null)
                bee = GetComponent<Bee>();

            return bee;
        }
    }

    Transform spawnXForm { get { return Bee.StingerSpawnXForm; } }

    bool IsAttacking { get { return Bee.State == BeeState.Attack; } }
    Animator Animator { get { return Bee.Animator; } }

    public IEnumerator AttackRoutine()
    {
        // Always face the player before we attack...if they are in range
        // yield return StartCoroutine(Bee.LookAtPlayerRoutine());

        // Play Attack Animation
        Bee.OnAttackFrame = false;
        Animator.Play("Attack");

        // Wait until we are on the attack frame
        while (IsAttacking && !Bee.OnAttackFrame)
        {
            Bee.LookAtPlayer();
            yield return new WaitForEndOfFrame();
        }   

        // Spawn the projectile
        if (IsAttacking)
        {
            // Spawn it using the spawnxForm to know where to place it 
            // As well as the enemy's forward to have the right rotation
            var stinger = BeeEnemyController.instance.GetStinger(spawnXForm);
            stinger.transform.forward = transform.forward;
            AudioManager.instance.Play(SFXLibrary.instance.beeAttack);

            // Delay one frame so that the projectile is aligned and what not 
            yield return new WaitForEndOfFrame();

            // Now we can detach it and launch it
            if (IsAttacking)
                stinger.Fired(transform.forward);
            else
                stinger.DestroyProjectile();
        }

        // Wait for the animation to finish playing before entering cool down
        while (IsAttacking && Bee.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            yield return new WaitForEndOfFrame();

        // Attack Cool Down
        var cooldownTime = Time.time + attackCoolDown;
        while (IsAttacking && Time.time < cooldownTime)
        {
            // Keep looking at the player while they are visible
            Bee.LookAtPlayer();
            yield return new WaitForEndOfFrame();
        }   

        // If the player is no longer visible, 
        // then we want to go back to idle
        // There is no "delay" here since we already did the "attack cooldown delay"
        if(!Bee.IsAwareOfPlayer)
            Bee.ChangeState(BeeState.Idle);

        yield return new WaitForEndOfFrame();
    }
}
