using Silk.NET.OpenGLES;
using Spark.Actors;
using Spark.Assets;

namespace Spark.Renderers;

public abstract class BaseRenderer
{
    public readonly List<ElementProxy> NeedRenderStaticMeshes = new();
    public readonly List<PointLightActor> PointLightActors = new();
    public readonly List<SpotLightActor> SpotLightActors = new();
    public readonly List<ElementProxy> OpaqueStaticMeshes = new();
    public readonly List<ElementProxy> MaskedStaticMeshes = new();
    public readonly List<ElementProxy> TranslucentStaticMeshes = new();
    public virtual void Initialize(GL gl)
    {
    }
    public virtual void Render(GL gl, CameraActor camera)
    {
        Filter(camera);
    }
    public virtual void UnInitialize(GL gl)
    {
    }

    protected static unsafe (uint vao, uint vbo, uint ebo) CreateQuad(GL gl)
    {
        Span<float> vertices = [
            -1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
            -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
            1.0f, 1.0f, 0.0f, 1.0f, 1.0f,
            1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
        ];
        Span<int> indices = [0, 1, 2, 2, 1, 3];
        var vao = gl.GenVertexArray();
        var vbo = gl.GenBuffer();
        var ebo = gl.GenBuffer();
        gl.BindVertexArray(vao);
        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        fixed(void* p = vertices)
        {
            gl.BufferData(GLEnum.ArrayBuffer, (nuint)vertices.Length * sizeof(float), p, GLEnum.StaticDraw);
        }

        gl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);
        fixed (void* p = indices)
        {
            gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)indices.Length * sizeof(uint), p, GLEnum.StaticDraw);
        }

        // Location
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 5 * sizeof(float), (void*)0);
        // Texcoord
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, GLEnum.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
        gl.BindVertexArray(0);
        return (vao, vbo, ebo);
    }
    private void Filter(CameraActor camera)
    {
        NeedRenderStaticMeshes.Clear();
        PointLightActors.Clear();
        OpaqueStaticMeshes.Clear();
        MaskedStaticMeshes.Clear();
        TranslucentStaticMeshes.Clear();
        SpotLightActors.Clear();
        camera.Engine.Octree.FrustumCulling(NeedRenderStaticMeshes, camera.GetPlanes());
        camera.Engine.Octree.FrustumCulling(PointLightActors, camera.GetPlanes());
        camera.Engine.Octree.FrustumCulling(SpotLightActors, camera.GetPlanes());
        foreach (var proxy in NeedRenderStaticMeshes)
        {
            var element = proxy.Element;
            if (element.Material == null)
                continue;
            switch (element.Material.BlendMode)
            {
                // 混合模式
                case BlendMode.Opaque:
                    OpaqueStaticMeshes.Add(proxy);
                    break;
                case BlendMode.Masked:
                    MaskedStaticMeshes.Add(proxy);
                    break;
                case BlendMode.Translucent:
                    TranslucentStaticMeshes.Add(proxy);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

public class RenderFeatures
{
    public bool PreZ { get; set; } = false;
}

public class GlDebugGroup(string groupName) : IDisposable
{
    public string GroupName = groupName;
    private GL? _gl;

    public GlDebugGroup PushGroup(GL gl)
    {
        this._gl = gl;
        _gl.PushGroup(GroupName);
        return this;
    }
    public void Dispose()
    {
        _gl?.PopGroup();
    }
}
public static class GlExternFunctions
{
    private static bool _supportDebugGroup = true;
    public static void PushGroup(this GL gl, string debugInfo)
    {
#if DEBUG
        if (_supportDebugGroup == false)
            return;
        try
        {
            gl.PushDebugGroup(DebugSource.DebugSourceApplication, 1, (uint)debugInfo.Length, debugInfo);
        }
        catch
        {
            _supportDebugGroup = false;
        }
#endif
    }

    public static void PopGroup(this GL gl)
    {
#if DEBUG
        if (_supportDebugGroup == false)
            return;
        try
        {
            gl.PopDebugGroup();
        }
        catch
        {
            _supportDebugGroup = false;
        }
#endif
    }
}