using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

class BlitPass : ScriptableRenderPass
{
    Material material;
    RTHandle cameraColorTarget;

    public BlitPass(Material material, RenderPassEvent renderPassEvent)
    {
        this.material = material;
        this.renderPassEvent = renderPassEvent;
    }

    public void SetTarget(RTHandle colorHandle)
    {
        cameraColorTarget = colorHandle;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureTarget(cameraColorTarget);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cameraData = renderingData.cameraData;
        if (cameraData.camera.cameraType != CameraType.Game)
            return;

        if (material == null)
            return;

        CommandBuffer cmd = CommandBufferPool.Get();

        Blitter.BlitCameraTexture(cmd, cameraColorTarget, cameraColorTarget, material, 0);
     
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }
}

internal class BlitRendererFeature : ScriptableRendererFeature
{
    public Material material;
    public RenderPassEvent renderPassEvent;

    BlitPass renderPass = null;

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                    ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(renderPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer,
                                        in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderPass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderPass.SetTarget(renderer.cameraColorTargetHandle);
        }
    }

    public override void Create()
    {
        renderPass = new BlitPass(material, renderPassEvent);
    }
}
