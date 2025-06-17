using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LiquidShaderController : MonoBehaviour
{
    private Material material;

    [Header("Fill Settings")]
    [Range(0, 1)] public float fillAmount = 0.5f;
    public float fillMin = -0.5f;
    public float fillMax = 0.5f;

    [Header("Wobble Settings")]
    public float wobbleX = 0f;
    public float wobbleY = 0f;

    [Header("Color Settings")]
    public Color baseColor = new Color(0, 0.5f, 1f, 1f);
    public Color emissionColor = new Color(0, 1f, 1f, 1f);

    private void Awake()
    {
        // Clone the material to make it unique per object
        Renderer renderer = GetComponent<Renderer>();
        material = renderer.material;
    }

    private void Update()
    {
        if (material == null) return;

        material.SetFloat("_Fill", fillAmount);
        material.SetFloat("_FillMin", fillMin);
        material.SetFloat("_FillMax", fillMax);

        material.SetFloat("_WobbleX", wobbleX);
        material.SetFloat("_WobbleY", wobbleY);

        material.SetColor("_Color", baseColor);
        material.SetColor("_EmissionColor", emissionColor);
    }

    // Optional method to programmatically change fill (e.g., from pouring logic)
    public void SetFill(float value)
    {
        fillAmount = Mathf.Clamp01(value);
    }
}
