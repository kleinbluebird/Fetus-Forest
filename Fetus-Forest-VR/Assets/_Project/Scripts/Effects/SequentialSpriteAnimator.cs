using UnityEngine;
using System;

public class SequentialSpriteAnimator : MonoBehaviour
{
    [System.Serializable]
    public struct AnimationClipData
    {
        public Texture2D texture;
        public int rows;
        public int columns;
        public int totalFrames;
        public float fps;
    }

    public AnimationClipData[] clips;
    public Renderer targetRenderer;

    public event Action OnAllAnimationsFinished;

    private Material mat;
    private int currentClip = 0;
    private float clipStartTime = 0f;
    private bool isPlaying = false;

    void Start()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        mat = targetRenderer.material;
        targetRenderer.enabled = false; // 初始隐藏
        
        Debug.Log($"[Animator] 已加载 {clips.Length} 段动画");
    }

    void Update()
    {
        if (!isPlaying || clips.Length == 0) return;

        float elapsed = Time.time - clipStartTime;
        float clipDuration = clips[currentClip].totalFrames / clips[currentClip].fps;
        
        // Debug.Log($"[Animator] 播放中 Clip {currentClip + 1}/{clips.Length} | 已播放 {elapsed:F2}s / 总时长 {clipDuration:F2}s");

        if (elapsed >= clipDuration)
        {
            Debug.Log($"[Animator] Clip {currentClip + 1} 播放结束");
            currentClip++;

            if (currentClip >= clips.Length)
            {
                // 播放完成
                Debug.Log("[Animator] 所有动画播放完成");
                isPlaying = false;
                targetRenderer.enabled = false;
                OnAllAnimationsFinished?.Invoke();
                return;
            }

            LoadClip(currentClip);
        }
    }

    public void Play()
    {
        if (clips.Length == 0) return;
        currentClip = 0;
        Debug.Log("[Animator] 播放开始");
        LoadClip(0);
        targetRenderer.enabled = true;
        isPlaying = true;
    }

    void LoadClip(int index)
    {
        var clip = clips[index];
        mat.SetTexture("_MainTex", clip.texture);
        mat.SetFloat("_Rows", clip.rows);
        mat.SetFloat("_Columns", clip.columns);
        mat.SetFloat("_TotalFrames", clip.totalFrames);
        mat.SetFloat("_FPS", clip.fps);
        mat.SetFloat("_StartTime", Time.time); // 重置帧
        mat.SetFloat("_FrameIndex", 0); // 重置帧

        clipStartTime = Time.time;
        
        Debug.Log($"[Animator] 加载 Clip {index + 1}: {clip.texture.name} | 行 {clip.rows} | 列 {clip.columns} | 总帧数 {clip.totalFrames} | FPS {clip.fps}");
    }
}