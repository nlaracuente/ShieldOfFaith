using UnityEngine;

public class PathNode : MonoBehaviour
{
    [SerializeField, Tooltip("True: will use the direction, duration, and rotation speed to stop/look when it arrives")]
    bool hasStopRoutine = false;
    public bool HasStopRoutine { get { return hasStopRoutine; } }

    [SerializeField, Tooltip("Which direction to look at when they reach here")]
    Direction directionToLookAt;
    public Vector3 DirectionToLookAt { get { return GameManager.instance.DirectionNameToVector(directionToLookAt); } }

    [SerializeField, Tooltip("How many seconds to stay where before moving again")]
    float duration = 1f;
    public float Duration { get { return duration; } }

    [SerializeField, Tooltip("How quickly to face the direction")]
    float rotationSpeed = 15f;
    public float RotationSpeed { get { return rotationSpeed; } }

    [SerializeField]
    Color nodeColor = Color.red;

    [SerializeField]
    float nodeRadius = .25f;

    public void DrawGizmo()
    {
        // Draw a the node
        Gizmos.color = nodeColor;
        Gizmos.DrawSphere(transform.position, nodeRadius);

       // if (TargetMarker != null)
       //     TargetMarker.Value = $"1";
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a the node
        Gizmos.color = nodeColor;
        Gizmos.DrawSphere(transform.position, nodeRadius);
    }
}
