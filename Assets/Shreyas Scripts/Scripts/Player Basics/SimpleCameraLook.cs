using UnityEngine;
using UnityEngine.InputSystem;
namespace Shreyas
{
    public class SimpleCameraLookInputSystem : MonoBehaviour
    {
        [Header("Mouse Look Settings")]
        public float mouseSensitivity = 100f;
        public float controllerSensitivity = 300f; // This is usually higher

        public Transform playerBody;

        [Header("Camera Sway")]
        public float swayAmount = 2f;
        public float swaySmoothness = 4f;

        private float xRotation = 0f;
        private float roll = 0f;

        private Vector2 lookInput;
        private PlayerControls inputActions;
        public FirstPersonMovementInputSystem FirstPersonMovementInputSystem;

        void Awake()
        {
            inputActions = new PlayerControls();
            inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;
        }

        void OnEnable() => inputActions.Enable();
        void OnDisable() => inputActions.Disable();

        void Start()
        {

            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
        }

        void Update()
        {
            if (FirstPersonMovementInputSystem.playerBusy)
                return;

            bool isGamepad = Gamepad.current != null && Gamepad.current.enabled && Gamepad.current.wasUpdatedThisFrame;

            float currentSensitivity = isGamepad ? controllerSensitivity : mouseSensitivity;

            float mouseX = lookInput.x * currentSensitivity * Time.deltaTime;
            float mouseY = lookInput.y * currentSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            playerBody.Rotate(Vector3.up * mouseX);

            float targetRoll = -mouseX * swayAmount;
            roll = Mathf.Lerp(roll, targetRoll, Time.deltaTime * swaySmoothness);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, roll);
        }

    }

}

