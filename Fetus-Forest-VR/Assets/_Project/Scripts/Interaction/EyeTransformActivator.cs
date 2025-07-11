using System;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EyeTransformActivator : MonoBehaviour
{
    [Header("VFX Settings")]
    public VisualEffect vfxGraph;
    public float particleSize = 5.0f;
    public float spawnRate = 3.0f;
    
    [Header("Gamma/Gain Control")]
    public Volume globalVolume;
    public float minGamma = -0.729f;
    public float maxGamma = -0.397f;
    public float minGain = -0.143f;
    public float maxGain = -0.246f;
    public float transitionDistance = 10f;

    private LiftGammaGain liftGammaGain;
    private Transform playerTransform;
    private float startX;
    private bool isActivated = false;
    private bool isRestoring = false;


    private void Start()
    {
        if (globalVolume != null && globalVolume.profile != null)
        {
            // 克隆运行时 Volume Profile 副本
            globalVolume.profile = Instantiate(globalVolume.profile);

            if (globalVolume.profile.TryGet(out liftGammaGain))
            {
                // 启用 override 状态
                liftGammaGain.gamma.overrideState = true;
                liftGammaGain.gain.overrideState = true;

                // 初始化 Gamma 和 Gain 的 W 分量
                Vector4 gInit = liftGammaGain.gamma.value;
                Vector4 gainInit = liftGammaGain.gain.value;

                liftGammaGain.gamma.Override(new Vector4(gInit.x, gInit.y, gInit.z, maxGamma));
                liftGammaGain.gain.Override(new Vector4(gainInit.x, gainInit.y, gainInit.z, maxGain));

                // Debug.Log($"[Start] Init Gamma W: {maxGamma}, Gain W: {maxGain}");
            }
            else
            {
                Debug.LogWarning("[Start] Failed to get LiftGammaGain from profile.");
            }
        }
        else
        {
            Debug.LogWarning("[Start] Global Volume not assigned.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // 需要确保玩家物体有 "Player" 标签
        {
            if (vfxGraph != null)
            {
                vfxGraph.SetBool("EnableTransform", true);
                vfxGraph.SetFloat("MinParticleSize",particleSize);
                vfxGraph.SetFloat("MaxParticleSize",particleSize);
                vfxGraph.SetFloat("SpawnRate", spawnRate);
                
                // Debug.Log("Flipbook Player Activated.");
            }
            
            if (globalVolume != null && globalVolume.profile.TryGet(out liftGammaGain))
            {
                playerTransform = other.transform;

                if (isRestoring)
                {
                    Debug.Log("[TriggerEnter] Detected re-entry, begin restoring gamma and gain");
                    StopAllCoroutines();
                    StartCoroutine(RestoreGammaAndGain());
                }
                else
                {
                    Debug.Log("[TriggerEnter] First entry, begin gradient based on movement");
                    startX = playerTransform.position.x;
                    isActivated = true;
                }
            }
            
        }
    }
    
    private void Update()
    {
        if (isActivated && playerTransform != null && liftGammaGain != null && !isRestoring)
        {
            float xDistance = Mathf.Clamp(playerTransform.position.x - startX, 0, transitionDistance);
            float t = xDistance / transitionDistance;

            float gammaW = Mathf.Lerp(maxGamma, minGamma, t);
            float gainW = Mathf.Lerp(maxGain, minGain, t);

            Vector4 currentGamma = liftGammaGain.gamma.value;
            Vector4 currentGain = liftGammaGain.gain.value;

            bool gammaChanged = Mathf.Abs(currentGamma.w - gammaW) > 0.01f;
            bool gainChanged = Mathf.Abs(currentGain.w - gainW) > 0.01f;

            if (gammaChanged)
            {
                liftGammaGain.gamma.Override(new Vector4(currentGamma.x, currentGamma.y, currentGamma.z, gammaW));
            }

            if (gainChanged)
            {
                liftGammaGain.gain.Override(new Vector4(currentGain.x, currentGain.y, currentGain.z, gainW));
            }

            // Debug 输出
            // Debug.Log($"[Update] xDistance: {xDistance:F2}, GammaW: {gammaW:F3}, GainW: {gainW:F3}");
        }
    }
    
    private System.Collections.IEnumerator RestoreGammaAndGain()
    {
        isRestoring = true;

        Vector4 currentGamma = liftGammaGain.gamma.value;
        Vector4 currentGain = liftGammaGain.gain.value;

        float duration = 5.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float gammaW = Mathf.Lerp(currentGamma.w, maxGamma, t);
            float gainW = Mathf.Lerp(currentGain.w, maxGain, t);

            liftGammaGain.gamma.Override(new Vector4(currentGamma.x, currentGamma.y, currentGamma.z, gammaW));
            liftGammaGain.gain.Override(new Vector4(currentGain.x, currentGain.y, currentGain.z, gainW));

            yield return null;
        }

        liftGammaGain.gamma.Override(new Vector4(currentGamma.x, currentGamma.y, currentGamma.z, maxGamma));
        liftGammaGain.gain.Override(new Vector4(currentGain.x, currentGain.y, currentGain.z, maxGain));

        Debug.Log("[RestoreGammaAndGain] Completed. Gamma & Gain reset to max.");
        isRestoring = false;
        isActivated = false;
    }
}
