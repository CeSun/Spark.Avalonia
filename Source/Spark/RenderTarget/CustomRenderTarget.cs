using Silk.NET.OpenGLES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.RenderTarget;

public class CustomRenderTarget : BaseRenderTarget
{
    public CustomRenderTarget(int width, int height)
    {
        Resize(width, height);
    }

    public override void Resize(int width, int height)
    {
        base.Resize(width, height);
    }
    public void Resize(GL gl, int width, int height)
    {
        if (FrameBufferObject != 0)
        {
            gl.DeleteFramebuffer(FrameBufferObject);
        }
        FrameBufferObject = gl.GenFramebuffer();
    }

    public override BaseRenderTarget Use(GL gl)
    {
        Resize(gl, Width, Height);
        return base.Use(gl);
    }
}
