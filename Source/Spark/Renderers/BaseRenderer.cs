using Silk.NET.OpenGLES;
using Spark.Avalonia.Actors;

namespace Spark.Avalonia.Renderers;

public interface IRenderer
{
    void Initialize(GL gl);
    void Render(GL gl, CameraActor Camera);
    void Uninitialize(GL gl);

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