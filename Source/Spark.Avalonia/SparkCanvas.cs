using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Silk.NET.OpenGLES;
using System.Diagnostics;

namespace Spark.Avalonia;

public class SparkCanvas : OpenGlControlBase
{
    Engine engine = new Engine();
    Stopwatch deltaTimeStopwatch = new Stopwatch();

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);
        deltaTimeStopwatch.Start();
    }
    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        deltaTimeStopwatch.Stop();
        float deltaTime = (float)deltaTimeStopwatch.Elapsed.TotalSeconds;
        deltaTimeStopwatch.Restart();
        engine.Update(deltaTime);
        engine.Render(GL.GetApi(gl.GetProcAddress));
        RequestNextFrameRendering();
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        base.OnOpenGlDeinit(gl);
    }
}
