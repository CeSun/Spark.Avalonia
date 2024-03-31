using Spark.Assets;
using StbImageSharp;
using SharpGLTF.Schema2;
using Texture = Spark.Assets.Texture;
using Silk.NET.OpenGLES;
using System.Numerics;
using Material = Spark.Assets.Material;
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
                var Color = mesh.GetVertexAccessor("COLOR_0")?.AsColorArray();
                for (var i = 0; i < Position.Count; i++)
                {
                    Vertex vertex = new Vertex() 
                    {
                        Position = Position[i],
                        TexCoord = TexCoord[i],
                        Normal = Normal[i],
                    };
                    if (Color != null)
                    {
                        vertex.Color = new Vector3(Color[i].X, Color[i].Y, Color[i].Z);
                    }

                    element.Vertices.Add(vertex);
                }
                // ebo
                element.Indices.AddRange(mesh.IndexAccessor.AsIndicesArray());
                // 材质
                if (mesh.Material != null)
                {
                    element.Material = new Material()
                    {
                        BlendMode = mesh.Material.Alpha switch
                        {
                            AlphaMode.OPAQUE => BlendMode.Opaque,
                            AlphaMode.MASK => BlendMode.Masked,
                            AlphaMode.BLEND => BlendMode.Translucent,
                            _ => BlendMode.Opaque,
                        }
                    };
                    var channel = mesh.Material.FindChannel("BaseColor");
                    if (channel != null && channel.Value.Texture != null)
                    {
                        element.Material.BaseColor = engine.ImportTextureFromMemory(channel.Value.Texture.PrimaryImage.Content.Content.ToArray());
                        element.Material.BaseColor.GammaCorrection = true;
                    }
                    else if (channel != null)
                    {
                        ;
                        for (int i = 0; i < element.Vertices.Count; i++)
                        {
                            element.Vertices[i] = element.Vertices[i] with
                            {
                                Color = new Vector3(channel.Value.Color.X, channel.Value.Color.Y, channel.Value.Color.Z)
                            };
                        }
                    }
                    channel = mesh.Material.FindChannel("Normal");
                    if (channel != null && channel.Value.Texture != null)
                    {
                        element.Material.Normal = engine.ImportTextureFromMemory(channel.Value.Texture.PrimaryImage.Content.Content.ToArray());
                    }
                    channel = mesh.Material.FindChannel("MetallicRoughness");
                    if (channel != null && channel.Value.Texture != null)
                    {
                        element.Material.MetallicRoughness = engine.ImportTextureFromMemory(channel.Value.Texture.PrimaryImage.Content.Content.ToArray());
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
