using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static Shreyas.Interactable;
using Unity.VisualScripting;
using Shreyas;
using TMPro;
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

    private void Awake()
    {
        instance = this;
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


    }
    public void UseInventoryItem()
    {
        if (Input.GetMouseButton(0))
        {

            ItemData selectedItem = InventoryManager.instance.GetCurrentSelectedItem();
            if (selectedItem != null)
            {
                switch (selectedItem.itemTag)
                {
                    case ("Broom"):
                        animator.SetBool("IsUsingBroom", true);
                        break;

                    case ("Axe"):
                        animator.SetBool("IsUsingAxe", true);
                        break;

                    case ("Lighter"):
                        animator.SetBool("CanUseAxe", false);
                        animator.SetBool("CanUseBroom", false);
                        animator.SetBool("CanUseLighter", true);
                        break;

                    // Add more cases for new types
                    default:
                        animator.SetBool("IsUsingAxe", false);
                        animator.SetBool("IsUsingBroom", false);
                        animator.SetBool("IsPunching", true);
                        break;
                }
            }


        }
    }
    [SerializeField] private LayerMask interactLayerMask;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private Camera playerCamera;

    private bool isSignVisible = false;
    private GameObject lastOutlinedObject = null;
    private void HandleInteractionRaycast()
    {
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
                    ShowInteractSign("LMB - Interact");
                    OutlineObject(hitObject);
                    return;
                }
            }

            if (pickup != null)
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
        if (Input.GetMouseButtonDown(0))
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
                }

            }
            
            if (selectedItem != null)
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
        if (Input.GetMouseButtonUp(0))
            animator.SetBool("IsUsingBroom", false);

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

    private void HandleScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) currentIndex = (currentIndex + 1) % inventory.Length;
        else if (scroll < 0f) currentIndex = (currentIndex - 1 + inventory.Length) % inventory.Length;
    }
    [SerializeField] Animator animator;
    private void UpdateHands()
    {
        for (int i = 0; i < handModels.Length; i++)
        {
            handModels[i].SetActive(false);
        }

        if (inventory[currentIndex]?.data != null)
        {
            string tag = inventory[currentIndex].data.itemTag;
            for (int i = 0; i < handModels.Length; i++)
            {
                if (handModels[i].CompareTag(tag))
                {
                    handModels[i].SetActive(true);
                    switch (handModels[i].tag)
                    {
                        case ("Broom"):
                            animator.SetBool("CanUseAxe", false);
                            animator.SetBool("CanUseBroom", true);
                            animator.SetBool("CanUseLighter", false);
                            break;

                        case ("Axe"):
                            animator.SetBool("CanUseAxe", true);
                            animator.SetBool("CanUseBroom", false);
                            animator.SetBool("CanUseLighter", false);
                            break;

                        case ("Lighter"):
                            animator.SetBool("CanUseAxe", false);
                            animator.SetBool("CanUseBroom", false);
                            animator.SetBool("CanUseLighter", true);
                            break;

                        // Add more cases for new types
                        default:
                            Debug.LogWarning("No interaction defined for this type.");
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
}
