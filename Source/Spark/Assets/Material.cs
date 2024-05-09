using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Assets;

public enum BlendMode
{
    Opaque,
    Masked,
    Translucent
}

public class Material
{
    public BlendMode BlendMode;

    private readonly Texture?[] _textures = new Texture?[10];
    public Texture? BaseColor { get => _textures[0]; set => _textures[0] = value; }
    public Texture? Normal { get => _textures[1]; set => _textures[1] = value; }
    public Texture? MetallicRoughness { get => _textures[2]; set => _textures[2] = value; }
}
