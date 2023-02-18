using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [SerializeField] Transform orientation;

    [Header("Wallrun")]
    [SerializeField] KeyCode jumpkey = KeyCode.Space;
    [SerializeField] float wallDistance = 0.5f;
    [SerializeField] float minimumJumpHeight = 1.5f;
    [SerializeField] float wallrunGravity;
    [SerializeField] float wallrunJumpForce = 5f;
    [SerializeField] float wallrunForwardForce = 0.05f;

    [Header("Camera")]
    [SerializeField] Camera cam;
    [SerializeField] float fov;
    [SerializeField] float wallrunFov;
    [SerializeField] float wallrunFovTime;
    [SerializeField] float camTilt;
    [SerializeField] float camTiltTime;
    [SerializeField] float nearClip;
    [SerializeField] float wallrunNearClip;

    public float currentTilt { get; private set; }

    bool wallLeft = false;
    bool wallRight = false;

    RaycastHit wallLeftHit;
    RaycastHit wallRightHit;
    Rigidbody rb;

    bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight);
    }

    void CheckWall()
    {
        wallLeft = (Physics.Raycast(transform.position, -orientation.right, out wallLeftHit, wallDistance) && wallLeftHit.transform.CompareTag("WallRunnable"));
        wallRight = (Physics.Raycast(transform.position, orientation.right, out wallRightHit, wallDistance) && wallRightHit.transform.CompareTag("WallRunnable"));
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        CheckWall();
        if(CanWallRun())
        {
            if(wallLeft) StartWallRun();
            if(wallRight) StartWallRun();
            else StopWallRun();
        }
        else StopWallRun();
    }

    void StartWallRun()
    {
        cam.nearClipPlane = wallrunNearClip;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wallrunFov, wallrunFovTime * Time.deltaTime);

        rb.useGravity = false;
        rb.AddForce(Vector3.down * wallrunGravity, ForceMode.Force);
        rb.AddForce(orientation.forward * wallrunForwardForce, ForceMode.Force);

        if(wallLeft) currentTilt = Mathf.Lerp(currentTilt, -camTilt, camTiltTime * Time.deltaTime);
        if(wallRight) currentTilt = Mathf.Lerp(currentTilt, camTilt, camTiltTime * Time.deltaTime);
        
        if(Input.GetKeyDown(jumpkey))
        {
            if(wallLeft)
            {
                Vector3 wallrunJumpDir = transform.up + wallLeftHit.normal;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(wallrunJumpDir * wallrunJumpForce * 100, ForceMode.Force);
            }
            if(wallRight)
            {
                Vector3 wallrunJumpDir = transform.up + wallRightHit.normal;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(wallrunJumpDir * wallrunJumpForce * 100, ForceMode.Force);
            }
        }
    }

    void StopWallRun()
    {
        cam.nearClipPlane = nearClip;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, wallrunFovTime * Time.deltaTime);
        currentTilt = Mathf.Lerp(currentTilt, 0, camTiltTime * Time.deltaTime);
        rb.useGravity = true;
    }
}
