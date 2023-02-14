using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] int fpsCap;
    [Space, SerializeField] Transform orientation;

    [Header("Parameters")]
    [SerializeField] float playerHeight = 2;
    [SerializeField] float jumpForce = 15;

    [Header("Movement")]
    [SerializeField] Animator cameraAnimator;
    [SerializeField] CapsuleCollider playerCollider;
    [SerializeField] float walkSpeed = 4;
    [SerializeField] float sprintSpeed = 6;
    [SerializeField] float acceleration = 10;
    [SerializeField] float airMultiplier = 0.5f;

    [Header("Ground Detection")]
    [SerializeField] float groundDistance = 0.4f;
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform crouchGroundCheck;
    [SerializeField] LayerMask groundMask;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6;
    [SerializeField] float airDrag = 2;

    [Header("Headbob Parameters")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] bool headbobEnabled = true;
    [SerializeField] float headbobSpeed = 14f;
    [SerializeField] float headbobAmount = 0.05f;
    [SerializeField] float sprintingHeadbobSpeedY = 18f;
    [SerializeField] float sprintingHeadbobAmountY = 0.11f;
    [SerializeField] float sprintingHeadbobSpeedX = 18f;
    [SerializeField] float sprintingHeadbobAmountX = 0.11f;

    [Header("Footsteps Parameters")]
    [SerializeField] float walkFootstepDelay = 0.6f;
    [SerializeField] float runFootstepDelay = 1f;
    [SerializeField] float crouchFootstepDelay = 1.5f;
    [SerializeField] List<AudioSource> audioClips;
    float tempFootstepDelay;

    [Header("Misc")]
    [SerializeField] AudioSource bodyRollAudio;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpkey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;


    float timer;
    float timer2;
    bool isMoving;    
    bool isRunning;
    bool isGrounded;
    bool isCrouched;
    float moveSpeed;
    float rbDrag = 6f;
    float verticalMovement;
    float horizontalMovement;
    int random = 0;

    int debugCount = 0;

    Vector3 slopeMoveDirection;
    Vector3 moveDirection;
    Quaternion defaultRot;
    Vector3 defaultPos;
    RaycastHit slopeHit;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        moveSpeed = walkSpeed;

        defaultPos = cameraTransform.localPosition;
        defaultRot = cameraTransform.localRotation;

        StartCoroutine(PlayFootstepSounds());

        // DEBUG
        //Application.targetFrameRate = fpsCap;
    }

    bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f, groundMask))
            if(slopeHit.normal != Vector3.up) return true;
        return false;
    }

    void Update()
    {
        isGrounded = Physics.Raycast((isCrouched ? crouchGroundCheck : groundCheck).position, Vector3.down, playerHeight/2+0.1f, groundMask);

        if(Input.GetButtonDown("Crouch"))
        {
            isCrouched = true;
            playerCollider.height = 1f;
            isGrounded = false;
        }
        if(Input.GetButtonUp("Crouch"))
        {
            isCrouched = false;
            playerCollider.height = 2f;
            isGrounded = true;
        }        
        
        ControlHeadbob();
        ControlSpeed();
        ControlDrag();
        TakeInput();

        if(Input.GetKeyDown(jumpkey) && isGrounded) Jump();
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    void FixedUpdate()
    {
        if(isMoving) Move();
    }

    void ControlDrag()
    {
        if(isGrounded) rb.drag = groundDrag;
        else rb.drag = airDrag;
    }

    void ControlSpeed()
    {
        if(Input.GetKey(sprintKey) && isGrounded)
        {
            isRunning = true;
            tempFootstepDelay = runFootstepDelay;
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            isRunning = false;
            tempFootstepDelay = walkFootstepDelay;
            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
        }
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

    void Move()
    {
        if(OnSlope()) rb.AddForce(slopeMoveDirection.normalized * moveSpeed * (isGrounded ? 1 : airMultiplier), ForceMode.Acceleration);
        else rb.AddForce(moveDirection.normalized * moveSpeed * (isGrounded ? 1 : airMultiplier), ForceMode.Acceleration);
    }

    void ControlHeadbob()
    {
        timer += Time.deltaTime * (isRunning ? sprintingHeadbobSpeedY : isMoving ? headbobSpeed : 0);
        timer2 += Time.deltaTime * (isRunning ? sprintingHeadbobSpeedX : 0);
        cameraTransform.localPosition = new Vector3
        (
            defaultPos.x,
            defaultPos.y + Mathf.Sin(timer) * (isRunning ? sprintingHeadbobAmountY : isMoving ? headbobAmount : 0),
            defaultPos.z
        );
        cameraTransform.localRotation = new Quaternion
        (
            defaultRot.x,
            defaultRot.y + Mathf.Sin(timer2) * (isRunning ? sprintingHeadbobAmountX : 0),
            defaultRot.z,
            defaultRot.w
        );
    }

    public void Respawn()
    {
        transform.position = Vector3.zero + (Vector3.up * 2.4f);
    }

    IEnumerator PlayFootstepSounds()
    {
        while (true)
        {
            if (!isMoving || !isGrounded)
            {
                yield return new WaitForSeconds(tempFootstepDelay/2);
                continue;
            }
            random = Random.Range(0, audioClips.Count - 1);
            audioClips[random].Play();
            debugCount++;
            yield return new WaitForSeconds(tempFootstepDelay);
        }
    } 
}
