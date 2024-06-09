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



    [Header("Keybinds")]
    [SerializeField] KeyCode jumpkey = KeyCode.Space;

    [HideInInspector] public bool isEnhanced;
    bool isMoving;
    bool isGrounded;
    float moveSpeed;
    float verticalMovement;
    float horizontalMovement;

    int frameDelay = 0;

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
        
        if(frameDelay == 0) 
        {
            isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, playerHeight/2+0.1f, groundMask);
            if(isGrounded) rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        } else frameDelay--;

        moveSpeed = Mathf.Lerp(moveSpeed, isEnhanced ? enhancedSpeed : walkSpeed, acceleration * Time.deltaTime);
        
        ControlDrag();
        TakeInput();

        if(Input.GetKeyDown(jumpkey) && isGrounded) Jump(jumpForce);
    }

    void FixedUpdate()
    {
        if(isMoving)
            rb.AddForce(moveDirection.normalized * moveSpeed * (isGrounded ? 1 : airMultiplier), ForceMode.Acceleration);
        ControlDrag();
    }

    void ControlDrag()
    {
        if(isGrounded) rb.drag = groundDrag;
        else rb.drag = airDrag;
        if(new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude >= (isEnhanced ? enhancedMaxVel : maxVelocity))
            rb.velocity = new Vector3((rb.velocity.normalized * (isEnhanced ? enhancedMaxVel : maxVelocity)).x, rb.velocity.y, (rb.velocity.normalized * (isEnhanced ? enhancedMaxVel : maxVelocity)).z);
    }

    public void Jump(float power)
    {
        isGrounded = false;
        frameDelay = 10;

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
}
