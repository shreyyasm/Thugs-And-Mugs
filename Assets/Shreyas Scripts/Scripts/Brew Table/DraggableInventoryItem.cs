using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shreyas
{
    public class DraggableInventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static GameObject currentDraggedClone; // This is what BrewSlot will use
        private GameObject clone;
        private Image originalImage;
        private Canvas canvas;

        private void Awake()
        {
            originalImage = GetComponent<Image>();
            canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Only drag if image has sprite
            if (originalImage.sprite == null) return;

            // Disable original image
            originalImage.enabled = false;

            // Create clone
            clone = Instantiate(gameObject, canvas.transform);
            clone.name = "DraggedClone";
            clone.transform.position = transform.position;
            clone.GetComponent<Image>().enabled = true;
            clone.transform.GetChild(0).gameObject.SetActive(false);

            // Remove this script from the clone so it doesn't interfere
            Destroy(clone.GetComponent<DraggableInventoryItem>());

            // Track the clone for the BrewSlot
            currentDraggedClone = clone;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (clone != null)
            {
                RectTransform rt = clone.GetComponent<RectTransform>();
                rt.anchoredPosition += eventData.delta / canvas.scaleFactor;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // If clone wasn't dropped on BrewSlot, destroy it
            if (clone != null && clone.transform.parent == canvas.transform)
            {
                Destroy(clone);
                originalImage.enabled = true; // Re-enable original
            }

            currentDraggedClone = null;
            clone = null;
        }
    }

}
