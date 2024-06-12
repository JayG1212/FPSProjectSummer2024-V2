// Written by Jay Gunderson
// 06/11/2024
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    // Movement

    public float moveSpeed; // Walking speed
    public Transform orientation; // Player's orientation
    
    float horizontalInput; // Horizontal keyboard inputs
    float verticalInput; // Vertical keyboard inputs

    Vector3 moveDirection; 
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Gets the rigidbody we declared in our editor
        rb.freezeRotation = true; // Freezes our player's rotation, preventing us from falling over like earlier
    }

  
    // Update is called once per frame
    void Update()
    {
        MyInput(); // Fetches keyboard input each frame
    }
    private void FixedUpdate() // Better for physics related updates
    {
        MovePlayer();
    }
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); // A and D & Left/Right arrow keys
        verticalInput = Input.GetAxisRaw("Vertical"); // W and S & Up/Down arrow keys
    }

    private void MovePlayer()
    {
        // Calculates movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput; // Allows us to walk in the dirrection we are looking
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force); // Adds force to our movements
    }
}
