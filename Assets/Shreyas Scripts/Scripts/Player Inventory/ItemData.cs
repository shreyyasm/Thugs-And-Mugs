using UnityEngine;

namespace Shreyas
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public Sprite itemIcon;
        public string itemTag;
    }
}

