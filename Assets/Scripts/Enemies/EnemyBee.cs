using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBee : Enemy
{
    [SerializeField]
    Stinger projectilePrefab;

    [SerializeField]
    Transform projectileSpawnXForm;

    protected override IEnumerator EnemeyRoutine()
    {
        while (!IsDead)
        {
            // Wait until the player is close enough to beging attacking
            while (!IsDead && !PlayerInRange)
                yield return new WaitForEndOfFrame();

            // Attack
            if (!IsDead)
            {
                Animator.SetTrigger("Attack");
                yield return new WaitForEndOfFrame();
                Animator.SetTrigger("ActionTriggered");
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
            }

            // Wait until we are on the attack frame
            var totalTime = Time.time + timeBeforeAttack;
            while (!IsDead && Time.time < totalTime)
                yield return new WaitForEndOfFrame();

            // Spawn the projectile
            if (!IsDead)
            {
                // Spawn it using the spawnxForm to know where to place it 
                // As well as the enemy's forward to have the right rotation
                var projectile = Instantiate(projectilePrefab).GetComponent<Stinger>();
                projectile.transform.position = projectileSpawnXForm.transform.position;
                projectile.transform.forward = transform.forward;
                yield return new WaitForEndOfFrame();

                // Now we can detach it and launch it
                if (!IsDead)
                    projectile.Fired(transform.forward);

                // Wait until we can attack again
                totalTime = Time.time + timeBetweenAttacks;
                while (!IsDead && Time.time < totalTime)
                    yield return new WaitForEndOfFrame();
            }
        }

        // Trigger death & disable colliders so that it cannot be triggered again and allows the player to move through them without getting hurt
        Animator.SetTrigger("Death");
        Collider.enabled = false;
        yield return new WaitForEndOfFrame();

        Destroy(gameObject, timeBeforeRemovingEnemy);
    }
}
