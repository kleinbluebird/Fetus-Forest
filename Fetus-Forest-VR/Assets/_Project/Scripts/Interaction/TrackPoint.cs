using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using AK.Wwise; // Add Wwise namespace

public class TrackPoint : MonoBehaviour
{
	[Header("Input")] public PlayerInputActions inputActions;

	[Header("Motion Root and Player")]
	[SerializeField] private Transform motionRoot;
	[SerializeField] private Transform playerTransform;

	[Header("Appearance and Movement Parameters")]
	[SerializeField] private float curveMoveDuration = 5.0f;
	[SerializeField] private float arcHeight = 1.0f;
	[SerializeField] private AnimationCurve progressCurve = null;

	[Header("Initial Spawn Position")]
	[SerializeField] private bool useCustomSpawnPosition = false;
	[SerializeField] private Transform customSpawnTransform;

	[Header("Target Sequence")]
	[SerializeField] private Transform[] targetPositions;
	[SerializeField] private bool loopTargets = false;

	[Header("Pickup Detection (Trigger)")]
	[SerializeField] private LayerMask playerLayer = 1;
	[SerializeField] private SphereCollider pickupCollider;
	[SerializeField] private bool useTriggerPickup = true;

	[Header("Pickup UI (Image)")]
	[SerializeField] private GameObject promptUI;
	[SerializeField] private Image promptImage;
	[SerializeField] private CanvasGroup promptCanvasGroup;
	[SerializeField] private float promptFadeInDuration = 0.3f;
	[SerializeField] private float promptFadeOutDuration = 0.2f;

	[Header("Intro Animation Wait")]
	[SerializeField] private bool waitIntroThenMove = true;
	[SerializeField] private int introLayerIndex = 0;
	[SerializeField] private float introMinDuration = 0.05f;
	[SerializeField] private float introTimeout = 5f;

	[Header("Speed Control")]
	[SerializeField] private bool useDistanceBasedDuration = true;
	[SerializeField] private float secondsPerUnit = 0.6f;
	[SerializeField] private float minCurveDuration = 0.5f;
	[SerializeField] private float durationScale = 1.0f;

	[Header("Final Position Disappear Animation")]
	[SerializeField] private string intoEyeTriggerParam = "IntoEye";
	[SerializeField] private bool waitDisappearThenDisable = true;
	[SerializeField] private int disappearLayerIndex = 0;
	[SerializeField] private float disappearMinDuration = 0.05f;
	[SerializeField] private float disappearTimeout = 5f;

	[SerializeField] private ConnectionLineController connectionLine;

	[Header("Silk UI Control")]
	[SerializeField] private SilkUIController silkUIController;

	// Private variables
	private int currentTargetIndex = -1;
	private bool isPickupInProgress = false;
	private bool isPlayerNearby = false;
	private bool isPromptVisible = false;
	private bool canPickup = false;
	private bool isAtLastPosition = false;
	private bool isDisappearing = false;
	private bool isMoving = false;
	private Animator animator;

	private void Awake()
	{
		if (motionRoot == null) motionRoot = transform;
		inputActions = new PlayerInputActions();
		animator = GetComponent<Animator>();
		if (animator == null) animator = GetComponentInChildren<Animator>();
		if (pickupCollider == null) pickupCollider = GetComponent<SphereCollider>();
	}

	private void OnEnable()
	{
		inputActions.Enable();
		inputActions.Player.PickUp.performed += OnPickupInput;

		// Set initial spawn position immediately
		Vector3 initialSpawnPos;
		if (useCustomSpawnPosition)
		{
			initialSpawnPos = customSpawnTransform.position;
		}
		else
		{
			initialSpawnPos = motionRoot.position;
		}
		motionRoot.position = initialSpawnPos;

		canPickup = false;
		HidePromptUI();

		// Initialize and activate connection line
		if (connectionLine != null)
		{
			connectionLine.Activate();
		}

		// Notify Silk UI on first activation
		if (silkUIController != null)
		{
			silkUIController.OnFirstActivation();
		}

		StartCoroutine(SpawnSequence());
	}

	private void OnDisable()
	{
		inputActions.Disable();
		inputActions.Player.PickUp.performed -= OnPickupInput;
		StopAllCoroutines();
		HidePromptUI();

		if (connectionLine != null)
		{
			connectionLine.Deactivate();
		}
	}

	private IEnumerator SpawnSequence()
	{
		// Wait for intro animation to complete
		if (waitIntroThenMove && animator != null)
		{
			yield return WaitForIntroAnimation();
		}

		// Move from spawn position to first target along curve
		Transform firstTarget = GetNextTarget();
		if (firstTarget != null)
		{
			// Notify Silk UI of locked target position
			if (silkUIController != null)
			{
				silkUIController.SetLockedTargetPosition(firstTarget.position);
			}

			isMoving = true;
			Debug.Log($"[TrackPoint] Starting movement to first target position");

			float d = Vector3.Distance(motionRoot.position, firstTarget.position);
			float dur = useDistanceBasedDuration ? Mathf.Max(minCurveDuration, d * secondsPerUnit) : curveMoveDuration;
			dur *= Mathf.Max(0.0001f, durationScale);
			yield return MoveAlongArc(motionRoot.position, firstTarget.position, arcHeight, dur);
			currentTargetIndex = 0;
			motionRoot.position = firstTarget.position;

			// Check if we've reached the last target position
			isAtLastPosition = (targetPositions.Length == 1);

			isMoving = false;
			Debug.Log($"[TrackPoint] Reached first target position, movement complete");
		}

		// First automatic movement complete, allow pickup (unless at last position)
		canPickup = !isAtLastPosition;
		Debug.Log($"[TrackPoint] SpawnSequence complete - canPickup set to: {canPickup}, isAtLastPosition: {isAtLastPosition}");

		// If at last position, log but don't do anything special
		if (isAtLastPosition)
		{
			Debug.Log($"[TrackPoint] Already at last target position on spawn, pickup disabled");
		}

		// Show prompt UI if player is nearby and pickup is allowed
		if (canPickup && isPlayerNearby && !isMoving)
		{
			Debug.Log($"[TrackPoint] Showing prompt UI after spawn sequence");
			ShowPromptUI();
		}
	}

	private IEnumerator WaitForIntroAnimation()
	{
		// Minimum wait to avoid pop-in
		yield return new WaitForSeconds(introMinDuration);

		if (animator != null)
		{
			var clips = animator.GetCurrentAnimatorClipInfo(introLayerIndex);
			if (clips != null && clips.Length > 0 && clips[0].clip != null)
			{
				float speed = Mathf.Abs(animator.speed) < 0.0001f ? 1f : Mathf.Abs(animator.speed);
				float waitSeconds = clips[0].clip.length / speed;
				float remaining = Mathf.Clamp(waitSeconds - introMinDuration, 0f, introTimeout);
				if (remaining > 0f)
				{
					yield return new WaitForSeconds(remaining);
				}
				yield break;
			}
		}
		// Fallback to timeout if animation clip not found
		yield return new WaitForSeconds(introTimeout);
	}

	private void OnPickupInput(InputAction.CallbackContext _)
	{
		Debug.Log($"[TrackPoint] PickUp input detected - isPickupInProgress: {isPickupInProgress}, canPickup: {canPickup}, isPlayerNearby: {isPlayerNearby}");

		if (isPickupInProgress)
		{
			Debug.Log("[TrackPoint] Pickup already in progress, ignoring input");
			return;
		}
		if (!canPickup)
		{
			Debug.Log("[TrackPoint] Pickup not allowed");
			return;
		}
		if (!isPlayerNearby)
		{
			Debug.Log("[TrackPoint] Player not nearby");
			return;
		}

		Debug.Log("[TrackPoint] Starting NextHop sequence");

		// Notify Silk UI of pickup event
		if (silkUIController != null)
		{
			silkUIController.OnPickup();
		}

		// Trigger connection line pulse effect
		if (connectionLine != null)
		{
			connectionLine.Pulse();
		}

		StartCoroutine(NextHop());
	}

	private IEnumerator NextHop()
	{
		isPickupInProgress = true;
		Debug.Log("[TrackPoint] NextHop started");
		HidePromptUI();

		Transform next = GetNextTarget();
		if (next != null)
		{
			Debug.Log($"[TrackPoint] Found next target at position: {next.position}");

			// Notify Silk UI of locked target position
			if (silkUIController != null)
			{
				silkUIController.SetLockedTargetPosition(next.position);
			}

			isMoving = true;
			Debug.Log($"[TrackPoint] Starting movement to next target position");

			float d = Vector3.Distance(motionRoot.position, next.position);
			float dur = useDistanceBasedDuration ? Mathf.Max(minCurveDuration, d * secondsPerUnit) : curveMoveDuration;
			dur *= Mathf.Max(0.0001f, durationScale);
			Debug.Log($"[TrackPoint] Movement distance: {d:F2}, duration: {dur:F2}");

			yield return MoveAlongArc(motionRoot.position, next.position, arcHeight, dur);
			currentTargetIndex = GetClampedNextIndex();
			motionRoot.position = next.position;

			Debug.Log($"[TrackPoint] Movement complete, current target index: {currentTargetIndex}");

			// Check if we've reached the last target position
			isAtLastPosition = (currentTargetIndex == targetPositions.Length - 1);

			// If at last position, disable pickup
			if (isAtLastPosition)
			{
				canPickup = false;
				HidePromptUI();
				Debug.Log($"[TrackPoint] Reached last target position (index: {currentTargetIndex}), pickup disabled");
			}

			isMoving = false;
			Debug.Log($"[TrackPoint] Reached target position, movement finished");

			// Trigger Wwise event after NextHop completes
			Debug.Log("[TrackPoint] About to trigger FogDisappear event");
			TriggerFogDisappearEvent();
		}
		else
		{
			Debug.LogWarning("[TrackPoint] No next target found!");
		}

		isPickupInProgress = false;
		Debug.Log("[TrackPoint] NextHop finished");

		// Show UI only if not at last position and player is nearby
		if (!isAtLastPosition && isPlayerNearby && !isMoving)
		{
			Debug.Log("[TrackPoint] Showing prompt UI");
			ShowPromptUI();
		}
	}

	private void TriggerFogDisappearEvent()
	{
		// Post Wwise event "FogDisappear"
		AkSoundEngine.PostEvent("FogDisappear", gameObject);
		Debug.Log("[TrackPoint] Posted Wwise event: FogDisappear");
	}

	private Transform GetNextTarget()
	{
		if (targetPositions == null || targetPositions.Length == 0) return null;
		int nextIndex = GetClampedNextIndex();
		return targetPositions[Mathf.Clamp(nextIndex, 0, targetPositions.Length - 1)];
	}

	private int GetClampedNextIndex()
	{
		if (targetPositions == null || targetPositions.Length == 0) return 0;
		int next = currentTargetIndex + 1;
		if (next >= targetPositions.Length)
		{
			return loopTargets ? 0 : targetPositions.Length - 1;
		}
		return next;
	}

	private IEnumerator MoveAlongArc(Vector3 from, Vector3 to, float height, float duration)
	{
		Debug.Log($"[TrackPoint] MoveAlongArc started - from: {from}, to: {to}, duration: {duration}");

		if (duration <= 0f) duration = 0.01f;
		float elapsed = 0f;
		Vector3 mid = (from + to) * 0.5f + Vector3.up * height;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			float p = progressCurve != null ? progressCurve.Evaluate(t) : Mathf.SmoothStep(0f, 1f, t);
			Vector3 a = Vector3.Lerp(from, mid, p);
			Vector3 b = Vector3.Lerp(mid, to, p);
			motionRoot.position = Vector3.Lerp(a, b, p);

			// Hide prompt UI if visible during movement
			if (isPromptVisible)
			{
				HidePromptUI();
			}

			yield return null;
		}
		motionRoot.position = to;
		Debug.Log($"[TrackPoint] MoveAlongArc completed");
	}

	#region Trigger Pickup Detection
	private void OnTriggerEnter(Collider other)
	{
		if (!useTriggerPickup) return;
		if (pickupCollider == null || !pickupCollider.enabled) return;
		if (!other || ((1 << other.gameObject.layer) & playerLayer) == 0) return;
		if (other.isTrigger && !pickupCollider.isTrigger) return;

		Debug.Log($"[TrackPoint] Player entered trigger range, state: LastPosition={isAtLastPosition}, CanPickup={canPickup}, Disappearing={isDisappearing}, Moving={isMoving}");
		isPlayerNearby = true;

		// Show prompt UI only when stationary and pickup is allowed
		if (canPickup && !isMoving)
		{
			ShowPromptUI();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!useTriggerPickup) return;
		if (pickupCollider == null || !pickupCollider.enabled) return;
		if (!other || ((1 << other.gameObject.layer) & playerLayer) == 0) return;
		if (other.isTrigger && !pickupCollider.isTrigger) return;

		Debug.Log($"[TrackPoint] Player left trigger range");
		isPlayerNearby = false;
		// Hide prompt UI regardless of state
		HidePromptUI();
	}
	#endregion

	#region Prompt UI Control
	private void ShowPromptUI()
	{
		if (promptUI == null || isPromptVisible) return;
		promptUI.SetActive(true);
		StartCoroutine(FadeInPrompt());
	}

	private void HidePromptUI()
	{
		if (promptUI == null || !isPromptVisible) return;
		StartCoroutine(FadeOutPrompt());
	}

	private IEnumerator FadeInPrompt()
	{
		if (promptCanvasGroup == null) yield break;
		isPromptVisible = true;
		float t = 0f;
		while (t < promptFadeInDuration)
		{
			t += Time.deltaTime;
			float a = Mathf.Lerp(0f, 1f, t / Mathf.Max(0.0001f, promptFadeInDuration));
			promptCanvasGroup.alpha = a;
			yield return null;
		}
		promptCanvasGroup.alpha = 1f;
	}

	private IEnumerator FadeOutPrompt()
	{
		if (promptCanvasGroup == null) yield break;
		float t = 0f;
		while (t < promptFadeOutDuration)
		{
			t += Time.deltaTime;
			float a = Mathf.Lerp(1f, 0f, t / Mathf.Max(0.0001f, promptFadeOutDuration));
			promptCanvasGroup.alpha = a;
			yield return null;
		}
		promptCanvasGroup.alpha = 0f;
		promptUI.SetActive(false);
		isPromptVisible = false;
	}
	#endregion

	#region Disappear Animation Handler

	private IEnumerator DisappearSequence()
	{
		if (isDisappearing) yield break;

		isDisappearing = true;
		Debug.Log($"[TrackPoint] Disappear animation sequence started, setting {intoEyeTriggerParam} = true");

		// Hide/deactivate connection line
		if (connectionLine != null)
		{
			connectionLine.SetVisible(false);
		}

		// Notify Silk UI of final disappear
		if (silkUIController != null)
		{
			silkUIController.OnFinalDisappear();
		}

		// Set IntoEye to true, trigger disappear animation
		if (animator != null && !string.IsNullOrEmpty(intoEyeTriggerParam))
		{
			animator.SetTrigger(intoEyeTriggerParam);

			// Wait for state transition
			yield return new WaitForSeconds(0.1f);

			// Check if disappear animation started
			yield return StartCoroutine(WaitForTransitionToDisappear());
		}

		// Wait for disappear animation to complete
		if (waitDisappearThenDisable && animator != null)
		{
			// Minimum wait to avoid pop-in
			yield return new WaitForSeconds(disappearMinDuration);

			// Get current animation clip
			var clips = animator.GetCurrentAnimatorClipInfo(disappearLayerIndex);
			if (clips != null && clips.Length > 0 && clips[0].clip != null)
			{
				float clipLength = clips[0].clip.length;
				float speed = Mathf.Abs(animator.speed) < 0.0001f ? 1f : Mathf.Abs(animator.speed);
				float waitSeconds = clipLength / speed;

				Debug.Log($"[TrackPoint] Disappear animation playing, length: {clipLength:F2} seconds");

				// Wait for animation to complete, but not longer than timeout
				float remaining = Mathf.Clamp(waitSeconds - disappearMinDuration, 0f, disappearTimeout);
				if (remaining > 0f)
				{
					yield return new WaitForSeconds(remaining);
				}
			}
			else
			{
				// Fallback to timeout if animation info unavailable
				yield return new WaitForSeconds(disappearTimeout);
			}
		}

		Debug.Log($"[TrackPoint] Disappear animation complete");
	}

	#region External Trigger Interface

	/// <summary>
	/// Public method to trigger disappear animation externally
	/// </summary>
	/// <param name="triggerParamName">Trigger parameter name</param>
	public void TriggerDisappearAnimation(string triggerParamName = null)
	{
		if (isDisappearing)
		{
			Debug.LogWarning("[TrackPoint] Disappear animation already playing, ignoring repeat trigger");
			return;
		}

		// Use specified param name or default
		string paramToUse = !string.IsNullOrEmpty(triggerParamName) ? triggerParamName : intoEyeTriggerParam;

		Debug.Log($"[TrackPoint] External trigger for disappear animation, param: {paramToUse}");

		// Start disappear sequence
		StartCoroutine(DisappearSequence());
	}

	#endregion

	private IEnumerator WaitForTransitionToDisappear()
	{
		float startTime = Time.time;
		float timeout = 1.0f;
		string lastClipName = "";

		while (Time.time - startTime < timeout)
		{
			var currentClips = animator.GetCurrentAnimatorClipInfo(disappearLayerIndex);
			if (currentClips.Length > 0)
			{
				string currentClipName = currentClips[0].clip.name;

				// If animation clip changed, transition is complete
				if (currentClipName != lastClipName)
				{
					lastClipName = currentClipName;

					// If switched to disappear animation, transition complete
					if (currentClipName.Contains("Disappear") || currentClipName.Contains("disappear"))
					{
						Debug.Log($"[TrackPoint] State transition complete, disappear animation started");
						yield break;
					}
				}
			}

			yield return new WaitForSeconds(0.05f);
		}

		Debug.LogWarning($"[TrackPoint] State transition wait timeout");
	}

	#endregion

	#region Debug Methods

	[ContextMenu("Debug Animator States")]
	private void DebugAnimatorStates()
	{
		if (animator == null)
		{
			Debug.LogWarning("[TrackPoint] Animator component not found");
			return;
		}

		Debug.Log($"[TrackPoint] === Animator Debug Info ===");
		Debug.Log($"[TrackPoint] Total layers: {animator.layerCount}");
		Debug.Log($"[TrackPoint] Current layer index: {disappearLayerIndex}");
		Debug.Log($"[TrackPoint] IntoEye param name: {intoEyeTriggerParam}");

		// Check if IntoEye parameter exists
		bool hasIntoEyeParam = false;
		foreach (var param in animator.parameters)
		{
			if (param.name == intoEyeTriggerParam)
			{
				hasIntoEyeParam = true;
				Debug.Log($"[TrackPoint] Found parameter: {param.name} (type: {param.type})");
				break;
			}
		}

		if (!hasIntoEyeParam)
		{
			Debug.LogError($"[TrackPoint] Parameter not found: {intoEyeTriggerParam}");
		}

		// Show current state info
		var currentState = animator.GetCurrentAnimatorStateInfo(disappearLayerIndex);
		var currentClips = animator.GetCurrentAnimatorClipInfo(disappearLayerIndex);

		Debug.Log($"[TrackPoint] Current animation clip: {(currentClips.Length > 0 ? currentClips[0].clip.name : "none")}");
		Debug.Log($"[TrackPoint] Animation progress: {currentState.normalizedTime:F2}");
		Debug.Log($"[TrackPoint] =========================");
	}

	#endregion
}