using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float sensX = 6f;
    [SerializeField] float sensY = 6f;
    [SerializeField] Transform camera;
    [SerializeField] Transform camTransform;
    [SerializeField] Transform camPos;
    [SerializeField] Transform orientation;
    [SerializeField] WallRunning wallrunScript;
    [SerializeField] Transform GunHandler;

    Transform currentPos;

    float mouseX;
    float mouseY;
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
        TakeInput();

        camera.transform.position = currentPos.position;
        camera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, wallrunScript.currentTilt);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        GunHandler.rotation = camera.transform.rotation;
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