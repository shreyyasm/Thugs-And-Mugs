using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;
namespace Shreyas
{
    public class InventorySlot : MonoBehaviour
    {
        public Image icon;
        public GameObject highlight;
        public string Name;
        public int SlotNumber;

        public void SetItem(ItemData item)
        {
            icon.sprite = item.itemIcon;
            icon.enabled = true;
            Name = item.itemName;
        }

        public void ClearSlot()
        {
            icon.sprite = null;
            icon.enabled = false;
            Name = null;
            highlight.SetActive(false);
        }
    }

}

