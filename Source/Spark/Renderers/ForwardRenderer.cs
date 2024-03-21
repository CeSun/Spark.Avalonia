using Silk.NET.OpenGLES;
using Spark.Assets;
using Spark.Avalonia.Actors;
using Spark.Avalonia.Renderers;
using System.Xml.Linq;

namespace Spark.Renderers;

public class ForwardRenderer : IRenderer
{
    List<ElementProxy> NeedRenderStaticMeshs = new List<ElementProxy>();
    List<ElementProxy> OpaqueStaticMeshs = new List<ElementProxy>();
    List<ElementProxy> MaskedStaticMeshs = new List<ElementProxy>();
    List<ElementProxy> TranslucentStaticMeshs = new List<ElementProxy>();
    
    Shader? PreZMaskedShader = null;
    Shader? PreZOpaqueShader = null;
#if DEBUG
    GLDebugGroup PreZGroup = new GLDebugGroup("PreZ Pass");
    GLDebugGroup OpaqueGroup = new GLDebugGroup("Opaque Pass");
    GLDebugGroup MaskedGroup = new GLDebugGroup("Masked Pass");
#endif
    public void Render(GL gl, CameraActor Camera)
    {
        LightShadowMapPass(gl);
        using (Camera.RenderTarget.Use(gl))
        {
            NeedRenderStaticMeshs.Clear();
            Camera.Engine.Octree.FrustumCulling(NeedRenderStaticMeshs, Camera.GetPlanes());
            Filter();
            Clear(gl, Camera);
            PreZPass(gl, Camera);
        }
    }

    private void Filter()
    {
        OpaqueStaticMeshs.Clear();
        MaskedStaticMeshs.Clear();
        OpaqueStaticMeshs.Clear();
        foreach (var proxy in NeedRenderStaticMeshs)
        {
            var element = proxy.Element;
            if (element.Material == null)
                continue;
            if (element.Material.BlendMode == Avalonia.Assets.BlendMode.Opaque)
            {
                OpaqueStaticMeshs.Add(proxy);
            }
            else if (element.Material.BlendMode == Avalonia.Assets.BlendMode.Masked)
            {
                MaskedStaticMeshs.Add(proxy);
            }
            else if (element.Material.BlendMode == Avalonia.Assets.BlendMode.Translucent)
            {
                TranslucentStaticMeshs.Add(proxy);
            }
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
