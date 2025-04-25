using UnityEngine;

public class SimpleCameraLook : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    [Header("Camera Sway")]
    public float swayAmount = 2f;
    public float swaySmoothness = 4f;

    private float xRotation = 0f;
    private float roll = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Vertical look (pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Horizontal look (yaw) on player body
        playerBody.Rotate(Vector3.up * mouseX);

        // Camera sway based on mouseX
        float targetRoll = -mouseX * swayAmount;
        roll = Mathf.Lerp(roll, targetRoll, Time.deltaTime * swaySmoothness);

        // Apply pitch + roll to camera only
        transform.localRotation = Quaternion.Euler(xRotation, 0f, roll);
    }
}
