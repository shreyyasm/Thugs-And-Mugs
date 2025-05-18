using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Dhiraj
{
    public class AController : MonoBehaviour
    {
        public List<Collider> weaponColliders = new List<Collider>();
        public void Enable()
        {
            foreach (var collider in weaponColliders)
            {
                collider.enabled = true;
            }
        }

        public void Disable()
        {
            foreach (var collider in weaponColliders)
            {
                collider.enabled = false;
            }
        }
    }
}
