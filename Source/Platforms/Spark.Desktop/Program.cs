using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using Spark.Avalonia;
using Spark.Avalonia.Actors;
using Spark.Avalonia.Renderers;

var options = WindowOptions.Default with { API = new GraphicsAPI(ContextAPI.OpenGLES, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 0)) };
var window = Window.Create(options);

var Engine = new Engine();
GL? gl = null;

var camera1 = Engine.CreateActor<CameraActor>();

window.Resize += size =>
{
    Engine.DefaultRenderTarget.Width = size.X;
    Engine.DefaultRenderTarget.Height = size.Y;
};
window.Load += () =>
{
    Engine.DefaultRenderTarget.Width = window.Size.X;
    Engine.DefaultRenderTarget.Height = window.Size.Y;
    gl = GL.GetApi(window);
};
window.Render += deltaTime =>
{
    Engine.Render(gl!);
};

window.Update += deltaTime =>
{
    Engine.Update((float)deltaTime);
};

window.Closing += () =>
{
    
};
window.Run();