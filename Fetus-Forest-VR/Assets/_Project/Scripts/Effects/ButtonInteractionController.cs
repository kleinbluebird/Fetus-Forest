using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FetusForest.Control;

namespace FetusForest.Effects
{
	/// <summary>
	/// 按钮交互控制器
	/// 实现按钮选中、点击、禁用状态的视觉效果
	/// </summary>
	public class ButtonInteractionController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
	{
		[Header("组件引用")]
		[SerializeField] private Button button;
		[SerializeField] private CanvasGroup rootCanvasGroup;
		[SerializeField] private GameObject selectedSignal;
		[SerializeField] private CanvasGroup selectedSignalCanvasGroup;
		
		[Header("动效参数")]
		[SerializeField] private float fadeInDuration = 0.2f;
		[SerializeField] private float fadeOutDuration = 0.15f;
		[SerializeField] private float clickAlphaDuration = 0.1f;
		
		[Header("Alpha值设置")]
		[SerializeField] private float normalAlpha = 0.6f;
		[SerializeField] private float hoverAlpha = 1.0f;
		[SerializeField] private float disabledAlpha = 0.2f;
		
		[Header("鼠标检测设置")]
		[SerializeField] private bool enableMouseDetection = true;
		[SerializeField] private float mouseDetectionRadius = 100f; // 鼠标检测半径
		
		private bool isSelected = false;
		private bool isHovered = false;
		private bool isMouseOverButton = false; // 鼠标是否在按钮上
		private Coroutine fadeCoroutine;
		private Coroutine clickCoroutine;
		private RectTransform buttonRectTransform;
		private Canvas parentCanvas;
		
		// 确保全局只有一个Selected Signal显示
		private static ButtonInteractionController currentSignalOwner;
		
		private void Awake()
		{
			InitializeComponents();
		}
		
		private void Start()
		{
			// 若为 Continue 按钮，且无存档，则禁用（并保持禁用态alpha）
			if (IsContinueButton())
			{
				bool hasSave = MiniSaveManager.Instance != null && MiniSaveManager.Instance.HasSave();
				SetButtonInteractable(hasSave);
			}
			
			// 根据可交互状态设置初始视觉，避免被覆盖
			if (button != null && button.interactable)
			{
				SetNormalState();
			}
			else
			{
				SetDisabledState();
				SafeHideSelectedSignal(this);
			}
			
			// 如果是Begin按钮，设置为默认选中
			if (IsBeginButton())
			{
				StartCoroutine(SelectBeginButtonOnStart());
			}
		}
		
		private bool IsBeginButton() { return gameObject.name.ToLower().Contains("begin"); }
		private bool IsContinueButton() { return gameObject.name.ToLower().Contains("continue"); }
		
		private void Update()
		{
			// 检测回车键点击
			if (isSelected && Input.GetKeyDown(KeyCode.Return))
			{
				TriggerClickEffect();
			}
			
			// 检测鼠标位置
			if (enableMouseDetection)
			{
				CheckMousePosition();
			}
		}
		
		/// <summary>
		/// 检查鼠标是否在按钮区域内
		/// </summary>
		private void CheckMousePosition()
		{
			if (buttonRectTransform == null) return;
			
			Vector2 mousePosition = Input.mousePosition;
			Vector2 buttonScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, buttonRectTransform.position);
			
			// 计算鼠标与按钮的距离
			float distance = Vector2.Distance(mousePosition, buttonScreenPosition);
			
			// 如果鼠标在按钮检测半径内
			if (distance <= mouseDetectionRadius)
			{
				if (!isMouseOverButton)
				{
					isMouseOverButton = true;
					OnMouseEnterButton();
				}
			}
			else
			{
				if (isMouseOverButton)
				{
					isMouseOverButton = false;
					OnMouseLeaveButton();
				}
			}
		}
		
		/// <summary>
		/// 鼠标进入按钮区域
		/// </summary>
		private void OnMouseEnterButton()
		{
			if (button == null || !button.interactable) return;
			
			// 鼠标优先：设置为当前选中对象，同时收回其它按钮的Selected Signal
			if (EventSystem.current != null)
			{
				EventSystem.current.SetSelectedGameObject(gameObject);
			}
			
			isHovered = true;
			ShowSelectedSignal();
			SetHoverState();
		}
		
		/// <summary>
		/// 鼠标离开按钮区域
		/// </summary>
		private void OnMouseLeaveButton()
		{
			if (button == null || !button.interactable) return;
			
			isHovered = false;
			
			// 如果键盘选中状态，保持Selected Signal显示
			if (isSelected)
			{
				SetSelectedState();
			}
			else
			{
				HideSelectedSignal();
				SetNormalState();
			}
		}
		
		/// <summary>
		/// 初始化组件
		/// </summary>
		private void InitializeComponents()
		{
			// 自动获取组件
			if (button == null)
				button = GetComponent<Button>();
				
			if (rootCanvasGroup == null)
				rootCanvasGroup = GetComponent<CanvasGroup>();
				
			if (selectedSignal == null)
				selectedSignal = transform.Find("Selected Signal")?.gameObject;
				
			// 确保Selected Signal有CanvasGroup且初始为0
			if (selectedSignal != null)
			{
				if (selectedSignalCanvasGroup == null)
				{
					selectedSignalCanvasGroup = selectedSignal.GetComponent<CanvasGroup>();
				}
				if (selectedSignalCanvasGroup == null)
				{
					selectedSignalCanvasGroup = selectedSignal.AddComponent<CanvasGroup>();
				}
				selectedSignalCanvasGroup.alpha = 0f;
			}
			
			if (buttonRectTransform == null)
				buttonRectTransform = GetComponent<RectTransform>();
				
			if (parentCanvas == null)
				parentCanvas = GetComponentInParent<Canvas>();
				
			// 如果没有CanvasGroup，创建一个
			if (rootCanvasGroup == null)
				rootCanvasGroup = gameObject.AddComponent<CanvasGroup>();
		}
		
		/// <summary>
		/// 游戏开始时默认选中Begin按钮
		/// </summary>
		private System.Collections.IEnumerator SelectBeginButtonOnStart()
		{
			yield return new WaitForSeconds(0.1f);
			
			if (button != null)
			{
				button.Select();
				OnSelect(null);
			}
		}
		
		/// <summary>
		/// 鼠标进入（Unity事件系统）
		/// </summary>
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (button == null || !button.interactable) return;
			if (!isHovered)
			{
				OnMouseEnterButton();
			}
		}
		
		/// <summary>
		/// 鼠标离开（Unity事件系统）
		/// </summary>
		public void OnPointerExit(PointerEventData eventData)
		{
			if (button == null || !button.interactable) return;
			if (isHovered)
			{
				OnMouseLeaveButton();
			}
		}
		
		/// <summary>
		/// 鼠标点击
		/// </summary>
		public void OnPointerClick(PointerEventData eventData)
		{
			if (button == null || !button.interactable) return;
			TriggerClickEffect();
		}
		
		/// <summary>
		/// 按钮被选中（键盘导航）
		/// </summary>
		public void OnSelect(BaseEventData eventData)
		{
			if (button == null || !button.interactable) return;
			isSelected = true;
			ShowSelectedSignal();
			SetSelectedState();
		}
		
		/// <summary>
		/// 按钮取消选中
		/// </summary>
		public void OnDeselect(BaseEventData eventData)
		{
			if (button == null || !button.interactable) return;
			// 只有在鼠标确实不在按钮上且没有键盘选中时才允许取消选中
			if (!isMouseOverButton && !isHovered)
			{
				isSelected = false;
				HideSelectedSignal();
				SetNormalState();
			}
			else
			{
				isSelected = true;
			}
		}
		
		/// <summary>
		/// 显示Selected Signal（全局唯一）
		/// </summary>
		private void ShowSelectedSignal()
		{
			if (selectedSignal == null) return;
			// 确保本地CanvasGroup存在
			if (selectedSignalCanvasGroup == null)
			{
				selectedSignalCanvasGroup = selectedSignal.GetComponent<CanvasGroup>();
				if (selectedSignalCanvasGroup == null)
					selectedSignalCanvasGroup = selectedSignal.AddComponent<CanvasGroup>();
			}
			
			// 先隐藏上一个拥有者，保证全局唯一（安全调用）
			if (currentSignalOwner != null && currentSignalOwner != this)
			{
				SafeHideSelectedSignal(currentSignalOwner);
				if (currentSignalOwner != null) currentSignalOwner.isSelected = false;
			}
			currentSignalOwner = this;
			
			// 停止之前的动画
			SafeStopCoroutine(this, ref fadeCoroutine);
			
			fadeCoroutine = StartCoroutine(FadeSelectedSignal(selectedSignalCanvasGroup.alpha, 1f, fadeInDuration));
		}
		
		/// <summary>
		/// 隐藏Selected Signal
		/// </summary>
		private void HideSelectedSignal()
		{
			if (selectedSignal == null || selectedSignalCanvasGroup == null) return;
			
			// 停止之前的动画
			SafeStopCoroutine(this, ref fadeCoroutine);
			
			fadeCoroutine = StartCoroutine(FadeSelectedSignal(selectedSignalCanvasGroup.alpha, 0f, fadeOutDuration));
			
			if (currentSignalOwner == this)
			{
				currentSignalOwner = null;
			}
		}
		
		/// <summary>
		/// 触发点击效果
		/// </summary>
		private void TriggerClickEffect()
		{
			if (rootCanvasGroup == null) return;
			
			SafeStopCoroutine(this, ref clickCoroutine);
			clickCoroutine = StartCoroutine(ClickAlphaEffect());
		}
		
		/// <summary>
		/// 设置正常状态
		/// </summary>
		private void SetNormalState()
		{
			if (rootCanvasGroup != null)
				rootCanvasGroup.alpha = normalAlpha;
		}
		
		/// <summary>
		/// 设置选中状态（键盘导航）
		/// </summary>
		private void SetSelectedState()
		{
			if (rootCanvasGroup != null)
				rootCanvasGroup.alpha = normalAlpha; // 选中/悬停统一为0.6
		}
		
		/// <summary>
		/// 设置悬停状态（鼠标悬停）
		/// </summary>
		private void SetHoverState()
		{
			if (rootCanvasGroup != null)
				rootCanvasGroup.alpha = normalAlpha; // 悬停同样为0.6
		}
		
		/// <summary>
		/// 设置禁用状态
		/// </summary>
		private void SetDisabledState()
		{
			if (rootCanvasGroup != null)
				rootCanvasGroup.alpha = disabledAlpha;
		}
		
		/// <summary>
		/// Selected Signal渐显渐隐动画
		/// </summary>
		private System.Collections.IEnumerator FadeSelectedSignal(float startAlpha, float endAlpha, float duration)
		{
			if (selectedSignalCanvasGroup == null) yield break;
			float elapsedTime = 0f;
			while (elapsedTime < duration)
			{
				elapsedTime += Time.deltaTime;
				float progress = elapsedTime / duration;
				float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, progress);
				selectedSignalCanvasGroup.alpha = currentAlpha;
				yield return null;
			}
			selectedSignalCanvasGroup.alpha = endAlpha;
			fadeCoroutine = null;
		}
		
		/// <summary>
		/// 点击Alpha变化动画
		/// </summary>
		private System.Collections.IEnumerator ClickAlphaEffect()
		{
			if (rootCanvasGroup == null) yield break;
			float startAlpha = rootCanvasGroup.alpha;
			float targetAlpha = hoverAlpha; // 1.0
			// 快速变亮到1.0
			float elapsedTime = 0f;
			while (elapsedTime < clickAlphaDuration / 2f)
			{
				elapsedTime += Time.deltaTime;
				float progress = elapsedTime / (clickAlphaDuration / 2f);
				rootCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
				yield return null;
			}
			// 始终回落到选中/悬停的统一视觉：0.6
			elapsedTime = 0f;
			float finalAlpha = normalAlpha;
			while (elapsedTime < clickAlphaDuration / 2f)
			{
				elapsedTime += Time.deltaTime;
				float progress = elapsedTime / (clickAlphaDuration / 2f);
				rootCanvasGroup.alpha = Mathf.Lerp(targetAlpha, finalAlpha, progress);
				yield return null;
			}
			rootCanvasGroup.alpha = finalAlpha; // 0.6
			clickCoroutine = null;
		}
		
		/// <summary>
		/// 设置按钮交互状态
		/// </summary>
		public void SetButtonInteractable(bool interactable)
		{
			if (button == null) return;
			button.interactable = interactable;
			if (interactable)
			{
				if (isHovered) SetHoverState();
				else if (isSelected) SetSelectedState();
				else SetNormalState();
			}
			else
			{
				SetDisabledState();
				SafeHideSelectedSignal(this);
			}
		}
		
		/// <summary>
		/// 强制保持选中状态
		/// </summary>
		public void ForceKeepSelected()
		{
			isSelected = true;
			ShowSelectedSignal();
			SetSelectedState();
		}
		
		private void OnDestroy()
		{
			SafeStopCoroutine(this, ref fadeCoroutine);
			SafeStopCoroutine(this, ref clickCoroutine);
			if (currentSignalOwner == this) currentSignalOwner = null;
		}
		
		// ===== 安全辅助方法 =====
		private static void SafeStopCoroutine(MonoBehaviour runner, ref Coroutine c)
		{
			if (c != null && runner != null)
			{
				runner.StopCoroutine(c);
				c = null;
			}
		}
		
		private static void SafeHideSelectedSignal(ButtonInteractionController ctrl)
		{
			if (ctrl == null) return;
			if (ctrl.fadeCoroutine != null) ctrl.StopCoroutine(ctrl.fadeCoroutine);
			if (ctrl.selectedSignalCanvasGroup == null)
			{
				if (ctrl.selectedSignal != null)
				{
					ctrl.selectedSignalCanvasGroup = ctrl.selectedSignal.GetComponent<CanvasGroup>() ?? ctrl.selectedSignal.AddComponent<CanvasGroup>();
				}
			}
			if (ctrl.selectedSignalCanvasGroup != null)
			{
				ctrl.selectedSignalCanvasGroup.alpha = 0f;
			}
			if (currentSignalOwner == ctrl) currentSignalOwner = null;
		}
	}
}
