using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRigController : MonoBehaviour
{
    [SerializeField]
    bool isATrackingCamera;

    [Header("Autoscrolling Camera")]
    [SerializeField]
    float scrollSpeed = 3f;

    [SerializeField]
    Transform startingXForm;

    [SerializeField]
    Transform endingXForm;

    [Header("Tracking Camera")]
    [SerializeField] Vector3 Offset;
    [SerializeField] float trackingSpeed = 0.3f;
    [SerializeField] Vector3 axisToTrack = Vector3.forward;
    // [SerializeField] float rotationDegrees = 60f;

    Vector3 velocity = Vector3.zero;
    Player Player { get { return GameManager.instance.Player; } }

    LevelState levelState { get { return LevelController.instance.State; } }
    public bool MoveCamera { set; protected get; }

    private void Start()
    {
        transform.position = startingXForm.position;

        Offset = Player.transform.position - transform.position;
    }

    private void LateUpdate()
    {
        if (!MoveCamera)
            return;

        if(isATrackingCamera)
        {
            Vector3 targetPosition = Player.transform.position + Offset;
            if (axisToTrack.x < 1) targetPosition.x = transform.position.x;
            if (axisToTrack.y < 1) targetPosition.y = transform.position.y;
            if (axisToTrack.z < 1) targetPosition.z = transform.position.z;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, trackingSpeed);
            return;
        }

        if (Vector3.Distance(transform.position, endingXForm.position) > 0.01f)
            transform.position = Vector3.MoveTowards(transform.position, endingXForm.position, scrollSpeed * Time.deltaTime);
        else
            SnapToEnd();
    }

    public void SnapToEnd() => transform.position = endingXForm.position;

    //private void LateUpdate()
    //{
    //    Vector3 targetPosition = Player.transform.position + Offset;

    //    if (axisToTrack.x < 1) targetPosition.x = transform.position.x;
    //    if (axisToTrack.y < 1) targetPosition.y = transform.position.y;
    //    if (axisToTrack.z < 1) targetPosition.z = transform.position.z;

    //    transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, trackingSpeed);
    //    RotateAroundTarget(targetPosition);
    //}

    //private void RotateAroundTarget(Vector3 targetPosition)
    //{
    //    // Try to rotate with mouse first else use buttons
    //    if (RotateCameraRequested())
    //        transform.RotateAround(targetPosition, Vector3.up, Input.GetAxis("Mouse X") * rotationDegrees * Time.deltaTime);
    //    else
    //    {
    //        var left = Input.GetKey(KeyCode.Q) ? -1f : 0f;
    //        var right = Input.GetKey(KeyCode.E) ? 1f : 0f;
    //        var dir = left + right;

    //        if (dir != 0)
    //            transform.RotateAround(targetPosition, Vector3.up, dir * rotationDegrees * Time.deltaTime);
    //    }
    //}

    //bool RotateCameraRequested()
    //{
    //    // For those who don't have a second mouse button or prefer to hold crtl and left mouse click
    //    var holdingCrtl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    //    return Input.GetMouseButton(1) || (Input.GetMouseButton(0) && holdingCrtl);
    //}
}
