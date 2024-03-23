using Spark.Assets;
using Spark.Avalonia;
using Spark.Avalonia.Assets;
using StbImageSharp;
using SharpGLTF.Schema2;
using Texture = Spark.Avalonia.Assets.Texture;
using Silk.NET.OpenGLES;
using System.Numerics;
using Material = Spark.Avalonia.Assets.Material;
using Spark.Actors;

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
        engine.AddRenderTask(texture.SetupRender);
        return texture;
    }

    public static Texture ImportTextureFromMemory(this Engine engine, byte[] bytes)
    {
        var result = ImageResult.FromMemory(bytes);
        var texture = ImportTexture(result);
        engine.AddRenderTask(texture.SetupRender);
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
        foreach(var glMeshes in model.LogicalMeshes)
        {
            foreach(var mesh in glMeshes.Primitives)
            {
                Element element = new Element();
                // 顶点
                var Position = mesh.GetVertexAccessor("POSITION").AsVector3Array();
                var Normal = mesh.GetVertexAccessor("NORMAL").AsVector3Array();
                var TexCoord = mesh.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
                var Color = mesh.GetVertexAccessor("COLOR_0").AsColorArray();
                for (var i = 0; i < Position.Count; i++)
                {
                    Vertex vertex = new Vertex() 
                    {
                        Position = Position[i],
                        TexCoord = TexCoord[i],
                        Normal = Normal[i],
                        Color = new Vector3(Color[i].X, Color[i].Y, Color[i].Z)
                    };

                    element.Vertices.Add(vertex);
                }
                // ebo
                element.Indices.AddRange(mesh.IndexAccessor.AsIndicesArray());
                // 材质
                if (mesh.Material != null)
                {
                    element.Material = new Material()
                    {
                        ShaderModel = ShaderModel.BlinnPhong,
                        BlendMode = mesh.Material.Alpha switch
                        {
                            AlphaMode.OPAQUE => BlendMode.Opaque,
                            AlphaMode.MASK => BlendMode.Masked,
                            AlphaMode.BLEND => BlendMode.Translucent,
                            _ => BlendMode.Opaque,
                        }
                    };
                    if (mesh.Material.Channels.Count() > 0)
                    {
                        element.Material.Diffuse = engine.ImportTextureFromMemory(mesh.Material.Channels.First().Texture.PrimaryImage.Content.Content.ToArray());
                        var channel = mesh.Material.FindChannel("Normal");
                        if (channel != null)
                        {
                            element.Material.Normal = engine.ImportTextureFromMemory(channel.Value.Texture.PrimaryImage.Content.Content.ToArray());
                        }
                    }
                }
                element.SetupBTN();
                element.SetupConvexHull();
                engine.AddRenderTask(element.SetupRender);
                staticMesh.Elements.Add(element);
            }
            
        }
        return staticMesh;
    }

}
