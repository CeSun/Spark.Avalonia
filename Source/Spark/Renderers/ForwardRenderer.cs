using Silk.NET.OpenGLES;
using Spark.Actors;
using Spark.Assets;
using Spark.Avalonia;
using Spark.Avalonia.Actors;
using Spark.Avalonia.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Renderers;

public class ForwardRenderer : IRenderer
{
    List<Element> NeedRenderStaticMeshs = new List<Element>();
    List<Element> OpaqueStaticMeshs = new List<Element>();
    List<Element> MaskedStaticMeshs = new List<Element>();
    List<Element> TranslucentStaticMeshs = new List<Element>();
    
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
        foreach(var  element in NeedRenderStaticMeshs)
        {
            if (element.Material == null)
                continue;
            if (element.Material.BlendMode == Avalonia.Assets.BlendMode.Opaque)
            {
                OpaqueStaticMeshs.Add(element);
            }
            else if (element.Material.BlendMode == Avalonia.Assets.BlendMode.Masked)
            {
                MaskedStaticMeshs.Add(element);
            }
            else if (element.Material.BlendMode == Avalonia.Assets.BlendMode.Translucent)
            {
                TranslucentStaticMeshs.Add(element);
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
    public void PreZPass(GL gl, CameraActor Camera)
    {

    }

    public void RenderSkybox(GL gl, CameraActor Camera)
    {

    }
}
