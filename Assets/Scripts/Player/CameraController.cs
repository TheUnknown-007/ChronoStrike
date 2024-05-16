using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float sensX = 6f;
    [SerializeField] float sensY = 6f;
    [SerializeField] Transform mainCamera;
    [SerializeField] Transform camTransform;
    [SerializeField] Transform camPos;
    [SerializeField] Transform orientation;
    [SerializeField] WallRunning wallrunScript;
    [SerializeField] Transform GunHandler;

    Transform currentPos;

    public float mouseX;
    public float mouseY;
    float multiplier = 0.01f;
    float xRotation;
    float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentPos = camPos;
    }

    void Update()
    {
        if(PlayerManager.instance.isDead) return;

        if(Input.GetButtonDown("Fire1"))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        TakeInput();

        mainCamera.transform.position = currentPos.position;
        mainCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        GunHandler.rotation = mainCamera.transform.rotation;
    }

    void LateUpdate()
    {
        GunHandler.position = camTransform.transform.position;
    }

    void TakeInput()
    {
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        yRotation += mouseX * sensX * multiplier;
        xRotation -= mouseY * sensY * multiplier;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }

    public void ChangeCameraPosition(Transform newPos)
    {
        currentPos = newPos;
    }

    public void ResetCameraPosition()
    {
        currentPos = camPos;
    }
}