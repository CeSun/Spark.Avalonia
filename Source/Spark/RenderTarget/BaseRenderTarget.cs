using Silk.NET.OpenGLES;

namespace Spark.RenderTarget;

public class BaseRenderTarget : IDisposable
{
    public uint FrameBufferObject;

    public int Width { private set; get; }

    public int Height { private set; get; }

    private GL? _gl;
    public void Dispose()
    {
        if (_gl == null) 
            return;
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl = null;
    }

    public virtual BaseRenderTarget Use(GL gl)
    {
        this._gl = gl;
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferObject);
        return this;
    }

    public virtual void Resize(int width, int height)
    {
        this.Width = width;
        this.Height = height;
    }
}
