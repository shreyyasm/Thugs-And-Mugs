using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static Shreyas.Interactable;
using Unity.VisualScripting;
using Shreyas;
using TMPro;
using UnityEngine.InputSystem;
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [System.Serializable]
    public class InventoryItem
    {
        public ItemData data;
        public GameObject itemObject;
    }

    public Transform itemHolder;
    public GameObject[] handModels; // Match index to tag e.g., index 0 = "broom"
    public List<InventorySlot> uiSlots;

    private InventoryItem[] inventory = new InventoryItem[8];
    private int currentIndex = 0;

    public TextMeshProUGUI InteractSign;
    public FirstPersonMovementInputSystem FirstPersonMovementInput;

    // Pickup variables
    public GameObject heldObject;
    private Rigidbody heldRB;

    [Header("Pickup Settings")]
    public float pickupMoveSpeed = 10f;
    public float hoverDistance = 2f;
    public float throwForce = 500f;

    private PlayerControls inputActions;
    private bool inputInteract;
    public bool inputDrop;
    private bool wasKeyJustReleased = false; // Detect if key was just released

    private bool inventoryEnabled = true;


    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();
    private void Awake()
    {
        instance = this;

        inputActions = new PlayerControls();
        inputActions.Player.Interact.performed += ctx => inputInteract = true;
        inputActions.Player.Interact.canceled += ctx => inputInteract = false;
        inputActions.Player.DropItem.performed += ctx => inputDrop = true;
        inputActions.Player.DropItem.canceled += ctx => inputDrop = false;
        inputActions.Player.Interact.canceled += ctx =>
        {
            inputInteract = false;
            wasKeyJustReleased = true; // Set flag when the key is released
        };
    }

    private void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (FirstPersonMovementInput.playerBusy)   
            return;
        HandleScroll();
        UpdateHands();
        UpdateHighlight();
        HandleInteractionRaycast(); // 👈 Add this
        //UseInventoryItem();
        InteractByInventoryItems();


        HandlePickup();

        if (inputDrop)
        {
            DropCurrentItem();
        }

    }   
    [SerializeField] private LayerMask interactLayerMask;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private Camera playerCamera;

    private bool isSignVisible = false;
    private GameObject lastOutlinedObject = null;
    private void HandleInteractionRaycast()
    {
        if (!inventoryEnabled)
            return;
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject;
            Interactable interactable = hitObject.GetComponent<Interactable>();
            PickupTrigger pickup = hitObject.GetComponent<PickupTrigger>();

            ItemData selectedItem = GetCurrentSelectedItem();

            if (interactable != null && interactable.CanBeInteracted)
            {
                // Allow interaction if requiredItemTag is empty or matches selected item tag
                bool canInteract = string.IsNullOrEmpty(interactable.requiredItemTag) ||
                                   (selectedItem != null && selectedItem.itemTag == interactable.requiredItemTag);

                if (canInteract)
                {

                    ShowInteractSign("E - Interact");
                   
                    OutlineObject(hitObject);
                    return;
                }
            }

            if (pickup != null && inventoryEnabled)
            {
                ShowInteractSign("E - Pick");               
                OutlineObject(hitObject);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    handModels[currentIndex].SetActive(false);
                    animator.SetBool("Interact", true);                  
                    PickupItem(pickup.itemData, pickup.gameObject);
                }
                   

                return;
            }

            HideInteractSign();
            ClearLastOutlined();
        }
        else
        {
            HideInteractSign();
            ClearLastOutlined();
        }
    }

    

    private void ShowInteractSign(string message)
    {
        if (!isSignVisible)
        {
            InteractSign.enabled = true;
            isSignVisible = true;
        }

        InteractSign.text = message;
    }

    private void HideInteractSign()
    {
        if (isSignVisible)
        {
            InteractSign.enabled = false;
            isSignVisible = false;
        }
    }

    private void OutlineObject(GameObject obj)
    {
        if (obj != lastOutlinedObject)
        {
            DisableOutline(lastOutlinedObject);
            EnableOutline(obj);
            lastOutlinedObject = obj;
        }
    }

    private void ClearLastOutlined()
    {
        DisableOutline(lastOutlinedObject);
        lastOutlinedObject = null;
    }


    private bool axeAnimToggle = false;

    public void InteractByInventoryItems()
    {
        if (inputInteract)
        {
            ItemData selectedItem = InventoryManager.instance.GetCurrentSelectedItem();
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask))
            {

                Interactable interactable = hit.collider.GetComponent<Interactable>();
                if (interactable && interactable.CanBeInteracted)
                {
                    if(interactable.interactableType == InteractableType.TreeInteraction)
                        LeanTween.delayedCall(0.25f, () => { interactable.Interact(selectedItem);});
                    else
                        interactable.Interact(selectedItem);

                    if (heldObject == null && interactable.isPickable)
                    {
                        PickUpObject(hit.collider.gameObject);
                        InventoryManager.instance.SetInventoryEnabled(false); // to disable
                        animator.SetBool("InterectHold", true);

                    }
                        
                }

            }
            
            if (selectedItem != null && inventoryEnabled)
            {
                switch (selectedItem.itemTag)
                {
                    case ("Broom"):
                        animator.SetBool("IsUsingBroom", true);
                        break;

                    case "Axe":
                        if (axeAnimToggle)
                            animator.SetTrigger("AxeAnim2");  // Trigger 2nd animation
                        else
                            animator.SetTrigger("AxeAnim1");  // Trigger 1st animation
                        axeAnimToggle = !axeAnimToggle;      // Flip toggle
                        break;

                    case ("Lighter"):
                        animator.SetBool("CanUseAxe", false);
                        animator.SetBool("CanUseBroom", false);
                        animator.SetBool("CanUseLighter", true);
                        break;

                    // Add more cases for new types
                    default:
                        animator.SetBool("IsUsingAxe", false);
                        //animator.SetBool("IsUsingBroom", false);
                        animator.SetBool("IsPunching", true);
                        break;
                }
            }
           
        }
        else
        {

            animator.SetBool("IsUsingAxe", false);
       
            animator.SetBool("IsPunching", false);
            animator.SetBool("IsBlocking", false);
        }
        if (wasKeyJustReleased)
        {
            animator.ResetTrigger("AxeAnim1");
            animator.ResetTrigger("AxeAnim2");
            animator.SetBool("IsUsingBroom", false);
            wasKeyJustReleased = false; // Reset flag after handling
        }
            

    }
    private void EnableOutline(GameObject obj)
    {
        if (obj == null) return;
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    private void DisableOutline(GameObject obj)
    {
        if (obj == null) return;
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }


    public ItemData GetCurrentSelectedItem()
    {
        if (inventory[currentIndex] != null)
            return inventory[currentIndex].data;
        return null;
    }

    public void PickupItem(ItemData itemData, GameObject itemInWorld)
    {
        int index = GetFirstEmptySlot();
        if (index == -1) return;

        GameObject storedItem = Instantiate(itemInWorld, itemHolder);
        storedItem.SetActive(false);
        inventory[index] = new InventoryItem { data = itemData, itemObject = storedItem };
        Destroy(itemInWorld);
        UpdateUI();
    }

    private int GetFirstEmptySlot()
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null) return i;
        }
        return -1;
    }
    private void DropCurrentItem()
    {
        InventoryItem currentItem = inventory[currentIndex];
        if (currentItem == null || currentItem.itemObject == null)
            return;

        // Detach item from the holder
        GameObject droppedItem = currentItem.itemObject;
        droppedItem.transform.SetParent(null);

        // Position it in front of the player
        Vector3 dropPosition = playerCamera.transform.position + playerCamera.transform.forward * 2f;
        droppedItem.transform.position = dropPosition;
        droppedItem.transform.rotation = Quaternion.identity;
        droppedItem.SetActive(true);

        // Enable physics if applicable
        Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(playerCamera.transform.forward * 2f, ForceMode.Impulse);
        }

        Collider col = droppedItem.GetComponent<Collider>();
        if (col != null)
            col.enabled = true;

        // Clear the slot
        inventory[currentIndex] = null;
        UpdateUI();
        UpdateHands();
        UpdateHighlight();
    }

    private void HandleScroll()
    {
        if (!inventoryEnabled) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) currentIndex = (currentIndex + 1) % inventory.Length;
        else if (scroll < 0f) currentIndex = (currentIndex - 1 + inventory.Length) % inventory.Length;
    }

    [SerializeField] Animator animator;
    private void UpdateHands()
    {
        if (!inventoryEnabled) return;

        // Always start by disabling all hand models
        for (int i = 0; i < handModels.Length; i++)
        {
            handModels[i].SetActive(false);
        }

        // If there's no item in the selected slot, skip everything else
        if (inventory[currentIndex]?.data == null)
        {
            animator.SetBool("CanUseAxe", false);
            animator.SetBool("CanUseBroom", false);
            animator.SetBool("CanUseLighter", false);
            return;
        }

        string tag = inventory[currentIndex].data.itemTag;
        for (int i = 0; i < handModels.Length; i++)
        {
            if (handModels[i].CompareTag(tag))
            {
                handModels[i].SetActive(true);
                switch (tag)
                {
                    case "Broom":
                        animator.SetBool("CanUseAxe", false);
                        animator.SetBool("CanUseBroom", true);
                        animator.SetBool("CanUseLighter", false);
                        break;

                    case "Axe":
                        animator.SetBool("CanUseAxe", true);
                        animator.SetBool("CanUseBroom", false);
                        animator.SetBool("CanUseLighter", false);
                        break;

                    case "Lighter":
                        animator.SetBool("CanUseAxe", false);
                        animator.SetBool("CanUseBroom", false);
                        animator.SetBool("CanUseLighter", true);
                        break;

                    default:
                        Debug.LogWarning("No interaction defined for this type.");
                        break;
                }
            }
        }
    }


    private void UpdateUI()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (inventory[i] != null)
                uiSlots[i].SetItem(inventory[i].data);
            else
                uiSlots[i].ClearSlot();
        }
    }

    private void UpdateHighlight()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            uiSlots[i].highlight.SetActive(i == currentIndex);
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
                InventoryManager.instance.SetInventoryEnabled(true);  // to re-enable
                animator.SetBool("InterectHold", false);
            }          
            else
            {
                MoveHeldObject();
            }
        }

        inputInteract = false;
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
        if (lookDirection != Vector3.zero)
        {
            heldObject.transform.rotation = Quaternion.Slerp(
           heldObject.transform.rotation,
           Quaternion.LookRotation(lookDirection),
           Time.deltaTime * 10f
       );
        }

    }
    public void SetInventoryEnabled(bool enabled)
    {
        inventoryEnabled = enabled;

        if (!enabled)
        {
            // Hide all hands when inventory is disabled
            for (int i = 0; i < handModels.Length; i++)
            {
                handModels[i].SetActive(false);
            }

            // Optional: disable animator states
            animator.SetBool("CanUseAxe", false);
            animator.SetBool("CanUseBroom", false);
            animator.SetBool("CanUseLighter", false);
            animator.SetBool("IsPunching", false);
        }
        else
        {
            // Reactivate correct hand if inventory was re-enabled
            UpdateHands();
        }
    }

}
