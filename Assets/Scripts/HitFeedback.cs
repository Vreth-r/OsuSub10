using UnityEngine;
using TMPro;

public class HitFeedback : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float fadeDuration = 0.5f;

    private TextMeshPro text;
    private float lifetime = 1f;
    private float timer;

    void Start()
    {
        text = GetComponent<TextMeshPro>();
        timer = 0f;
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
        else if (timer >= lifetime - fadeDuration)
        {
            float alpha = 1f - ((timer - (lifetime - fadeDuration)) / fadeDuration);
            text.alpha = alpha;
        }
    }

    public void SetText(string content, Color color)
    {
        if (!text) text = GetComponent<TextMeshPro>();
        text.text = content;
        text.color = color;
    }
}
