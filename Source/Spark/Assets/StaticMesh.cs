using Jitter.LinearMath;
using Silk.NET.OpenGLES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Assets;

public class StaticMesh
{
    public List<Element> Elements { get; private set; } = new List<Element>();
}

public class ElementProxy
{
    public ElementProxy(Element element)
    {
        this.Element = element;
    }
    public Element Element { get; private set; }

    public Matrix4x4 ModelTransform;

}
public class Element
{
    public uint VertexArrayObjectIndex {  get; private set; }
    public uint VertexBufferObjectIndex { get; private set; }
    public uint ElementBufferObjectIndex { get; private set; }
    public int IndicesCount { get; private set; }

    public Material? Material;
    public List<Vertex> Vertices { get; private set; } = new List<Vertex>();
    public List<uint> Indices { get; private set; } = new List<uint>();
    public unsafe void SetupRender(GL gl)
    {
        if (VertexArrayObjectIndex != 0)
            return;
        VertexArrayObjectIndex = gl.GenVertexArray();
        VertexBufferObjectIndex = gl.GenBuffer();
        ElementBufferObjectIndex = gl.GenBuffer();
        gl.BindVertexArray(VertexArrayObjectIndex);
        gl.BindBuffer(GLEnum.ArrayBuffer, VertexBufferObjectIndex);
        fixed (Vertex* p = CollectionsMarshal.AsSpan(Vertices))
        {
            gl.BufferData(GLEnum.ArrayBuffer, (nuint)(Vertices.Count * sizeof(Vertex)), p, GLEnum.StaticDraw);
        }
        gl.BindBuffer(GLEnum.ElementArrayBuffer, ElementBufferObjectIndex);
        fixed (uint* p = CollectionsMarshal.AsSpan(Indices))
        {
            gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)(Indices.Count * sizeof(uint)), p, GLEnum.StaticDraw);
        }

        // Location
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)0);
        // Normal
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 3, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)sizeof(Vector3));

        // Tangent
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 3, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(2 * sizeof(Vector3)));

        // BitTangent
        gl.EnableVertexAttribArray(3);
        gl.VertexAttribPointer(3, 3, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(3 * sizeof(Vector3)));

        // Color
        gl.EnableVertexAttribArray(4);
        gl.VertexAttribPointer(4, 3, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(4 * sizeof(Vector3)));
        // TexCoord
        gl.EnableVertexAttribArray(5);
        gl.VertexAttribPointer(5, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(5 * sizeof(Vector3)));
        gl.BindVertexArray(0);
        IndicesCount = Indices.Count;
        Vertices.Clear();
        Indices.Clear();
    }

    public List<Vector3> ConvexHull = new List<Vector3>();

    public void SetupConvexHull()
    {
        ConvexHull.Clear();
        List<Vector3> vertices = new List<Vector3>();
        Vertices.ForEach(V => vertices.Add(V.Position));
        var list = JConvexHull.Build(vertices, JConvexHull.Approximation.Level10);
        foreach(var i in list)
        {
            ConvexHull.Add(vertices[i]);
        }
    }
    public void SetupBTN()
    {
        for (int i = 0; i < Indices.Count; i += 3)
        {
            var p1 = Vertices[(int)Indices[i]];
            var p2 = Vertices[(int)Indices[i + 1]];
            var p3 = Vertices[(int)Indices[i + 2]];

            Vector3 Edge1 = p2.Position - p1.Position;
            Vector3 Edge2 = p3.Position - p1.Position;
            Vector2 DeltaUV1 = p2.TexCoord - p1.TexCoord;
            Vector2 DeltaUV2 = p3.TexCoord - p1.TexCoord;

            float f = 1.0f / (DeltaUV1.X * DeltaUV2.Y - DeltaUV2.X * DeltaUV1.Y);

            Vector3 tangent1;
            Vector3 bitangent1;

            tangent1.X = f * (DeltaUV2.Y * Edge1.X - DeltaUV1.Y * Edge2.X);
            tangent1.Y = f * (DeltaUV2.Y * Edge1.Y - DeltaUV1.Y * Edge2.Y);
            tangent1.Z = f * (DeltaUV2.Y * Edge1.Z - DeltaUV1.Y * Edge2.Z);
            tangent1 = Vector3.Normalize(tangent1);

            bitangent1.X = f * (-DeltaUV2.X * Edge1.X + DeltaUV1.X * Edge2.X);
            bitangent1.Y = f * (-DeltaUV2.X * Edge1.Y + DeltaUV1.X * Edge2.Y);
            bitangent1.Z = f * (-DeltaUV2.X * Edge1.Z + DeltaUV1.X * Edge2.Z);
            bitangent1 = Vector3.Normalize(bitangent1);

            p1.Tangent = tangent1;
            p2.Tangent = tangent1;
            p3.Tangent = tangent1;


            p1.BitTangent = bitangent1;
            p2.BitTangent = bitangent1;
            p3.BitTangent = bitangent1;

            Vertices[(int)Indices[i]] = p1;
            Vertices[(int)Indices[i + 1]] = p2;
            Vertices[(int)Indices[i + 2]] = p3;
        }
    }
}
public struct Vertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector3 Tangent;
    public Vector3 BitTangent;
    public Vector3 Color;
    public Vector2 TexCoord;
}