using Dhiraj;
using Shreyas;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shreyas
{
    [System.Serializable]
    public struct CanvasData
    {
        public Image icon;
        public TextMeshProUGUI nameText;
    }
    public class CustomerOrder : MonoBehaviour
    {
        public string CustomerName;
        
        public CanvasData canvasData; // Reference to the CanvasData struct for UI elements
        public GameObject CustomerOrderCamera;
        public GameObject OrderCanvas;
        public GameObject InteractSign;

        private Interactable interactable;

        public MenuItemData menuItemData; // Reference to the MenuItemData scriptable object for the order

        public bool isOrderPlaced = false; // Flag to check if the order is placed
        private void Awake()
        {
            interactable = GetComponent<Interactable>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            interactable.CanBeInteracted = true;
        }

        // Update is called once per frame
        void Update()
        {

        }


        public void SearchMenu()
        {
            //Menu Code will go here

        }


        public void GiveOrder()
        {
            if (isOrderPlaced) return;
            canvasData.icon.sprite = menuItemData.icon; // Set the icon from MenuItemData
            canvasData.nameText.text = menuItemData.name; // Set the name from MenuItemData
            CustomerOrderCamera.SetActive(true);
            OrderCanvas.SetActive(true);
            InteractSign.SetActive(false);
            GameManager.Instance.EnableMouseCursor();
            GetComponent<Interactable>().firstPersonController.playerBusy = false;
            GetComponent<Interactable>().inventoryManager.SetInventoryCanvas(false);
        }
        public void PlaceOrder()
        {
            CustomerOrderCamera.SetActive(false);
            OrderCanvas.SetActive(false);
            InteractSign.SetActive(false);
            GameManager.Instance.DisableMouseCursor();
            GetComponent<Interactable>().firstPersonController.playerBusy = false;
            GetComponent<Interactable>().inventoryManager.SetInventoryCanvas(true);

            TaskManager.instance.AssignTask($"{CustomerName} ordered {menuItemData.name}", 15);
            isOrderPlaced = true;
        }
        public void CancelOrder()
        {
            CustomerOrderCamera.SetActive(false);
            OrderCanvas.SetActive(false);
            InteractSign.SetActive(false);
            GameManager.Instance.DisableMouseCursor();
            GetComponent<Interactable>().firstPersonController.playerBusy = false;
            GetComponent<Interactable>().inventoryManager.SetInventoryCanvas(true);
        }
    }
}

