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
       knife,
       bareHands
        // Add more types here
    }

    [Header("WeaponType Settings")]
    public WeaponType weaponType;

    public int weaponDamage;
    public int knockbackForce;
    public GameObject bloodVFX;
    public InventoryManager inventoryManager;

    public bool hit;
    private void Awake()
    {
        inventoryManager = FindAnyObjectByType<InventoryManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Customer"))
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            if (!hit && inventoryManager.usingWeaponDuration)
            {
                switch (weaponType)
                {
                    case WeaponType.bareHands:
                        HitTarget(other);
                        SFXManager.Instance.PlaySFX("Inventory/BarehandHit", 0.7f);
                        break;

                    case WeaponType.axe:
                        HitTarget(other);
                        SFXManager.Instance.PlaySFX("Inventory/AxeHit", 0.7f);
                        break;

                    case WeaponType.knucles:
                        HitTarget(other);
                        SFXManager.Instance.PlaySFX("Inventory/KnucleHit", 0.7f);
                       
                        break;

                    case WeaponType.sickle:
                        HitTarget(other);
                        SFXManager.Instance.PlaySFX("Inventory/SickleHit", 1f);
                        break;

                    case WeaponType.katana:
                        HitTarget(other);
                        SFXManager.Instance.PlaySFX("Inventory/KatanaHit", 0.7f);
                        break;

                    case WeaponType.bat:
                        HitTarget(other);
                        SFXManager.Instance.PlaySFX("Inventory/BatHit",1f);
                        break;

                    case WeaponType.knife:
                        HitTarget(other);
                        SFXManager.Instance.PlaySFX("Inventory/KnifeHit", 0.7f);
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
            // Knockback direction is directly away from the player
            Vector3 knockbackDir = inventoryManager.transform.forward;
            knockbackDir.y += upwardForce; // Add upward arc
            knockbackDir.Normalize();

            //hitRb.AddForce(knockbackDir * pushForce, ForceMode.Impulse);
        }

        other.gameObject.GetComponent<AManager>()?.DealDamage(weaponDamage, gameObject, knockbackForce, transform);
        if (other.transform.root.GetComponent<Ragdoll>())
            other.transform.root.GetComponent<Ragdoll>().DealDamage();


        hit = true;
        inventoryManager.usingWeaponDuration = false;
        LeanTween.delayedCall(0.5f, () => { hit = false; });
    }

}
