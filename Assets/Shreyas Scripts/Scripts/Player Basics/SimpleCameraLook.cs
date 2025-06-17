using UnityEngine;
using UnityEngine.InputSystem;

namespace Shreyas
{
    public class SimpleCameraLookInputSystem : MonoBehaviour
    {
        [Header("Mouse Look Settings")]
        public float mouseSensitivity = 100f;
        public float controllerSensitivity = 300f;

        public Transform playerBody;
        public FirstPersonMovementInputSystem FirstPersonMovementInputSystem;

        private float xRotation = 0f;
        private Vector2 lookInput;

        private PlayerControls inputActions;

        void Awake()
        {
            inputActions = new PlayerControls();
            inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            inputActions.Player.Look.canceled += _ => lookInput = Vector2.zero;
        }

        void OnEnable() => inputActions.Enable();
        void OnDisable() => inputActions.Disable();

        void Update()
        {
            if (FirstPersonMovementInputSystem.playerBusy)
                return;

            if (FirstPersonMovementInputSystem.restrictMovement)
              return;

            bool usingGamepad = Gamepad.current != null && Gamepad.current.enabled && Gamepad.current.wasUpdatedThisFrame;
            float sensitivity = usingGamepad ? controllerSensitivity : mouseSensitivity;

            float mouseX = lookInput.x * sensitivity * Time.deltaTime;
            float mouseY = lookInput.y * sensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // Rotate the camera (up/down)
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Rotate the body (left/right)
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
