using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public AudioSource musicSource;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI comboText;
    public GameObject gameOverPanel;
    private CanvasGroup gameOverPanelCanvasGroup;
    public Image background;
    public Image bgFader;

    [Header("Beatmap")]
    public TextAsset beatmapFile; // json
    public GameObject hitCirclePrefab;
    public GameObject sliderPrefab;
    public GameObject hitFeedbackPrefab;
    public Transform spawnParent;
    public Transform hitTextSpawnParent;

    [Header("Timing")]
    public float approachDuration = 1.0f;

    private List<HitObjectData> hitObjects = new();
    public float startTime;
    private int score = 0;
    private float accuracy = 100f;
    private int hits = 0;
    private int perfectHits = 0;
    private int goodHits = 0;
    private int misses = 0;
    private int totalNotes = 0;
    private int currentCombo = 0;
    private int highestCombo = 0;

    private bool hitLock = false;

    private int displayedScore = 0;
    private float displayedAccuracy = 100f; // starts at full

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        if (!string.IsNullOrEmpty(BeatmapSession.selectedBeatmapPath))
        {
            LoadBeatmap(BeatmapSession.selectedBeatmapPath);
            gameOverPanelCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (gameOverPanelCanvasGroup != null)
            {
                gameOverPanelCanvasGroup.alpha = 0;
                gameOverPanel.SetActive(false);
            }
            bgFader.color = new Color(0f, 0f, 0f, 0f);
        }
        else
        {
            Debug.LogWarning("No beatmap path provided");
        }

        StartCoroutine(StartGame());
    }

    void Update()
    {
        // Animate score
        displayedScore = Mathf.Lerp(displayedScore, score, 5f * Time.deltaTime) > score - 1 ? score : Mathf.RoundToInt(Mathf.Lerp(displayedScore, score, 5f * Time.deltaTime));
        scoreText.text = $"Score: {displayedScore}";

        // Animate accuracy
        accuracy = hits > 0 ? ((perfectHits + (goodHits / 2)) / (float)hits) * 100f : 100f;
        displayedAccuracy = Mathf.Lerp(displayedAccuracy, accuracy, 5f * Time.deltaTime);
        accuracyText.text = $"Accuracy: {displayedAccuracy:F1}%";

        comboText.text = $"Combo: x{currentCombo}";

        // hit detection
        if ((Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(0)) && !hitLock)
        {
            TryHitCircle();
            hitLock = true;
        }

        if ((Input.GetKeyUp(KeyCode.X) || Input.GetKeyUp(KeyCode.Z) || Input.GetMouseButtonUp(0)) && hitLock)
        {
            hitLock = false;
        }

    }

    IEnumerator StartGame()
    {
        yield return FadeIn(bgFader, 0.9f, 1f);
        yield return new WaitForSeconds(1f); // short delay
        startTime = Time.time;
        musicSource.Play();
        float scoreScreenDelay = hitObjects[^1].duration + 3f;
        foreach (HitObjectData obj in hitObjects)
        {
            float delay = 0f;
            delay = obj.time - approachDuration - (Time.time - startTime);

            if (delay > 0) yield return new WaitForSeconds(delay);

            if (obj.type == "slider")
            {
                SpawnSlider(obj);
            }
            else
            {
                SpawnHitCircle(obj);
            }
        }

        yield return new WaitForSeconds(scoreScreenDelay); // small delay before summarys screen
        GameOver();
    }

    public IEnumerator FadeIn(Image image, float targetAlpha, float duration)
    {
        if (image == null) yield break;

        Color currentColor = image.color;
        float startAlpha = currentColor.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            image.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        // Ensure final alpha is exactly the target
        image.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }

    void TryHitCircle()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 clickPos = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        RaycastHit2D hit = Physics2D.Raycast(clickPos, Vector2.zero);
        if (hit.collider != null)
        {
            HitCircle circle = hit.collider.GetComponent<HitCircle>();
            circle?.OnClicked();
        }
    }

    public void LoadBeatmap(string path)
    {
        string json = File.ReadAllText(path);
        BeatmapData data = JsonUtility.FromJson<BeatmapData>(json);
        data.LoadAssets();
        approachDuration = data.approachTime;
        hitObjects = data.hitObjects;
        totalNotes = hitObjects.Count;
        background.sprite = data.backgroundSprite;
        musicSource.clip = data.previewAudio;
    }


    void SpawnHitCircle(HitObjectData obj)
    {
        Vector2 screenPos = new Vector2(obj.x * Screen.width, obj.y * Screen.height);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        GameObject go = Instantiate(hitCirclePrefab, worldPos, Quaternion.identity, spawnParent);
        SpriteRenderer sprite = go.GetComponent<SpriteRenderer>();

        Color originalColor = sprite.color;
        originalColor.a = 0f;
        sprite.color = originalColor;
        HitCircle hc = go.GetComponent<HitCircle>();
        hc.hitTime = obj.time;
        hc.approachDuration = approachDuration;
    }

    void SpawnSlider(HitObjectData obj)
    {
        GameObject go = Instantiate(sliderPrefab, Vector3.zero, Quaternion.identity, spawnParent);
        HitSlider slider = go.GetComponent<HitSlider>();

        // Convert normalized screen positions to world positions
        Vector2 startScreen = new Vector2(obj.x * Screen.width, obj.y * Screen.height);
        Vector2 endScreen = new Vector2(obj.endX * Screen.width, obj.endY * Screen.height);

        Vector3 worldStart = Camera.main.ScreenToWorldPoint(new Vector3(startScreen.x, startScreen.y, 10f));
        Vector3 worldEnd = Camera.main.ScreenToWorldPoint(new Vector3(endScreen.x, endScreen.y, 10f));

        // Assign required data
        slider.startPos = worldStart;
        slider.endPos = worldEnd;
        slider.hitTime = obj.time;
        slider.approachDuration = approachDuration;
        slider.duration = obj.duration;
    }


    public void RegisterHit(int isHit)
    {
        // isHit == 0: miss
        // isHit == 1: good
        // isHit == 2: perfect
        hits++;

        if (isHit == 2)
        {
            perfectHits++;
            currentCombo++;
            score += 300 * currentCombo;
        }
        else if (isHit == 1)
        {
            goodHits++;
            currentCombo++;
            score += 100 * currentCombo;
        }
        else if (isHit == 0) // redundant but its a good catch condition
        {
            misses++;
            if (currentCombo > highestCombo)
            {
                highestCombo = currentCombo;
            }
            currentCombo = 0;
        }
    }

    public void ShowHitFeedback(Vector3 worldPos, string message, Color color)
    {
        GameObject go = Instantiate(hitFeedbackPrefab, worldPos, Quaternion.identity, hitTextSpawnParent);
        go.GetComponent<HitFeedback>().SetText(message, color);
    }

    void GameOver()
    {
        gameOverPanel.SetActive(true);
        gameOverPanel.GetComponent<CompleteMenu>().Refresh(accuracy, score, highestCombo, perfectHits, goodHits, misses);
        StartCoroutine(FadeInGameOverPanel(0.5f));
    }

    public void LoadSceneSelect()
    {
        FadeController.TransitionToScene("SongSelect");
    }
    
    IEnumerator FadeInGameOverPanel(float duration)
    {
        float elapsed = 0f;
        gameOverPanelCanvasGroup.interactable = false;
        gameOverPanelCanvasGroup.blocksRaycasts = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            gameOverPanelCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        gameOverPanelCanvasGroup.alpha = 1f;
        gameOverPanelCanvasGroup.interactable = true;
        gameOverPanelCanvasGroup.blocksRaycasts = true;
    }
}
