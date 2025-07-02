using Dhiraj;
using Shreyas;
using UnityEngine;
using UnityEngine.InputSystem;
using static Shreyas.Interactable;

public class Weapon : MonoBehaviour
{
    
    public enum WeaponType
    {        
       gun,
       Shortgun,
       axe,
       knucles,
       sickle,
       katana,
       bat,
       knife
        // Add more types here
    }

    [Header("WeaponType Settings")]
    public WeaponType weaponType;

    public int weaponDamage;
    public int knockbackForce;
    public GameObject bloodVFX;
    public InventoryManager inventoryManager;

    bool hit;
    private void Awake()
    {
         if(weaponType != WeaponType.gun || weaponType != WeaponType.Shortgun)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Customer") && other.gameObject.GetComponent<AManager>())
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            if (!hit && inventoryManager.usingWeapon)
            {
                switch (weaponType)
                {                   
                    case WeaponType.axe:
                        HitTarget(other);
                        break;

                    case WeaponType.knucles:
                        HitTarget(other);
                        break;

                    case WeaponType.sickle:
                        HitTarget(other);
                        break;

                    case WeaponType.katana:
                        HitTarget(other);
                        break;

                    case WeaponType.bat:
                        HitTarget(other);
                        break;

                    case WeaponType.knife:
                        HitTarget(other);
                        break;
                }
            }
            if (!hit)
            {
                switch (weaponType)
                {
                    case WeaponType.gun:
                        HitTarget(other);

                        break;

                    case WeaponType.Shortgun:
                        HitTarget(other);
                        break;

                }
            }


        }


    }
    public void HitTarget(Collider other)
    {
        float pushForce = 3f;
        float upwardForce = 1f;
        // Apply knockback to the object we hit (must have Rigidbody)
        Rigidbody hitRb = other.attachedRigidbody;
        if (hitRb != null && !hitRb.isKinematic)
        {
            Vector3 direction = (other.transform.position - transform.position).normalized;
            direction.y += upwardForce; // Add slight upward arc
            direction.Normalize();

            hitRb.AddForce(direction * pushForce, ForceMode.Impulse);
        }

        other.gameObject.GetComponent<AManager>().DealDamage(weaponDamage, gameObject, knockbackForce);
        //Instantiate(bloodVFX, hitPoint, Quaternion.identity);
        hit = true;
        LeanTween.delayedCall(0.5f, () => { hit = false; });

        

        
    }
}
