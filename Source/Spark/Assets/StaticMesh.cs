using Jitter.LinearMath;
using Silk.NET.OpenGLES;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Spark.Assets;

public class StaticMesh
{
    public List<Element> Elements { get; private set; } = new List<Element>();
}

public class ElementProxy(Element element)
{
    public Element Element { get; private set; } = element;

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

    public List<Vector3> ConvexHull = [];

    public void SetupConvexHull()
    {
        ConvexHull.Clear();
        var vertices = new List<Vector3>();
        Vertices.ForEach(v => vertices.Add(v.Position));
        var list = JConvexHull.Build(vertices, JConvexHull.Approximation.Level10);
        foreach(var i in list)
        {
            ConvexHull.Add(vertices[i]);
        }
    }
    public void SetupBtn()
    {
        for (var i = 0; i < Indices.Count; i += 3)
        {
            var p1 = Vertices[(int)Indices[i]];
            var p2 = Vertices[(int)Indices[i + 1]];
            var p3 = Vertices[(int)Indices[i + 2]];

            var edge1 = p2.Position - p1.Position;
            var edge2 = p3.Position - p1.Position;
            var deltaUv1 = p2.TexCoord - p1.TexCoord;
            var deltaUv2 = p3.TexCoord - p1.TexCoord;

            var f = 1.0f / (deltaUv1.X * deltaUv2.Y - deltaUv2.X * deltaUv1.Y);

            Vector3 tangent1;
            Vector3 bitangent1;

            tangent1.X = f * (deltaUv2.Y * edge1.X - deltaUv1.Y * edge2.X);
            tangent1.Y = f * (deltaUv2.Y * edge1.Y - deltaUv1.Y * edge2.Y);
            tangent1.Z = f * (deltaUv2.Y * edge1.Z - deltaUv1.Y * edge2.Z);
            tangent1 = Vector3.Normalize(tangent1);

            bitangent1.X = f * (-deltaUv2.X * edge1.X + deltaUv1.X * edge2.X);
            bitangent1.Y = f * (-deltaUv2.X * edge1.Y + deltaUv1.X * edge2.Y);
            bitangent1.Z = f * (-deltaUv2.X * edge1.Z + deltaUv1.X * edge2.Z);
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