using UnityEngine;

[ExecuteInEditMode]
public class Billboard : MonoBehaviour
{
    new Camera camera;
    Camera MainCamera
    {
        get
        {
            if (camera == null)
                camera = Camera.main;
            return camera;
        }
    }

    //Orient the camera after all movement is completed this frame to avoid jittering
    void LateUpdate()
    {
        var worldPosition = transform.position + MainCamera.transform.rotation * Vector3.forward;
        var worldUp = MainCamera.transform.rotation * Vector3.up;
        transform.LookAt(worldPosition, worldUp);
    }
}
