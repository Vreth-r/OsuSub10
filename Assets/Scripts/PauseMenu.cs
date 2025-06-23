using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("UI")]
    public GameObject pauseMenuUI;
    public Button resumeButton;
    public Button retryButton;
    public Button quitButton;

    private float pausedTime;
    private float totalPausedDuration;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        totalPausedDuration = 0f;

        resumeButton.onClick.AddListener(Resume);
        retryButton.onClick.AddListener(Restart);
        quitButton.onClick.AddListener(QuitToMenu);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        if (IsPaused) return;

        IsPaused = true;
        pausedTime = Time.time;

        Time.timeScale = 0f;
        GameManager.Instance.musicSource.Pause();
        pauseMenuUI.SetActive(true);
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;

        // Sync startTime with pause
        float pauseDuration = Time.time - pausedTime;
        totalPausedDuration += pauseDuration;
        GameManager.Instance.startTime += pauseDuration;

        GameManager.Instance.musicSource.UnPause();
        pauseMenuUI.SetActive(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance.LoadSceneSelect();
    }
}
