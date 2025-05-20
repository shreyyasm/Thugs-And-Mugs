using Shreyas;
using UnityEngine;
using UnityEngine.EventSystems;
using static Shreyas.InventoryManager;

namespace Shreyas
{
    public class BrewSlot : MonoBehaviour, IDropHandler
    {
        public enum SlotType { Mug, Drink }
        public SlotType slotType;

        public void OnDrop(PointerEventData eventData)
        {
            GameObject dropped = DraggableInventoryItem.currentDraggedClone;
            if (dropped == null) return;

            if (slotType == SlotType.Mug)
            {
                if (dropped.GetComponent<InventorySlot>().Name != "Mug")
                    return;
                else
                {
                    dropped.transform.SetParent(transform);
                    dropped.transform.localPosition = Vector3.zero;
                    InventoryManager.instance.DropItemByChoice(dropped.GetComponent<InventorySlot>().SlotNumber, BrewManager.Instance.mugHolder);
                    Debug.Log($"Item dropped in {slotType} slot.");
                    BrewManager.Instance.Mug = dropped;
                    BrewManager.Instance.MugReady = true;
                    BrewManager.Instance.EnableMaking();
                    gameObject.SetActive(false);


                }
            }
            if (slotType == SlotType.Drink)
            {
                if (dropped.GetComponent<InventorySlot>().Name != "Drink")
                    return;
                else
                {
                    dropped.transform.SetParent(transform);
                    dropped.transform.localPosition = Vector3.zero;
                    InventoryManager.instance.DropItemByChoice(dropped.GetComponent<InventorySlot>().SlotNumber, BrewManager.Instance.drinkHolder);
                    Debug.Log($"Item dropped in {slotType} slot.");
                    BrewManager.Instance.Drink = dropped;
                    BrewManager.Instance.DrinkReady = true;
                    BrewManager.Instance.EnableMaking();
                    gameObject.SetActive(false);
                }
            }

        }
    }
}


