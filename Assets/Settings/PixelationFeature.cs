using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelationFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class PixelationSettings
    {
        [Range(1, 32)]
        public int pixelSize = 1;
        public Material pixelationMaterial;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public PixelationSettings settings = new PixelationSettings();

    class PixelationPass : ScriptableRenderPass
    {
        private Material material;
        private int pixelSize;

        public PixelationPass(Material mat)
        {
            material = mat;
        }

        public void SetPixelSize(int size) => pixelSize = size;

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!Application.isPlaying)
                return;
            if (renderingData.cameraData.isSceneViewCamera)
                return;

            var cmd = CommandBufferPool.Get("PixelationPass");

            // Use cameraColorTargetHandle as per deprecation warning
            var cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            int tempRTId = Shader.PropertyToID("_TempPixelationRT");
            cmd.GetTemporaryRT(tempRTId, desc.width, desc.height, 0, FilterMode.Point, desc.graphicsFormat);

            bool validMaterial = material != null && material.shader != null && material.shader.isSupported;

            if (validMaterial && pixelSize > 1)
            {
                material.SetFloat("_PixelSize", pixelSize);
                cmd.Blit(cameraColorTargetHandle, tempRTId, material, 0);
                cmd.Blit(tempRTId, cameraColorTargetHandle);
            }
            else
            {
                cmd.Blit(cameraColorTargetHandle, tempRTId);
                cmd.Blit(tempRTId, cameraColorTargetHandle);
            }

            cmd.ReleaseTemporaryRT(tempRTId);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    private PixelationPass pass;

    public override void Create()
    {
        pass = new PixelationPass(settings.pixelationMaterial)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.isSceneViewCamera)
            return;

        var stack = VolumeManager.instance.stack;
        var volume = stack.GetComponent<PixelationVolume>();
        if (volume != null && volume.IsActive())
            pass.SetPixelSize(volume.pixelSize.value);
        else
            pass.SetPixelSize(1);

        renderer.EnqueuePass(pass);
    }
}