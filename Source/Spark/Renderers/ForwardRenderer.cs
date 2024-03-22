using Silk.NET.OpenGLES;
using Spark.Assets;
using Spark.Avalonia.Actors;
using Spark.Avalonia.Renderers;
using System.Xml.Linq;

namespace Spark.Renderers;

public class ForwardRenderer : IRenderer
{
    RenderFeatures RenderFeatures;

    public ForwardRenderer(RenderFeatures renderFeatures)
    {
        this.RenderFeatures = renderFeatures;
    }

    readonly List<ElementProxy> NeedRenderStaticMeshs = new();
    readonly List<ElementProxy> OpaqueStaticMeshs = new();
    readonly List<ElementProxy> MaskedStaticMeshs = new();
    readonly List<ElementProxy> TranslucentStaticMeshs = new();

    readonly List<ElementProxy> LambertStaticMeshs = new();
    readonly List<ElementProxy> BlinnPhongStaticMeshs = new();


    Shader? PreZMaskedShader = null;
    Shader? PreZOpaqueShader = null;
#if DEBUG
    readonly GLDebugGroup PreZGroup = new("PreZ Pass");
    readonly GLDebugGroup OpaqueGroup = new("Opaque Pass");
    readonly GLDebugGroup MaskedGroup = new("Masked Pass");
#endif
    public void Render(GL gl, CameraActor Camera)
    {
        LightShadowMapPass(gl);
        using (Camera.RenderTarget.Use(gl))
        {
            Filter(Camera);
            Clear(gl, Camera);
            if (RenderFeatures.PreZ == true)
            {
                PreZPass(gl, Camera);
            }

        }
    }

    private void Filter(CameraActor Camera)
    {
        OpaqueStaticMeshs.Clear();
        MaskedStaticMeshs.Clear();
        OpaqueStaticMeshs.Clear();
        LambertStaticMeshs.Clear();
        BlinnPhongStaticMeshs.Clear();
        NeedRenderStaticMeshs.Clear();
        Camera.Engine.Octree.FrustumCulling(NeedRenderStaticMeshs, Camera.GetPlanes());
        foreach (var proxy in NeedRenderStaticMeshs)
        {
            var element = proxy.Element;
            if (element.Material == null)
                continue;

            // 混合模式
            if (element.Material.BlendMode == Avalonia.Assets.BlendMode.Opaque)
                OpaqueStaticMeshs.Add(proxy);
            else if (element.Material.BlendMode == Avalonia.Assets.BlendMode.Masked)
                MaskedStaticMeshs.Add(proxy);
            else if (element.Material.BlendMode == Avalonia.Assets.BlendMode.Translucent)
                TranslucentStaticMeshs.Add(proxy);

            // 光源模型
            if (element.Material.ShaderModel == Avalonia.Assets.ShaderModel.Lambert)
                LambertStaticMeshs.Add(proxy);
            else if (element.Material.ShaderModel == Avalonia.Assets.ShaderModel.BlinnPhong)
                BlinnPhongStaticMeshs.Add(proxy);
        }
    }

    private void Clear(GL gl, CameraActor Camera)
    {
        ClearBufferMask ClearFlag = ClearBufferMask.None;
        if ((Camera.ClearFlag & CameraClearFlag.DepthFlag) == CameraClearFlag.DepthFlag)
            ClearFlag |= ClearBufferMask.DepthBufferBit;
        if ((Camera.ClearFlag & CameraClearFlag.ColorFlag) == CameraClearFlag.ColorFlag)
        {
            gl.ClearColor(Camera.ClearColor);
            ClearFlag |= ClearBufferMask.ColorBufferBit;
        }
        if ((Camera.ClearFlag & CameraClearFlag.Skybox) == CameraClearFlag.Skybox)
            ClearFlag = ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit;
        gl.Clear(ClearFlag);
        if ((Camera.ClearFlag & CameraClearFlag.Skybox) == CameraClearFlag.Skybox)
            RenderSkybox(gl, Camera);
    }
    private void LightShadowMapPass(GL gl)
    {

    }
    public unsafe void PreZPass(GL gl, CameraActor Camera)
    {
#if DEBUG
        using(PreZGroup.PushGroup(gl))
#endif
        {
            gl.Disable(EnableCap.Blend);
            gl.Enable(EnableCap.DepthTest);
#if DEBUG
            using (OpaqueGroup.PushGroup(gl))
#endif
            { 
                using (PreZOpaqueShader!.Using(gl))
                {
                    PreZOpaqueShader.SetMatrix("Projection", Camera.ProjectTransform);
                    PreZOpaqueShader.SetMatrix("View", Camera.ViewTransform);
                    foreach (var proxy in OpaqueStaticMeshs)
                    {
                        PreZOpaqueShader.SetMatrix("Model", proxy.ModelTransform);
                        gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                        gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
                    }
                }
            }
#if DEBUG
            using (MaskedGroup.PushGroup(gl))
#endif
            {
                gl.Enable(EnableCap.AlphaTest);
                using (PreZMaskedShader!.Using(gl))
                {
                    PreZMaskedShader.SetMatrix("Projection", Camera.ProjectTransform);
                    PreZMaskedShader.SetMatrix("View", Camera.ViewTransform);
                    foreach (var proxy in MaskedStaticMeshs)
                    {
                        PreZMaskedShader.SetMatrix("Model", proxy.ModelTransform);
                        PreZMaskedShader.SetInt("BaseColor", 0);
                        gl.ActiveTexture(GLEnum.Texture0);
                        gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.Diffuse!.TextureId);
                        gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                        gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
                    }
                }
            }
        }
        

    }

    public void RenderSkybox(GL gl, CameraActor Camera)
    {

    }

    public void Initialize(GL gl)
    {
        PreZMaskedShader = gl.CreateShader("PreZ.vert", "PreZ.frag", new() { "_BLENDMODE_MASKED_" });
        PreZOpaqueShader = gl.CreateShader("PreZ.vert", "PreZ.frag");
    }
    public void Uninitialize(GL gl)
    {
    }
}
