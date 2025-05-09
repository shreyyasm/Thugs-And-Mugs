using UnityEngine;

namespace Dhiraj
{
    [CreateAssetMenu(fileName = "marketItem Data", menuName = "Dhiraj/ItemData")]
    public class ItemData : ScriptableObject
    {
        public int requirementLevel;
        [Space (10)]
        public float price;
        public Sprite icon;
        public string name;
    }
}
