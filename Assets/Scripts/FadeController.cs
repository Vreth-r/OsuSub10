using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Collections.Generic;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;

    private static FadeController instance;

    private List<AudioSource> sceneAudioSources = new List<AudioSource>();
    private Dictionary<AudioSource, float> originalVolumes = new Dictionary<AudioSource, float>();

    [SerializeField] private bool fadeAudio = true;

    private void Awake()
    {
        // Ensure only one instance exists (singleton pattern)
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeOut());
    }

    public static void TransitionToScene(string sceneName)
    {
        if (instance != null)
            instance.StartCoroutine(instance.FadeAndSwitchScene(sceneName));
    }

    private IEnumerator FadeAndSwitchScene(string sceneName)
    {
        if (fadeAudio)
            StartCoroutine(FadeAudio(true, fadeDuration));

        yield return StartCoroutine(FadeIn());

        SceneManager.LoadScene(sceneName);
        // Audio will fade back in in OnSceneLoaded
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(1 - (t / fadeDuration));
            fadeImage.color = c;
            yield return null;
        }
    }

    private IEnumerator FadeAudio(bool fadeOut, float duration)
    {
        // Cache all active AudioSources in the scene
        sceneAudioSources.Clear();
        originalVolumes.Clear();


        // HORENDOUS but im running out of time 
        foreach (var source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (source.enabled && source.gameObject.activeInHierarchy && source.volume > 0f)
            {
                sceneAudioSources.Add(source);
                originalVolumes[source] = source.volume;
            }
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float volumeFactor = fadeOut ? (1f - progress) : progress;

            foreach (var source in sceneAudioSources)
            {
                if (source != null)
                    source.volume = originalVolumes[source] * volumeFactor;
            }

            yield return null;
        }

        // Final cleanup to set exact volumes
        foreach (var source in sceneAudioSources)
        {
            if (source != null)
                source.volume = fadeOut ? 0f : originalVolumes[source];
        }
    }
}
