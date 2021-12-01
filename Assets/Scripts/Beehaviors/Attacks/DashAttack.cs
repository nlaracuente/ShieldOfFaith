using System.Collections;
using UnityEngine;

/// <summary>
/// When the player is within attack range, the bee moves to the player's current spot with a charge 
/// 
/// NOTE: not fully developed since we changed how states are transitioned
/// </summary>
public class DashAttack : MonoBehaviour, IEnemyAttackHandler
{
    [SerializeField, Tooltip("How fast to move to the player")]
    float chargeSpeed = 15f;

    [SerializeField, Tooltip("How many seconds before it can charge again")]
    float coolDownTime = 3f;

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

    bool IsAttacking { get { return Bee.State == BeeState.Attack && Bee.IsAwareOfPlayer; } }
    Animator Animator { get { return Bee.Animator; } }
    Shield Shield { get { return GameManager.instance.Shield; } }
    Player Player { get { return GameManager.instance.Player; } }
    Vector3 PlayerPosition
    {
        get
        {
            return new Vector3(
                Player.transform.position.x,
                transform.position.y,
                Player.transform.position.z
          );
        }
    }
    

    bool IsDead { get { return Bee.IsDead || Bee.State == BeeState.Dead; } }

    public IEnumerator AttackRoutine()
    {
        // Always face the player before we attack...if they are in range
        yield return StartCoroutine(Bee.LookAtPlayerRoutine());

        // We need to turn this flag ON now so that when the "charge" animation
        // transitions into "moving" it stays on the "moving" animation
        Bee.SetIsMovingAnimationBool(true);

        // Play Charge Animation
        Animator.Play("Charge");

        // Allow a frame to happen so that the "clip info" has had time to change
        // to the new clip name then we can wait for that to transition into moving
        yield return new WaitForEndOfFrame();

        // Wait until charge is done
        while (IsAttacking && Bee.Animator.GetCurrentAnimatorStateInfo(0).IsName("Charge"))
            yield return new WaitForEndOfFrame();

        // Move to the player
        var destination = new Vector3(
            Player.transform.position.x,
            transform.position.y,
            Player.transform.position.z
        );

        // Because this is a "dash" we want the bee to commit to it while it still can
        // This means we want the bee to keep dashing until they are not longer able to
        // regardless if the player is there or not.
        // Because we want the bee to go in a straight line and not avoid obstacles
        // we will not be relying on the NavMesh Agent for movement. 
        // We also need to verify that the destination is free otherwise we:
        //  - if collision with the player: cause damage / enter cool down
        //  - if collision with the shield while being held: trigger bonk / enter cool down
        //  - if collision with a wall: do the same as collision with the shield 

        // Keep moving until collision or destination is reached (or bee is dead)
        while (!IsDead)
        {
            var bonk = false;

            // Face the direction we are moving
            Bee.Rotate(destination);
            yield return new WaitForEndOfFrame();

            // Test where we going to be 
            Debug.DrawRay(transform.position, transform.forward * chargeSpeed * Time.fixedDeltaTime, Color.blue, 1f);
            //var hits = Physics.RaycastAll(transform.position, transform.forward, chargeSpeed * Time.fixedDeltaTime);
            //foreach (var hit in hits)
            //{
            //    switch(hit.collider.tag)
            //    {
            //        case "Wall":
            //        case "Shield":
            //            bonk = hit.collider.tag == "Shield" && Shield.IsAttachedToPlayer || hit.collider.tag == "Wall";
            //            break;
            //    }

            //    if (bonk)
            //        break;
            //}

            // We need to start moving until the NavMesh Agent is seen as moving
            Bee.Move(destination, chargeSpeed);
            while (!IsDead && !Bee.IsMoving)
                yield return new WaitForEndOfFrame();

            // Now we can wait until the NavMesh Agent is no longer moving
            // We will consider that as "destination reached"
            while (!IsDead && Bee.IsMoving)
                yield return new WaitForEndOfFrame();

            // Move 
            var position = Bee.Rigidbody.position + transform.forward * chargeSpeed * Time.fixedDeltaTime;
            Bee.Rigidbody.MovePosition(position);
            // Bee.transform.position = Vector3.MoveTowards(Bee.transform.position, destination, chargeSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();

            // Interesting, the bee died...so exit
            if (IsDead)
                yield break;

            // Must have hit the shield while equiped or a wall
            if(bonk)
            {
                Debug.Log("Bonk");
                Animator.Play("Bonk");
                yield return new WaitForEndOfFrame();

                // Wait for the animation to finish playing
                while (!Bee.IsDead && Bee.Animator.GetCurrentAnimatorStateInfo(0).IsName("Bonk"))
                    yield return new WaitForEndOfFrame();

                break; // to stop moving
            } 
            else
            {
                // Coollided with the player so we will stop moving
                if(Vector3.Distance(transform.position, PlayerPosition) < 0.01f)
                {
                    Debug.Log("Collided with player");
                    Bee.StopMoving();
                    break;
                }
            }
        }

        // Attack Cool Down
        var cooldown = Time.time + coolDownTime;
        while (IsAttacking && Time.time < cooldown)
        {
            // Keep looking at the player
            Bee.LookAtPlayer();
            yield return new WaitForEndOfFrame();
        }   

        if(!Bee.IsAwareOfPlayer)
            Bee.ChangeState(BeeState.Idle);
    }
}
