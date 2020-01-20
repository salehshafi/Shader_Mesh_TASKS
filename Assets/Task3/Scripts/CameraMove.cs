using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform target;

    public float speed = 2;

    void LateUpdate()
    {
        // Cursor Input to view in X-Y Axis
        transform.RotateAround(target.position, transform.right, Input.GetAxis("Vertical") * speed);
        transform.RotateAround(target.position, transform.up, Input.GetAxis("Horizontal") * speed);


        // Scroll Input to Zoom in-out (Z-Axis)
        float fov = GetComponent<Camera>().fieldOfView;
        fov += Input.GetAxis("Mouse ScrollWheel") * speed;
        fov = Mathf.Clamp(fov, 5, 100);
        GetComponent<Camera>().fieldOfView = fov;
    }

}
