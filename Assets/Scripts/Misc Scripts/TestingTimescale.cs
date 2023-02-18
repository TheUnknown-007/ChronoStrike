using UnityEngine;

public class TestingTimescale : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) Time.timeScale = 1;
        if (Input.GetKeyDown(KeyCode.Alpha1)) Time.timeScale = 0.1f;
        if (Input.GetKeyDown(KeyCode.Alpha2)) Time.timeScale = 0.2f;
        if (Input.GetKeyDown(KeyCode.Alpha3)) Time.timeScale = 0.3f;
        if (Input.GetKeyDown(KeyCode.Alpha4)) Time.timeScale = 0.4f;
        if (Input.GetKeyDown(KeyCode.Alpha5)) Time.timeScale = 0.5f;
        if (Input.GetKeyDown(KeyCode.Alpha6)) Time.timeScale = 0.6f;
        if (Input.GetKeyDown(KeyCode.Alpha7)) Time.timeScale = 0.7f;
        if (Input.GetKeyDown(KeyCode.Alpha8)) Time.timeScale = 0.8f;
        if (Input.GetKeyDown(KeyCode.Alpha9)) Time.timeScale = 0.9f;
    }
}
