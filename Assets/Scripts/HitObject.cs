using UnityEngine;

public class HitObject : MonoBehaviour
{
    public float hitTime; // time in seconds since music started when the note will spawn
    public float approachDuration = 1f;
    public float perfectWindow = 0.15f;
    public float goodWindow = 1.0f;

    public bool hasBeenHit = false;
    public GameObject approachCircle;
    public SpriteRenderer spriteRenderer;
    public float fade = 0f;
    public float fadeSpeed = 2f;
    public bool isFadingOut = false;

    public Vector3 originalScale;
    public Vector3 targetScale;
    public float scaleSpeed = 5f;
    public bool isScalingUp = false;

    public void Start()
    {
        Color c = spriteRenderer.color;
        c.a = 0f; // start transparent
        spriteRenderer.color = c;

        originalScale = transform.localScale;
    }

    // to be called in update in subclasses
    public void BasicFunctions()
    {
        float musicTime = Time.time - GameManager.Instance.startTime; // the time of the music related to the time it started
        float deltaTime = hitTime - musicTime; // the difference between what the current music time is vs the time the hitcircle is supposed to spawn

        // fade in during approach
        if (!isFadingOut && deltaTime > 0f)
        {
            FadeIn();
        }

        // Animate approach circle shrinking as time approaches hitTime
        if (approachCircle && deltaTime > 0f)
        {
            ApproachCircleUpdate(deltaTime);
        }

        // if miss by expiry
        if (!hasBeenHit && deltaTime < 0f)
        {
            Miss();
        }

        if (isScalingUp) ScaleUp();
        if (isFadingOut) FadeOut();
    }

    public void FadeIn()
    {
        /*
        float t = 1f - Mathf.Clamp01(deltaTime / approachDuration);
        fade = Mathf.Lerp(0f, 1f, t); */
        fade = Mathf.MoveTowards(fade, 1f, (fadeSpeed / 6) * Time.deltaTime);
        SetAlpha(fade);
    }
    
    public void ApproachCircleUpdate(float deltaTime)
    {
        float t = 1f - Mathf.Clamp01(deltaTime / approachDuration);
        float scale = Mathf.Lerp(3f, 1f, t);
        approachCircle.transform.localScale = Vector3.one * scale;
    }

    public void Miss()
    {
        hasBeenHit = true;
        GetComponent<Collider2D>().enabled = false;
        GameManager.Instance.RegisterHit(0);
        GameManager.Instance.ShowHitFeedback(transform.position, "Miss!", Color.red);
        isFadingOut = true;
    }

    // Handle scale up
    public void ScaleUp()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
    }

    // fade out if hit or missed
    public void FadeOut()
    {
        fade = Mathf.MoveTowards(fade, 0f, fadeSpeed * Time.deltaTime);
        SetAlpha(fade);
        if (fade <= 0f) Destroy(gameObject);
    }

    public virtual void OnClicked()
    {
        if (hasBeenHit) return;

        hasBeenHit = true;
        GetComponent<Collider2D>().enabled = false;

        float musicTime = Time.time - GameManager.Instance.startTime;
        float delta = Mathf.Abs(hitTime - musicTime);

        if (delta <= perfectWindow)
        {
            GameManager.Instance.RegisterHit(2);
            GameManager.Instance.ShowHitFeedback(transform.position, "Perfect!", Color.yellow);
            isScalingUp = true;
            targetScale = originalScale * 1.3f;
        }
        else if (delta <= goodWindow)
        {
            GameManager.Instance.RegisterHit(1);
            GameManager.Instance.ShowHitFeedback(transform.position, "Good!", Color.green);
            StartScaleUp();
        }
        else
        {
            GameManager.Instance.RegisterHit(0);
            GameManager.Instance.ShowHitFeedback(transform.position, "Miss!", Color.red);
        }

        isFadingOut = true;
    }

    public void StartScaleUp()
    {
        isScalingUp = true;
        targetScale = originalScale * 1.3f; // pop up slightly
    }

    public void SetAlpha(float a)
    {
        if (spriteRenderer)
        {
            Color c = spriteRenderer.color;
            c.a = a;
            spriteRenderer.color = c;
        }
    }
}