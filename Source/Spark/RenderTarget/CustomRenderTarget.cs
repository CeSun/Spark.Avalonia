using SharpGLTF.Schema2;
using Silk.NET.OpenGLES;
using Spark.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spark.RenderTarget;


public class CustomRenderTarget : BaseRenderTarget
{
    public bool SizeDirty = true;
    public uint ColorId { get; private set; }
    public uint DepthId { get; private set; }
    public bool IsHdr {  get;  set; }
    public bool HasStencil { get; set; }

    public TextureFilter Filter { get; set; }
    public CustomRenderTarget(int width, int height)
    {
        Resize(width, height);
    }

    public override void Resize(int width, int height)
    {
        base.Resize(width, height);
        SizeDirty = true;
    }
    public unsafe void Resize(GL gl, int width, int height)
    {
        if (FrameBufferObject == 0)
        {
            FrameBufferObject = gl.GenFramebuffer();
        }
        gl.BindFramebuffer(GLEnum.Framebuffer, FrameBufferObject);
        if (ColorId != 0)
            gl.DeleteTexture(ColorId);
        if (DepthId != 0)
            gl.DeleteTexture(ColorId);
        ColorId = gl.GenTexture();
        gl.BindTexture(GLEnum.Texture2D, ColorId);
        if (IsHdr == false) 
            gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba8, (uint)width, (uint)height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, (void*)0);
        else
            gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba16f, (uint)width, (uint)height, 0, GLEnum.Rgba, GLEnum.HalfFloat, (void*)0);

        GLEnum glFilter = Filter switch 
        {
            TextureFilter.Liner => GLEnum.Linear,
            TextureFilter.Nearest => GLEnum.Nearest,
            _ => GLEnum.Nearest
        };
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)glFilter);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)glFilter);
        gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, ColorId, 0);

        DepthId = gl.GenTexture();
        gl.BindTexture(GLEnum.Texture2D, DepthId);
        if (HasStencil == false)
            gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.DepthComponent24, (uint)width, (uint)height, 0, GLEnum.DepthComponent, GLEnum.UnsignedInt, (void*)0);
        else
            gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Depth24Stencil8, (uint)width, (uint)height, 0, GLEnum.DepthComponent, GLEnum.UnsignedInt248, (void*)0);

        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
        gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.DepthAttachment, GLEnum.Texture2D, DepthId, 0);
        gl.DrawBuffers(new GLEnum[] { GLEnum.ColorAttachment0 });
        gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

    public override BaseRenderTarget Use(GL gl)
    {
        if (SizeDirty)
        {
            Resize(gl, Width, Height);
            SizeDirty = false;
        }
        return base.Use(gl);
    }
}
