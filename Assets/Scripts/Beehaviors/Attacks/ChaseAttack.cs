using System.Collections;
using UnityEngine;

/// <summary>
/// Bee looks around until it sees the player then dashes to the place where they last saw the player
/// </summary>
public class ChaseAttack : MonoBehaviour, IEnemyAttackHandler
{
    [SerializeField, Tooltip("How fast to move to the player")]
    float chaseSpeed = 7f;

    [SerializeField, Tooltip("How many seconds after colliding with player before resuming chase")]
    float coolDownTime = 1.5f;

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

    bool InAttackState { get { return Bee.State == BeeState.Attack; } }
    bool IsAttacking { get { return InAttackState && Bee.IsAwareOfPlayer; } }
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

    public IEnumerator AttackRoutine()
    {
        // Stop Moving and look at the player
        Bee.StopMoving();
        yield return StartCoroutine(Bee.LookAtPlayerRoutine());

        // We need to turn this flag ON now so that when the "charge" animation
        // transitions into "moving" it stays on the "moving" animation
        Bee.SetIsMovingAnimationBool(true);

        // Capture where the player is currently so that we can pursue that until we are moving
        // Since the player might have moved out of the way by the time the animations kicks into chase mode
        var destination = PlayerPosition;

        // Play Charge Animation (the windup before the chase)
        Bee.OnDashFrame = false;
        Bee.Animator.Play("Charge");
        yield return new WaitForEndOfFrame();

        // Wait until we are on the attack frame
        // We always want to dash for a bit event if thep layer has moved
        while (InAttackState && !Bee.OnDashFrame)
            yield return new WaitForEndOfFrame();

        // Because the bee might be "not moving" when it spots the player
        // we want to trigger movement until we have "isMoving = true"
        while (InAttackState && !Bee.IsMoving)
        {
            Bee.Move(destination, chaseSpeed);
            yield return new WaitForFixedUpdate();
        }

        // Keep moving while the player is visible, 
        // if the player stops being visible then we want to know the last known spot they where
        // and keep moving there. If the player becomes visible at any point then we want to re-engage them
        while(InAttackState)
        {
            if(Bee.IsAwareOfPlayer)
            {
                // Keep pursuing the player
                while (IsAttacking && Bee.IsMoving)
                {
                    Bee.Move(PlayerPosition, chaseSpeed);
                    yield return new WaitForFixedUpdate();
                }
            }
            else
            {
                // Bee is not dead/aware of the player and the bee hasn't stopped moving
                // So keep moving until we reach the destination of these conditions change
                while (InAttackState && !Bee.IsAwareOfPlayer && Bee.IsMoving)
                    yield return new WaitForFixedUpdate(); // Keep moving

                // Bee might have reached the destination and still did not find the player
                // Break out of this loop
                if (!Bee.IsAwareOfPlayer)
                    break;
            }

            yield return new WaitForEndOfFrame();
        }

        // Since we are still in attack mode but must have hit something that is stopping 
        // our movement we will ensure the bee stoppes moving
        if(InAttackState)
        {
            Bee.StopMoving();
            yield return new WaitForEndOfFrame();
        }

        var coolDown = Time.time + coolDownTime;
        while (InAttackState && Time.time < coolDown)
        {
            // Only looks at the player if it is aware of the player
            // Therefore it is safe to call even when we are waiting because the player is no longer visible
            Bee.LookAtPlayer();
            yield return new WaitForEndOfFrame();
        }

        // If the player is no longer in sight and
        // the state didn't change because we bonked - then we can change to idle
        if (!IsAttacking && Bee.State != BeeState.Bonk)
            Bee.ChangeState(BeeState.Idle);
    }
}
