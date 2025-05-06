using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shreyas
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonMovementInputSystem : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 5f;
        public float runSpeed = 9f;
        public float jumpForce = 5f;
        public float gravity = -9.81f;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;

        private Vector2 inputMovement;
        private bool inputJump;
        private bool inputRun;


        private PlayerControls inputActions;
        public bool playerBusy;

        private void Awake()
        {
            inputActions = new PlayerControls();

            inputActions.Player.Move.performed += ctx => inputMovement = ctx.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += ctx => inputMovement = Vector2.zero;

            inputActions.Player.Jump.performed += ctx => inputJump = true;

            inputActions.Player.Run.performed += ctx => inputRun = true;
            inputActions.Player.Run.canceled += ctx => inputRun = false;

            controller = GetComponent<CharacterController>();
        }

        private void OnEnable() => inputActions.Enable();
        private void OnDisable() => inputActions.Disable();

        public GameObject WoodCutCamera;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                playerBusy = false;
                WoodCutCamera.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                //playerCamera.orthographic = false;
            }
            HandleMovement();

        }

        private void HandleMovement()
        {
            if (playerBusy)
                return;

            isGrounded = controller.isGrounded;

            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;

            Vector3 move = transform.right * inputMovement.x + transform.forward * inputMovement.y;
            float currentSpeed = inputRun ? runSpeed : walkSpeed;
            controller.Move(move * currentSpeed * Time.deltaTime);

            if (isGrounded && inputJump)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }
            inputJump = false;

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
