using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
namespace Shreyas
{
    public class Interactable : MonoBehaviour
    {
        public enum InteractableType
        {
            defaultItem,
            TreeInteraction,
            BroomInteraction,
            WoodCuttingMachineInteraction,
            BarrelInteraction,
            // Add more types here
        }

        [Header("Interactable Settings")]
        public InteractableType interactableType;

        public GameObject CuttingMachineCamera;
        public Camera CameraMain;
        public FirstPersonMovementInputSystem firstPersonController;
        public bool CanBeInteracted;
        public bool isPickable;
        public string requiredItemTag;

        private void Update()
        {

        }

        public void Interact(ItemData selectedItem = null, GameObject item = null)
        {
            switch (interactableType)
            {
                case InteractableType.TreeInteraction:
                    if (selectedItem != null && selectedItem.itemTag == "Axe")
                        InteractWithTree();
                    else
                        Debug.Log("You need an Axe to interact with this tree.");
                    break;

                case InteractableType.BroomInteraction:
                    if (selectedItem != null && selectedItem.itemTag == "Broom")
                        InteractWithBroom();
                    else
                        Debug.Log("You need a Broom to interact with this.");
                    break;

                case InteractableType.WoodCuttingMachineInteraction:
                    InteractWithCuttingMachine();
                    break;

                case InteractableType.BarrelInteraction:
                    if (selectedItem != null && selectedItem.itemTag == "Barrel")
                        InteractWithBarShelf(item);
                    break;

                default:
                    //Debug.LogWarning("No interaction defined for this type.");
                    break;
            }
        }

        private void InteractWithTree()
        {
            TreeScript tree = GetComponent<TreeScript>();
            if (tree != null)
            {
                Vector3 playerPosition = Camera.main.transform.position; // assuming single player cam
                tree.TakeDamage(34f, playerPosition);
            }
        }

        private void InteractWithBroom()
        {
            Debug.Log("You picked up the broom.");
            // Add broom-specific logic here
        }
        public void InteractWithCuttingMachine()
        {
            firstPersonController.playerBusy = true;
            CuttingMachineCamera.SetActive(true);
            /*Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;*/
            //CameraMain.orthographic = true;
        }
        public void InteractWithBarShelf(GameObject barrel)
        {
            ShelfSystem shelf = GetComponent<ShelfSystem>();
            if (shelf != null)
            {
             
                //Vector3 playerPosition = Camera.main.transform.position; // assuming single player cam
                shelf.FillShelf(barrel);
            }
        }

    }

}

