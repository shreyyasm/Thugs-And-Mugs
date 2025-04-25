using UnityEngine;

// --- Enums ---
public enum MaterialType { Wood, Brick, Metal }

public class BuildPreview : MonoBehaviour
{
    public Material validMat;
    public Material invalidMat;
    private Renderer previewRenderer;
    public bool CanPlaceHere { get; private set; }

    void Start()
    {
        previewRenderer = GetComponent<Renderer>();
    }

    public void UpdatePreview(Vector3 targetPos, bool isValid)
    {
        transform.position = targetPos;
        previewRenderer.material = isValid ? validMat : invalidMat;
        CanPlaceHere = isValid;
    }
}