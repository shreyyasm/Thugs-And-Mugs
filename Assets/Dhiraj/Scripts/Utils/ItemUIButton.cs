using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;
using static UnityEditor.UIElements.ToolbarMenu;

namespace Dhiraj
{
    public class ItemUIButton : Item
    {
        public Button thisButton;        

        public void OnClick()
        {
            Item newItem = Instantiate(marketManager.cartItem, marketManager.cartContent);
            newItem.priceText.text = $"<color=green>{priceText.text.Replace("<br>", "")}</color>";
            newItem.itemUI.sprite = itemUI.sprite;
            newItem.nameText.text = nameText.text;
            newItem.currentData = currentData;
            newItem.marketManager = marketManager;
            marketManager.CartItem.Add(currentData);
            marketManager.CalculatTotalAmount();
        }


        public void RemoveItemFromCart()
        {
            /* foreach (var item in marketManager.CartItem)
             {
                 if(item == currentData)
                 {
                     marketManager.CartItem.Remove(item);
                     Destroy(this.gameObject);
                     marketManager.CalculatTotalAmount();
                 }
             }*/

            if (marketManager.CartItem.Remove(currentData))
            {
                Destroy(this.gameObject);
                marketManager.CalculatTotalAmount();
            }

            /*for (int i = marketManager.CartItem.Count - 1; i >= 0; i--)
            {
                if (marketManager.CartItem[i] == currentData)
                {
                    marketManager.CartItem.RemoveAt(i);
                    Destroy(this.gameObject);
                    marketManager.CalculatTotalAmount();
                    break;
                }
            }*/
        }
    }
}
