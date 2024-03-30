using Silk.NET.OpenGLES;
using Spark.Actors;
using Spark.Assets;

namespace Spark.Renderers;

public abstract class BaseRenderer
{
    public readonly List<ElementProxy> NeedRenderStaticMeshs = new();
    public readonly List<PointLightActor> PointLightActors = new();
    public readonly List<SpotLightActor> SpotLightActors = new();
    public readonly List<ElementProxy> OpaqueStaticMeshs = new();
    public readonly List<ElementProxy> MaskedStaticMeshs = new();
    public readonly List<ElementProxy> TranslucentStaticMeshs = new();
    public virtual void Initialize(GL gl)
    {
    }
    public virtual void Render(GL gl, CameraActor Camera)
    {
        Filter(Camera);
    }
    public virtual void Uninitialize(GL gl)
    {
    }

    private void Filter(CameraActor Camera)
    {
        NeedRenderStaticMeshs.Clear();
        PointLightActors.Clear();
        OpaqueStaticMeshs.Clear();
        MaskedStaticMeshs.Clear();
        TranslucentStaticMeshs.Clear();
        SpotLightActors.Clear();
        Camera.Engine.Octree.FrustumCulling(NeedRenderStaticMeshs, Camera.GetPlanes());
        Camera.Engine.Octree.FrustumCulling(PointLightActors, Camera.GetPlanes());
        Camera.Engine.Octree.FrustumCulling(SpotLightActors, Camera.GetPlanes());
        foreach (var proxy in NeedRenderStaticMeshs)
        {
            var element = proxy.Element;
            if (element.Material == null)
                continue;
            // 混合模式
            if (element.Material.BlendMode == BlendMode.Opaque)
                OpaqueStaticMeshs.Add(proxy);
            else if (element.Material.BlendMode == BlendMode.Masked)
                MaskedStaticMeshs.Add(proxy);
            else if (element.Material.BlendMode == BlendMode.Translucent)
                TranslucentStaticMeshs.Add(proxy);
        }
    }
}

public class RenderFeatures
{
    public bool PreZ { get; set; } = false;
}

public class GLDebugGroup : IDisposable
{
    public string GroupName;
    private GL? gl;
    public GLDebugGroup(string GroupName)
    {
        this.GroupName = GroupName;
    }

    public GLDebugGroup PushGroup(GL gl)
    {
        this.gl = gl;
        if (gl != null)
        {
            gl.PushGroup(GroupName);
        }
        return this;
    }
    public void Dispose()
    {
        if (gl != null)
        {
            gl.PopGroup();
        }
    }
}
public static class GLExternFunctions
{
    static bool SupportDebugGroup = true;
    public static void PushGroup(this GL gl, string DebugInfo)
    {
#if DEBUG
        if (SupportDebugGroup == false)
            return;
        try
        {
            gl.PushDebugGroup(DebugSource.DebugSourceApplication, 1, (uint)DebugInfo.Length, DebugInfo);
        }
        catch
        {
            SupportDebugGroup = false;
        }
#endif
    }

    public static void PopGroup(this GL gl)
    {
#if DEBUG
        if (SupportDebugGroup == false)
            return;
        try
        {
            gl.PopDebugGroup();
        }
        catch
        {
            SupportDebugGroup = false;
        }
#endif
    }
}