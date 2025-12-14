using UnityEngine;

namespace FetusForest.Effects
{
    /// <summary>
    /// 控制3D Title模型的渐隐动效
    /// 游戏开始时，标题从白色逐渐变暗到目标灰度值，然后保持
    /// </summary>
    public class TitleFadeController : MonoBehaviour
    {
        [Header("动效设置")]
        [SerializeField] private float fadeSpeed = 0.5f;            // 渐隐速度（每帧变化量）
        [SerializeField] private float startGrayValue = 1.0f;       // 起始灰度值（白色）
        [SerializeField] private float targetGrayValue = 0.5471f;   // 目标灰度值（深灰）
        [SerializeField] private bool autoStart = true;             // 是否自动开始动效
        [SerializeField] private bool enableDebug = true;           // 是否启用调试信息
        
        [Header("高级设置")]
        [SerializeField] private float speedMultiplier = 0.001f;    // 速度倍数（用于微调）
        [SerializeField] private bool useSmoothTransition = true;   // 是否使用平滑过渡
        
        [Header("组件引用")]
        [SerializeField] private Renderer titleRenderer;            // Title模型的Renderer组件
        
        private Material titleMaterial;                             // Title材质
        private float currentGrayValue;                             // 当前灰度值
        private bool isActive = false;                              // 动效是否激活
        private bool isCompleted = false;                           // 动效是否完成
        private float lastUpdateTime;                               // 上次更新时间
        private int frameCount;                                     // 帧计数器
        
        // 材质属性名称
        private static readonly string BaseColorProperty = "_BaseColor";
        private static readonly string ColorProperty = "_Color";
        
        private void Start()
        {
            InitializeTitleMaterial();
            
            if (autoStart)
            {
                StartFadeEffect();
            }
        }
        
        private void Update()
        {
            if (isActive && !isCompleted)
            {
                UpdateFadeEffect();
                frameCount++;
            }
        }
        
        /// <summary>
        /// 初始化Title材质
        /// </summary>
        private void InitializeTitleMaterial()
        {
            // 如果没有手动指定Renderer，尝试自动获取
            if (titleRenderer == null)
            {
                titleRenderer = GetComponent<Renderer>();
            }
            
            if (titleRenderer == null)
            {
                Debug.LogError("TitleFadeController: 未找到Renderer组件！请确保此脚本挂载在包含Renderer组件的GameObject上。");
                return;
            }
            
            // 获取材质
            titleMaterial = titleRenderer.material;
            
            if (titleMaterial == null)
            {
                Debug.LogError("TitleFadeController: 材质未初始化，无法开始动效。");
                return;
            }
            
            // 检查材质是否包含必要的颜色属性
            bool hasBaseColor = titleMaterial.HasProperty(BaseColorProperty);
            bool hasColor = titleMaterial.HasProperty(ColorProperty);
            
            if (!hasBaseColor && !hasColor)
            {
                Debug.LogWarning("TitleFadeController: 材质不包含_BaseColor或_Color属性。请确保使用正确的材质。");
            }
            
            // 设置初始灰度值为白色
            currentGrayValue = startGrayValue;
            UpdateMaterialColor();
            
            if (enableDebug)
            {
                Debug.Log($"TitleFadeController: 初始化完成 - 起始灰度值: {startGrayValue:F4}");
                Debug.Log($"TitleFadeController: 目标灰度值: {targetGrayValue:F4}");
                Debug.Log($"TitleFadeController: 动效速度: {fadeSpeed:F4}");
            }
        }
        
        /// <summary>
        /// 开始渐隐动效
        /// </summary>
        public void StartFadeEffect()
        {
            if (titleMaterial == null)
            {
                Debug.LogWarning("TitleFadeController: 材质未初始化，无法开始动效。");
                return;
            }
            
            isActive = true;
            isCompleted = false;
            currentGrayValue = startGrayValue;
            lastUpdateTime = Time.time;
            frameCount = 0;
            UpdateMaterialColor();
            
            // Debug.Log("TitleFadeController: 渐隐动效已开始 - 从白色渐变到目标值");
        }
        
        /// <summary>
        /// 停止渐隐动效
        /// </summary>
        public void StopFadeEffect()
        {
            isActive = false;
            Debug.Log("TitleFadeController: 渐隐动效已停止");
        }
        
        /// <summary>
        /// 更新渐隐动效
        /// </summary>
        private void UpdateFadeEffect()
        {
            if (titleMaterial == null) return;
            
            // 计算实际的速度变化量
            float actualSpeed = fadeSpeed * speedMultiplier;
            
            // 从起始值向目标值渐变
            if (useSmoothTransition)
            {
                currentGrayValue = Mathf.MoveTowards(currentGrayValue, targetGrayValue, actualSpeed);
            }
            else
            {
                if (currentGrayValue > targetGrayValue)
                {
                    currentGrayValue -= actualSpeed;
                    if (currentGrayValue < targetGrayValue)
                    {
                        currentGrayValue = targetGrayValue;
                    }
                }
            }
            
            // 检查是否到达目标值
            if (Mathf.Approximately(currentGrayValue, targetGrayValue))
            {
                currentGrayValue = targetGrayValue;
                isCompleted = true;
                
                if (enableDebug)
                {
                    Debug.Log($"TitleFadeController: 渐隐动效完成 - 当前灰度值: {currentGrayValue:F4}");
                }
            }
            
            // 确保灰度值在有效范围内
            currentGrayValue = Mathf.Clamp(currentGrayValue, targetGrayValue, startGrayValue);
            
            // 更新材质颜色
            UpdateMaterialColor();
            
            // 定期输出调试信息
            if (enableDebug && frameCount % 60 == 0 && !isCompleted) // 每60帧输出一次
            {
                float timeSinceStart = Time.time - lastUpdateTime;
                float progress = (startGrayValue - currentGrayValue) / (startGrayValue - targetGrayValue) * 100f;
                Debug.Log($"TitleFadeController: 渐隐中 - 灰度值: {currentGrayValue:F4}, 进度: {progress:F1}%, 运行时间: {timeSinceStart:F2}s");
            }
        }
        
        /// <summary>
        /// 更新材质颜色
        /// </summary>
        private void UpdateMaterialColor()
        {
            if (titleMaterial == null) return;
            
            // 创建新的颜色（灰度值）
            Color newColor = new Color(currentGrayValue, currentGrayValue, currentGrayValue, 1.0f);
            
            // 更新材质属性
            if (titleMaterial.HasProperty(BaseColorProperty))
            {
                titleMaterial.SetColor(BaseColorProperty, newColor);
            }
            else if (titleMaterial.HasProperty(ColorProperty))
            {
                titleMaterial.SetColor(ColorProperty, newColor);
            }
        }
        
        /// <summary>
        /// 设置渐隐速度
        /// </summary>
        /// <param name="speed">新的速度值</param>
        public void SetFadeSpeed(float speed)
        {
            fadeSpeed = Mathf.Max(0.0f, speed);
            if (enableDebug)
            {
                Debug.Log($"TitleFadeController: 动效速度已设置为 {fadeSpeed:F4}");
            }
        }
        
        /// <summary>
        /// 设置速度倍数
        /// </summary>
        /// <param name="multiplier">新的速度倍数</param>
        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = Mathf.Max(0.0001f, multiplier);
            if (enableDebug)
            {
                Debug.Log($"TitleFadeController: 速度倍数已设置为 {speedMultiplier:F4}");
            }
        }
        
        /// <summary>
        /// 设置目标灰度值
        /// </summary>
        /// <param name="targetGray">目标灰度值</param>
        public void SetTargetGrayValue(float targetGray)
        {
            targetGrayValue = Mathf.Clamp01(targetGray);
            
            if (enableDebug)
            {
                Debug.Log($"TitleFadeController: 目标灰度值已设置为 {targetGrayValue:F4}");
            }
        }
        
        /// <summary>
        /// 设置起始灰度值
        /// </summary>
        /// <param name="startGray">起始灰度值</param>
        public void SetStartGrayValue(float startGray)
        {
            startGrayValue = Mathf.Clamp01(startGray);
            
            if (enableDebug)
            {
                Debug.Log($"TitleFadeController: 起始灰度值已设置为 {startGrayValue:F4}");
            }
        }
        
        /// <summary>
        /// 立即设置到指定灰度值
        /// </summary>
        /// <param name="grayValue">目标灰度值</param>
        public void SetGrayValue(float grayValue)
        {
            currentGrayValue = Mathf.Clamp(grayValue, targetGrayValue, startGrayValue);
            UpdateMaterialColor();
            
            if (enableDebug)
            {
                Debug.Log($"TitleFadeController: 灰度值已设置为 {currentGrayValue:F4}");
            }
        }
        
        /// <summary>
        /// 获取当前灰度值
        /// </summary>
        /// <returns>当前灰度值</returns>
        public float GetCurrentGrayValue()
        {
            return currentGrayValue;
        }
        
        /// <summary>
        /// 获取动效进度百分比
        /// </summary>
        /// <returns>动效进度百分比 (0-100)</returns>
        public float GetProgressPercentage()
        {
            if (Mathf.Approximately(startGrayValue, targetGrayValue))
                return 100f;
                
            return (startGrayValue - currentGrayValue) / (startGrayValue - targetGrayValue) * 100f;
        }
        
        /// <summary>
        /// 检查动效是否完成
        /// </summary>
        /// <returns>动效是否完成</returns>
        public bool IsEffectCompleted()
        {
            return isCompleted;
        }
        
        /// <summary>
        /// 检查动效是否激活
        /// </summary>
        /// <returns>动效是否激活</returns>
        public bool IsEffectActive()
        {
            return isActive;
        }
        
        /// <summary>
        /// 重置动效到起始状态
        /// </summary>
        public void ResetEffect()
        {
            isActive = false;
            isCompleted = false;
            currentGrayValue = startGrayValue;
            UpdateMaterialColor();
            
            if (enableDebug)
            {
                Debug.Log("TitleFadeController: 动效已重置到起始状态");
            }
        }
        
        /// <summary>
        /// 切换调试模式
        /// </summary>
        public void ToggleDebug()
        {
            enableDebug = !enableDebug;
            Debug.Log($"TitleFadeController: 调试模式已{(enableDebug ? "启用" : "禁用")}");
        }
        
        /// <summary>
        /// 获取动效统计信息
        /// </summary>
        /// <returns>动效统计信息</returns>
        public string GetEffectStats()
        {
            float timeSinceStart = Time.time - lastUpdateTime;
            float progress = GetProgressPercentage();
            string status = isCompleted ? "已完成" : (isActive ? "进行中" : "未开始");
            
            return $"状态: {status}, 进度: {progress:F1}%, 运行时间: {timeSinceStart:F2}s, 帧数: {frameCount}, 当前灰度: {currentGrayValue:F4}";
        }
        
        private void OnDestroy()
        {
            // 清理材质引用
            if (titleMaterial != null)
            {
                DestroyImmediate(titleMaterial);
            }
        }
        
        private void OnValidate()
        {
            // 在编辑器中验证参数
            fadeSpeed = Mathf.Max(0.0f, fadeSpeed);
            speedMultiplier = Mathf.Max(0.0001f, speedMultiplier);
            startGrayValue = Mathf.Clamp01(startGrayValue);
            targetGrayValue = Mathf.Clamp01(targetGrayValue);
            
            // 确保起始值大于目标值
            if (startGrayValue <= targetGrayValue)
            {
                startGrayValue = targetGrayValue + 0.001f;
            }
            
            // 输出验证后的参数
            if (Application.isPlaying && enableDebug)
            {
                Debug.Log($"TitleFadeController: 参数验证完成 - 起始值: {startGrayValue:F4}, 目标值: {targetGrayValue:F4}, 速度: {fadeSpeed:F4}");
            }
        }
        
        private void OnGUI()
        {
            if (!enableDebug || !Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 350, 180));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("Title渐隐控制器 - 调试信息", GUI.skin.box);
            GUILayout.Label($"状态: {(isActive ? (isCompleted ? "已完成" : "进行中") : "未开始")}");
            GUILayout.Label($"当前灰度: {currentGrayValue:F4}");
            GUILayout.Label($"起始值: {startGrayValue:F4} → 目标值: {targetGrayValue:F4}");
            GUILayout.Label($"进度: {GetProgressPercentage():F1}%");
            GUILayout.Label($"运行时间: {Time.time - lastUpdateTime:F2}s");
            GUILayout.Label($"帧数: {frameCount}");
            
            if (isCompleted)
            {
                GUILayout.Label("✓ 动效已完成，保持目标值", GUI.skin.box);
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
