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

        [Header("Interaction Settings")]
        public float interactRadius = 3f;
        public LayerMask interactableLayerMask;
        public Camera playerCamera;

        [Header("Pickup Settings")]
        public float pickupMoveSpeed = 10f;
        public float hoverDistance = 2f;
        public float throwForce = 500f;

        [Header("References")]
        public Animator axeAnimator;
        public GameObject Axe;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;

        private Vector2 inputMovement;
        private bool inputJump;
        private bool inputRun;
        private bool inputInteract;
        private bool inputFire;

        private PlayerControls inputActions;
        private GameObject lastLookedObject;

        // Pickup variables
        public GameObject heldObject;
        private Rigidbody heldRB;



        public bool playerBusy;

        private void Awake()
        {
            inputActions = new PlayerControls();

            inputActions.Player.Move.performed += ctx => inputMovement = ctx.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += ctx => inputMovement = Vector2.zero;

            inputActions.Player.Jump.performed += ctx => inputJump = true;

            inputActions.Player.Run.performed += ctx => inputRun = true;
            inputActions.Player.Run.canceled += ctx => inputRun = false;

            inputActions.Player.Interact.performed += ctx => inputInteract = true;
            inputActions.Player.Fire.performed += ctx => inputFire = true;
        }

        private void OnEnable() => inputActions.Enable();
        private void OnDisable() => inputActions.Disable();

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            if (playerCamera == null)
                playerCamera = Camera.main;
        }
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
            if (playerBusy)
                return;
            HandleMovement();
            //HandleInteraction();
            //HandlePickup();
        }

        private void HandleMovement()
        {
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

        private void HandleInteraction()
        {
            if (pickedItem) return;

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactRadius, interactableLayerMask))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (hitObject != lastLookedObject)
                {
                    if (lastLookedObject && lastLookedObject.TryGetComponent(out Outline lastOutline))
                        lastOutline.enabled = false;

                    lastLookedObject = hitObject;
                }

                if (hitObject.TryGetComponent(out Outline outline) && !outline.enabled)
                    outline.enabled = true;

                if (!pickedItem && hitObject.TryGetComponent(out Interactable interactable))
                {
                    bool isTree = interactable.interactableType == Interactable.InteractableType.TreeInteraction;

                  

                    if (heldObject == null)
                    {
                        if (inputInteract)
                        {
                            
                           
                           
                        }

                        if (inputFire && isTree)
                        {
                            Axe.SetActive(true);
                            axeAnimator.SetBool("SwingAxe", true);

                            interactable.Interact();

                            LeanTween.delayedCall(0.2f, () =>
                            {
                                axeAnimator.SetBool("SwingAxe", false);
                            });
                        }
                    }
                }
            }
            else
            {
   
                if (lastLookedObject)
                {
                    if (lastLookedObject.TryGetComponent(out Outline lastOutline))
                        lastOutline.enabled = false;

                    if (lastLookedObject.TryGetComponent(out Interactable interactable) &&
                        interactable.interactableType == Interactable.InteractableType.TreeInteraction)
                    {
                        Axe.SetActive(false);
                    }

                    lastLookedObject = null;
                }
            }
        }


        private bool pickedItem = false;
        private bool justPickedUp = false; // NEW

        void HandlePickup()
        {
            if (heldObject != null)
            {
                if (inputInteract && pickedItem && !justPickedUp)
                {
                    DropObject();
                }
                else if (inputFire && pickedItem && !justPickedUp)
                {
                    ThrowObject(10);
                }
                else
                {
                    MoveHeldObject();
                }
            }

            inputInteract = false;
            inputFire = false;
            justPickedUp = false; // reset after 1 frame
        }

        void PickUpObject(GameObject obj)
        {
            heldObject = obj;
            heldRB = heldObject.GetComponent<Rigidbody>();
            if (heldRB != null)
            {

                heldRB.useGravity = false;
                heldRB.freezeRotation = true;
                heldRB.isKinematic = true; // Set to kinematic when picked up

                // Smooth out the movement by setting interpolation
                heldRB.interpolation = RigidbodyInterpolation.Interpolate; // Ensure smooth movement when moved manually

                Vector3 lookDirection = playerCamera.transform.forward;
                lookDirection.y = 0f; // Optional: Only rotate on Y axis if you don't want it tilted up/down
                heldObject.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
            pickedItem = true;
            justPickedUp = true; // Mark that we JUST picked up something
        }


        void DropObject()
        {
            if (!pickedItem) return;

            if (heldRB != null)
            {
                heldRB.useGravity = true;
                heldRB.freezeRotation = false;
                heldRB.isKinematic = false;
            }

            heldObject = null;
            heldRB = null;
            pickedItem = false;
        }

        void ThrowObject(float throwForce)
        {
            if (!pickedItem) return;

            if (heldRB != null)
            {
                heldRB.useGravity = true;
                heldRB.freezeRotation = false;
                heldRB.isKinematic = false;

                heldRB.linearVelocity = Vector3.zero; // 🛠️ Zero the velocity before throwing

                heldRB.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
            }

            heldObject = null;
            heldRB = null;
            pickedItem = false;
        }

        void MoveHeldObject()
        {
            if (heldRB == null) return;

            // Get flat forward direction (ignore vertical tilt)
            Vector3 flatForward = playerCamera.transform.forward;
            flatForward.y = 0;
            flatForward.Normalize();

            // Base target position in front of camera
            Vector3 targetPos = playerCamera.transform.position + flatForward * hoverDistance;

            // Get the pitch (X rotation) of the camera
            float pitch = playerCamera.transform.localEulerAngles.x;

            // Normalize pitch to a -90 to 90 range
            if (pitch > 180f)
                pitch -= 360f;

            // Invert the pitch logic for height adjustment (when you look up, it goes up, when you look down, it goes down)
            float heightAdjustment = Mathf.Clamp(-pitch / 10f, 1f, 10f);  // Inverted, so looking up increases the height

            // Apply the height adjustment based on camera's pitch
            targetPos.y += heightAdjustment;

            // Move the object smoothly toward the target position
            Vector3 direction = (targetPos - heldObject.transform.position);
            heldRB.MovePosition(heldObject.transform.position + direction * pickupMoveSpeed * Time.deltaTime); // Use MovePosition for smooth movement

            // Rotate object to always face the camera
            Vector3 lookDirection = flatForward;
            if(lookDirection != Vector3.zero)
            {
                heldObject.transform.rotation = Quaternion.Slerp(
               heldObject.transform.rotation,
               Quaternion.LookRotation(lookDirection),
               Time.deltaTime * 10f
           );
            }
           
        }
    }

}
