using Silk.NET.OpenGLES;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.RenderTarget;

public class BaseRenderTarget : IDisposable
{
    public uint FrameBufferObject;

    public int Width;

    public int Height;

    public void Dispose()
    {
    }

    public BaseRenderTarget Use(GL gl)
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferObject);
        gl.Viewport(new Size(Width, Height));
        return this;
    }

}
