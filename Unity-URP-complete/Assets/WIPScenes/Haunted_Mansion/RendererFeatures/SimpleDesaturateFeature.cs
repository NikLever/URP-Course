//    Copyright (C) 2020 Ned Makes Games

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program. If not, see <https://www.gnu.org/licenses/>.

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SimpleDesaturateFeature : ScriptableRendererFeature
{
    class RenderPass : ScriptableRenderPass
    {
        private string profilingName;
        private Material material;
        private RTHandle sourceHandle;
        private readonly RTHandle tempTextureHandle;

        public RenderPass(string profilingName, Material material) : base()
        {
            this.profilingName = profilingName;
            this.material = material;
            tempTextureHandle = RTHandles.Alloc("_TempDesaturateTexture", name: "_TempDesaturateTexture");
        }

        public void SetSource(RTHandle source)
        {
            this.sourceHandle = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilingName);

            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDesc.depthBufferBits = 0;

            cmd.GetTemporaryRT(Shader.PropertyToID(tempTextureHandle.name), cameraTextureDesc, FilterMode.Bilinear);
            Blit(cmd, sourceHandle, tempTextureHandle, material, 0);
            Blit(cmd, tempTextureHandle, sourceHandle);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(tempTextureHandle.name));
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material material;
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    [SerializeField]
    private Settings settings = new Settings();

    private RenderPass renderPass;

    public override void Create()
    {
        this.renderPass = new RenderPass(name, settings.material);
        renderPass.renderPassEvent = settings.renderEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        renderPass.SetSource(renderer.cameraColorTargetHandle);  // use of target after allocation
    }
}
