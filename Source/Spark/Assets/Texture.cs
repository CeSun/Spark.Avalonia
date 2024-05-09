using Silk.NET.OpenGLES;
using System.Runtime.InteropServices;

namespace Spark.Assets;

public enum TextureChannel
{
    Rgb,
    Rgba
}

public enum TextureFilter
{
    Nearest,
    Liner
}
public static class ChannelHelper
{
    public static GLEnum ToGlEnum(this TextureChannel channel)
    {
        return channel switch
        {
            TextureChannel.Rgb => GLEnum.Rgb,
            TextureChannel.Rgba => GLEnum.Rgba,
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
        var colorSpace = Channel.ToGlEnum();
        if (GammaCorrection)
        {
            colorSpace = colorSpace switch
            {
                GLEnum.Rgb => GLEnum.Srgb8,
                GLEnum.Rgba => GLEnum.Srgb8Alpha8,
                _ => GLEnum.Srgb8
            };
        }
        fixed (void* p = CollectionsMarshal.AsSpan(Data))
        {
            gl.TexImage2D(GLEnum.Texture2D, 0, (int)colorSpace, (uint)Width, (uint)Height, 0, Channel.ToGlEnum(), GLEnum.UnsignedByte, p);
        }
        gl.BindTexture(GLEnum.Texture2D, 0);
        Data.Clear();
    }
}

