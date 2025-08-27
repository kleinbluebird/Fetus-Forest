using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FetusForest.Control
{
	/// <summary>
	/// 简易场景控制器，提供黑场过渡与Begin/Continue/Exit入口
	/// </summary>
	public class SimpleSceneController : MonoBehaviour
	{
		private static SimpleSceneController _instance;
		public static SimpleSceneController Instance => _instance;
		
		[Header("场景名")]
		[SerializeField] private string firstSceneName = "Beginning Page"; // 仅作参考，不再作为判断依据
		[SerializeField] private string introPageScene = "IntroPage";
		[SerializeField] private string[] chapterScenes = new[] { "Chapter_0", "Chapter_1", "Chapter_2" };
		
		[Header("黑场过渡")]
		[SerializeField] private Canvas fadeCanvas;
		[SerializeField] private Image fadeImage;
		[SerializeField] private float fadeInDuration = 0.4f;   // 从黑到明
		[SerializeField] private float fadeOutDuration = 0.4f;  // 从明到黑
		[SerializeField] private float introStaySeconds = 5f;
		[SerializeField] private bool autoFadeInOnSceneLoaded = true;
		[SerializeField] private bool skipFadeInOnFirstScene = true; // 首场景（buildIndex==0）跳过淡入
		[SerializeField] private bool easeInFadeIn = true; // 淡入使用先慢后快
		
		private bool isFading = false;
		
		private void Awake()
		{
			// 单例常驻
			if (_instance != null && _instance != this)
			{
				Destroy(gameObject);
				return;
			}
			_instance = this;
			DontDestroyOnLoad(gameObject);
			
			EnsureFadeCanvas();
			// 初始场景如需跳过淡入，则初始alpha置0；否则置黑等待淡入
			bool isFirstScene = SceneManager.GetActiveScene().buildIndex == 0;
			if (skipFadeInOnFirstScene && isFirstScene)
			{
				SetFadeAlpha(0f);
			}
			else
			{
				SetFadeAlpha(1f);
			}
		}
		
		private void OnEnable()
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
		}
		
		private void OnDisable()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
		
		private void Start()
		{
			// 应用启动后的首次淡入（若不是首场景或未跳过）
			bool isFirstScene = SceneManager.GetActiveScene().buildIndex == 0;
			if (!(skipFadeInOnFirstScene && isFirstScene))
			{
				StartCoroutine(Fade(0f));
			}
		}
		
		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (!autoFadeInOnSceneLoaded) return;
			bool isFirstScene = scene.buildIndex == 0;
			// 首场景按需跳过淡入
			if (skipFadeInOnFirstScene && isFirstScene) return;
			// 如果当前仍是全黑或接近全黑，则触发淡入
			if (fadeImage != null && fadeImage.color.a > 0.9f && !isFading)
			{
				StartCoroutine(Fade(0f));
			}
		}
		
		private void EnsureFadeCanvas()
		{
			if (fadeCanvas == null || fadeImage == null)
			{
				var go = new GameObject("FadeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
				fadeCanvas = go.GetComponent<Canvas>();
				fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
				var imgGo = new GameObject("FadeImage", typeof(Image));
				imgGo.transform.SetParent(go.transform, false);
				fadeImage = imgGo.GetComponent<Image>();
				fadeImage.color = new Color(0f, 0f, 0f, 0f);
				fadeImage.raycastTarget = false; // 不拦截射线
				var rt = fadeImage.rectTransform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
				DontDestroyOnLoad(go);
			}
		}
		
		private void SetFadeAlpha(float a)
		{
			EnsureFadeCanvas();
			if (fadeImage == null) return;
			var c = fadeImage.color; c.a = a; fadeImage.color = c;
		}
		
		// 绑定到Begin按钮
		public void OnClickBegin()
		{
			MiniSaveManager.Instance?.SaveNewGame();
			StartCoroutine(BeginFlow());
		}
		
		// 绑定到Continue按钮
		public void OnClickContinue()
		{
			int chapter = MiniSaveManager.Instance != null ? MiniSaveManager.Instance.LoadFurthestChapter() : -1;
			if (chapter < 0 || chapter >= chapterScenes.Length) return;
			StartCoroutine(LoadWithFade(chapterScenes[chapter]));
		}
		
		// 绑定到Exit按钮
		public void OnClickExit()
		{
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
			Application.Quit();
			#endif
		}
		
		private IEnumerator BeginFlow()
		{
			yield return LoadWithFade(introPageScene);
			yield return new WaitForSeconds(introStaySeconds);
			yield return LoadWithFade(chapterScenes[0]);
		}
		
		private IEnumerator LoadWithFade(string sceneName)
		{
			// Fade Out（至黑）：固定从0到1
			yield return Fade(1f);
			var op = SceneManager.LoadSceneAsync(sceneName);
			while (!op.isDone) yield return null;
			// Fade In（从黑至可见）：固定从1到0（先慢后快）
			yield return Fade(0f);
		}
		
		private IEnumerator Fade(float targetAlpha)
		{
			EnsureFadeCanvas();
			if (fadeImage == null) yield break;
			isFading = true;
			// 固定起点：淡出(→1)从0开始；淡入(→0)从1开始
			float start = targetAlpha >= 1f ? 0f : 1f;
			float duration = targetAlpha >= 1f ? fadeOutDuration : fadeInDuration;
			SetFadeAlpha(start);
			float t = 0f;
			while (t < duration)
			{
				t += Time.unscaledDeltaTime;
				float u = Mathf.Clamp01(t / duration);
				// 淡入：先慢后快（ease-in），u*u；淡出：保持线性
				float eased = (targetAlpha < 1f && easeInFadeIn) ? (u * u) : u;
				float a = Mathf.Lerp(start, targetAlpha, eased);
				SetFadeAlpha(a);
				yield return null;
			}
			SetFadeAlpha(targetAlpha);
			isFading = false;
		}
	}
}
