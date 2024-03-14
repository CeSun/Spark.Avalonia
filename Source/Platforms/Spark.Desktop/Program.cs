using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using Spark.Avalonia;
using Spark.Avalonia.Renderers;

var options = WindowOptions.Default with { API = new GraphicsAPI(ContextAPI.OpenGLES, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 0)) };
var window = Window.Create(options);

window.Run();
var engine = new Engine();
var renderer = new BaseRenderer(engine);
GL? gl = null;


window.Resize += size =>
{
    engine.DefaultRenderTarget.Width = size.X;
    engine.DefaultRenderTarget.Height = size.Y;
};
window.Load += () =>
{
    engine.DefaultRenderTarget.Width = window.Size.X;
    engine.DefaultRenderTarget.Height = window.Size.Y;
    gl = GL.GetApi(window);
};
window.Render += deltaTime =>
{
    engine.Render(gl!);
};

window.Update += deltaTime =>
{
    engine.Update((float)deltaTime);
};

window.Closing += () =>
{
    
};