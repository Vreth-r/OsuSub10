using UnityEngine;
public class HitSlider : HitObject
{
    // Slider data
    public Vector3 startPos;
    public Vector3 endPos;
    public float duration;

    public GameObject followCircle;
    public LineRenderer lineRenderer;

    public float threshold = 5f;

    // Runtime
    private float travelProgress = 0f;
    private bool isComplete = false;
    private float sliderStartTime;

    private Vector3 targetFCScale;
    private bool isScalingFollowCircle = false;
    private float scaleFCSpeed = 100f;

    public void Start()
    {
        base.Start();
        transform.position = startPos;
        followCircle.transform.position = startPos;

        if (lineRenderer)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, startPos);
            SetLineRendererAlpha(0f);
        }

        SetAlpha(0f);

        followCircle.transform.localScale = Vector3.one;
        sliderStartTime = hitTime + 0.01f;
    }

    public void Update()
    {
        float musicTime = Time.time - GameManager.Instance.startTime;
        float delta = hitTime - musicTime;

        // Fade in visuals
        if (!isFadingOut && delta > 0f)
        {
            FadeIn();
            AnimateLineRendererDuringApproach(musicTime);
        }

        if (approachCircle && delta > 0f)
        {
            ApproachCircleUpdate(delta);
        }

        // Start slider logic
        if (!isComplete && musicTime >= sliderStartTime)
        {
            AnimateFollowCircle(musicTime);
            travelProgress = Mathf.Clamp01((musicTime - sliderStartTime) / duration);
            followCircle.transform.position = Vector3.Lerp(startPos, endPos, travelProgress);

            // End of slider — evaluate result
            if (travelProgress >= 1f)
            {
                EvaluateHit();
                isComplete = true;
                isFadingOut = true;
                StartScaleUp();
            }
        }

        if (isFadingOut)
        {
            FadeOut();
        }
    }

    public new void FadeIn()
    {
        base.FadeIn();
        SetLineRendererAlpha(fade);
    }

    public new void FadeOut()
    {
        fade = Mathf.MoveTowards(fade, 0f, fadeSpeed * Time.deltaTime);
        SetAlpha(fade);
        SetLineRendererAlpha(fade);

        if (followCircle)
        {
            SpriteRenderer sr = followCircle.GetComponent<SpriteRenderer>();
            Color c = sr.color;
            c.a = fade;
            sr.color = c;
        }

        if (fade <= 0f && isComplete)
        {
            Destroy(gameObject);
        }
    }

    private void AnimateLineRendererDuringApproach(float musicTime)
    {
        if (!lineRenderer) return;

        float approachStart = hitTime - approachDuration;
        float t = Mathf.InverseLerp(approachStart, hitTime, musicTime);
        t = Mathf.Clamp01(t);

        Vector3 currentEnd = Vector3.Lerp(startPos, endPos, t * 4f);
        lineRenderer.SetPosition(1, currentEnd);
    }

    private void AnimateFollowCircle(float musicTime)
    {
        if (!isScalingFollowCircle)
        {
            SpriteRenderer sr = followCircle.GetComponent<SpriteRenderer>();
            float spriteWorldRadius = sr.bounds.extents.x;
            float currentScale = followCircle.transform.localScale.x;
            float baseRadius = spriteWorldRadius / currentScale;

            float desiredScale = threshold / baseRadius;
            targetFCScale = new Vector3(desiredScale, desiredScale, 1f);

            isScalingFollowCircle = true;
        }

        if (followCircle.transform.localScale != targetFCScale)
        {
            followCircle.transform.localScale = Vector3.MoveTowards(
                followCircle.transform.localScale,
                targetFCScale,
                scaleFCSpeed * Time.deltaTime
            );
        }
    }

    private void EvaluateHit()
    {
        bool stillHolding = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X);

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        float distance = Vector3.Distance(mouseWorld, followCircle.transform.position);

        bool insideThreshold = distance <= threshold;

        if (stillHolding && insideThreshold)
        {
            GameManager.Instance.RegisterHit(2); // Perfect
            GameManager.Instance.ShowHitFeedback(endPos, "Break!", Color.cyan);
        }
        else
        {
            Miss();
        }
    }

    private void SetLineRendererAlpha(float a)
    {
        if (!lineRenderer) return;

        Color start = lineRenderer.startColor;
        Color end = lineRenderer.endColor;

        start.a = a;
        end.a = a;

        lineRenderer.startColor = start;
        lineRenderer.endColor = end;
    }
}
