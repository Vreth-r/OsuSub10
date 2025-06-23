using UnityEngine;
using UnityEngine.UI;

public class CircleAudioVisualizer : MonoBehaviour
{
    public Image circleImage; // assign in inspector
    public float sensitivity = 100f;
    public float smoothSpeed = 5f;

    private float[] samples = new float[512];
    private Vector3 originalScale;
    private Vector3 maxScale = new Vector3(10f, 10f, 0);

    void Start()
    {
        originalScale = circleImage.rectTransform.localScale;
    }

    void Update()
    {
        AudioListener.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
        
        // Calculate average loudness (focus on bass if desired)
        float loudness = 0f;
        for (int i = 0; i < 64; i++) // first 64 bins for low frequencies
        {
            loudness += samples[i];
        }

        float scaleFactor = 1f + (loudness * sensitivity);
        Vector3 targetScale = originalScale * scaleFactor;

        if (targetScale.x >= maxScale.x && targetScale.y >= maxScale.y)
        {
            targetScale = maxScale;
        }

        // Smooth pulsing animation
            circleImage.rectTransform.localScale = Vector3.Lerp(
            circleImage.rectTransform.localScale, 
            targetScale, 
            Time.deltaTime * smoothSpeed
        );
    }
}

