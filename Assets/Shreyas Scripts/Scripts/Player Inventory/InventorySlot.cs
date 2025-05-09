using UnityEngine;
using UnityEngine.UI;
namespace Shreyas
{
    public class InventorySlot : MonoBehaviour
    {
        public Image icon;
        public GameObject highlight;

        public void SetItem(ItemData item)
        {
            icon.sprite = item.itemIcon;
            icon.enabled = true;
        }

        public void ClearSlot()
        {
            icon.sprite = null;
            icon.enabled = false;
            highlight.SetActive(false);
        }
    }

}

