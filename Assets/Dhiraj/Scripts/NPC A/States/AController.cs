using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Dhiraj
{
    public class AController : MonoBehaviour
    {
        public GameObject mainBody;
        public List<Collider> weaponColliders = new List<Collider>();
        public Rigidbody bullet;
        public Transform _bulletSpwanPoint;

        public float bulletForce = 1000f;
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

        public void EnableOneCollider(int num)
        {
            weaponColliders[num].enabled = true;
        }


        public void Shoot()
        {
            Rigidbody bullet = Instantiate(this.bullet, _bulletSpwanPoint.position, _bulletSpwanPoint.rotation);
            bullet.AddForce(_bulletSpwanPoint.forward * bulletForce, ForceMode.Impulse);
            Destroy(bullet, 5);
        }

        public void Dead()
        {
            DestroyImmediate(mainBody);
        }

    }
    
}
