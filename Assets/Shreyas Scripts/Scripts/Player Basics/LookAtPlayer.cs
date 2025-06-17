using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;  // Assign the player's camera in the Inspector
    [SerializeField] private bool flip = false;       // Optional: flip if it faces backward
    private void Start()
    {
        playerCamera = FindFirstObjectByType<Camera>().transform;
    }

    void LateUpdate()
    {
        if (playerCamera == null)
            return;

        // Make the canvas look at the camera
        Vector3 lookDirection = transform.position - playerCamera.position;
        if (flip)
            lookDirection = -lookDirection;

        transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    }
}
