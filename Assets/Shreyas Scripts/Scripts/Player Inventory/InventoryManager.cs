using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System;
using Dhiraj;
using EPOOutline;

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
        public FirstPersonMovementInputSystem firstPersonMovementInput;

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
            UpdateHands();
        }
        RaycastHit hitGrab;

        void Update()
        {
            if (firstPersonMovementInput.playerBusy)
                return;

            HandleScroll();

            if (inputDrop)
            {
                DropCurrentItem();
                inputDrop = false; // Reset after use to prevent repeat calls
            }

            if (firstPersonMovementInput.isBlocking)
                return;

            UpdateHighlight();
            HandleInteractionRaycast();
            InteractByInventoryItems();
            HandlePickup();

            if (isHoldingObject)
            {
                FollowCamera();
                HandleRotation();
                CheckCollisionAndSetMaterial();
                if (Input.GetMouseButtonDown(0)) // Place object
                {
                    PlaceObject();
                }
            }
            AcessBuildMenu();

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



            Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
            Ray ray = playerCamera.ScreenPointToRay(screenCenter);

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask))
            {
               
                if(hit.collider.gameObject.layer == 12)
                {
                    HandleRaycastHold(hit);
                   
                }

                GameObject hitObject = hit.collider.gameObject;
                // Cache components to avoid repeated GetComponent calls
                var interactable = hitObject.GetComponent<Interactable>();
                var pickup = hitObject.GetComponent<PickupTrigger>();
                var selectedItem = GetCurrentSelectedItem();

                if (interactable != null && interactable.CanBeInteracted)
                {
                    bool canInteract = string.IsNullOrEmpty(interactable.requiredItemTag) ||
                                       (selectedItem != null && selectedItem.itemTag == interactable.requiredItemTag);

                    if (canInteract)
                    {
                        ShowInteractSign("E - Interact");
                        isInteracting = true;
                        OutlineObject(hitObject);

                        if (Input.GetKeyDown(KeyCode.C))
                        {
                            if (hit.collider.GetComponent<Interactable>().interactableType == Interactable.InteractableType.CraftItem)
                            {
                               
                                if(tempBuildObject  != null)
                                    Destroy(tempBuildObject.gameObject);
                              
                            }
                        }
                        return;
                    }
                  
                }

                if (pickup != null)
                {
                    ShowInteractSign("E - Pick");
                    OutlineObject(hitObject);
                    isInteracting = true;

                    if (inputInteract)
                    {
                        GameObject currentHand = handModels[currentIndex];
                        if (currentHand != null)
                            currentHand.SetActive(false);

                        DisableBarrels();

                        animator.SetBool("Interact", true);
                        animator.SetBool("CanUseBroom", false);
                        animator.SetBool("CanUseAxe", false);
                        animator.SetBool("CanUseHammer", false);

                        pickup.transform.SetParent(null);
                        pickup.transform.position = new Vector3(0, -0.3f, 1.5f);
                        PickupItem(pickup.itemData, pickup.gameObject);

                        LeanTween.delayedCall(0.05f, UpdateHands);
                    }
                   

                    return;
                }
            }
           
            // Shared cleanup
            isInteracting = false;
            HideInteractSign();
            ClearLastOutlined();
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
        public GameObject puffparticle;

        public void InteractByInventoryItems()
        {
          
            if (Input.GetMouseButtonDown(0) && GetCurrentSelectedItem() != null && GetCurrentSelectedItem().MouseUse)
            {
                ItemData selectedItem = GetCurrentSelectedItem();
                if (selectedItem != null && inventoryEnabled)
                {
                    string tag = selectedItem.itemTag;

                    switch (tag)
                    {
                        case "Broom":
                            if (animator.GetBool("CanUseBroom"))
                                animator.SetTrigger("IsUsingBroom");
                            SetAnimatorStates(false, true, false, false, false,false);
                            break;

                        case "Axe":
                            if (animator.GetBool("CanUseAxe"))
                            {
                                animator.SetTrigger(axeAnimToggle ? "AxeAnim2" : "AxeAnim1");
                              
                                axeAnimToggle = !axeAnimToggle;
                            }
                            break;

                        case "Lighter":
                            SetAnimatorStates(false, false, true, false, false,false);
                            break;

                        case "Hammer":
                            if (animator.GetBool("CanUseHammer"))
                                animator.SetTrigger("IsUsingHammer");
                            SetAnimatorStates(false, false, false, false, false, true);
                            break;

                        default:
                            animator.SetBool("IsUsingAxe", false);
                            break;
                    }
                }
                else
                {
                    animator.SetBool("IsUsingAxe", false);
                }

                if (wasKeyJustReleased)
                    wasKeyJustReleased = false;

                Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
                Ray ray = playerCamera.ScreenPointToRay(screenCenter);
                if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask) && isInteracting)
                {
                    InventoryItem currentItem = inventory[currentIndex];
                    GameObject hitObj = hit.collider.gameObject;
                    Interactable interactable = hitObj.GetComponent<Interactable>();

                    if (interactable != null && interactable.CanBeInteracted)
                    {
                        if (interactable.interactableType == Interactable.InteractableType.TreeInteraction)
                        {
                            LeanTween.delayedCall(0.25f, () => interactable.Interact(selectedItem));
                        }
                        else if (interactable.interactableType == Interactable.InteractableType.BarrelInteraction)
                        {
                            interactable.Interact(selectedItem, currentItem.itemObject);

                            Barrel currentBarrel = currentItem.itemObject.GetComponent<Barrel>();
                            string currentBarrelName = currentBarrel.Name;

                            for (int i = 0; i < BarrelsModels.Count; i++)
                            {
                                GameObject barrelObj = BarrelsModels[i];
                                Animator barrelAnimator = barrelObj.GetComponent<Animator>();
                                Barrel barrelData = barrelObj.GetComponent<Barrel>();

                                barrelAnimator.SetBool("OpenBarrel", barrelData.Name == currentBarrelName);
                            }
                        }
                        else if (interactable.interactableType == Interactable.InteractableType.CraftItem)
                        {
                            if (PlayerStatics.Instance.Woods > 0)
                            {
                                if (inventory[currentIndex].data.itemTag == "Hammer")
                                {
                                    LeanTween.delayedCall(0.25f, () =>
                                    {
                                        interactable.Interact(selectedItem);
                                        Instantiate(puffparticle, hit.point, Quaternion.identity);
                                    });
                                }
                            }
                            else
                            {
                                //Show No woods and SFX
                            }



                        }
                    }
                }
            }
            if (inputInteract)
            {
                ItemData selectedItem = GetCurrentSelectedItem();
               
                if (wasKeyJustReleased)
                    wasKeyJustReleased = false;

                Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
                Ray ray = playerCamera.ScreenPointToRay(screenCenter);
                if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask) && isInteracting)
                {
                    InventoryItem currentItem = inventory[currentIndex];
                    GameObject hitObj = hit.collider.gameObject;
                    Interactable interactable = hitObj.GetComponent<Interactable>();

                    if (interactable != null && interactable.CanBeInteracted && interactable.E_Interaction)
                    {

                        interactable.Interact(selectedItem);


                        if (heldObject == null && interactable.isPickable)
                        {
                            PickUpObject(hitObj);
                            HideInteractSign();
                            ClearLastOutlined();

                            SetInventoryEnabled(false);
                            animator.SetBool("InterectHold", true);
                            animator.SetBool("IsBlocking", false);
                        }
                    }
                }
            }

        }
        private void SetAnimatorStates(bool useAxe, bool useBroom, bool useLighter, bool useDrink, bool useBarrel, bool useHammer)
        {
            animator.SetBool("CanUseAxe", useAxe);
            animator.SetBool("CanUseBroom", useBroom);
            animator.SetBool("CanUseLighter", useLighter);
            animator.SetBool("UseDrink", useDrink);
            animator.SetBool("UseBarrel", useBarrel);
            animator.SetBool("CanUseHammer", useHammer);
        }

        public void SetDrinkBlend(float value)
        {
            animator.SetBool("UseDrink", true);
            animator.ResetTrigger("IsUsingBroom");
            animator.SetBool("CanUseAxe", false);
            animator.ResetTrigger("AxeAnim1");
            animator.ResetTrigger("AxeAnim2");
            animator.SetBool("CanUseLighter", false);
            animator.SetBool("CanUseBroom", false);
            animator.SetBool("UseBarrel", false);
            animator.SetBool("CanUseHammer", false);
            animator.ResetTrigger("IsUsingHammer");
            animator.SetFloat("Drinks", value);

        }

        private void EnableOutline(GameObject obj)
        {
            if (obj == null) return;
            Outlinable outline = obj.GetComponent<Outlinable>();
            if (outline != null)
            {
                outline.enabled = true;
            }
        }

        private void DisableOutline(GameObject obj)
        {
            if (obj == null) return;
            Outlinable outline = obj.GetComponent<Outlinable>();
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
            }
            if (storedItem.CompareTag("Mug"))
            {
                if (storedItem.GetComponent<Mug>().isFilled)
                {
                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.baseColor = storedItem.GetComponent<Mug>().liquidShaderController.baseColor;
                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.emissionColor = storedItem.GetComponent<Mug>().liquidShaderController.baseColor;
                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.fillAmount = storedItem.GetComponent<Mug>().liquidShaderController.fillAmount;
                }

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
        public void PickupBarrel()
        {
            InventoryItem currentItem = inventory[currentIndex];
            if (currentItem != null && currentItem.itemObject != null)
            {
                Barrel barrelComponent = currentItem.itemObject.GetComponent<Barrel>();


                if (barrelComponent != null)
                {
                    string targetTag = barrelComponent.itemType.ToString();
                    for (int i = 0; i < BarrelsModels.Count; i++)
                    {
                        BarrelsModels[i].SetActive(false);
                        string var = BarrelsModels[i].GetComponent<Barrel>().Name;
                        string var2 = barrelComponent.Name;
                        if (var == var2)
                        {

                            BarrelsModels[i].SetActive(true);
                        }
                    }
                }

            }
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

            if (droppedItem.CompareTag("Mug"))
            {
                if (droppedItem.GetComponent<Mug>().isFilled)
                {
                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.baseColor = Color.white;
                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.emissionColor = Color.white;
                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.fillAmount = 0;
                }

            }
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
            droppedItem.transform.position = itemHolder.transform.position + playerCamera.transform.forward;
            droppedItem.transform.rotation = Quaternion.identity;
            droppedItem.GetComponent<BoxCollider>().enabled = true;
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
        public GameObject tempGameObject;

        public void SetTemp_GameObject(int index)
        {
            InventoryItem currentItem = inventory[index];
            if (currentItem == null || currentItem.itemObject == null)
                return;

            GameObject droppedItem = currentItem.itemObject;
            tempGameObject = droppedItem;
        }
        public void DropItemByChoice(int index, Transform parent)
        {
            InventoryItem currentItem = inventory[index];
            if (currentItem == null || currentItem.itemObject == null)
                return;

            GameObject droppedItem = currentItem.itemObject;
            tempGameObject = droppedItem;

            // Cache world scale before reparenting
            Vector3 worldScale = droppedItem.transform.lossyScale;

            // Set parent and maintain world position/rotation/scale
            droppedItem.transform.SetParent(parent, true);

            // Force exact local alignment
            droppedItem.transform.localPosition = Vector3.zero;
            droppedItem.transform.localRotation = Quaternion.Euler(-90, 0, 0);

            // Correct scale relative to new parent
            Vector3 parentScale = parent.lossyScale;
            droppedItem.transform.localScale = new Vector3(
                worldScale.x / parentScale.x,
                worldScale.y / parentScale.y,
                worldScale.z / parentScale.z
            );

            // Enable colliders and object
            if (droppedItem.TryGetComponent(out Collider col))
                col.enabled = true;

            droppedItem.SetActive(true);

            // Clear inventory slot
            inventory[index] = null;
            UpdateUI();
            UpdateHands();
            UpdateHighlight();
            droppedItem.layer = 0;
            BarrelsModels.Remove(droppedItem);

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
                animator.SetBool("CanUseHammer", false);
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
                            case ("Barrel"):

                                if (animator.GetBool("UseBarrel"))
                                {
                                    animator.SetBool("UseBarrel", false);
                                    LeanTween.delayedCall(0.1f, () => { animator.SetBool("UseBarrel", true); });
                                }
                                else
                                    animator.SetBool("UseBarrel", true);



                                animator.SetBool("CanUseAxe", false);
                                animator.SetBool("CanUseBroom", false);
                                animator.SetBool("CanUseLighter", false);
                                animator.SetBool("UseDrink", false);
                                animator.SetBool("CanUseHammer", false);

                                break;

                            case "Broom":
                                animator.SetBool("CanUseAxe", false);
                                animator.SetBool("CanUseBroom", true);
                                animator.SetBool("CanUseLighter", false);
                                animator.SetBool("UseDrink", false);
                                animator.SetBool("UseBarrel", false);
                                animator.SetBool("CanUseHammer", false);

                                break;

                            case "Axe":
                                animator.SetBool("CanUseAxe", true);
                                animator.SetBool("CanUseBroom", false);
                                animator.SetBool("CanUseLighter", false);
                                animator.SetBool("UseDrink", false);
                                animator.SetBool("UseBarrel", false);
                                animator.SetBool("CanUseHammer", false);

                                break;

                            case ("Lighter"):
                                animator.SetBool("CanUseAxe", false);
                                animator.SetBool("CanUseBroom", false);
                                animator.SetBool("CanUseLighter", true);
                                animator.SetBool("UseDrink", false);
                                animator.SetBool("UseBarrel", false);
                                animator.SetBool("CanUseHammer", false);

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

                                InventoryItem currentItem = inventory[currentIndex];
                                if (currentItem == null || currentItem.itemObject == null)
                                    return;

                                GameObject droppedItem = currentItem.itemObject;

                                if (droppedItem.GetComponent<Mug>().isFilled)
                                {
                                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.baseColor = droppedItem.GetComponent<Mug>().liquidShaderController.baseColor;
                                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.emissionColor = droppedItem.GetComponent<Mug>().liquidShaderController.baseColor;
                                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.fillAmount = droppedItem.GetComponent<Mug>().liquidShaderController.fillAmount;
                                }
                                else
                                {
                                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.fillAmount = 0;
                                }


                                break;
                            case ("Hammer"):
                               
                                animator.SetBool("CanUseAxe", false);
                                animator.SetBool("CanUseBroom", false);
                                animator.SetBool("CanUseLighter", false);
                                animator.SetBool("UseDrink", false);
                                animator.SetBool("UseBarrel", false);
                                animator.SetBool("CanUseHammer", true);

                                break;

                            // Add more cases for new types
                            default:
                                animator.SetBool("IsUsingAxe", false);

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
                    if (handModels[i] != null)
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
        public void SetInventoryCanvas(bool enabled)
        {
            inventoryCanvas.SetActive(enabled);
            PlayerModelVisual.SetActive(enabled);
            SetInventoryEnabled(enabled);
        }

        public void DisableBarrels()
        {
            if (BarrelsModels.Count != 0)
            {
                foreach (GameObject i in BarrelsModels)
                {
                    i.SetActive(false);
                }
            }

        }

        //Object placement Code

        [Header("References")]
        public Image progressCircle;

        [Header("Grab Settings")]
        public float rayDistance = 3f;
        public float holdTimeThreshold = 1f;
        public float grabHeight = 1.5f;
        public float rotationSpeed = 100f;
        public Material validMaterial;
        public Material invalidMaterial;

        private GameObject grabbedObject;
        private float holdTimer;
        private bool isGrabbing;
        private bool isHoldingObject;

        private Vector3 grabOffset; public float grabDistance = 2f;
        private Renderer[] objectRenderers;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector3 lastMousePos;
        private Dictionary<Renderer, Material> originalMaterials = new();
        private Vector3 originalObjectHeight;
        private List<Collider> grabbedColliders = new(); private float currentGrabDistance;
        public float mouseSensitivity = 0.01f; // tweak this value for faster/slower movement
        private bool isObjectColliding = false; private Collider grabbedCollider;

        private GrabbedObjectCollisionChecker collisionChecker;
        void HandleRaycastHold(RaycastHit hit)
        {
            if (Input.GetMouseButton(0))
            {

                holdTimer += Time.deltaTime;
                progressCircle.fillAmount = holdTimer / holdTimeThreshold;

                if (holdTimer >= holdTimeThreshold)
                {
                    GrabObject(hit.collider.gameObject);
                    holdTimer = 0;
                    progressCircle.fillAmount = 0;
                }

            }
            else
            {
                holdTimer = 0;
                progressCircle.fillAmount = 0;
            }
        }

        void GrabObject(GameObject obj)
        {
            grabbedObject = obj;
            isHoldingObject = true;

            // Add or get the collision checker component
            collisionChecker = grabbedObject.GetComponent<GrabbedObjectCollisionChecker>();
            if (collisionChecker == null)
            {
                collisionChecker = grabbedObject.AddComponent<GrabbedObjectCollisionChecker>();
            }

            originalObjectHeight = grabbedObject.transform.position;
            currentGrabDistance = Vector3.Distance(playerCamera.transform.position, originalObjectHeight);
            inventoryEnabled = false;



            // Store and set colliders to trigger
            grabbedColliders.Clear();
            grabbedColliders.AddRange(grabbedObject.GetComponentsInChildren<Collider>());
            foreach (var col in grabbedColliders)
            {
                col.isTrigger = true;
            }

            // Set rigidbody kinematic
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = true;

            // Store and set material
            objectRenderers = grabbedObject.GetComponentsInChildren<Renderer>();
            originalMaterials.Clear();
            foreach (var r in objectRenderers)
            {
                if (r != null && r.material != null)
                    originalMaterials[r] = r.material;
            }

            SetMaterial(validMaterial);

            // Set fixed grab height
            originalObjectHeight = grabbedObject.transform.position;
            lastMousePos = Input.mousePosition;

            // Optional: prevent self-collision during grab
            Physics.IgnoreLayerCollision(gameObject.layer, grabbedObject.layer, true);
        }

        [Header("Grab Offset (to align with crosshair)")]
        public Vector3 grabOffsets = Vector3.zero; // Editable in Inspector

        void FollowCamera()
        {
            if (grabbedObject == null) return;

            // 1. Get center of screen
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

            // 2. Create a ray from camera through screen center
            Ray ray = playerCamera.ScreenPointToRay(screenCenter);

            // 3. Move object along that ray direction at controlled distance
            Vector3 targetPos = ray.origin + ray.direction.normalized * currentGrabDistance;

            // 4. Lock Y position to original grab height
            targetPos.y = originalObjectHeight.y;

            // 5. Move the object
            grabbedObject.transform.position = targetPos;

            // 6. Adjust distance with vertical mouse input
            float verticalInput = Input.GetAxis("Mouse Y");
            float sensitivity = 0.1f;
            currentGrabDistance += verticalInput * sensitivity;
            currentGrabDistance = Mathf.Clamp(currentGrabDistance, 0.5f, 10f);
        }


        void HandleRotation()
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f && grabbedObject != null)
            {
                Transform parent = grabbedObject.transform.root;
                parent.Rotate(Vector3.up, scroll * rotationSpeed * Time.deltaTime * 20f, Space.World);
            }
        }

        void CheckCollisionAndSetMaterial()
        {
 
            if (collisionChecker == null && !buildingItem)
            {
                SetMaterial(validMaterial);
            }

            if (collisionChecker == null && buildingItem)
            {
                SetMaterial(buildMaterial);
            }

            isObjectColliding = collisionChecker.isColliding;
            if(!buildingItem)
                SetMaterial(isObjectColliding ? invalidMaterial : validMaterial);
            else
                SetMaterial(isObjectColliding ? invalidMaterial : buildMaterial);
        }


        void SetMaterial(Material mat)
        {
            foreach (Renderer rend in objectRenderers)
            {
                if (rend != null)
                    rend.material = mat;
            }
        }

        void RestoreOriginalMaterial()
        {
            if (!buildingItem)
            {
                foreach (var kvp in originalMaterials)
                {
                    if (kvp.Key != null && kvp.Value != null)
                        kvp.Key.material = kvp.Value;
                }
            }
            else
            {
                foreach (var kvp in originalMaterials)
                {
                    if (kvp.Key != null && kvp.Value != null)
                        kvp.Key.material = buildMaterial;
                }

            }
           
        }

        void PlaceObject()
        {
            if (isObjectColliding)
            {
                Debug.Log("Can't place object while colliding.");
                return;
            }

            // Restore colliders
            foreach (var col in grabbedColliders)
            {
                if (col != null)
                    col.isTrigger = false;
            }

            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            RestoreOriginalMaterial();
            Physics.IgnoreLayerCollision(gameObject.layer, grabbedObject.layer, false);
            grabbedObject.layer = 12;
            Destroy(grabbedObject.GetComponent<GrabbedObjectCollisionChecker>());
            if (buildingItem)
                grabbedObject.GetComponent<CraftableItem>().Canvas.SetActive(true);

            grabbedObject = null;
            isHoldingObject = false; inventoryEnabled = true;
            buildingItem = false;
           

        }

        //Build System
        [Header("Build Settings")]
        public GameObject BuildCanvas;
        public GameObject BuildPostProcessing;
        public bool buildMenuOpen;
        public bool buildingItem;
        public List<GameObject> CraftableItems;
        public Material buildMaterial;
        [SerializeField] private float forwardDistance = 3f; // How far in front of the player to spawn
        [SerializeField] private float raycastHeight = 5f; // How high above the ground to start the ray
        [SerializeField] private LayerMask groundLayer; // LayerMask for ground (assign in inspector)

        public void AcessBuildMenu()
        {

            if(Input.GetKeyDown(KeyCode.Tab))
            {
                if (!buildMenuOpen)
                {
                    BuildCanvas.SetActive(true);
                    BuildPostProcessing.SetActive(true);
                    GameManager.Instance.EnableMouseCursor();
                   firstPersonMovementInput.restrictMovement = true;
                    inventoryEnabled = false;
                    buildMenuOpen = true;
                    
                }
                else
                {
                    BuildCanvas.SetActive(false);
                    BuildPostProcessing.SetActive(false);
                    GameManager.Instance.DisableMouseCursor();
                    firstPersonMovementInput.restrictMovement = false;
                    inventoryEnabled = true;
                    buildMenuOpen = false;
                }
               
            }
        }
        GameObject tempBuildObject;
        public void BuildItem(int index)
        {
            buildingItem = true;
            BuildCanvas.SetActive(false);
            BuildPostProcessing.SetActive(false);
            GameManager.Instance.DisableMouseCursor();
            firstPersonMovementInput.restrictMovement = false;       
            buildMenuOpen = false;

          

            // Calculate forward point
            Vector3 forwardPoint = playerCamera.transform.position + playerCamera.transform.forward * forwardDistance;

            // Raycast from above the target point down to find ground
            Vector3 rayOrigin = new Vector3(forwardPoint.x, forwardPoint.y + raycastHeight, forwardPoint.z);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayer))
            {
                // Calculate object height
                float objectHeight = 0f;

                if (CraftableItems[index].TryGetComponent<Renderer>(out var renderer))
                {
                    objectHeight = renderer.bounds.size.y;
                }
                else if (CraftableItems[index].TryGetComponent<Collider>(out var collider))
                {
                    objectHeight = collider.bounds.size.y;
                }

                // Offset the spawn position upward so it sits *on* the ground
                Vector3 spawnPos = hit.point + Vector3.up * (objectHeight / 2f);

                GameObject obj = Instantiate(CraftableItems[index], spawnPos, Quaternion.identity);
                tempBuildObject = obj;
                grabbedObject = obj;
                isHoldingObject = true;


                if (collisionChecker == null)
                {
                    collisionChecker = grabbedObject.AddComponent<GrabbedObjectCollisionChecker>();
                }

                // Store and set colliders to trigger
                grabbedColliders.Clear();
                grabbedColliders.AddRange(grabbedObject.GetComponentsInChildren<Collider>());
                foreach (var col in grabbedColliders)
                {
                    col.isTrigger = true;
                }

                // Set rigidbody kinematic
                Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                if (rb) rb.isKinematic = true;

                // Store and set material
                objectRenderers = grabbedObject.GetComponentsInChildren<Renderer>();
                originalMaterials.Clear();
                foreach (var r in objectRenderers)
                {
                    if (r != null && r.material != null)
                        originalMaterials[r] = r.material;
                }

                SetMaterial(buildMaterial);

                // Set fixed grab height
                originalObjectHeight = grabbedObject.transform.position;
                lastMousePos = Input.mousePosition;

                // Optional: prevent self-collision during grab
                Physics.IgnoreLayerCollision(gameObject.layer, grabbedObject.layer, true);

            }
            else
            {
                Debug.LogWarning("Ground not found when trying to spawn the object.");
            }

        }


     

    }
}

