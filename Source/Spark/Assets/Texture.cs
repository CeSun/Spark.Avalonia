using Silk.NET.OpenGLES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Avalonia.Assets;

public enum TextureChannel
{
    RGB,
    RGBA
}
public class Texture
{
    public int Width;
    
    public int Height;

    public uint TextureId;

    public TextureChannel Channel;

    public List<byte> Data = new List<byte>();

    public void SetupRender(GL gl)
    {

    }
}

