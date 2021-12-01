using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LookAtDirection : MonoBehaviour, IEnemyIdleHandler
{
    [System.Serializable]
    struct Directions
    {
        [Tooltip("Direction to look at")]
        public Direction direction;

        [Tooltip("How long to take to rotate")]
        public float speed;

        [Tooltip("How long to stay looking at this direction")]
        public float duration;
    }

    [SerializeField, Tooltip("Directions, in order, for the bee to look at while idled")]
    List<Directions> directions;

    /// <summary>
    /// Keeps track of the queue so that we can resume wherever we were at last
    /// </summary>
    Queue<Directions> queue;

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

    /// <summary>
    /// Keeps track of the current index on the list we are at
    /// </summary>
    int index = 0;

    public IEnumerator IdleRoutine()
    {
        while (IsIdled && directions.Count > 0)
        {
            // Dequeue/Enqueue so that we can look at it later
            var dir = directions[index];

            var vector = GameManager.instance.DirectionNameToVector(dir.direction);
            vector.y = 0f; // prevent looking "up/down"

            // Rotate to the desired direction
            yield return StartCoroutine(Bee.RotateRoutine(vector, dir.speed));

            // Keep looking at this direction for the expected time before moving to the next direction
            var time = Time.time + dir.duration;
            while (IsIdled && Time.time < time)
                yield return new WaitForEndOfFrame();

            // Now that we've moved and waited 
            // We can move on to the next direction
            if(IsIdled)
            {
                index++;
                if (index >= directions.Count)
                    index = 0;
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
