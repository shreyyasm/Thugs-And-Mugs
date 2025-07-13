using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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

        [Header("Head Nod Settings")]
        public float nodStrength = 5f;     // How much to rotate Y
        public float nodDuration = 0.4f;   // Total time of nod
        private float nodAngle = 0f;
        private Coroutine nodCoroutine;

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
            if (FirstPersonMovementInputSystem.playerBusy || FirstPersonMovementInputSystem.restrictMovement)
                return;

            bool usingGamepad = Gamepad.current != null && Gamepad.current.enabled && Gamepad.current.wasUpdatedThisFrame;
            float sensitivity = usingGamepad ? controllerSensitivity : mouseSensitivity;

            float mouseX = lookInput.x * sensitivity * Time.deltaTime;
            float mouseY = lookInput.y * sensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // Apply both pitch (X) and nod (Y) to camera
            transform.localRotation = Quaternion.Euler(xRotation, nodAngle, 0f);

            // Rotate the player body (Y)
            playerBody.Rotate(Vector3.up * mouseX);
        }

        /// <summary>
        /// Call this method to play a head nod (once).
        /// </summary>
        public void TriggerHeadNod()
        {
            if (nodCoroutine != null)
                StopCoroutine(nodCoroutine);
            nodCoroutine = StartCoroutine(PlayNod());
        }

        private IEnumerator PlayNod()
        {
            float elapsed = 0f;
            while (elapsed < nodDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / nodDuration;
                // Full nod: Left → Right → Center
                nodAngle = Mathf.Sin(t * Mathf.PI * 2f) * nodStrength;
                yield return null;
            }

            nodAngle = 0f;
        }


    }
}
