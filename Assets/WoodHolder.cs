using UnityEngine;

public class WoodHolder : MonoBehaviour
{
    [Header("Log Separation Settings")]
    public float separationForce = 2f; // How strong the logs are pushed apart

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is tagged as "LogBundle"
        if (other.CompareTag("LogBundle"))
        {
            Transform bundleTransform = other.transform;

            int childCount = bundleTransform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform log = bundleTransform.GetChild(i);

                // Unparent the log
                log.SetParent(null);
                

                // Add Rigidbody if not already there
                if (!log.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb = log.gameObject.AddComponent<Rigidbody>();
                }

                // Optional: Set Rigidbody properties
                rb.mass = 1f;
                rb.interpolation = RigidbodyInterpolation.Interpolate;

                // Add a small random force to scatter logs
                Vector3 randomDirection = (Vector3.up + Random.insideUnitSphere).normalized;
                rb.AddForce(randomDirection * separationForce, ForceMode.Impulse);
            }

            // Destroy the empty bundle parent
            Destroy(bundleTransform.gameObject);
        }
    }
}
