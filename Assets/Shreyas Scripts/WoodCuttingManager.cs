using UnityEngine;
namespace Shreyas
{
    public class WoodCuttingManager : MonoBehaviour
    {
        [Header("References")]
        public Camera topDownCamera;
        public LayerMask logLayerMask;       // For full logs
        public LayerMask halfLogLayerMask;   // For half logs
        public LayerMask snapPointLayerMask;
        public LayerMask tableLayerMask;
        public float pickUpHeight = 1.5f;
        public float snapDistance = 1f;
        public float moveSpeed = 1f;

        private GameObject hoveredLog;
        public GameObject hoveredHalfLog;
        public GameObject grabbedLog;
        private GameObject snappedLog;
        private Transform nearestSnapPoint;
        private bool isDragging = false;

        public BoxCollider SawCollider;

        public FirstPersonMovementInputSystem FirstPersonMovementInputSystem;
        private void Update()
        {
            if (!FirstPersonMovementInputSystem.playerBusy)
                return;

            if (!isDragging)
            {
                if (snappedLog == null)
                    HandleHoverFullLog();

                HandleHoverHalfLog();
            }

            HandlePickupAndSnap();
            MoveSnappedLog();
        }

        void HandleHoverFullLog()
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
                if (hoveredLog != null)
                {
                    EnableOutline(hoveredLog, false);
                    hoveredLog = null;
                }
            }
        }

        void HandleHoverHalfLog()
        {
            if (grabbedLog != null) return;
            if (hoveredLog != null) return;

            Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, halfLogLayerMask))
            {
                GameObject halfLog = hit.collider.gameObject;
                if (hoveredHalfLog != halfLog)
                {
                    ClearHover();
                    hoveredHalfLog = halfLog;
                    EnableOutline(hoveredHalfLog, true);
                }
            }
            else
            {
                if (hoveredHalfLog != null)
                {
                    EnableOutline(hoveredHalfLog, false);
                    hoveredHalfLog = null;
                }
            }
        }

        void HandlePickupAndSnap()
        {
            if (Input.GetMouseButtonDown(0) && !isDragging)
            {
                if (hoveredLog != null)
                {
                    grabbedLog = hoveredLog;
                    ClearHover();
                    isDragging = true;
                    SawCollider.isTrigger = false;
                }
                else if (hoveredHalfLog != null)
                {
                    grabbedLog = hoveredHalfLog;
                    ClearHover();
                    isDragging = true;
                    SawCollider.isTrigger = false;
                }
                SawCollider.isTrigger = false;
            }

            if (grabbedLog != null && isDragging)
            {
                MoveGrabbedLog();
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (nearestSnapPoint != null && grabbedLog.layer != 10)
                {
                    grabbedLog.transform.position = new Vector3(nearestSnapPoint.position.x, grabbedLog.transform.position.y, nearestSnapPoint.position.z);
                    grabbedLog.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                    Rigidbody rb = grabbedLog.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                        rb.useGravity = true;
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }

                    snappedLog = grabbedLog;
                    grabbedLog = null;
                    isDragging = false;
                    EnableOutline(snappedLog, true);
                }
                else
                {
                    grabbedLog = null;
                    isDragging = false;
                }
                SawCollider.isTrigger = true;
                nearestSnapPoint = null;
                SawCollider.isTrigger = true;
            }
        }

        void MoveSnappedLog()
        {
            if (snappedLog == null) return;

            float moveInput = 0f;
            if (Input.GetKey(KeyCode.W))
            {
                moveInput = 1f;
            }

            if (moveInput != 0f)
            {
                Vector3 moveDirection = snappedLog.transform.forward;
                snappedLog.transform.position += moveDirection * moveInput * moveSpeed * Time.deltaTime;
            }
        }

        void MoveGrabbedLog()
        {
            Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~(logLayerMask | halfLogLayerMask | snapPointLayerMask | tableLayerMask)))
            {
                Vector3 targetPosition = hit.point;
                targetPosition.y = pickUpHeight;
                grabbedLog.transform.position = targetPosition;

                FindNearestSnapPoint();

                if (nearestSnapPoint != null)
                {
                    float distance = Vector3.Distance(grabbedLog.transform.position, nearestSnapPoint.position);
                    if (distance <= snapDistance && grabbedLog.layer != 10)
                    {
                        grabbedLog.transform.position = nearestSnapPoint.position;
                        grabbedLog.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        //SawCollider.isTrigger = true;
           
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

            if (hoveredHalfLog != null)
            {
                EnableOutline(hoveredHalfLog, false);
                hoveredHalfLog = null;
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

}

