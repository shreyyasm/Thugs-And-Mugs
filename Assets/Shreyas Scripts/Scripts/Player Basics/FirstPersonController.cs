using System.Collections;
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
        private bool inputPunch;
        private bool inputDefence;
        private bool wasKeyJustReleased = false; // Detect if key was just released

        private PlayerControls inputActions;
        public bool playerBusy;
        public bool isBlocking;
        public bool isPunching;

        private void Awake()
        {
            inputActions = new PlayerControls();

            inputActions.Player.Move.performed += ctx => inputMovement = ctx.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += ctx => inputMovement = Vector2.zero;

            inputActions.Player.Jump.performed += ctx => inputJump = true;

            inputActions.Player.Run.performed += ctx => inputRun = true;
            inputActions.Player.Run.canceled += ctx => inputRun = false;

            inputActions.Player.Punch.performed += ctx => Punch();

            inputActions.Player.Defence.started += ctx => inputDefence = true;
            inputActions.Player.Defence.canceled += ctx =>

            {
                inputDefence = false;
                wasKeyJustReleased = true; // Set flag when the key is released
            };

            controller = GetComponent<CharacterController>();
        }

        private void OnEnable() => inputActions.Enable();
        private void OnDisable() => inputActions.Disable();

        public GameObject WoodCutCamera;
        private bool punchAnimToggle = false;
        public Animator animator;
        public InventoryManager inventoryManager;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                playerBusy = false;
                if(WoodCutCamera) WoodCutCamera.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                //playerCamera.orthographic = false;
            }

            HandleMovement();
            
            if (inputDefence)
            {
                isBlocking = true;
                animator.SetBool("IsBlocking",true);           
                animator.SetBool("InterectHold", false);

                //Inventory System
                inventoryManager.SetInventoryEnabled(false);
                inventoryManager.DropObject();
                inventoryManager.HideInteractSign();
                inventoryManager.ClearLastOutlined();
            }
            if (wasKeyJustReleased)
            {
                animator.SetBool("IsBlocking", false);
                isBlocking = false;
                inventoryManager.SetInventoryEnabled(true);
                wasKeyJustReleased = false; // Reset flag after handling
            }

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
        public float punchCooldown = 0.5f; // Time between punches
        private bool canPunch = true;

        public void Punch()
        {
            if (!canPunch) return; // Block if in cooldown
            animator.SetBool("IsPunching",true);

            if (punchAnimToggle)
            {
                animator.SetTrigger("Punch1");
                animator.ResetTrigger("Punch2");
                StartCoroutine(PunchCooldown()); // Start cooldown
            }
            else
            {
                animator.SetTrigger("Punch2");
                animator.ResetTrigger("Punch1");
                StartCoroutine(PunchCooldown()); // Start cooldown
            }

            punchAnimToggle = !punchAnimToggle;

           
        }

        private IEnumerator PunchCooldown()
        {
            canPunch = false;
            
            yield return new WaitForSeconds(punchCooldown);
            animator.SetBool("IsPunching", false);
            canPunch = true;
        }

    }
}
