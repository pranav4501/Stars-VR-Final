using UnityEngine;
using System.Globalization;
public class CameraController: MonoBehaviour
{
    public float speed = 1.0f;
    public float zoomSpeed = 2.0f;

    void Update()
    {   
        float horizontal = Input.GetAxis("Horizontal") * speed;
        float vertical = Input.GetAxis("Vertical") * speed;
        float zoom = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        
        // Rotate the camera
        if (Input.GetMouseButton(0))
        {
            float rotationX = Input.GetAxis("Mouse X") * speed;
            float rotationY = Input.GetAxis("Mouse Y") * speed;
            transform.Rotate(rotationY, rotationX, 0);
        }

        transform.Translate(horizontal * Time.deltaTime, vertical * Time.deltaTime, zoom * Time.deltaTime, Space.World);
    }
}
