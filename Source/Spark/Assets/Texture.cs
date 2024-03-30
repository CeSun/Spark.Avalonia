using Silk.NET.OpenGLES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Assets;

public enum TextureChannel
{
    RGB,
    RGBA
}

public static class ChannelHelper
{
    public static GLEnum ToGLEnum(this TextureChannel channel)
    {
        return channel switch
        {
            TextureChannel.RGB => GLEnum.Rgb,
            TextureChannel.RGBA => GLEnum.Rgba,
            _ => throw new NotImplementedException()
        };
    }

}

public class Texture
{
    public int Width;
    
    public int Height;

    public uint TextureId;

    public TextureChannel Channel;

    public List<byte> Data = new List<byte>();

    public bool GammaCorrection;
    public unsafe void SetupRender(GL gl)
    {
        if (TextureId > 0)
            return;
        TextureId = gl.GenTexture();
        gl.BindTexture(GLEnum.Texture2D, TextureId);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
        GLEnum ColorSpace = Channel.ToGLEnum();
        if (GammaCorrection == true)
        {
            ColorSpace = ColorSpace switch
            {
                GLEnum.Rgb => GLEnum.Srgb8,
                GLEnum.Rgba => GLEnum.Srgb8Alpha8,
                _ => GLEnum.Srgb8
            };
        }
        fixed (void* p = CollectionsMarshal.AsSpan(Data))
        {
            gl.TexImage2D(GLEnum.Texture2D, 0, (int)ColorSpace, (uint)Width, (uint)Height, 0, Channel.ToGLEnum(), GLEnum.UnsignedByte, p);
        }
        gl.BindTexture(GLEnum.Texture2D, 0);
        Data.Clear();
    }
}

