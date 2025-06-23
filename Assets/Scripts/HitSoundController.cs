using UnityEngine;

public class HitSoundController : MonoBehaviour
{
    public static HitSoundController Instance;

    [Header("Set this to the filename (without extension) in Resources folder")]
    public string clickSoundFileName = "ClickSound"; // no .mp3/.wav

    private AudioSource audioSource;
    private AudioClip clickSound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Load AudioClip from Resources
        clickSound = Resources.Load<AudioClip>(clickSoundFileName);
        if (clickSound == null)
        {
            Debug.LogWarning("Click sound not found in Resources folder. File name: " + clickSoundFileName);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Z))
        {
            if (clickSound != null)
            {
                audioSource.volume = 1f; // yarg the fader be 
                audioSource.PlayOneShot(clickSound);
            }
        }
    }
}