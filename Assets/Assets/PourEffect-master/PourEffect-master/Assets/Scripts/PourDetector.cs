using Shreyas;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PourDetector : MonoBehaviour
{
    public int pourThreshold = 45;
    public Transform origin;
    public GameObject streamPrefab;

    private bool isPouring = false;
    private Stream currentStream;
    GameObject StreamPrefabSpawn;

    public float pourRate = 0.2f; // Amount of fill added per second
    public float maxRayDistance = 1.5f;
    public LayerMask mugLayer; // Assign a layer for mugs only

    private LineRenderer line;
    private LiquidShaderController currentMug;
    private bool isHittingMug;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }
    public Renderer liquidRenderer;      // Assign the MeshRenderer or SkinnedMeshRenderer
    public Color liquidColor = Color.cyan; // Desired color set from Inspector
    void Start()
    {
        if (liquidRenderer != null)
        {
            // Set the _Color property on the material
            liquidRenderer.material.SetColor("_Color", liquidColor);
        }
    }

    private void Update()
    {
        Pouring();
        
        if(StreamPrefabSpawn != null && CaculatePourAngle() <= 1)
        {
            LineRenderer line = StreamPrefabSpawn.GetComponent<LineRenderer>();

            if (line == null || line.positionCount < 2)
                return;


            // Get stream end position from LineRenderer
            Vector3 start = line.GetPosition(0);
            Vector3 end = line.GetPosition(1);
            Vector3 direction = (end - start).normalized;

            // Raycast to detect a mug
            Ray ray = new Ray(start, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxRayDistance, mugLayer))
            {
                LiquidShaderController mug = hit.collider.GetComponent<LiquidShaderController>();
                mug.baseColor = liquidColor;
                mug.emissionColor = liquidColor;
                if (mug != null)
                {
                    currentMug = mug;
                    isHittingMug = true;

                    mug.fillAmount = Mathf.Clamp01(mug.fillAmount + pourRate * Time.deltaTime);
                }
                if(mug.fillAmount == 1)
                {
                    mug.GetComponentInParent<Mug>().isFilled = true;
                    BrewManager.Instance.DrinkComplete();
                    InventoryManager.instance.PlayerModelVisual.SetActive(true);
                    Destroy(gameObject);
                   
                }
            }
            else
            {
                isHittingMug = false;
                currentMug = null;
            }
        }
        
       
    }

    public void Pouring()
    {
        bool pourCheck = CaculatePourAngle() < pourThreshold;
        if (isPouring != pourCheck)
        {
            isPouring = pourCheck;

            if (isPouring)
            {
                StartPour();
            }
            else
            {
                EndPour();
            }
        }
        //Debug.Log(CaculatePourAngle());
        if (StreamPrefabSpawn != null && CaculatePourAngle() > 0)
        {
            ///StreamPrefabSpawn.GetComponent<LineRenderer>().startWidth = 0;
            //StreamPrefabSpawn.GetComponent<LineRenderer>().endWidth = 0.025f;
            LineRenderer line = StreamPrefabSpawn.GetComponent<LineRenderer>();
            StartCoroutine(SmoothLineWidth(line, 0f, 0.025f, 1f)); // 1 second smooth transition
            StreamPrefabSpawn.GetComponent<Stream>().Particle.transform.localScale = new Vector3(2f, 2f, 2f);


        }
        if (StreamPrefabSpawn != null && CaculatePourAngle() < -15 && CaculatePourAngle() > -25)
        {
            //StreamPrefabSpawn.GetComponent<LineRenderer>().startWidth = 0.005f;
            //StreamPrefabSpawn.GetComponent<LineRenderer>().endWidth = 0.05f;
            LineRenderer line = StreamPrefabSpawn.GetComponent<LineRenderer>();
            StartCoroutine(SmoothLineWidth(line, 0.02f, 0.04f, 1f)); // 1 second smooth transition
            StreamPrefabSpawn.GetComponent<Stream>().Particle.transform.localScale = new Vector3(3f, 3f, 3f);


        }
        if (StreamPrefabSpawn != null && CaculatePourAngle() < -40)
        {
            //StreamPrefabSpawn.GetComponent<LineRenderer>().startWidth = 0.025f;
            //StreamPrefabSpawn.GetComponent<LineRenderer>().endWidth = 0.06f;
            LineRenderer line = StreamPrefabSpawn.GetComponent<LineRenderer>();
            StartCoroutine(SmoothLineWidth(line, 0.03f, 0.06f, 1f)); // 1 second smooth transition
            StreamPrefabSpawn.GetComponent<Stream>().Particle.transform.localScale = new Vector3(4f, 4f, 4f);

        }
    }
    IEnumerator SmoothLineWidth(LineRenderer line, float targetStartWidth, float targetEndWidth, float duration)
    {
        if (line == null) yield break;

        float elapsed = 0f;
        float initialStart = line.startWidth;
        float initialEnd = line.endWidth;

        while (elapsed < duration)
        {
            // Exit if the line object has been destroyed
            if (line == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            line.startWidth = Mathf.Lerp(initialStart, targetStartWidth, t);
            line.endWidth = Mathf.Lerp(initialEnd, targetEndWidth, t);

            yield return null;
        }

        if (line != null)
        {
            line.startWidth = targetStartWidth;
            line.endWidth = targetEndWidth;
        }
    }

    public void StartPour()
    {
        Debug.Log("Start");
        currentStream = CreateStream();
        currentStream.Begin();
    }

    public void EndPour()
    {
        Debug.Log("End");
        currentStream.End();
        StreamPrefabSpawn = null;
        currentStream = null;
        liquidRenderer = null;
    }

    private float CaculatePourAngle()
    {
        return transform.forward.y * Mathf.Rad2Deg;
    }
    public Stream CreateStream()
    {
        GameObject streamObject = Instantiate(streamPrefab, origin.position,Quaternion.identity,transform);
        StreamPrefabSpawn = streamObject;
        liquidRenderer = streamObject.GetComponent<LineRenderer>();
        liquidRenderer.material.SetColor("_Color", liquidColor);
        return streamObject.GetComponent<Stream>();
    }
}