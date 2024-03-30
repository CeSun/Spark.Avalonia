using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Avalonia.Assets;

public enum BlendMode
{
    Opaque,
    Masked,
    Translucent
}

public class Material
{
    public BlendMode BlendMode;
    Texture?[] Textures = new Texture?[10];
    public Texture? BaseColor { get => Textures[0]; set => Textures[0] = value; }
    public Texture? Normal { get => Textures[1]; set => Textures[1] = value; }
    public Texture? MetallicRoughness { get => Textures[2]; set => Textures[2] = value; }
}
