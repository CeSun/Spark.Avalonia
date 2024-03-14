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

public enum ShaderModel
{
    Lambert,
    BlinnPhong
}
public class Material
{
    public BlendMode BlendMode;
    public ShaderModel ShaderModel;
    Texture[] Textures = new Texture[10];

}
