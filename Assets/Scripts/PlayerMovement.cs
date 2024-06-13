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

    private float moveSpeed; // Walking speed
    public float walkSpeed;
    public float runSpeed;
    public KeyCode sprintKey = KeyCode.LeftShift;
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

    // Jumping
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;
    public KeyCode jumpKey = KeyCode.Space;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Gets the rigidbody we declared in our editor
        rb.freezeRotation = true; // Freezes our player's rotation, preventing us from falling over like earlier
        readyToJump = true;
        
    }

  
    // Update is called once per frame
    void Update()
    {
        // Ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround); //  Deterrmiens if player is grounded
        MyInput(); // Fetches keyboard input each frame
        SpeedControl();
       
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

        // Checks if space is pressed, then jump if it was
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // Calculates movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput; // Allows us to walk in the dirrection we are looking

        // On Ground
        if (grounded) 
        {
            // Checks if isRunning is true, and if so will set current speed to runspeed
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            
            if (isRunning)
            {
                moveSpeed = runSpeed;
            }

            else
            {
                moveSpeed = walkSpeed;
            }
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force); // Adds force to our movements  
        }
        else if(!grounded)
        {
            moveSpeed = walkSpeed;
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force); // Adds force to our movements 
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        // Limits velocity
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
        
        
    }

    private void Jump()
    {
        // Reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse); // Moves player up by jump force (Jumps)
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

}
