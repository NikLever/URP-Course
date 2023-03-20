using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TintFeature : ScriptableRendererFeature
{
    class TintPass : ScriptableRenderPass
    {
        Material material;
        RTHandle cameraColorTarget;
        Color color;

        public TintPass(Material mat, RenderPassEvent renderEvent)
        {
            material = mat;
            renderPassEvent = renderEvent;
        }

        public void SetTarget(RTHandle colorHandle, Color col)
        {
            cameraColorTarget = colorHandle;
            color = col;
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
            
            material.SetColor("_Color", color);
            Blit(cmd, cameraColorTarget, cameraColorTarget, material, 0);
          
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }
    }

    public Shader shader;
    public Color color;
    public RenderPassEvent renderEvent;

    Material material;

    TintPass renderPass = null;

    public override void Create()
    {
        material = CoreUtils.CreateEngineMaterial(shader);
        renderPass = new TintPass(material, renderEvent);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer,
                                        in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            // Calling ConfigureInput with the ScriptableRenderPassInput.Color argument
            // ensures that the opaque texture is available to the Render Pass.
            renderPass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderPass.SetTarget(renderer.cameraColorTargetHandle, color);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                    ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(renderPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(material);
    }
}

