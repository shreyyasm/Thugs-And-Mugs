using Shreyas;
using System;
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
                    InventoryManager.instance.SetTemp_GameObject(dropped.GetComponent<InventorySlot>().SlotNumber);
                    if (!InventoryManager.instance.tempGameObject.GetComponent<Mug>().isFilled)
                    {
                        dropped.transform.SetParent(transform);
                        dropped.transform.localPosition = Vector3.zero;
                        InventoryManager.instance.DropItemByChoice(dropped.GetComponent<InventorySlot>().SlotNumber, BrewManager.Instance.mugHolder);
                        Debug.Log($"Item dropped in {slotType} slot.");
                        BrewManager.Instance.Mug = InventoryManager.instance.tempGameObject;
                        BrewManager.Instance.MugReady = true;
                        BrewManager.Instance.EnableMaking();
                        BrewManager.Instance.gameObject.layer = 6;
                        gameObject.SetActive(false);


                    }

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
                    BrewManager.Instance.Drink = InventoryManager.instance.tempGameObject;
                    BrewManager.Instance.DrinkReady = true;
                    BrewManager.Instance.EnableMaking();
                    BrewManager.Instance.gameObject.layer = 6;
                    gameObject.SetActive(false);
                }
            }

        }
        public void ResetSlot()
        {
            gameObject.SetActive(true);
        }
    }
}


