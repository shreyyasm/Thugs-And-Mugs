using UnityEngine;

namespace Dhiraj
{
    [CreateAssetMenu(fileName = "marketItem Data", menuName = "Dhiraj/CartItemData")]
    public class CartItemData : ScriptableObject
    {
        public int requirementLevel;
        [Space (10)]
        public string name;
        public float price;
        public Sprite icon;
        public MenuItemData thisMenuData;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Automatically set name to asset file name
            string path = UnityEditor.AssetDatabase.GetAssetPath(this);
            this.name = System.IO.Path.GetFileNameWithoutExtension(path);
        }
#endif
    }
}
