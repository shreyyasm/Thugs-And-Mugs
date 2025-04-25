using UnityEngine;

public enum BuildMode
{
    Ground,
    Wall
}

public class BuildManager : MonoBehaviour
{
    public GameObject groundPrefab;
    public GameObject wallPrefab;
    public GameObject groundPreviewPrefab;
    public GameObject wallPreviewPrefab;
    public MaterialInventory inventory;

    public int groundCost = 5;
    public int wallCost = 10;
    public MaterialType groundMaterial = MaterialType.Wood;
    public MaterialType wallMaterial = MaterialType.Wood;

    public float gridSize = 1f;

    private GameObject currentPreview;
    private BuildPreview previewScript;
    private BuildMode currentMode = BuildMode.Ground;

    void Start()
    {
        SetPreview();
    }

    void Update()
    {
        // Switch build mode with mouse scroll
        if (Input.mouseScrollDelta.y != 0)
        {
            currentMode = (BuildMode)(((int)currentMode + 1) % System.Enum.GetValues(typeof(BuildMode)).Length);
            SetPreview();
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPosition = GetSnappedPosition(hit.point, gridSize);

            bool canPlace = false;
            Quaternion rotation = Quaternion.identity;

            if (currentMode == BuildMode.Ground)
            {
                canPlace = CheckPlacementValid(targetPosition, "Ground");
            }
            else if (currentMode == BuildMode.Wall)
            {
                (canPlace, rotation) = CheckWallPlacementValid(targetPosition);
            }

            previewScript?.UpdatePreview(targetPosition, canPlace);

            if (Input.GetMouseButtonDown(0) && previewScript != null && previewScript.CanPlaceHere)
            {
                if (currentMode == BuildMode.Ground && inventory.HasEnough(groundMaterial, groundCost))
                {
                    Instantiate(groundPrefab, targetPosition, Quaternion.identity);
                    inventory.Spend(groundMaterial, groundCost);
                }
                else if (currentMode == BuildMode.Wall && inventory.HasEnough(wallMaterial, wallCost))
                {
                    Instantiate(wallPrefab, targetPosition, rotation);
                    inventory.Spend(wallMaterial, wallCost);
                }
            }
        }
    }

    Vector3 GetSnappedPosition(Vector3 rawPosition, float gridSize)
    {
        return new Vector3(
            Mathf.Round(rawPosition.x / gridSize) * gridSize,
            Mathf.Round(rawPosition.y / gridSize) * gridSize,
            Mathf.Round(rawPosition.z / gridSize) * gridSize
        );
    }

    void SetPreview()
    {
        if (currentPreview != null)
            Destroy(currentPreview);

        GameObject prefabToUse = currentMode == BuildMode.Ground ? groundPreviewPrefab : wallPreviewPrefab;
        currentPreview = Instantiate(prefabToUse);
        previewScript = currentPreview.GetComponent<BuildPreview>();
    }

    bool CheckPlacementValid(Vector3 pos, string tag)
    {
        Collider[] hits = Physics.OverlapBox(pos, Vector3.one * (gridSize / 2f));
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Tile"))
                return false;
        }
        return true;
    }

    (bool, Quaternion) CheckWallPlacementValid(Vector3 pos)
    {
        Vector3[] directions = new Vector3[]
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };

        foreach (Vector3 dir in directions)
        {
            Vector3 checkPos = pos + dir * gridSize;
            Collider[] hits = Physics.OverlapBox(checkPos, Vector3.one * (gridSize / 2f));
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Ground"))
                {
                    Quaternion rotation = Quaternion.LookRotation(-dir); // Rotate wall to face ground tile
                    Collider[] selfHits = Physics.OverlapBox(pos, Vector3.one * (gridSize / 2f));
                    foreach (var selfHit in selfHits)
                    {
                        if (selfHit.CompareTag("Tile"))
                            return (false, rotation);
                    }
                    return (true, rotation);
                }
            }
        }

        return (false, Quaternion.identity);
    }
}
