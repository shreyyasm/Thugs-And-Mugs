using UnityEngine;
using TMPro;
using System.Collections;

public class TaskManager : MonoBehaviour
{
    public static TaskManager instance;

    [Header("References")]
    public GameObject taskPrefab;             // The prefab with Text and TimerText
    public Transform sideTaskParent;          // Assign the "SideTask" UI container in the Inspector

    [Header("Timing")]
    public float realSecondsPerGameMinute = 12f;  // 1 in-game minute = 12 real seconds (so 5 in-game = 60 sec)

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void AssignTask(string taskText, int timer)
    {
        GameObject taskInstance = Instantiate(taskPrefab, sideTaskParent);

        // Find the timer label and start the timer
        TextMeshProUGUI OrderText = taskInstance.GetComponent<TextMeshProUGUI>();
        StartCoroutine(RunTaskTimer(taskText, taskInstance, OrderText, timer)); // 5 in-game minutes
    }

    IEnumerator RunTaskTimer(string Ordertext, GameObject taskGO, TextMeshProUGUI timerText, int inGameMinutes)
    {
        float totalRealTime = inGameMinutes * realSecondsPerGameMinute;
        float elapsed = 0f;
        int lastDisplayedMinutes = -1;

        string colorHexGreen = "#00FF00";
        string colorHexYellow = "#FFA500"; // orange
        string colorHexRed = "#FF0000";

        while (elapsed < totalRealTime)
        {
            float remaining = totalRealTime - elapsed;
            int displayMinutes = Mathf.CeilToInt(remaining / realSecondsPerGameMinute);

            if (displayMinutes != lastDisplayedMinutes)
            {
                string colorTag;

                float ratio = elapsed / totalRealTime;
                if (ratio < 0.33f)
                    colorTag = colorHexGreen;
                else if (ratio < 0.66f)
                    colorTag = colorHexYellow;
                else
                    colorTag = colorHexRed;

                timerText.text = $"<color=white>{Ordertext} </color><color={colorTag}>({displayMinutes} min)</color>";
                lastDisplayedMinutes = displayMinutes;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(taskGO);
    }



}
