using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A patrolling bee will:
///     - move to preset destinations
///     - face a direction as dictated by the node
///     - remain in that stop for N seconds before moving to the next stop
/// </summary>
public class Patrol : MonoBehaviour, IEnemyMovementHandler
{
    [SerializeField, Tooltip("How fast the bee moves")]
    protected float patrolSpeed = 7f;

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

    [SerializeField,Tooltip("Nodes for the bee to traverse on")]
    List<PathNode> path;
    public List<PathNode> Path { get { return path != null ? path : null; } }

    int currentIndex = 0;
    bool InMoveState { get { return Bee.State == BeeState.Move; } }

    /// <summary>
    /// TOOD: make this smoother
    /// </summary>
    public IEnumerator MoveRoutine()
    {
        while(InMoveState && Path.Count > 0)
        {
            var node = Path[currentIndex];
            var destination = new Vector3(
                node.transform.position.x,
                Bee.transform.position.y,
                node.transform.position.z
            );

            // Set the destination and keep moving until we arrive
           
            while (InMoveState && Vector3.Distance(Bee.Rigidbody.position, destination) > 0.01f)
            {
                Bee.Move(destination, patrolSpeed);
                yield return new WaitForEndOfFrame();
            }
                

            // If the node has a routine for when the bee arrives then we will continue the code
            // otherwise we will move on to the next iteration.
            if(InMoveState && !node.HasStopRoutine)
            {
                // Safetest to tell the bee to stop moving to avoid overshooting since it does not autobreak
                Bee.StopMoving();

                // Ensure we are working with the next node
                IncreaseIndex();

                // We want to stop / look at the next destination
                node = Path[currentIndex];
                destination = new Vector3(
                    node.transform.position.x,
                    Bee.transform.position.y,
                    node.transform.position.z
                );

                Bee.LookAtTarget(destination);
                yield return new WaitForEndOfFrame();
                continue;
            }

            // Node Routine
            if (InMoveState)
            {
                Bee.StopMoving();
                yield return new WaitForEndOfFrame();
                // Look at the desired direction for the current node
                yield return StartCoroutine(Bee.RotateRoutine(node.DirectionToLookAt, node.RotationSpeed));
            }                

            // Wait until it is time to move again
            var time = Time.time + node.Duration;
            while (InMoveState && Time.time < time)
                yield return new WaitForEndOfFrame();

            if(InMoveState)
                IncreaseIndex();

            yield return new WaitForEndOfFrame();
        }
    }

    private void IncreaseIndex()
    {
        currentIndex += 1;
        if (currentIndex >= Path.Count)
            currentIndex = 0;
    }
}
