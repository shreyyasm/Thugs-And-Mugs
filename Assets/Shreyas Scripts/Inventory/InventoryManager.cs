using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static Shreyas.Interactable;

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
        HandleScroll();
        UpdateHands();
        UpdateHighlight();
        HandleInteractionRaycast(); // 👈 Add this
    }
    [SerializeField] private LayerMask interactLayerMask;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private Camera playerCamera;

    private void HandleInteractionRaycast()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask))
            {
                PickupTrigger pickup = hit.collider.GetComponent<PickupTrigger>();
                if (pickup != null && pickup.itemData != null)
                {
                    PickupItem(pickup.itemData, pickup.gameObject);
                }
            }
        }
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
