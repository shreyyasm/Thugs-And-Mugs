using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

namespace Shreyas
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager instance;
        public bool isInteracting;
        [System.Serializable]
        public class InventoryItem
        {
            public ItemData data;
            public GameObject itemObject;
        }

        public Transform itemHolder;
        public Transform BarrelHolder;
        public GameObject[] handModels; // Match index to tag e.g., index 0 = "broom"
        public List<GameObject> BarrelsModels; // Match index to tag e.g., index 0 = "broom"
        public List<InventorySlot> uiSlots;

        private InventoryItem[] inventory = new InventoryItem[8];
        public int currentIndex = 0;

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
            inputActions.Player.Interact.canceled += ctx =>
            {
                inputInteract = false;
                wasKeyJustReleased = true; // Set flag when the key is released
            };

            inputActions.Player.DropItem.performed += ctx => inputDrop = true;
            inputActions.Player.DropItem.canceled += ctx => inputDrop = false;
            
        }

        private void Start()
        {
            UpdateUI();
            UpdateHands();
        }

        void Update()
        {
            if (FirstPersonMovementInput.playerBusy)
                return;
            HandleScroll();

            if (inputDrop)
            {
                DropCurrentItem();
                inputDrop = false; // Reset after use to prevent repeat calls
            }

            if (FirstPersonMovementInput.isBlocking)
                return;

            UpdateHighlight();
            HandleInteractionRaycast();
            InteractByInventoryItems();
            HandlePickup();
            
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
                        isInteracting = true;
                        OutlineObject(hitObject);
                        return;
                    }
                }

                if (pickup != null && inventoryEnabled)
                {
                    ShowInteractSign("E - Pick");
                    OutlineObject(hitObject);
                    isInteracting = true;
                    if (inputInteract)
                    {
                        if(handModels[currentIndex] != null)
                            handModels[currentIndex].SetActive(false);
                        //DisableBarrel();
                        
                       
                        animator.SetBool("Interact", true);
                        pickup.gameObject.transform.SetParent(null);
                        pickup.gameObject.transform.position = new Vector3(0, -0.3f, 1.5f);
                        PickupItem(pickup.itemData, pickup.gameObject);
                        LeanTween.delayedCall(0.05f, () => { UpdateHands(); });

                    }


                    return;
                }
                isInteracting = false;
                HideInteractSign();
                ClearLastOutlined();
            }
            else
            {
                isInteracting = false;
                HideInteractSign();
                ClearLastOutlined();
            }
        }



        private void ShowInteractSign(string message)
        {
            //if (!isSignVisible)
            //{
            //    InteractSign.enabled = true;
            //    isSignVisible = true;
            //}

            //InteractSign.text = message;
        }

        public void HideInteractSign()
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

        public void ClearLastOutlined()
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
                if (selectedItem != null && inventoryEnabled)
                {
                    switch (selectedItem.itemTag)
                    {
                        case ("Broom"):
                            animator.SetBool("CanUseAxe", false);
                            animator.SetBool("CanUseBroom", true);
                            animator.SetBool("CanUseLighter", false);
                            animator.SetBool("UseDrink", false);
                            animator.SetBool("UseBarrel", false);
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
                            animator.SetBool("UseDrink", false);
                            animator.SetBool("UseBarrel", false);
                            break;

                        case ("BlackBlaze"):
                            SetDrinkBlend(1f);
                            break;

                        case ("CactusBomb"):
                            SetDrinkBlend(2f);
                            break;

                        case ("DesertDraught"):
                            SetDrinkBlend(3f);
                            break;

                        case ("FireOut"):
                            SetDrinkBlend(4f);
                            break;

                        case ("FrothyMug"):
                            SetDrinkBlend(5f);
                            break;

                        case ("ShinerSip"):
                            SetDrinkBlend(6f);
                            break;

                        case ("SnakeBite"):
                            SetDrinkBlend(7f);
                            break;

                        case ("StraightShot"):
                            SetDrinkBlend(8f);
                            break;

                        case ("WidowKiss"):
                            SetDrinkBlend(9f);
                            break;

                        case ("Mug"):
                            SetDrinkBlend(10f);
                            break;

                       

                        // Add more cases for new types
                        default:
                            animator.SetBool("IsUsingAxe", false);

                            //animator.SetBool("IsUsingBroom", false);
                            break;
                    }
                }

                else
                {

                    animator.SetBool("IsUsingAxe", false);
                    //animator.SetBool("IsBlocking", false);
                }
                if (wasKeyJustReleased)
                {
                    animator.ResetTrigger("AxeAnim1");
                    animator.ResetTrigger("AxeAnim2");
                    animator.SetBool("IsUsingBroom", false);
                    wasKeyJustReleased = false; // Reset flag after handling
                }
                Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
                if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask) && isInteracting)
                {
                    InventoryItem currentItem = inventory[currentIndex];
                    Interactable interactable = hit.collider.GetComponent<Interactable>();
                    if (interactable && interactable.CanBeInteracted )
                    {
                        if (interactable.interactableType == Interactable.InteractableType.TreeInteraction)
                            LeanTween.delayedCall(0.25f, () => { interactable.Interact(selectedItem); });

                        else if (interactable.interactableType == Interactable.InteractableType.BarrelInteraction)
                        {
                            interactable.Interact(selectedItem, currentItem.itemObject);
                            BarrelsModels[currentIndex].GetComponent<Animator>().SetBool("OpenBarrel",true);

                        }
                           

                        else
                            interactable.Interact(selectedItem);

                       

                        if (heldObject == null && interactable.isPickable)
                        {
                            PickUpObject(hit.collider.gameObject);
                            HideInteractSign();
                            ClearLastOutlined();

                            SetInventoryEnabled(false); // to disable
                            animator.SetBool("InterectHold", true);
                            animator.SetBool("IsBlocking", false);
                        }

                    }

                }

                

            }
           


        }
        public void SetDrinkBlend(float value)
        {
            animator.SetBool("UseDrink", true);
            animator.SetBool("IsUsingBroom", false);
            animator.SetBool("CanUseAxe", false);
            animator.ResetTrigger("AxeAnim1");
            animator.ResetTrigger("AxeAnim2");
            animator.SetBool("CanUseLighter", false);
            animator.SetBool("CanUseBroom", false);
            animator.SetBool("UseBarrel", false);
          
            if (drinkLerpCoroutine != null)
                StopCoroutine(drinkLerpCoroutine);

            drinkLerpCoroutine = StartCoroutine(SmoothSetDrinkValue(value));
        }
        private Coroutine drinkLerpCoroutine;
        [SerializeField] private float drinksChangeSpeed = 3f;
        private IEnumerator SmoothSetDrinkValue(float target)
        {
            float current = animator.GetFloat("Drinks");

            while (Mathf.Abs(current - target) > 0.01f)
            {
                current = Mathf.MoveTowards(current, target, drinksChangeSpeed * Time.deltaTime);
                animator.SetFloat("Drinks", current);
                yield return null;
            }

            animator.SetFloat("Drinks", target); // Ensure it lands exactly
            drinkLerpCoroutine = null;
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

            GameObject storedItem = itemInWorld;
            storedItem.GetComponent<Rigidbody>().isKinematic = true;
            storedItem.GetComponent<Rigidbody>().useGravity = false;

            if (storedItem.CompareTag("Barrel"))
            {              
                storedItem.transform.SetParent(BarrelHolder);
                storedItem.transform.localPosition = Vector3.zero;
                storedItem.transform.localRotation = Quaternion.identity;
                storedItem.SetActive(false);
                storedItem.GetComponent<Outline>().enabled = false;
                storedItem.GetComponent<BoxCollider>().enabled = false;
                BarrelsModels.Add(storedItem);
                EnableBarrel();
            }
            else
            {
                storedItem.transform.SetParent(itemHolder);
                storedItem.SetActive(false);
            }
               

            inventory[index] = new InventoryItem { data = itemData, itemObject = storedItem };
            //Destroy(itemInWorld);
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

            GameObject droppedItem = currentItem.itemObject;

            // Cache world position/rotation/scale
            Vector3 worldPos = droppedItem.transform.position;
            Quaternion worldRot = droppedItem.transform.rotation;
            Vector3 worldScale = droppedItem.transform.lossyScale;

            // Unparent and restore transform manually
            droppedItem.transform.SetParent(null);
            droppedItem.transform.position = worldPos;
            droppedItem.transform.rotation = worldRot;
            droppedItem.transform.localScale = worldScale; // Apply world scale directly

            // Position it in front of the player
            droppedItem.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 2f;
            droppedItem.transform.rotation = Quaternion.identity;
            droppedItem.GetComponent<BoxCollider>().enabled= true;
            droppedItem.SetActive(true);

            // Enable physics
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
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
            BarrelsModels.Remove(droppedItem);
            Debug.Log("Drop");
        }


        public void ClearInventorySlot()
        {
            // Clear the slot
            inventory[currentIndex] = null;
            UpdateUI();
            UpdateHands();
            UpdateHighlight();
        }
        private void HandleScroll()
        {
            if (!inventoryEnabled) return;

            int previousIndex = currentIndex;

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
            {
                currentIndex = (currentIndex + 1) % inventory.Length;
            }
            else if (scroll < 0f)
            {
                currentIndex = (currentIndex - 1 + inventory.Length) % inventory.Length;
            }

            if (currentIndex != previousIndex)
            {
                UpdateHands();  // Only call when index actually changes
            }
        }
        [SerializeField] Animator animator;
        public void UpdateHands()
        {
            if (!inventoryEnabled) return;

            // Always start by disabling all hand models
            for (int i = 0; i < handModels.Length; i++)
            {
                if (handModels[i] != null)
                    handModels[i].SetActive(false);
            }

            // If there's no item in the selected slot, skip everything else
            if (inventory[currentIndex]?.data == null)
            {
                animator.SetBool("CanUseAxe", false);
                animator.SetBool("CanUseBroom", false);
                animator.SetBool("CanUseLighter", false);
                animator.SetBool("UseDrink", false);
                animator.SetBool("UseBarrel", false);

                return;
            }

            string tag = inventory[currentIndex].data.itemTag;
            for (int i = 0; i < handModels.Length; i++)
            {
                if (handModels[i] != null)
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
                                animator.SetBool("UseDrink", false);
                                animator.SetBool("UseBarrel", false);
                             
                                break;

                            case "Axe":
                                animator.SetBool("CanUseAxe", true);
                                animator.SetBool("CanUseBroom", false);
                                animator.SetBool("CanUseLighter", false);
                                animator.SetBool("UseDrink", false);
                                animator.SetBool("UseBarrel", false);
                               
                                break;
                         

                            case ("Lighter"):
                                animator.SetBool("CanUseAxe", false);
                                animator.SetBool("CanUseBroom", false);
                                animator.SetBool("CanUseLighter", true);
                                animator.SetBool("UseDrink", false);
                                animator.SetBool("UseBarrel", false);
                              
                                break;

                            case ("BlackBlaze"):
                                SetDrinkBlend(1f);
                                break;

                            case ("CactusBomb"):
                                SetDrinkBlend(2f);
                                break;

                            case ("DesertDraught"):
                                SetDrinkBlend(3f);
                                break;

                            case ("FireOut"):
                                SetDrinkBlend(4f);
                                break;

                            case ("FrothyMug"):
                                SetDrinkBlend(5f);
                                break;

                            case ("ShinerSip"):
                                SetDrinkBlend(6f);
                                break;

                            case ("SnakeBite"):
                                SetDrinkBlend(7f);
                                break;

                            case ("StraightShot"):
                                SetDrinkBlend(8f);
                                break;

                            case ("WidowKiss"):
                                SetDrinkBlend(9f);
                                break;

                            case ("Mug"):
                                SetDrinkBlend(10f);
                                break;

                            case ("Barrel"):
                                animator.SetBool("UseBarrel", true);
                                EnableBarrel();
                                  
                                

                                animator.SetBool("CanUseAxe", false);
                                animator.SetBool("CanUseBroom", false);
                                animator.SetBool("CanUseLighter", false);
                                animator.SetBool("UseDrink", false);

                              
                                break;

                            // Add more cases for new types
                            default:
                                animator.SetBool("IsUsingAxe", false);
                                //animator.SetBool("IsUsingBroom", false);
                                break;
                        }
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

        public void DropObject()
        {
            if (!pickedItem) return;

            if (heldRB != null)
            {
                heldRB.useGravity = true;
                heldRB.freezeRotation = false;
                heldRB.isKinematic = false;
                heldRB.linearVelocity = Vector3.zero; // 🛠️ Zero the velocity before throwing
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
                    if(handModels[i] != null)
                        handModels[i].SetActive(false);
                }

                // Optional: disable animator states
                animator.SetBool("CanUseAxe", false);
                animator.SetBool("CanUseBroom", false);
                animator.SetBool("CanUseLighter", false);
                animator.SetBool("UseBarrel", false);
                animator.SetBool("UseDrink", false);
            }
            else
            {
                // Reactivate correct hand if inventory was re-enabled
                UpdateHands();
            }
        }
        public GameObject inventoryCanvas;
        public GameObject PlayerModelVisual;
        public void SetInventoryCanvas( bool enabled)
        {
            inventoryCanvas.SetActive(enabled);
            PlayerModelVisual.SetActive(enabled);
            SetInventoryEnabled(enabled);
        }

        public void EnableBarrel()
        {
          
            LeanTween.delayedCall(0.5f, () =>
            {

               
                InventoryItem currentItem = inventory[currentIndex];
                if (currentItem == null || currentItem.itemObject == null) return;

                Barrel barrelComponent = currentItem.itemObject.GetComponent<Barrel>();
                if (barrelComponent == null) return;

                string targetTag = barrelComponent.itemType.ToString();
               

                for (int i = 0; i < BarrelsModels.Count; i++) // Fixed: use BarrelsModels.Length, not handModels.Length
                {
                    BarrelsModels[i].SetActive(false);

                   
                    
                    string var = BarrelsModels[i].GetComponent<Barrel>().Name;
                    string var2 = barrelComponent.Name;
                    Debug.Log($"{var} || {var2}");
                    //Debug.Log($"{BarrelsModels[i].CompareTag(barrelComponent.Name)}, {var == var2}");
                    if (var == var2)
                    {
                       
                        BarrelsModels[i].SetActive(true);
                    }

                }

            });


        }

      

    }

}

