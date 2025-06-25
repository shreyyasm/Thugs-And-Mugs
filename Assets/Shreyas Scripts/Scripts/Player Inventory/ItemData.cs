using UnityEngine;

namespace Shreyas
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public Sprite itemIcon;
        public string itemTag;
        public bool MouseUse;
        public bool isGun;
        public int TotalBullets;
        public int CurrentBullets;
        public int magSize;
    }
}

