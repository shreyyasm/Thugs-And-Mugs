using UnityEngine;
using static Shreyas.InventoryManager;
namespace Shreyas
{
    public class Barrel : MonoBehaviour
    {
        public ShelfSystem.ShelfItemType itemType; // Assign in Inspector (e.g., Wine, Vodka, etc.)
        public string Name;

        public GameObject[] sprites;
        public string GetTagForContents()
        {
            return itemType.ToString(); // Assumes the item tag matches the enum name
        }
        private void OnEnable()
        {
            //foreach(GameObject i in sprites)
            //{
            //    i.SetActive(false);
            //}
            //LeanTween.delayedCall(0.5f, () =>
            //{

            //    foreach (GameObject i in sprites)
            //    {
            //        i.SetActive(true);
            //    }
            //});

           
        }
        private void Start()
        {
            Name = itemType.ToString();
        }
    }
}


