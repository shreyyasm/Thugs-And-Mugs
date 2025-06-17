using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Dhiraj
{
    public class MarketManager : MonoBehaviour
    {
        /// <summary>
        /// Temporary Level
        public float level;
        public float AmountIHave;
        /// </summary>

        [Space(5)]
        public Item marketItem;
        public Item cartItem;

        [Space(5)]
        public CanvasGroup marketPlace;
        public Transform marketContent;
        public Transform cartContent;
        public SJManager SmallJerry;
        public WaypointBank waypointBank;

        [Space(5)]
        public TextMeshProUGUI TotalAmount;

        [Space(5)]
        public List<CartItemData> ItemList = new List<CartItemData>();
        public List<CartItemData> CartItem = new List<CartItemData>();

        private void Start()
        {
            //EnableMarket(true);
        }

        public void PopulateStore()
        {
            PopulateStore(ItemList, marketContent);
        }
        public void PopulateStore(List<CartItemData> values, Transform parent)
        {
            // Clear all existing children
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }

            // Proceed to populate
            if (values.Count > 0)
            {
                foreach (CartItemData item in values)
                {
                    Item newItem = Instantiate(marketItem, parent);
                    newItem.item.currentData = item;
                    newItem.item.priceText.text = "$<br>" + item.price.ToString();
                    newItem.item.itemUI.sprite = item.icon;
                    newItem.item.nameText.text = item.name;
                    newItem.item.marketManager = this;                    
                    if (item.requirementLevel > level)
                    {                        
                        newItem.GetComponent<Button>().interactable = false;
                    }
                }
            }
        }

        public void CalculatTotalAmount()
        {
            float totalAmount = 0;
            foreach (CartItemData item in CartItem)
            {
                totalAmount += item.price;
                if(totalAmount < AmountIHave)
                {
                    TotalAmount.text = $"Total : <color=green>{totalAmount.ToString("F2")}</color>";
                }
                else
                {
                    TotalAmount.text = $"Total : <color=red>{totalAmount.ToString("F2")}</color>";
                }
                                
            }            
        }

        public void EnableMarket(bool isEnable)
        {
            float targetAlpha = isEnable ? 1f : 0f;
            float duration = 0.1f; // Adjust transition duration as needed

            // Start tween
            LeanTween.alphaCanvas(marketPlace, targetAlpha, duration)
                .setOnComplete(() =>
                {
                    marketPlace.blocksRaycasts = isEnable;
                });

            PopulateStore();
        }

        public void PlaceOrder()
        {
            SJManager sJManager = Instantiate(SmallJerry.gameObject).GetComponent<SJManager>();
            sJManager.transform.position = waypointBank.path[0].position;
            sJManager.waypointBank = waypointBank;
            sJManager.agent.enabled = true;
            sJManager.gameObject.SetActive(true);
            sJManager.isWalkingWithBarrel = true;
            sJManager.requestedItems = new List<CartItemData>(CartItem);
        }

    }
}
