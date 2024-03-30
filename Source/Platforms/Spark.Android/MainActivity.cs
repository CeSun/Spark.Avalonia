using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl.Android;
using Spark;
using Spark.Actors;
using System.Drawing;
namespace Spark.Android;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : SilkActivity
{
    protected override void OnRun()
    {

        var options = ViewOptions.Default;
        options.API = new GraphicsAPI(ContextAPI.OpenGLES, ContextProfile.Core, ContextFlags.Default, new APIVersion(3, 0));
        var view = Silk.NET.Windowing.Window.GetView(options);

        var Engine = new Engine();


        var camera1 = Engine.CreateActor<CameraActor>();
        camera1.ClearColor = Color.LightGray;
        GL? gl = null;
        view.Resize += size =>
        {
            Engine.DefaultRenderTarget.Resize(size.X, size.Y);
        };
        view.Load += () =>
        {
            Engine.DefaultRenderTarget.Resize(view.Size.X, view.Size.Y);
            gl = GL.GetApi(view);
            Engine.Initialize(gl);
        };
        view.Render += deltaTime =>
        {
            Engine.Render(gl!);
        };

        view.Update += deltaTime =>
        {
            Engine.Update((float)deltaTime);
        };

        view.Closing += () =>
        {
            Engine.Uninitialize(gl!);
        };
        view.Run();

        view.Run();
    }
}