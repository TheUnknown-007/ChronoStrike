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
    [SerializeField] float enhancedMaxVel = 20;
    [SerializeField] float maxVelocity = 12;
    [SerializeField] float walkSpeed = 65;
    [SerializeField] float enhancedSpeed = 80;
    [SerializeField] float acceleration = 10;
    [SerializeField] float airMultiplier = 0.5f;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6;
    [SerializeField] float airDrag = 2;

    [Header("Slope Handling")]
    [SerializeField] float maxSlopeAngle;
    RaycastHit slopeHit;


    [Header("Keybinds")]
    [SerializeField] KeyCode jumpkey = KeyCode.Space;

    [HideInInspector] public bool isEnhanced;
    bool isMoving;
    bool isGrounded;
    float moveSpeed;
    float verticalMovement;
    float horizontalMovement;

    float frameDelay = 0;

    bool walkingOnSlope;

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
        
        if(frameDelay <= 0) 
        {
            isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, playerHeight/2+0.1f, groundMask);
            if(isGrounded && !walkingOnSlope) rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        } else frameDelay -= Time.deltaTime;

        moveSpeed = Mathf.Lerp(moveSpeed, isEnhanced ? enhancedSpeed : walkSpeed, acceleration * Time.deltaTime);
        
        ControlDrag();
        TakeInput();
        CheckSlope();
        
        rb.useGravity = !walkingOnSlope;
        GetComponent<ConstantForce>().enabled = !walkingOnSlope;

        if(Input.GetKeyDown(jumpkey) && isGrounded) Jump(jumpForce);
    }

    void FixedUpdate()
    {
        if(!isMoving) return;


        if(walkingOnSlope)
            rb.AddForce(GetSlopeMoveDirection()  * moveSpeed * (isGrounded ? 1 : airMultiplier), ForceMode.Acceleration);
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * (isGrounded ? 1 : airMultiplier), ForceMode.Acceleration);
    }

    void ControlDrag()
    {
        if(isGrounded) rb.drag = groundDrag;
        else rb.drag = airDrag;
        
        if(!walkingOnSlope)
        {
            Vector3 flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if(flatVelocity.magnitude >= (isEnhanced ? enhancedMaxVel : maxVelocity))
                rb.velocity = new Vector3((flatVelocity.normalized * (isEnhanced ? enhancedMaxVel : maxVelocity)).x, rb.velocity.y, (flatVelocity.normalized * (isEnhanced ? enhancedMaxVel : maxVelocity)).z);
        }
    }

    public void Jump(float power)
    {
        isGrounded = false;
        frameDelay = 1;

        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * power, ForceMode.Impulse);
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

        isMoving = true;
        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    }

    void CheckSlope()
    {
        walkingOnSlope = false;
        if(Physics.Raycast(groundCheck.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            walkingOnSlope = angle < maxSlopeAngle && angle != 0;
        }
    }

    Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
