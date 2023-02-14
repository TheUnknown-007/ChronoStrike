using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Climb : MonoBehaviour
{
    [Header("Parameters")]
    [Range(1.5f, 2.5f), SerializeField] float extraClimb = 2.4f;
    [SerializeField] float jumpSpeed;
    [SerializeField] float forwardDrift = 1;
    [SerializeField] float grabDistance;
    [SerializeField] float climbSpeed;
    [SerializeField] KeyCode grabKey = KeyCode.Space;

    [Header("References")]
    [SerializeField] Animator playerModel;
    [SerializeField] Transform ledgeClimb;
    [SerializeField] Transform orientation;
    [SerializeField] Collider collider;
    [SerializeField] Transform bottom;
    [SerializeField] Transform lowGrab;
    [SerializeField] Transform midGrab;
    [SerializeField] Transform highGrab;

    bool grabbing;
    bool ledgeClimbing = false;
    int grabType = -1;
    float toMove;
    float objectHeight;

    CameraController camScript;
    PlayerMovement moveScript;
    WallRunning wallRunScript;

    RaycastHit obstacle;
    RaycastHit debugg;
    Rigidbody rb;
    Vector3 target;
    Vector3 HandPos;
    Vector3 ledgeTarget;

    bool CanClimb()
    {
        return true;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        wallRunScript = GetComponent<WallRunning>(); 
        moveScript = GetComponent<PlayerMovement>(); 
        camScript = GetComponent<CameraController>();
    }

    void Update()
    {
        if(grabbing) {
            switch(grabType)
            {
                case 0:
                    GrabClimbLow();
                    break;
                case 1:
                    GrabClimbMid();
                    break;
                case 2:
                    GrabClimbHigh();
                    break;
                case 3:
                    HangOnLedge();
                    break;
            }
            
            return;
        }

        if(Input.GetKeyDown(grabKey) && CanClimb())
        {
            if(!Physics.Raycast(highGrab.transform.position, orientation.forward, out debugg, grabDistance)) // If lower than maximum height
            {
                if(Physics.Raycast(midGrab.transform.position, orientation.forward, out obstacle, grabDistance)) // In between mid and high
                    StartClimbing(2);
                else if(Physics.Raycast(lowGrab.transform.position, orientation.forward, out obstacle, grabDistance)) // In between low and mid
                    StartClimbing(1);
                else if(Physics.Raycast(bottom.transform.position, orientation.forward, out obstacle, grabDistance)) // In between bottom and low
                    StartClimbing(0);
                else grabType = -1;
            }
            else grabType = -1;
        }
        else grabType = -1;
    }

    void StartClimbing(int type)
    {
        if(obstacle.transform.CompareTag("WallRunnable")) return;

        wallRunScript.enabled = false;
        moveScript.enabled = false;
        collider.enabled = false;
        rb.velocity = Vector3.zero;

        float tempClimb = extraClimb;
        if(rb.velocity.z >= 15 || rb.velocity.x >= 15) tempClimb = extraClimb*2;

        objectHeight = obstacle.transform.localScale.y * 0.5f + obstacle.transform.position.y;
        toMove = objectHeight + tempClimb - transform.position.y;
        target = new Vector3(transform.position.x, transform.position.y + toMove, transform.position.z) + orientation.forward*forwardDrift;
        ledgeTarget = new Vector3(transform.position.x, transform.position.y + toMove - tempClimb*1.5f, transform.position.z);

        rb.useGravity = false;
        
        grabbing = true;
        if(obstacle.transform.CompareTag("Ledge")) grabType = 2;
        else grabType = type;
    }

    void StopClimbing()
    {
        ledgeClimbing = false;
        grabbing = false;
        grabType = -1;

        target = Vector3.zero;
        ledgeTarget = Vector3.zero;

        camScript.ResetCameraPosition();

        collider.enabled = true;
        rb.useGravity = true;
        moveScript.enabled = true;
        wallRunScript.enabled = true;
    }

    void HangOnLedge()
    {
        if(ledgeClimbing)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, climbSpeed*2*Time.deltaTime);
            if(transform.position == target) StopClimbing();
        }

        else if (Input.GetKeyDown(grabKey))
        {
            if(Vector3.Angle(-obstacle.normal, orientation.forward) <= 45)
                ledgeClimbing = true;
            else
            {
                rb.AddForce((orientation.forward + obstacle.normal + transform.up*2) * jumpSpeed, ForceMode.Impulse);
                StopClimbing();
            }
        }
    }

    void GrabClimbHigh()
    {
        // 0.7x
        transform.position = Vector3.MoveTowards(transform.position, ledgeTarget, climbSpeed*2.5f*Time.deltaTime);
        if(transform.position == ledgeTarget) grabType = 3;
    }

    void GrabClimbMid()
    {
        // 0.8x
        transform.position = Vector3.MoveTowards(transform.position, target, climbSpeed*1.5f*Time.deltaTime);
        if(transform.position == target) StopClimbing();
    }

    void GrabClimbLow()
    {
        // 0.9x
        transform.position = Vector3.MoveTowards(transform.position, target, climbSpeed*Time.deltaTime);
        if(transform.position == target) StopClimbing();
    }
}
