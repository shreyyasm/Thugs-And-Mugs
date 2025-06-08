using UnityEngine;

namespace Dhiraj
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "MenuItem", menuName = "Dhiraj/MenuItemData")]
    public class MenuItemData : ScriptableObject
    {
        public int requirementLevel;
        [Space(10)]
        public string name;
        public float price;
        public Vector2 priceRange;
        public Sprite icon;
        public bool isAvailable = true;
        public bool isInMenu = false;

        [TextArea(3, 5)]
        public string description;

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
