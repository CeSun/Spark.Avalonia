using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Silk.NET.OpenGLES;
using Spark.Avalonia.Actors;
using Spark.Avalonia.Renderers;
using System.Diagnostics;
using System.Drawing;

namespace Spark.Avalonia;

public class SparkCanvas : OpenGlControlBase
{
    public bool IsNeedFlushDefaultFBO = true;
    public SparkCanvas()
    {
        Engine = new Engine();
    }

    public static readonly RoutedEvent<RoutedEventArgs> BeginPlayEvent =
        RoutedEvent.Register<SparkCanvas, RoutedEventArgs>(nameof(SparkCanvas), RoutingStrategies.Direct);

    public event EventHandler<RoutedEventArgs> OnBeginPlay
    {
        add => AddHandler(BeginPlayEvent, value);
        remove => RemoveHandler(BeginPlayEvent, value);
    }
    public static readonly RoutedEvent<RoutedEventArgs> EndPlayEvent =
        RoutedEvent.Register<SparkCanvas, RoutedEventArgs>(nameof(SparkCanvas), RoutingStrategies.Direct);

    public event EventHandler<RoutedEventArgs> OnEndPlay
    {
        add => AddHandler(EndPlayEvent, value);
        remove => RemoveHandler(EndPlayEvent, value);
    }

    public Engine Engine { get; private set; }

    Stopwatch DeltaTimeStopwatch = new Stopwatch();

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);
        Engine.Initialize(GL.GetApi(gl.GetProcAddress));
        RoutedEventArgs args = new RoutedEventArgs(BeginPlayEvent);
        RaiseEvent(args);
        DeltaTimeStopwatch.Start();
    }
    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (IsNeedFlushDefaultFBO == true)
        {
            int fbo;
            gl.GetIntegerv((int)GLEnum.DrawFramebufferBinding, out fbo);
            Engine.DefaultRenderTarget.FrameBufferObject = (uint)fbo;
            Engine.DefaultRenderTarget.Width = (int)(Bounds.Width * VisualRoot!.RenderScaling);
            Engine.DefaultRenderTarget.Height = (int)(Bounds.Height * VisualRoot!.RenderScaling);
            IsNeedFlushDefaultFBO = false;
        }

        DeltaTimeStopwatch.Stop();
        float deltaTime = (float)DeltaTimeStopwatch.Elapsed.TotalSeconds;
        DeltaTimeStopwatch.Restart();
        Engine.Update(deltaTime);
        Engine.Render(GL.GetApi(gl.GetProcAddress));
        RequestNextFrameRendering();
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        base.OnOpenGlDeinit(gl);
        RoutedEventArgs args = new RoutedEventArgs(EndPlayEvent);
        RaiseEvent(args);
        Engine.Uninitialize(GL.GetApi(gl.GetProcAddress));
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        Engine.DefaultRenderTarget.Width = (int)(Bounds.Width * VisualRoot!.RenderScaling);
        Engine.DefaultRenderTarget.Height = (int)(Bounds.Height * VisualRoot!.RenderScaling);
    }
}
