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

    private GL? gl;
    public void Dispose()
    {
        if (gl != null)
        {
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            gl = null;
        }
    }

    public BaseRenderTarget Use(GL gl)
    {
        this.gl = gl;
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferObject);
        gl.Viewport(new Size(Width, Height));
        return this;
    }

}
