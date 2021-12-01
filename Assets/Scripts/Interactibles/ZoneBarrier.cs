using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Once all enemies in a zone are defeated this barrier is removed
/// allowing the player to progress
/// </summary>
public class ZoneBarrier : MonoBehaviour
{
    [SerializeField, Tooltip("How much to move into the ground before being removed")]
    float unitsToGround = 2f;

    [SerializeField, Tooltip("How much time it takes to move in seconds")]
    float moveTime = 2f;

    [SerializeField, Tooltip("The enemies that must be defeated for this barrier to be removed")]
    Enemy[] enemies;

    bool barrierRemovalTriggered = false;

    private void Update()
    {
        if (barrierRemovalTriggered || enemies == null || enemies.Where(e => e != null).Any())
            return;

        // All enemies are dead
        StartCoroutine(RemoveBarrierRoutine());
    }

    IEnumerator RemoveBarrierRoutine()
    {
        barrierRemovalTriggered = true;

        var speed = unitsToGround / moveTime;
        var destination = new Vector3(transform.position.x, transform.position.y - unitsToGround, transform.position.z);

        while(Vector3.Distance(transform.position, destination) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }
}
