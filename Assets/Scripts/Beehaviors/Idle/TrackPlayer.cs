using System.Collections;
using UnityEngine;

public class TrackPlayer : MonoBehaviour, IEnemyIdleHandler
{
    [SerializeField, Tooltip("How many seconds to wait before returning to original direction")]
    float resetDelay = 1.25f;

    [SerializeField, Tooltip("How quickly to rotate back to the original direction")]
    float rotationSpeed = 10f;

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

    bool IsIdled { get { return Bee.State == BeeState.Idle; } }

    Quaternion originalRotation;
    private void Start() => originalRotation = transform.rotation;

    /// <summary>
    /// Waits until the player is in sight then begins to look at them
    /// When the player is no longer in sight then it rotates to face 
    /// the original location it was looking at
    /// </summary>
    /// <returns></returns>
    public IEnumerator IdleRoutine()
    {
        while(IsIdled)
        {
            // Always look at the player
            Bee.LookAtPlayer(true);

            //// Look at the player
            //// TODO: if the angle is too large - smooth the rotation rather than snapping to it
            //if (Bee.IsAwareOfPlayer)
            //    Bee.LookAtPlayer();

            //// Rotate back only if we are not already at the original rotation
            //else if (Quaternion.Angle(transform.rotation, originalRotation) >= 0.1f)
            //{
            //    // Wait a before resetting while the player is not in view 
            //    // and bee is still in idle state
            //    var time = Time.time + resetDelay;
            //    while (IsIdled && !Bee.IsAwareOfPlayer && Time.time < time)
            //        yield return new WaitForEndOfFrame();

            //    // Smoothly rotate back to the original direction
            //    var targetRotation = originalRotation;
            //    while (IsIdled && !Bee.IsAwareOfPlayer && Quaternion.Angle(transform.rotation, targetRotation) >= 0.1f)
            //    {
            //        Bee.Rotate(targetRotation.eulerAngles, rotationSpeed);
            //        yield return new WaitForFixedUpdate();
            //    }

            //    // Snap to it
            //    if (IsIdled && !Bee.IsAwareOfPlayer)
            //        Bee.Rotate(originalRotation);
            //}

            yield return new WaitForEndOfFrame();
        }
    }
}
