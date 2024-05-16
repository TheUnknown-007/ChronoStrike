using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Space, SerializeField] Transform orientation;

    [Header("Parameters")]
    [SerializeField] float playerHeight = 2;
    [SerializeField] float jumpForce = 15;

    [Header("Movement")]
    [SerializeField] float walkSpeed = 4;
    [SerializeField] float acceleration = 10;
    [SerializeField] float airMultiplier = 0.5f;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6;
    [SerializeField] float airDrag = 2;

    [Header("Footsteps Parameters")]
    [SerializeField] float walkFootstepDelay = 0.6f;


    [Header("Keybinds")]
    [SerializeField] KeyCode jumpkey = KeyCode.Space;

    bool isMoving;
    bool isGrounded;
    float moveSpeed;
    float verticalMovement;
    float horizontalMovement;

    float maxMoveVelocity;

    Vector3 moveDirection;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        moveSpeed = walkSpeed;
    }

    void Update()
    {
        if(PlayerManager.instance.isDead) return;
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, playerHeight/2+0.1f, groundMask);
        moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
        
        ControlDrag();
        TakeInput();

        if(Input.GetKeyDown(jumpkey) && isGrounded) Jump();
        maxMoveVelocity = Math.Max(maxMoveVelocity, rb.velocity.magnitude);
    }

    void FixedUpdate()
    {
        if(isMoving) rb.AddForce(moveDirection.normalized * moveSpeed * (isGrounded ? 1 : airMultiplier), ForceMode.Acceleration);
        if(rb.velocity.magnitude >= 12) rb.velocity = Vector3.ClampMagnitude(rb.velocity, 12);
    }

    void ControlDrag()
    {
        if(isGrounded) rb.drag = groundDrag;
        else rb.drag = airDrag;
    }


    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void TakeInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
        if(horizontalMovement == 0 && verticalMovement == 0)
        {
            isMoving = false;
            return;
        }

        if(isGrounded) isMoving = true;
        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    }
}
