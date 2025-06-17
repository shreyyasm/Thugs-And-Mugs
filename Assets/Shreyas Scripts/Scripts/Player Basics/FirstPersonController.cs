using Dhiraj;
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
        private bool inputDefence;
        private bool wasKeyJustReleased = false;

        private PlayerControls inputActions;
        public bool playerBusy;
        public bool restrictMovement;
        public bool isBlocking;

        public GameObject WoodCutCamera;
        public Animator animator;
        public InventoryManager inventoryManager;

        public float punchCooldown = 0.5f;
        private bool punchAnimToggle = false;
        private bool canPunch = true;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            inputActions = new PlayerControls();

            inputActions.Player.Move.performed += ctx => inputMovement = ctx.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += _ => inputMovement = Vector2.zero;

            inputActions.Player.Jump.performed += _ => inputJump = true;

            inputActions.Player.Run.performed += _ => inputRun = true;
            inputActions.Player.Run.canceled += _ => inputRun = false;

            inputActions.Player.Punch.performed += _ => Punch();

            inputActions.Player.Defence.started += _ => inputDefence = true;
            inputActions.Player.Defence.canceled += _ =>
            {
                inputDefence = false;
                wasKeyJustReleased = true;
            };
        }

        private void OnEnable() => inputActions.Enable();
        private void OnDisable() => inputActions.Disable();

        private void Update()
        {
            if (playerBusy) return;

            if (restrictMovement) return;

            HandleMovement();
            HandleBlocking();
        }

        private void HandleMovement()
        {
            isGrounded = controller.isGrounded;

            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;

            Vector3 move = (transform.right * inputMovement.x + transform.forward * inputMovement.y).normalized;
            float currentSpeed = inputRun ? runSpeed : walkSpeed;
            controller.Move(move * currentSpeed * Time.deltaTime);

            if (isGrounded && inputJump)
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

            inputJump = false;

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleBlocking()
        {
            if (inputDefence)
            {
                if (!isBlocking)
                {
                    isBlocking = true;
                    animator.SetBool("IsBlocking", true);
                    animator.SetBool("InterectHold", false);

                    inventoryManager.SetInventoryEnabled(false);
                    inventoryManager.DropObject();
                    inventoryManager.HideInteractSign();
                    inventoryManager.ClearLastOutlined();
                }
            }
            else if (wasKeyJustReleased)
            {
                if (isBlocking)
                {
                    isBlocking = false;
                    animator.SetBool("IsBlocking", false);
                    inventoryManager.SetInventoryEnabled(true);
                }
                wasKeyJustReleased = false;
            }
        }

        public void Punch()
        {
            if (playerBusy || !canPunch) return;
            if (restrictMovement) return;
            animator.SetBool("IsPunching", true);

            if (punchAnimToggle)
            {
                animator.SetTrigger("Punch1");
                animator.ResetTrigger("Punch2");
            }
            else
            {
                animator.SetTrigger("Punch2");
                animator.ResetTrigger("Punch1");
            }

            punchAnimToggle = !punchAnimToggle;
            StartCoroutine(PunchCooldown());
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
