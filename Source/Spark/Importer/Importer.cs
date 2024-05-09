using Spark.Assets;
using StbImageSharp;
using SharpGLTF.Schema2;
using Texture = Spark.Assets.Texture;
using System.Numerics;
using Material = Spark.Assets.Material;

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
        var texture = new Texture
        {
            Width = result.Width,
            Height = result.Height,
            Channel = result.Comp switch
            {
                ColorComponents.RedGreenBlue => TextureChannel.Rgb,
                ColorComponents.RedGreenBlueAlpha => TextureChannel.Rgba,
                _ => throw new TextureChannelNotSupportException()
            }
        };
        texture.Data.AddRange(result.Data);
        return texture;
    }

    public static StaticMesh ImportStaticMeshFromGlb(this Engine engine, Stream stream)
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
                var position = mesh.GetVertexAccessor("POSITION").AsVector3Array();
                var normal = mesh.GetVertexAccessor("NORMAL").AsVector3Array();
                var texCoord = mesh.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
                var color = mesh.GetVertexAccessor("COLOR_0")?.AsColorArray();
                for (var i = 0; i < position.Count; i++)
                {
                    var vertex = new Vertex() 
                    {
                        Position = position[i],
                        TexCoord = texCoord[i],
                        Normal = normal[i],
                    };
                    if (color != null)
                    {
                        vertex.Color = new Vector3(color[i].X, color[i].Y, color[i].Z);
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
                        for (var i = 0; i < element.Vertices.Count; i++)
                        {
                            element.Vertices[i] = element.Vertices[i] with
                            {
                                Color = new Vector3(channel.Value.Color.X, channel.Value.Color.Y, channel.Value.Color.Z)
                            };
                        }
                    }
                    channel = mesh.Material.FindChannel("Normal");
                    if (channel is { Texture: not null })
                    {
                        element.Material.Normal = engine.ImportTextureFromMemory(channel.Value.Texture.PrimaryImage.Content.Content.ToArray());
                    }
                    channel = mesh.Material.FindChannel("MetallicRoughness");
                    if (channel != null && channel.Value.Texture != null)
                    {
                        element.Material.MetallicRoughness = engine.ImportTextureFromMemory(channel.Value.Texture.PrimaryImage.Content.Content.ToArray());
                    }
                }
                element.SetupBtn();
                element.SetupConvexHull();
                engine.AddRenderTask(element.SetupRender);
                staticMesh.Elements.Add(element);
            }
            
        }
        return staticMesh;
    }

}
