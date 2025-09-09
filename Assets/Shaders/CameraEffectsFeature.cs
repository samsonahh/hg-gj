using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraEffectsFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material fallbackMaterial;
        public RenderPassEvent injectionPoint = RenderPassEvent.AfterRendering;
        public bool applyInSceneView = false;
        public bool skipIfInactive = true;
    }

    class Pass : ScriptableRenderPass
    {
        private readonly Settings settings;
        private RTHandle tempColor;
        private const string ProfilerTag = "CameraEffectsPass";

        public Pass(Settings s) => settings = s;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData rd)
        {
            var desc = rd.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempColor, desc, name: "_CameraEffectsTemp");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData rd)
        {
            var cam = rd.cameraData.camera;
            if (!settings.applyInSceneView && rd.cameraData.isSceneViewCamera)
                return;

            Material mat = null;
            if (CameraEffects.TryGet(cam, out var controller))
            {
                if (settings.skipIfInactive && !controller.HasVisibleChange)
                    return;

                mat = controller.Material;
                if (mat != null)
                    controller.ApplyToMaterial(mat);
            }
            else
            {
                mat = settings.fallbackMaterial;
            }

            if (mat == null)
                return;

            var cmd = CommandBufferPool.Get(ProfilerTag);
            var src = rd.cameraData.renderer.cameraColorTargetHandle;

#if UNITY_600_0_OR_NEWER
            Blitter.BlitCameraTexture(cmd, src, tempColor, mat, 0);
            Blitter.BlitCameraTexture(cmd, tempColor, src);
#else
            Blit(cmd, src, tempColor, mat, 0);
            Blit(cmd, tempColor, src);
#endif

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }

        public void Dispose() => tempColor?.Release();
    }

    [SerializeField] private Settings settings = new();
    private Pass pass;

    public override void Create()
    {
        pass = new Pass(settings)
        {
            renderPassEvent = settings.injectionPoint
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData rd)
    {
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        pass?.Dispose();
    }
}