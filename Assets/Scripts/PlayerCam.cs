// Written by Jay Gunderson
// 06/11/2024
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    // Variables
    public float sensitivityX; // sensitivity for the X-axis
    public float sensitivityY; // sensitivity for the Y-axis

    public Transform orientation; // Player's orientation

    float xRotation; // X rotation of camera
    float yRotation; // Y rotation of camera
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Locks the cursor in the middle of the screen
        Cursor.visible = false; // Hides the cursor
    }

    // Update is called once per frame
    void Update()
    {
        // Mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivityX; // X-Axis 
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivityY; // Y-Axis
        
        // Rotations
        yRotation = yRotation + mouseX; 
        xRotation = xRotation - mouseY; 
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // This limits our y rotation to 90 degrees

        // Rotate camera and orientation of player
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        
    }
}
