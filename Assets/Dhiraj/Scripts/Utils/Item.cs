using Dhiraj;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Item : MonoBehaviour
{

    public ItemData currentData;
    public TextMeshProUGUI nameText;
    public Image itemUI;
    public TextMeshProUGUI priceText;
    public MarketManager marketManager;
}
