using Silk.NET.OpenGLES;
using Spark.Avalonia.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Assets;

public class StaticMesh
{
    List<Element> Elements = new List<Element>();
}

public class Element
{
    public Material? Material;
    public List<Vertex> Vertics = new List<Vertex>();
    public List<uint> Indices = new List<uint>();
    public void SetupRender(GL gl)
    {

    }
}
public struct Vertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector3 Color;
    public Vector2 TexCoord;
}