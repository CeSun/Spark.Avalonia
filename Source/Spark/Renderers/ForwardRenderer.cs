using Silk.NET.OpenGLES;
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
    public void Render(GL gl, CameraActor Camera)
    {
        using (Camera.RenderTarget.Use(gl))
        {
            Clear(gl, Camera);

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

    
    public void PreDepth(GL gl, CameraActor Camera)
    {

    }

    public void RenderSkybox(GL gl, CameraActor Camera)
    {

    }
}
