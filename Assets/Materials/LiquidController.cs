using UnityEngine;

public class LiquidController : MonoBehaviour
{
    public Material liquidMat;
    public Rigidbody rb;
    public float wobbleSpeed = 3f;
    public float wobbleDecay = 2f;
    [Range(0, 1)]
    public float fillAmount = 0.5f;
    public Color liquidColor = Color.cyan;

    private float wobbleX = 0f;
    private float wobbleY = 0f;
    private Vector3 lastPos;

    void Start()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        lastPos = transform.position;
    }

    void Update()
    {
        Vector3 velocity = (transform.position - lastPos) / Time.deltaTime;

        // Smoothly decay wobble over time
        wobbleX = Mathf.Lerp(wobbleX, velocity.x, Time.deltaTime * wobbleDecay);
        wobbleY = Mathf.Lerp(wobbleY, velocity.z, Time.deltaTime * wobbleDecay);

        // Pass sine wave modulated wobble to shader
        liquidMat.SetFloat("_WobbleX", Mathf.Sin(Time.time * wobbleSpeed) * wobbleX);
        liquidMat.SetFloat("_WobbleY", Mathf.Sin(Time.time * wobbleSpeed) * wobbleY);

        // Update fill amount and color
        liquidMat.SetFloat("_FillAmount", fillAmount);
        liquidMat.SetColor("_Color", liquidColor);

        lastPos = transform.position;
    }

    public void SetFill(float value)
    {
        fillAmount = Mathf.Clamp01(value);
    }

    public void SetColor(Color color)
    {
        liquidColor = color;
    }
}
