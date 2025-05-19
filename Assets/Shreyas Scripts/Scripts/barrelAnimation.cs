using Shreyas;
using UnityEngine;

namespace Shreyas
{
    public class barrelAnimation : MonoBehaviour
    {

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void BarrelEnable()
        {
            InventoryManager.instance.PickupBarrel();
        }
    }

}

