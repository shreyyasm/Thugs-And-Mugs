using Dhiraj;
using Shreyas;
using UnityEngine;

namespace Shreyas
{
    public class CustomerOrder : MonoBehaviour
    {
        public GameObject CustomerOrderCamera;
        public GameObject OrderCanvas;
        public GameObject InteractSign;

        private Interactable interactable;

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
        public void GiveOrder()
        {
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

