using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BeatmapBuilder : MonoBehaviour
{
    [Header("Audio Settings")]
    public string audioFileName = "phase_32ki";
    public AudioSource audioSource;

    [Header("Export Settings")]
    public string outputFileName = "phase_32ki.json";

    private bool isDragging = false;
    private Vector2 dragStart;
    private float dragStartTime;

    public string title;
    public string artist;
    public string diff;

    private BeatmapData beatmap;


    void Start()
    {
        // Load audio from Resources
        AudioClip clip = Resources.Load<AudioClip>(audioFileName);
        if (clip == null)
        {
            Debug.LogError($"Audio '{audioFileName}' not found in Resources.");
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();

        // Initialize beatmap data
        beatmap = new BeatmapData
        {
            approachTime = 1.0f,
            title = title,
            artist = artist,
            difficulty = diff,
            audio = audioFileName,
            background = audioFileName
        };
    }

    void Update()
    {
        if (!audioSource.isPlaying) return;

        // On left click down
        if (Input.GetMouseButtonDown(0) || Input.GetKeyUp(KeyCode.Z) || Input.GetKeyUp(KeyCode.X))
        {
            isDragging = true;
            dragStart = GetNormalizedMousePosition();
            dragStartTime = audioSource.time;
        }

        // On left click up
        if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Z) || Input.GetKeyUp(KeyCode.X)) && isDragging)
        {
            Vector2 dragEnd = GetNormalizedMousePosition();
            float endTime = audioSource.time;
            float distance = Vector2.Distance(dragStart, dragEnd);

            HitObjectData hit = new HitObjectData
            {
                x = dragStart.x,
                y = dragStart.y,
                time = dragStartTime,
                duration = 0f,
                endX = 0f,
                endY = 0f
            };

            if (distance > 0.05f)
            {
                // Slider
                hit.type = "slider";
                hit.endX = dragEnd.x;
                hit.endY = dragEnd.y;
                hit.duration = Mathf.Max(0.5f, endTime - dragStartTime); // Prevent super short sliders
                Debug.Log("Slider Placed");
            }
            else
            {
                // Circle
                hit.type = "circle";
                Debug.Log("Circle Placed");
            }

            beatmap.hitObjects.Add(hit);
            isDragging = false;
        }

        // Press S to save
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveBeatmap();
        }
    }

    Vector2 GetNormalizedMousePosition()
    {
        Vector2 mouse = Input.mousePosition;
        return new Vector2(mouse.x / Screen.width, mouse.y / Screen.height);
    }

    void SaveBeatmap()
    {
        string json = JsonUtility.ToJson(beatmap, true);
        string path = Path.Combine(Application.streamingAssetsPath, outputFileName);
        File.WriteAllText(path, json);
        Debug.Log($"✅ Beatmap saved to: {path}");
        Debug.Log($"🎯 Total objects: {beatmap.hitObjects.Count}");
    }
}
