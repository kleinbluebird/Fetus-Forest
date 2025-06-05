using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using ProfilingScope = UnityEngine.Rendering.ProfilingScope;

public class ScreenSpaceOutlines : ScriptableRendererFeature
{
    [System.Serializable]
    private class ViewSpaceNormalsTextureSettings
    {
        public RenderTextureFormat colorFormat = RenderTextureFormat.ARGBHalf;
        public int depthBufferBits = 0;
        public Color backgroundColor = Color.black;
    }
    private class ViewSpaceNormalsTexturePass : ScriptableRenderPass
    {
        // ViewSpaceNormalsTexturePass variables
        private ViewSpaceNormalsTextureSettings normalsTextureSettings;
        private RTHandle normals;
        private readonly List<ShaderTagId> shaderTagIdList;
        private readonly Material normalsMaterial;
        private FilteringSettings filteringSettings;

        public ViewSpaceNormalsTexturePass(RenderPassEvent renderPassEvent, LayerMask outlineLayerMask, ViewSpaceNormalsTextureSettings settings, Material normalsMaterial)
        {
            normalsTextureSettings = settings;
            
            this.normalsMaterial = normalsMaterial;
            shaderTagIdList = new List<ShaderTagId>
            {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("LightweightForward"),
                new ShaderTagId("SRPDefaultUnlit")
            };
            this.renderPassEvent = renderPassEvent;
            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, outlineLayerMask);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // normalsTextureDescriptor setup
            RenderTextureDescriptor normalsTextureDescriptor = cameraTextureDescriptor;
            normalsTextureDescriptor.colorFormat = normalsTextureSettings.colorFormat;
            normalsTextureDescriptor.depthBufferBits = normalsTextureSettings.depthBufferBits;
            
            // 分配 RTHandle（带自动释放参数）
            normals = RTHandles.Alloc(normalsTextureDescriptor, name: "_SceneViewSpaceNormals");
            
            ConfigureTarget(normals); // 设置 RTHandle 作为输出目标
            
            ConfigureClear(ClearFlag.All, normalsTextureSettings.backgroundColor);
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if(!normalsMaterial)
                return;
            
            CommandBuffer cmd = CommandBufferPool.Get();
            
            using (new ProfilingScope(cmd, new ProfilingSampler("SceneViewSpaceNormalsTextureCreation")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                DrawingSettings drawingSettings = CreateDrawingSettings(
                    shaderTagIdList, ref renderingData,
                    renderingData.cameraData.defaultOpaqueSortFlags);
                
                drawingSettings.overrideMaterial = normalsMaterial;

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (normals != null)
            {
                normals.Release();
                normals = null;
            }
        }

        public RTHandle GetNormalsTexture()
        {
            return normals;
        }
    }
    
    private class ScreenSpaceOutlinePass : ScriptableRenderPass
    {
        private readonly Material screenSpaceOutlineMaterial;
        private RTHandle cameraColorTargetHandle;
        private RTHandle temporaryBuffer;
        private RTHandle normalsTexture;

        
        public ScreenSpaceOutlinePass(RenderPassEvent renderPassEvent, Material screenSpaceOutlineMaterial)
        {
            this.renderPassEvent = renderPassEvent;
            this.screenSpaceOutlineMaterial = screenSpaceOutlineMaterial;
        }

        public void Setup(RTHandle normals)
        {
            normalsTexture = normals;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // 获取 cameraColorTarget
            cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            // 创建临时 RTHandle 作为中间缓冲区
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            temporaryBuffer = RTHandles.Alloc(descriptor, name: "_TemporaryBuffer");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!screenSpaceOutlineMaterial)
                return;

            if (normalsTexture != null)
            {
                screenSpaceOutlineMaterial.SetTexture("_SceneViewSpaceNormals", normalsTexture);
            }
            
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("ScreenSpaceOutlines")))
            {
                // 从相机颜色缓冲 Blit 到临时缓冲
                Blit(cmd, cameraColorTargetHandle, temporaryBuffer);

                // 从临时缓冲 Blit 回相机颜色缓冲，应用 Outline Shader
                Blit(cmd, temporaryBuffer, cameraColorTargetHandle, screenSpaceOutlineMaterial);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (temporaryBuffer != null)
            {
                temporaryBuffer.Release();
                temporaryBuffer = null;
            }
        }
    }

    // ScreenSpaceOutlines variables and methods
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    [SerializeField] private ViewSpaceNormalsTextureSettings viewSpaceNormalsTextureSettings = new ViewSpaceNormalsTextureSettings();
    [SerializeField] private LayerMask outlinesLayerMask = ~0;
    
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Material normalsMaterial;
    
    
    private ViewSpaceNormalsTexturePass viewSpaceNormalsTexturePass;
    private ScreenSpaceOutlinePass screenSpaceOutlinePass;
    
    public override void Create()
    {
        viewSpaceNormalsTexturePass = new ViewSpaceNormalsTexturePass(renderPassEvent, outlinesLayerMask, viewSpaceNormalsTextureSettings, normalsMaterial);
        screenSpaceOutlinePass = new ScreenSpaceOutlinePass(renderPassEvent, outlineMaterial);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(viewSpaceNormalsTexturePass);
        RTHandle normals = viewSpaceNormalsTexturePass.GetNormalsTexture();
        screenSpaceOutlinePass.Setup(normals);
        renderer.EnqueuePass(screenSpaceOutlinePass);
    }
}
