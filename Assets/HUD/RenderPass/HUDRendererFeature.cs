using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HUD
{
    public class HUDRendererFeature : ScriptableRendererFeature
    {
        private HUDRenderPass renderPass;

        public override void Create()
        {
            name = "HUDRendererFeature";
            renderPass = new HUDRenderPass();
            renderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!Application.isPlaying) { return; }
            
            renderer.EnqueuePass(renderPass);
        }
    }

    public class HUDRenderPass : ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var updateList = HUDRenderManager.Instance.GetUpdateEntryList();
            if (updateList.Count == 0)
            {
                return;
            }

            for (int i = updateList.Count - 1; i >= 0; i--)
            {
                var entry = updateList[i];
                if (entry.enabled)
                {
                    entry.func(context, renderingData.cameraData.camera);
                }
                else
                {
                    // func内部只允许进行移除标记，真正的移除要在最外层循环
                    updateList.RemoveAt(i);
                }
            }
        }
    }
}