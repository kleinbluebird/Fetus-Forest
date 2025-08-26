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

    public PlayerConstraintController constraintController;
    
	[Header("Track Point 消失动画触发")]
	[SerializeField] private bool enableTrackPointDisappearTrigger = false;
	[SerializeField] private int triggerFrame = 11; // 在第几帧触发
	[SerializeField] private TrackPoint targetTrackPoint; // 目标TrackPoint对象
	[SerializeField] private string intoEyeTriggerParam = "IntoEye"; // 消失动画的触发器参数名
	
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
        
        // Debug.Log($"[Animator] 已加载 {clips.Length} 段动画");
    }

    void Update()
    {
        if (!isPlaying || clips.Length == 0) return;

        float elapsed = Time.time - clipStartTime;
        float clipDuration = clips[currentClip].totalFrames / clips[currentClip].fps;
        
		// 计算当前帧数（只在第一个clip时计算）
		if (currentClip == 0 && enableTrackPointDisappearTrigger && targetTrackPoint != null)
		{
			int currentFrame = Mathf.FloorToInt(elapsed * clips[currentClip].fps);
			
			// 检查是否到达触发帧
			if (currentFrame == triggerFrame)
			{
				TriggerTrackPointDisappear();
			}
		}
		
        // Debug.Log($"[Animator] 播放中 Clip {currentClip + 1}/{clips.Length} | 已播放 {elapsed:F2}s / 总时长 {clipDuration:F2}s");

        if (elapsed >= clipDuration)
        {
            // Debug.Log($"[Animator] Clip {currentClip + 1} 播放结束");
            currentClip++;

            if (currentClip >= clips.Length)
            {
                // 播放完成
                // Debug.Log("[Animator] 所有动画播放完成");
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
        
        // Debug.Log($"[Animator] 加载 Clip {index + 1}: {clip.texture.name} | 行 {clip.rows} | 列 {clip.columns} | 总帧数 {clip.totalFrames} | FPS {clip.fps}");
        
        if (index == 0 && constraintController != null)
        {
            // Debug.Log("[Animator] 触发玩家约束逻辑");
            constraintController.StartConstraint();
        }
        
        // 当播放到 Element3 (第4个 clip, index=3) 时触发环境过渡
        if (index == 2)
        {
            var envManager = FindObjectOfType<EnvironmentTransitionManager>();
            if (envManager != null)
            {
                envManager.TriggerEnvironmentTransition();
            }
        }
    }

	private void TriggerTrackPointDisappear()
	{
		if (targetTrackPoint == null)
		{
			Debug.LogWarning("[SequentialSpriteAnimator] 目标TrackPoint未设置，无法触发消失动画");
			return;
		}
		
		Debug.Log($"[SequentialSpriteAnimator] 第{triggerFrame}帧触发TrackPoint消失动画");
		
		// 通过TrackPoint的公共方法触发消失动画
		targetTrackPoint.TriggerDisappearAnimation(intoEyeTriggerParam);
	}
	
	// 公共方法，用于外部手动触发
	public void TriggerTrackPointDisappearManual()
	{
		TriggerTrackPointDisappear();
	}
}