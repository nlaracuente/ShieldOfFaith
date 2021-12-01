using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitAroundTarget : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 axis = Vector3.up;
    [SerializeField] float radius = 2.0f;
    [SerializeField] float radiusSpeed = 0.5f;
    [SerializeField] float rotationSpeed = 80.0f;

    void Start()
    {
        transform.position = (transform.position - target.position).normalized * radius + target.position;
    }

    void Update()
    {
        transform.RotateAround(target.position, axis, rotationSpeed * Time.deltaTime);
        var desiredPosition = (transform.position - target.position).normalized * radius + target.position;
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * radiusSpeed);
    }
}
