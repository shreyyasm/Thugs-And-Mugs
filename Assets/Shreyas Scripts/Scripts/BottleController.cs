using UnityEngine;

namespace Shreyas
{
    public class BottleController : MonoBehaviour
    {
        public float rotationSpeed = 100f;
        public float followSpeed = 15f;
        public Camera mainCamera;

        void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        void Update()
        {
            FollowMouse();
            HandleRotation();
        }

        void FollowMouse()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero); // Flat surface at y=0
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 targetPos = ray.GetPoint(distance);
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
            }
        }

        void HandleRotation()
        {
            float rotateInput = 0f;

            if (Input.GetKey(KeyCode.A))
                rotateInput = 1f;
            else if (Input.GetKey(KeyCode.D))
                rotateInput = -1f;

            transform.Rotate(Vector3.right * rotateInput * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

}

