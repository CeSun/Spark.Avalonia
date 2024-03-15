using Spark.Assets;
using Spark.Avalonia;
using Spark.Avalonia.Assets;
using StbImageSharp;
using SharpGLTF.Schema2;
using Texture = Spark.Avalonia.Assets.Texture;
using Silk.NET.OpenGLES;

namespace Spark.Importer;

public class TextureChannelNotSupportException : Exception
{

}
public static class Importer
{
    public static Texture ImportTexture(this Engine engine, Stream stream)
    {
        var result = ImageResult.FromStream(stream);
        var texture = ImportTexture(result);
        engine.RenderMethods.Add(texture.SetupRender);
        return texture;
    }

    public static Texture ImportTextureFromMemory(this Engine engine, byte[] bytes)
    {
        var result = ImageResult.FromMemory(bytes);
        var texture = ImportTexture(result);
        engine.RenderMethods.Add(texture.SetupRender);
        return texture;
    }

    private static Texture ImportTexture(ImageResult result)
    {
        var texture = new Texture();
        texture.Width = result.Width;
        texture.Height = result.Height;
        texture.Channel = result.Comp switch
        {
            ColorComponents.RedGreenBlue => TextureChannel.RGB,
            ColorComponents.RedGreenBlueAlpha => TextureChannel.RGBA,
            _ => throw new TextureChannelNotSupportException()
        };
        texture.Data.AddRange(result.Data);
        return texture;
    }

    public static StaticMesh ImportStaticMeshFromGLB(this Engine engine, Stream stream)
    {
        var model = ModelRoot.ReadGLB(stream);
        return engine.ImportStaticMeshFromGltfModel(model);
    }

    private static StaticMesh ImportStaticMeshFromGltfModel(this Engine engine, ModelRoot model)
    {
        var staticMesh = new StaticMesh();
        foreach(var glMesh in model.LogicalMeshes)
        {
            foreach(var primitive in glMesh.Primitives)
            {
                primitive.GetVertexAccessor("");
            }
        }

        return staticMesh;
    }

}
