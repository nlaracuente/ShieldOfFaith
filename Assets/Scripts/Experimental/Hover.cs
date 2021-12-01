using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{    void Update()
    {
        var target = new Vector3(transform.position.x, Mathf.Cos(Time.time), transform.position.z);
        transform.position = target;
    }
}
