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
        public List<ItemData> ItemList = new List<ItemData>();
        public List<ItemData> CartItem = new List<ItemData>();

        private void Start()
        {
            //EnableMarket(true);
        }

        public void PopulateStore()
        {
            PopulateStore(ItemList, marketContent);
        }
        public void PopulateStore(List<ItemData> values, Transform parent)
        {
            // Clear all existing children
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }

            // Proceed to populate
            if (values.Count > 0)
            {
                foreach (ItemData item in values)
                {
                    Item newItem = Instantiate(marketItem, parent);
                    newItem.currentData = item;
                    newItem.priceText.text = "$<br>" + item.price.ToString();
                    newItem.itemUI.sprite = item.icon;
                    newItem.nameText.text = item.name;
                    newItem.marketManager = this;                    
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
            foreach (ItemData item in CartItem)
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
            LeanTween.alphaCanvas(marketPlace, targetAlpha, duration);           
        }

        public void PlaceOrder()
        {
            SJManager sJManager = Instantiate(SmallJerry.gameObject).GetComponent<SJManager>();
            sJManager.transform.position = waypointBank.path[0].position;
            sJManager.waypointBank = waypointBank;
            sJManager.agent.enabled = true;
            sJManager.gameObject.SetActive(true);
            sJManager.isWalkingWithBarrel = true;
        }

    }
}
