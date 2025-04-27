using UnityEngine;

public class WoodCuttingManager : MonoBehaviour
{
    [Header("References")]
    public Camera topDownCamera;
    public LayerMask logLayerMask;
    public LayerMask snapPointLayerMask;
    public float pickUpHeight = 1.5f;
    public float snapDistance = 1f; // Distance to auto-snap

    private GameObject hoveredLog;
    private GameObject grabbedLog;
    private Transform nearestSnapPoint;
    private bool isLogSnapped = false; // Track if a log is snapped

    private void Update()
    {
        if (!isLogSnapped) // Only allow hover and pickup if no log is snapped
        {
            HandleHover();
            HandlePickupAndSnap();
        }
    }

    void HandleHover()
    {
        if (grabbedLog != null) return;

        Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, logLayerMask))
        {
            GameObject log = hit.collider.gameObject;
            if (hoveredLog != log)
            {
                ClearHover();
                hoveredLog = log;
                EnableOutline(hoveredLog, true);
            }
        }
        else
        {
            ClearHover();
        }
    }

    void HandlePickupAndSnap()
    {
        if (Input.GetMouseButtonDown(0) && hoveredLog != null)
        {
            grabbedLog = hoveredLog;
            ClearHover();
        }

        if (grabbedLog != null)
        {
            MoveGrabbedLog();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (nearestSnapPoint != null)
            {
                // Snap the log and disable further interaction
                grabbedLog.transform.position = nearestSnapPoint.position;
                grabbedLog.transform.rotation = nearestSnapPoint.rotation;
                isLogSnapped = true; // Log is now snapped
            }
            grabbedLog = null;
            nearestSnapPoint = null;
        }
    }

    void MoveGrabbedLog()
    {
        Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~(logLayerMask | snapPointLayerMask)))
        {
            Vector3 targetPosition = hit.point;
            targetPosition.y = pickUpHeight;
            grabbedLog.transform.position = targetPosition;

            // Check for nearest snap point
            FindNearestSnapPoint();

            if (nearestSnapPoint != null)
            {
                float distance = Vector3.Distance(grabbedLog.transform.position, nearestSnapPoint.position);
                if (distance <= snapDistance)
                {
                    grabbedLog.transform.position = nearestSnapPoint.position;
                    grabbedLog.transform.rotation = nearestSnapPoint.rotation;
                }
            }
        }
    }

    void FindNearestSnapPoint()
    {
        Collider[] snapPoints = Physics.OverlapSphere(grabbedLog.transform.position, snapDistance * 2, snapPointLayerMask);

        float closestDistance = Mathf.Infinity;
        Transform closestPoint = null;

        foreach (var collider in snapPoints)
        {
            float dist = Vector3.Distance(grabbedLog.transform.position, collider.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPoint = collider.transform;
            }
        }

        nearestSnapPoint = closestPoint;
    }

    void ClearHover()
    {
        if (hoveredLog != null)
        {
            EnableOutline(hoveredLog, false);
            hoveredLog = null;
        }
    }

    void EnableOutline(GameObject obj, bool enable)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = enable;
        }
    }
}
