using UnityEngine;
namespace Shreyas
{
    public class Barrel : MonoBehaviour
    {
        public ShelfSystem.ShelfItemType itemType; // Assign in Inspector (e.g., Wine, Vodka, etc.)

        public string GetTagForContents()
        {
            return itemType.ToString(); // Assumes the item tag matches the enum name
        }
    }
}


