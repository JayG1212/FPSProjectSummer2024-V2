// Written by Jay Gunderson
// 06/11/2024
/*
 Update 06/11/2024:
    Added grounding and drag to my player's movement 
    to make movement smoother
 */
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


    // Ground checker
    public float playerHeight; // The height of the player
    public float groundDrag; // The drag we will add to our player's movement
    public LayerMask whatIsGround; // A layer we will apply to our planes
    bool grounded; // Tracks whether the player is grounded
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Gets the rigidbody we declared in our editor
        rb.freezeRotation = true; // Freezes our player's rotation, preventing us from falling over like earlier
    }

  
    // Update is called once per frame
    void Update()
    {
        // Ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround); //  Deterrmiens if player is grounded
        MyInput(); // Fetches keyboard input each frame

        // Handles drag
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
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
