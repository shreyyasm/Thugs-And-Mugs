using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dhiraj
{
    /// <summary>
    /// BarrelContainer is a MonoBehaviour that can be attached to a GameObject in Unity.
    /// It currently does not have any functionality but serves as a placeholder for future development.
    /// </summary>
    public class BarrelContainer : MonoBehaviour
    {
        public List<Transform> barrels = new List<Transform>();
        public int rows = 3;
        public int columns = 4;
        public float spacingX = 1.5f; // Adjust spacing as needed
        public float spacingZ = 1.5f;

        private void Start()
        {
            ArrangeBarrale();
        }

        public void ArrangeBarrale()
        {
            if (barrels.Count > 0) barrels.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform barrel = transform.GetChild(i);
                if (barrel != null)
                {
                    barrels.Add(barrel);
                }
            }

            for (int i = 0; i < barrels.Count; i++)
            {
                int row = i / columns;
                int column = i % columns;

                Vector3 localPos = new Vector3(column * spacingX, row * spacingZ, 0); // negative Z for forward stacking
                barrels[i].localPosition = localPos;
                barrels[i].rotation = Quaternion.Euler(0,90,-90);
            }
        }
    }

}
