using UnityEngine;

public class LogCutter : MonoBehaviour
{
    public GameObject logHalfPrefab;
    private bool isCut = false;
    private bool isInsideSaw = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Saw"))
        {
            isInsideSaw = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Saw") && isInsideSaw && !isCut)
        {
            SplitLog();
           
        }
    }

    void SplitLog()
    {
        isCut = true;

        // Create two halves
        GameObject firstHalf = Instantiate(logHalfPrefab, transform.position, transform.rotation);
        GameObject secondHalf = Instantiate(logHalfPrefab, transform.position, transform.rotation);

        // Scale them to half size on X-axis
        Vector3 originalScale = transform.localScale;
        firstHalf.transform.localScale = new Vector3(originalScale.x / 2f, originalScale.y, originalScale.z);
        secondHalf.transform.localScale = new Vector3(originalScale.x / 2f, originalScale.y, originalScale.z);

        // Move them left and right slightly
        Vector3 right = transform.right;
        float gap = 0.1f;
        Vector3 offset = right * (originalScale.x / 4f + gap);

        firstHalf.transform.position = transform.position - offset;
        secondHalf.transform.position = transform.position + offset;

        // Add Rigidbody for physics
        Rigidbody rb1 = firstHalf.AddComponent<Rigidbody>();
        Rigidbody rb2 = secondHalf.AddComponent<Rigidbody>();

        // Add small force sideways
        rb1.AddForce(-right * 2f, ForceMode.Impulse);
        rb2.AddForce(right * 2f, ForceMode.Impulse);

        // Destroy the original log
        Destroy(gameObject);
    }
}
