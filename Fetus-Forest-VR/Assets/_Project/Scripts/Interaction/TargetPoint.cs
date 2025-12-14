using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using AK.Wwise; // Add Wwise namespace

public class TargetPoint : MonoBehaviour
{
    [Header("拾取设置")]
    [SerializeField] private LayerMask playerLayer = 1; // 玩家层级
    [Tooltip("是否在首次拾取时使用挂载在TargetPoint上的SphereCollider范围进行检测")]
    [SerializeField] private bool useSphereColliderForFirstPickup = true;
    [Tooltip("首次拾取检测所用的SphereCollider；若留空将自动获取本物体上的SphereCollider")]
    [SerializeField] private SphereCollider firstPickupCollider;

    [Header("玩家引用")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private string playerTag = "Player";

    [Header("UI提示")]
    [SerializeField] private GameObject fIntroUI; // F Intro UI对象
    [SerializeField] private TextMeshProUGUI fIntroText; // F Intro文本组件
    [SerializeField] private CanvasGroup fIntroCanvasGroup; // 用于Alpha渐变
    
    [Header("动画设置")]
    [SerializeField] private float fadeInDuration = 0.5f; // UI淡入时间
    [SerializeField] private float fadeOutDuration = 0.3f; // UI淡出时间

    [Header("动画过渡")]
    [SerializeField] private string pickupBoolParam = "Pickup"; // 首次：置为true

    [Header("环境过渡通知")]
    [SerializeField] private EnvironmentTransitionManager envManager; // 手动挂载
    private bool envPickupNotified = false;

    [Header("消失动画等待")]
    [SerializeField] private bool waitDisappearThenDisable = true; // 是否等待消失动画播放完成后再禁用
    [SerializeField] private string disappearStateName = "Light Point Disappear"; // 消失动画状态名（可留空使用超时）
    [SerializeField] private int animatorLayerIndex = 0; // 动画层索引
    [SerializeField] private float disappearTimeout = 3f; // 超时时间
    [SerializeField] private float disappearMinDuration = 0.05f; // 至少等待一小段时间避免瞬切

    // 私有变量
    private bool isPlayerNearby = false;
    private bool isPickupInProgress = false;
    private bool isUIVisible = false;
    
    // 组件引用
    private PlayerInputActions inputActions;
    private Animator targetAnimator;
    
    #region Unity生命周期
    
    private void Awake()
    {
        // 获取组件引用
        targetAnimator = GetComponent<Animator>();
        if (useSphereColliderForFirstPickup && firstPickupCollider == null)
        {
            firstPickupCollider = GetComponent<SphereCollider>();
        }
        
        // 初始化Input Actions
        inputActions = new PlayerInputActions();
        
        // 初始化UI
        InitializeUI();
    }
    
    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.PickUp.performed += OnPickupInput;
    }
    
    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.PickUp.performed -= OnPickupInput;
    }
    
    private void Start()
    {
        // 确保UI初始状态为隐藏
        if (fIntroUI != null)
        {
            fIntroUI.SetActive(false);
        }

        // 自动查找玩家引用
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null) playerTransform = playerObj.transform;
        }
        if (playerTransform == null && Camera.main != null)
        {
            // 兜底：把相机所在的根对象当成玩家（第一人称/第三人称常见）
            playerTransform = Camera.main.transform.root;
        }
    }
    
    #endregion
    
    #region 初始化
    
    private void InitializeUI()
    {
        // 如果没有指定UI组件，尝试自动查找
        if (fIntroUI == null)
        {
            fIntroUI = GameObject.Find("F Intro");
        }
        
        if (fIntroUI != null && fIntroCanvasGroup == null)
        {
            fIntroCanvasGroup = fIntroUI.GetComponent<CanvasGroup>();
            if (fIntroCanvasGroup == null)
            {
                fIntroCanvasGroup = fIntroUI.AddComponent<CanvasGroup>();
            }
        }
        
        if (fIntroUI != null && fIntroText == null)
        {
            fIntroText = fIntroUI.GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    
    #endregion
    
    
    #region UI控制
    
    private void ShowPickupUI()
    {
        if (fIntroUI == null || isUIVisible)
            return;
            
        fIntroUI.SetActive(true);
        StartCoroutine(FadeInUI());
    }
    
    private void HidePickupUI()
    {
        if (fIntroUI == null || !isUIVisible)
            return;
            
        StartCoroutine(FadeOutUI());
    }
    
    private IEnumerator FadeInUI()
    {
        if (fIntroCanvasGroup == null) yield break;
        
        isUIVisible = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            fIntroCanvasGroup.alpha = alpha;
            yield return null;
        }
        
        fIntroCanvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOutUI()
    {
        if (fIntroCanvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            fIntroCanvasGroup.alpha = alpha;
            yield return null;
        }
        
        fIntroCanvasGroup.alpha = 0f;
        fIntroUI.SetActive(false);
        isUIVisible = false;
    }
    
    #endregion
    
    #region 输入处理
    
    private void OnPickupInput(InputAction.CallbackContext context)
    {
        if (isPickupInProgress || !isPlayerNearby)
            return;
            
        StartPickupProcess();
    }
    
    #endregion
    
    #region 拾取流程
    
    private void StartPickupProcess()
    {
        if (isPickupInProgress) return;
        
        isPickupInProgress = true;
        HidePickupUI();
        
        // Trigger Wwise event "FogDisappear" when F key is pressed
        AkSoundEngine.PostEvent("FogDisappear", gameObject);
        
        // 开始拾取动画序列（首次拾取：触发消失与交接）
        StartCoroutine(PickupSequence());
    }
    
    private IEnumerator PickupSequence()
    {
        // 置 Pickup=true，触发消失
        if (targetAnimator != null && !string.IsNullOrEmpty(pickupBoolParam))
        {
            targetAnimator.SetBool(pickupBoolParam, true);
        }
        
        // 等待消失动画
        if (waitDisappearThenDisable && targetAnimator != null)
        {
            float startTime = Time.time;
            bool enteredDisappear = string.IsNullOrEmpty(disappearStateName); // 若没填状态名，则不要求进入指定状态
            // 至少等待一个很短的时间，避免瞬切
            yield return new WaitForSeconds(disappearMinDuration);
            while (Time.time - startTime < disappearTimeout)
            {
                AnimatorStateInfo st = targetAnimator.GetCurrentAnimatorStateInfo(animatorLayerIndex);
                if (!string.IsNullOrEmpty(disappearStateName))
                {
                    if (st.IsName(disappearStateName))
                    {
                        enteredDisappear = true;
                    }
                }
                if (enteredDisappear && st.normalizedTime >= 0.99f)
                {
                    break; // 认为消失动画播放结束
                }
                yield return null;
            }
        }

        // 交接给 TrackPoint
        HandoverToTrackPoint();

        // 通知环境过渡（仅一次）
        if (!envPickupNotified && envManager != null)
        {
            envManager.OnPlayerPickedTarget();
            envPickupNotified = true;
        }
        // 停用自身
        gameObject.SetActive(false);
        yield break;
    }
    
    #endregion
    
    #region 首次拾取触发器

    private void OnTriggerEnter(Collider other)
    {
        if (!useSphereColliderForFirstPickup) return;
        if (firstPickupCollider == null || !firstPickupCollider.enabled) return;
        if (!other || ((1 << other.gameObject.layer) & playerLayer) == 0) return;
        if (other.isTrigger && !firstPickupCollider.isTrigger) return; // 可选：避免误触

        isPlayerNearby = true;
        ShowPickupUI();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!useSphereColliderForFirstPickup) return;
        if (firstPickupCollider == null || !firstPickupCollider.enabled) return;
        if (!other || ((1 << other.gameObject.layer) & playerLayer) == 0) return;
        if (other.isTrigger && !firstPickupCollider.isTrigger) return;

        isPlayerNearby = false;
        HidePickupUI();
    }

    #endregion

    [Header("首次->TrackPoint 交接")]
    [SerializeField] private TrackPoint nextTrackPoint; // 交接后的TrackPoint（建议在场景中预置为禁用）
    [SerializeField] private GameObject trackPointPrefab; // 或者使用预制体实例化
    [SerializeField] private bool instantiateTrackPointFromPrefab = false;
    [SerializeField] private Transform trackPointParent; // 实例化父节点

    private void HandoverToTrackPoint()
    {
        TrackPoint tp = nextTrackPoint;
        if (tp == null && instantiateTrackPointFromPrefab && trackPointPrefab != null)
        {
            GameObject go = Instantiate(trackPointPrefab, trackPointParent != null ? trackPointParent : null);
            tp = go.GetComponent<TrackPoint>();
            if (tp == null)
            {
                Debug.LogError("TrackPoint预制体上没有TrackPoint组件");
                return;
            }
        }
        if (tp == null)
        {
            Debug.LogWarning("未配置nextTrackPoint或预制体，无法交接");
            return;
        }
        
        // 启用TrackPoint
        if (!tp.gameObject.activeSelf) tp.gameObject.SetActive(true);
    }
}
