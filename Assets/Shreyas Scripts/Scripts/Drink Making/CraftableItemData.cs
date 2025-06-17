using UnityEngine;

namespace Shreyas
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "CraftableItem/Item")]
    public class CraftableItemData : ScriptableObject
    {
        public string itemName;
        public int woodRequied;
      
    }
}

