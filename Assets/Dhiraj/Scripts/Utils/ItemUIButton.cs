using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;
using static UnityEditor.UIElements.ToolbarMenu;

namespace Dhiraj
{
    public class ItemUIButton : Item
    {   
        public void OnClick(int itemType)
        {
            switch (itemType)
            {
                case 0: 
                    CartItemInteraction();
                    break;
                case 1:
                    MenuItemInteraction(0);
                    break;
                case 2:
                    MenuItemInteraction(1);
                    break;
            }
           
        }

        public void CartItemInteraction()
        {
            Item newItem = Instantiate(item.marketManager.cartItem, item.marketManager.cartContent);
            newItem.item.priceText.text = $"<color=green>{item.priceText.text.Replace("<br>", "")}</color>";
            newItem.item.itemUI.sprite = item.itemUI.sprite;
            newItem.item.nameText.text = item.nameText.text;
            newItem.item.currentData = item.currentData;
            newItem.item.marketManager = item.marketManager;
            item.marketManager.CartItem.Add(item.currentData);
            item.marketManager.CalculatTotalAmount();
        }

        public void MenuItemInteraction(int manager)
        {
            if(manager == 0)
            {
                menuItemUI.bMenuManager.OpenUpdatePriceMenu(menuItemUI.currentData.isInMenu);
                menuItemUI.bMenuManager.updatePriceUIData.description.text = menuItemUI.currentData.description;
                menuItemUI.bMenuManager.updatePriceUIData.itemName.text = menuItemUI.currentData.name;
                menuItemUI.bMenuManager.updatePriceUIData.newPrice.text = $"$ {menuItemUI.currentData.price.ToString()}";
                menuItemUI.bMenuManager.updatePriceUIData.itemImage.sprite = menuItemUI.currentData.icon;
                menuItemUI.bMenuManager.selectedItem = menuItemUI.currentData;
            }
            else
            {
                menuItemUI.fMenuManager.OpenUpdatePriceMenu();
                menuItemUI.fMenuManager.updatePriceUIData.description.text = menuItemUI.currentData.description;
                menuItemUI.fMenuManager.updatePriceUIData.itemName.text = menuItemUI.currentData.name;
                menuItemUI.fMenuManager.updatePriceUIData.newPrice.text = $"$ {menuItemUI.currentData.price.ToString()}";
                menuItemUI.fMenuManager.updatePriceUIData.itemImage.sprite = menuItemUI.currentData.icon;
                menuItemUI.fMenuManager.selectedItem = menuItemUI.currentData;
            }
           
        }

        public void RemoveItemFromCart()
        {
            if (item.marketManager.CartItem.Remove(item.currentData))
            {
                Destroy(this.gameObject);
                item.marketManager.CalculatTotalAmount();
            }
        }
    }
}
