using UnityEngine;
using UnityEngine.EventSystems;
using Dhiraj;
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

        public void  PourDrink()
        {
            BrewStationMakingCamera.SetActive(true);
            BrewCanvas.SetActive(false);
        }
        public bool IsOpen() => isStationOpen;

        public bool TryAddObject(GameObject obj)
        {
            if (obj.CompareTag("Mug") && currentMug == null)
            {
                currentMug = obj;
                PlaceObject(obj, mugHolder);
                Debug.Log("Mug placed.");
                return true;
            }
            else if (obj.GetComponent<Drink>() && currentDrink == null)
            {
                currentDrink = obj;
                PlaceObject(obj, drinkHolder);
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
