using UnityEngine;
using UnityEngine.UI;
using System.Collections;
namespace Shreyas
{
    public class TreeScript : MonoBehaviour
    {
        [Header("Health Settings")]
        public float maxHealth = 100f;
        public float currentHealth;

        [Header("UI")]
        public GameObject healthUI;
        public Slider healthSlider;

        [Header("UI Animation")]
        public float barDisappearDelay = 3f;
        public float smoothSpeed = 5f;

        [Header("UI Position Offset")]
        public Vector3 barOffsetDirection = new Vector3(0, 1.5f, 0); // upward offset
        public float barForwardOffset = 0.5f;

        [Header("On Destroy Settings")]
        public GameObject spawnOnDestroyPrefab;
        public float spawnHeightOffset = 5f;
        public GameObject treeVisualToDisable; // drag mesh or visual root here

        private Coroutine hideBarCoroutine;
        private Camera cam;

        private float targetHealth;

        private void Awake()
        {
            cam = Camera.main;
            currentHealth = maxHealth;
            targetHealth = maxHealth;

            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;

            healthUI.SetActive(false);
        }

        private void Update()
        {
            // Smooth health bar update
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealth, Time.deltaTime * smoothSpeed);

            float distance = Vector3.Distance(cam.transform.position, healthUI.transform.position);
            float scale = Mathf.Clamp(distance / 10f, 0.5f, 1.2f); // adjust 10f, 0.5f, and 1.2f to taste
            healthUI.transform.localScale = Vector3.one * scale;


        }

        public void TakeDamage(float amount, Vector3 hitPosition)
        {
            targetHealth = Mathf.Max(targetHealth - amount, 0);
            currentHealth = targetHealth;

            // Position bar offset from the tree based on hit direction
            Vector3 dir = (hitPosition - transform.position).normalized;
            Vector3 barPos = transform.position + dir * barForwardOffset + barOffsetDirection;
            healthUI.transform.position = barPos;

            // Rotate once to face the camera
            healthUI.transform.LookAt(cam.transform);
            healthUI.transform.Rotate(0, 180, 0); // flip for correct orientation

            healthUI.SetActive(true);

            // Restart hide timer
            if (hideBarCoroutine != null)
                StopCoroutine(hideBarCoroutine);
            hideBarCoroutine = StartCoroutine(HideBarAfterDelay());

            // If health is depleted
            if (targetHealth <= 0)
            {
                OnTreeDestroyed();
            }
        }

        private IEnumerator HideBarAfterDelay()
        {
            yield return new WaitForSeconds(barDisappearDelay);
            healthUI.SetActive(false);
        }

        private void OnTreeDestroyed()
        {
            // Disable visuals
            if (treeVisualToDisable != null)
                treeVisualToDisable.SetActive(false);

            // Optionally disable collider
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;

            // Spawn from above
            if (spawnOnDestroyPrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * spawnHeightOffset;
                Instantiate(spawnOnDestroyPrefab, spawnPos, Quaternion.identity);
            }

            // Hide health UI
            healthUI.SetActive(false);
        }
    }
}


