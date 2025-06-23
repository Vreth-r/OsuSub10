using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BeatmapData
{
    public float approachTime = 1.0f;
    public List<HitObjectData> hitObjects = new();

    // metadata
    public string title;
    public string artist;
    public string audio;
    public string background;
    [System.NonSerialized] public AudioClip previewAudio;
    [System.NonSerialized] public Sprite backgroundSprite;
    public string difficulty; 

    public void LoadAssets()
    {
        if (!string.IsNullOrEmpty(audio))
            previewAudio = Resources.Load<AudioClip>(audio);

        if (!string.IsNullOrEmpty(background))
        {
            backgroundSprite = Resources.Load<Sprite>(background);
        }
    }
}

[System.Serializable]
public class HitObjectData
{
    public float x;
    public float y;
    public float time;
    public string type;
    public float duration;
    public float endX;
    public float endY;
}


