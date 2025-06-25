using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject impactEffectPrefab;
    public float destroyDelay = 2f;

    private bool hasHit = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        ContactPoint contact = collision.contacts[0];
        Vector3 impactPosition = contact.point;

        // ✅ Make the effect face away from the surface (correct direction)
        Quaternion impactRotation = Quaternion.LookRotation(contact.normal);

        // Spawn and parent the effect
        if (impactEffectPrefab != null)
        {
            GameObject impact = Instantiate(impactEffectPrefab, impactPosition, impactRotation);
            impact.transform.SetParent(collision.transform);
        }


        // Optional: disable visuals and physics
        var renderer = GetComponent<MeshRenderer>();
        if (renderer) renderer.enabled = false;

        var collider = GetComponent<Collider>();
        if (collider) collider.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb) rb.linearVelocity = Vector3.zero;

        Destroy(gameObject, destroyDelay);
    }
}
