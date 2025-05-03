using System.Collections.Generic;
using UnityEngine;

namespace Dhiraj
{
    public class WaypointBank : MonoBehaviour
    {
        public List<Transform> path = new List<Transform>();

        public void Awake()
        {
            foreach (Transform t in transform)
            {
                path.Add(t);
            }
        }
    }
}
