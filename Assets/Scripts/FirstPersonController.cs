using UnityEngine;
using UnityEngine.InputSystem;

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

    private PlayerControls inputActions;

    private GameObject temp; // Stores the last interacted object

    private void Awake()
    {
        inputActions = new PlayerControls();

        inputActions.Player.Move.performed += ctx => inputMovement = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => inputMovement = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => inputJump = true; // Only set when pressed

        inputActions.Player.Run.performed += ctx => inputRun = true;
        inputActions.Player.Run.canceled += ctx => inputRun = false;

        inputActions.Player.Interact.performed += ctx => inputInteract = true;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        HandleMovement();
        PlayerInteract();
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        // Reset vertical velocity when grounded to avoid floating
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Get movement input
        Vector3 move = transform.right * inputMovement.x + transform.forward * inputMovement.y;
        float currentSpeed = inputRun ? runSpeed : walkSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jumping logic (only allow jumping when grounded)
        if (isGrounded && inputJump)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
        inputJump = false; // Reset jump after applying

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void PlayerInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRadius, interactableLayerMask))
        {
            temp = hit.collider.gameObject;
            Outline outline = hit.collider.GetComponent<Outline>();
            if (outline != null) outline.enabled = true;

            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                if (interactable.interactableType == Interactable.InteractableType.Tree)
                    Axe.SetActive(true);

                if (inputInteract)
                {
                    axeAnimator.SetBool("SwingAxe", true);
                    LeanTween.delayedCall(0.2f, () =>
                    {
                        axeAnimator.SetBool("SwingAxe", false);
                    });

                    interactable.Interact();
                }
            }
        }
        else
        {
            // Remove outline effect
            if (temp != null && temp.TryGetComponent(out Outline tempOutline))
                tempOutline.enabled = false;

            // Hide axe if looking away from a tree
            if (temp != null && temp.TryGetComponent(out Interactable interactable))
            {
                if (interactable.interactableType == Interactable.InteractableType.Tree)
                    Axe.SetActive(false);
            }
        }

        inputInteract = false; // Reset interaction input after one use
    }

    void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactRadius);
        }
    }
}
