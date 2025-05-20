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

    public void Update()
    {
        bool pourCheck = CaculatePourAngle() < pourThreshold;

        if(isPouring != pourCheck)
        {
            isPouring = pourCheck;

            if(isPouring)
            {
                StartPour();
            }
            else
            {
                EndPour();  
            }    
        }
        Debug.Log(CaculatePourAngle());
        if(StreamPrefabSpawn != null  && CaculatePourAngle() > 0)
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
        float elapsed = 0f;
        float initialStart = line.startWidth;
        float initialEnd = line.endWidth;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            line.startWidth = Mathf.Lerp(initialStart, targetStartWidth, t);
            line.endWidth = Mathf.Lerp(initialEnd, targetEndWidth, t);

            yield return null;
        }

        line.startWidth = targetStartWidth;
        line.endWidth = targetEndWidth;
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
    }

    private float CaculatePourAngle()
    {
        return transform.forward.y * Mathf.Rad2Deg;
    }
    public Stream CreateStream()
    {
        GameObject streamObject = Instantiate(streamPrefab, origin.position,Quaternion.identity,transform);
        StreamPrefabSpawn = streamObject;
        return streamObject.GetComponent<Stream>();
    }
}