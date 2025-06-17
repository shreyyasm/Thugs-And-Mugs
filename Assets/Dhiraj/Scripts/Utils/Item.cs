using Dhiraj;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dhiraj
{
    [Serializable]
    public struct MenuItemUI
    {
        public BackgroundMenuManager bMenuManager;
        public FrontMenuManager fMenuManager;
        public MenuItemData currentData;
        public TextMeshProUGUI itemName;
        public Image itemImage;
        public Image lockIcon;
        public Image backgroundImage;
        public Button thisButton;
    }
    [Serializable]
    public struct CartItemUI
    {
        public CartItemData currentData;
        public TextMeshProUGUI nameText;
        public Image itemUI;
        public TextMeshProUGUI priceText;
        public MarketManager marketManager;
    }
    public abstract class Item : MonoBehaviour
    {
        public CartItemUI item;
        public MenuItemUI menuItemUI;
    }
}
