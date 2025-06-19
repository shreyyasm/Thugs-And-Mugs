using UnityEngine;
using UnityEngine.EventSystems;
using Dhiraj;
using UnityEngine.Animations.Rigging;
namespace Shreyas
{
    public class BrewManager : Singleton<BrewManager>
    {
        public GameObject BrewStationCamera;
        public GameObject BrewStationMakingCamera;
        public GameObject BrewCanvas;

        [Header("Holders")]
        public Transform mugHolder;
        public Transform drinkHolder;

        private GameObject currentMug;
        private GameObject currentDrink;

        [HideInInspector] public static BrewManager Instance;
        private bool isStationOpen = false;

        public bool MugReady;
        public bool DrinkReady;

        public GameObject Mug;
        public GameObject Drink;
        public GameObject MakeButton;
        public FirstPersonMovementInputSystem firstPersonMovementInputSystem;

        public bool pouring;
        private void Awake()
        {
            Instance = this;
        }

        public void EnableMaking()
        {
            if(DrinkReady && MugReady)
                MakeButton.SetActive(true);
        }
        public void OpenBrewStation()
        {
            BrewStationCamera.SetActive(true);
            BrewCanvas.SetActive(true);
            isStationOpen = true;
            
            GameManager.Instance.EnableMouseCursor();
            

          
        }
        public Transform DrinkPourPosition;
        public GameObject BrewSlotMug;
        public GameObject BrewSlotDrink;
        public void  PourDrink()
        {
            Drink.transform.SetParent(null);
            BrewStationMakingCamera.SetActive(true);
            BrewCanvas.SetActive(false);
            Drink.GetComponent<PourDetector>().enabled = true;
            Drink.GetComponent<BottleController>().enabled = true;
            Drink.transform.position = DrinkPourPosition.position;
            Drink.transform.rotation = DrinkPourPosition.rotation;
            Mug.GetComponent<BoxCollider>().enabled = false;
            Mug.GetComponent<MeshCollider>().enabled = true;

            BrewSlotMug.SetActive(true);      
            InventorySlot slot = BrewSlotMug.GetComponentInChildren<InventorySlot>();
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
            DrinkReady = false;
            MugReady = false;   
            BrewSlotDrink.SetActive(true);
            InventorySlot slot2 = BrewSlotDrink.GetComponentInChildren<InventorySlot>();
            if (slot2 != null)
            {
                Destroy(slot2.gameObject);
            }
            MakeButton.SetActive(false);
            pouring = true;
            Drink.layer = 0;
        }

        public void DrinkComplete()
        {
            Mug.transform.SetParent(null);
            Drink.transform.SetParent(null);
            BrewStationMakingCamera.SetActive(false);
            BrewStationCamera.SetActive(false);
            BrewCanvas.SetActive(false);
            
            Mug.GetComponent<BoxCollider>().enabled = true;
            Mug.GetComponent<MeshCollider>().enabled = false;
            Mug.GetComponent<Rigidbody>().isKinematic = false;
            Mug.GetComponent<Rigidbody>().useGravity = true;
            Mug.layer = 6;
            Drink = null;
            Mug = null;
            firstPersonMovementInputSystem.playerBusy = false;
            gameObject.layer = 12;
            pouring = false;

        }
        public bool IsOpen() => isStationOpen;

        public bool TryAddObject(GameObject obj)
        {
            if (obj.CompareTag("Mug") && currentMug == null)
            {
                currentMug = obj;
                PlaceObject(obj, mugHolder);
               // Mug = obj;
                Debug.Log("Mug placed.");
                return true;
            }
            else if (obj.GetComponent<Drink>() && currentDrink == null)
            {
                currentDrink = obj;
                PlaceObject(obj, drinkHolder);
               // Drink = obj;
                Debug.Log("Drink placed.");
                return true;
            }

            Debug.Log("Object rejected.");
            return false;
        }

        private void PlaceObject(GameObject obj, Transform holder)
        {
            obj.transform.SetParent(holder);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            if (obj.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            if (obj.TryGetComponent(out Collider col))
                col.enabled = false;
        }

        public void Clear()
        {
            if (currentMug != null) Destroy(currentMug);
            if (currentDrink != null) Destroy(currentDrink);
            currentMug = null;
            currentDrink = null;
        }

        public bool IsReadyToBrew()
        {
            return currentMug != null && currentDrink != null;
        }
    }
}
