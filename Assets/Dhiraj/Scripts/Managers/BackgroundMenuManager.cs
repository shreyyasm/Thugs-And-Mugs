using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using static Cinemachine.DocumentationSortingAttribute;
using static UnityEditor.Progress;


namespace Dhiraj
{
    [Serializable]
    public struct UpdatePriceUI
    {
        public TextMeshProUGUI description;
        public TextMeshProUGUI itemName;
        public Image itemImage;
        public Button addRemove;
        public TMP_InputField newPrice;
    }
    public class BackgroundMenuManager : MonoBehaviour
    {
        public UpdatePriceUI updatePriceUIData;
        public List<MenuItemData> ItemList = new List<MenuItemData>();
        

        public GameObject closeButton;
        public Animator updatePriceUI;
        public Transform menuContain;




        public Item menuItem;
        public MenuItemData selectedItem;

        public void OpenUpdatePriceMenu(bool isAdd)
        {
            closeButton.SetActive(false);
            updatePriceUI.Play("Open");
            if (isAdd)
            {
                updatePriceUIData.addRemove.GetComponentInChildren<TextMeshProUGUI>().text = "Remove";
                updatePriceUIData.addRemove.onClick.RemoveAllListeners();
                updatePriceUIData.addRemove.onClick.AddListener(() => RemoveItemFromMenu(false));
            }
            else
            {
                updatePriceUIData.addRemove.GetComponentInChildren<TextMeshProUGUI>().text = "Add";
                updatePriceUIData.addRemove.onClick.RemoveAllListeners();
                updatePriceUIData.addRemove.onClick.AddListener(() => RemoveItemFromMenu(true));
            }
        }
        public void CloseUpdatePriceMenu()
        {
            closeButton.SetActive(true);
            updatePriceUI.Play("Close");
            selectedItem = null;
            updatePriceUIData.newPrice.textComponent.color = Color.black;
        }

        private void OnEnable()
        {
            PopulateStore(ItemList, menuContain);
        }

        public void RePopulate()
        {
            PopulateStore(ItemList, menuContain);
        }

        public void PopulateStore(List<MenuItemData> values, Transform parent)
        {
            // Clear all existing children
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }

            // Proceed to populate
            if (values.Count > 0)
            {
                foreach (MenuItemData item in values)
                {
                    if (!item.isAvailable) continue;

                    Item newItem = Instantiate(menuItem, parent);
                    newItem.menuItemUI.currentData = item;
                    if(!item.isInMenu) newItem.menuItemUI.lockIcon.gameObject.SetActive(true);
                    else newItem.menuItemUI.lockIcon.gameObject.SetActive(false);
                    //newItem.menuItemUI.currentPrice.text = "$<br>" + item.price.ToString();
                    newItem.menuItemUI.itemImage.sprite = item.icon;
                    newItem.menuItemUI.itemName.text = item.name;
                    newItem.menuItemUI.bMenuManager = this;

                    //if (!item.isAvailable)                        
                }
            }
        }


        public void SetNewPrice()
        {
            float newPriceValue;
            if (float.TryParse(updatePriceUIData.newPrice.text.Trim(), out newPriceValue))
            {
                // success
            }
            else
            {
                Debug.LogWarning("Invalid price input: " + updatePriceUIData.newPrice.text);
            }
            if (newPriceValue>=selectedItem.priceRange.x && newPriceValue <= selectedItem.priceRange.y)
            {
                selectedItem.price = newPriceValue;
                updatePriceUIData.newPrice.textComponent.color = Color.black;
                CloseUpdatePriceMenu();
                RePopulate();
            }
            else
            {
                updatePriceUIData.newPrice.textComponent.color = Color.red;
            }
        }
        public void RemoveItemFromMenu(bool isTrue)
        {
            selectedItem.isInMenu = isTrue;
            CloseUpdatePriceMenu();
            RePopulate();
        }
    }
}