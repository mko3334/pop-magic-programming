using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    SpriteRenderer sr;
    List<Sprite> frames = new List<Sprite>();
    float fps = 8f;
    bool loop = true;
    bool playing;
    float timer;
    int frameIndex;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    public void Play(List<Sprite> newFrames, float newFps, bool newLoop = true)
    {
        if (newFrames == null || newFrames.Count == 0) return;
        frames = new List<Sprite>(newFrames);
        fps = newFps > 0 ? newFps : 8f;
        loop = newLoop;
        frameIndex = 0;
        timer = 0f;
        playing = true;
        sr.sprite = frames[0];
    }

    public void SetStatic(Sprite sprite)
    {
        playing = false;
        frames.Clear();
        if (sprite != null) sr.sprite = sprite;
    }

    void Update()
    {
        if (!playing || frames.Count <= 1) return;
        timer += Time.deltaTime;
        if (timer < 1f / fps) return;
        timer = 0f;
        frameIndex = (frameIndex + 1) % frames.Count;
        if (frameIndex == 0 && !loop) { frameIndex = frames.Count - 1; playing = false; return; }
        sr.sprite = frames[frameIndex];
    }
}
