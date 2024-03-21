using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using Spark.Actors;
using Spark.Assets;
using Spark.Avalonia;
using Spark.Avalonia.Actors;
using Spark.Importer;
using System.Numerics;

var options = WindowOptions.Default with { 
    API = new GraphicsAPI(ContextAPI.OpenGLES, new APIVersion(3, 0)),
    FramesPerSecond = 0,
    UpdatesPerSecond = 0,
    VSync = false,
    ShouldSwapAutomatically = true
};
var window = Window.Create(options);

var Engine = new Engine();
GL? gl = null;
StaticMesh mesh = new StaticMesh();
using (var sr = new StreamReader("E:\\Spark.Engine\\Source\\Platform\\Resource\\Content\\StaticMesh\\Jason.glb"))
{
    mesh = Engine.ImportStaticMeshFromGLB(sr.BaseStream);
}
var sma = Engine.CreateActor<StaticMeshActor>();
sma.StaticMesh = mesh;
var camera1 = Engine.CreateActor<CameraActor>();
sma.Position = camera1.ForwardVector * 20 + camera1.RightVector * 0 + camera1.UpVector * 0;
sma.Scale = new Vector3(0.1f);
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
    Engine.Initialize(gl);
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
    Engine.Uninitialize(gl!);
};
window.Run();