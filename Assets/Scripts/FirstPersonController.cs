using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Interaction Settings")]
    public float interactRadius = 3f;
    public LayerMask interactableLayerMask;
    public Camera playerCamera; // Reference to the camera

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    public Animator axeAnimator;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = Camera.main; // Fallback to main camera
    }

    void Update()
    {
        HandleMovement();
        PlayerInteract();
        
    }

    void HandleMovement()
    {
        // Ground check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    GameObject temp;
    void PlayerInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        

        if (Physics.Raycast(ray, out hit, interactRadius, interactableLayerMask))
        {
            //Debug.Log("Interacted with: " + hit.collider.name);
            temp = hit.collider.gameObject;
            hit.collider.GetComponent<Outline>().enabled = true;
            //Optional: call a method on the object if it has a certain component
            //Example:
            if(Input.GetMouseButtonDown(0))
            {
                if (hit.collider.TryGetComponent(out Interactable interactable))
                {
                    interactable.Interact();
                    if (interactable.interactableType == Interactable.InteractableType.Tree)
                    {
                        axeAnimator.SetBool("SwingAxe", true);
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
            if (temp != null)
                temp.GetComponent<Outline>().enabled = false;
        }

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
