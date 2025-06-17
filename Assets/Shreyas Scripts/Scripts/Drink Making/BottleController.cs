using UnityEngine;

public class BottleController : MonoBehaviour
{
    [Header("Settings")]
    public float followSpeed = 15f;
    public float fixedYPosition = 1f; // Table height
    public float rotationSpeed = 100f;
    public Camera mainCamera;

    private bool isGrabbed = false;
    private Vector3 grabOffset;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        HandleMouseInput();

        if (isGrabbed)
        {
            FollowMouseXZ();
            HandleRotation();
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    isGrabbed = true;

                    // Get offset between object and hit point
                    grabOffset = transform.position - hit.point;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isGrabbed = false;
        }
    }

    void FollowMouseXZ()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane dragPlane = new Plane(Vector3.up, new Vector3(0, fixedYPosition, 0));

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 targetPosition = hitPoint + grabOffset;

            // Lock Y to table height
            targetPosition.y = fixedYPosition;

            // Smooth movement
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        }
    }

    void HandleRotation()
    {
        float input = 0f;

        if (Input.GetKey(KeyCode.A)) input = 1f;
        if (Input.GetKey(KeyCode.D)) input = -1f;

        transform.Rotate(Vector3.up * input * rotationSpeed * Time.deltaTime, Space.Self);
    }
}
