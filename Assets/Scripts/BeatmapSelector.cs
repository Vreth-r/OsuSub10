using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class BeatmapSelector : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI artistText;
    public TextMeshProUGUI difficultyText;
    public Image backgroundImage;
    public Button playButton;

    private List<BeatmapData> beatmaps = new();
    private int currentIndex = 0;

    void Start()
    {
        LoadAllBeatmaps();
        UpdateUI();
    }

    void LoadAllBeatmaps()
    {
        string folderPath = Application.streamingAssetsPath;

        // Get all JSON files
        string[] files = Directory.GetFiles(folderPath, "*.json");

        foreach (string filePath in files)
        {
            string jsonText = File.ReadAllText(filePath);
            BeatmapData data = JsonUtility.FromJson<BeatmapData>(jsonText);
            data.LoadAssets();
            beatmaps.Add(data);

            Debug.Log($"✅ Loaded beatmap: {data.title} with {data.hitObjects.Count} objects");
        }
    }

    public void NextBeatmap()
    {
        currentIndex = (currentIndex + 1) % beatmaps.Count;
        UpdateUI();
    }

    public void PreviousBeatmap()
    {
        currentIndex = (currentIndex - 1 + beatmaps.Count) % beatmaps.Count;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (beatmaps.Count == 0) return;

        BeatmapData current = beatmaps[currentIndex];

        titleText.text = current.title;
        artistText.text = current.artist;
        difficultyText.text = $"Difficulty: {current.difficulty}";

        backgroundImage.sprite = current.backgroundSprite;

        playButton.interactable = true;
    }

    public void PlaySelectedBeatmap()
    {
        string path = $"{beatmaps[currentIndex].title}";
        BeatmapSession.selectedBeatmapPath = Path.Combine(Application.streamingAssetsPath, path + ".json");
        FadeController.TransitionToScene("Main");
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}