using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl.Android;
using Spark.Actors;
using Spark.Assets;
using Spark.Avalonia;
using Spark.Avalonia.Actors;
using System.Drawing;
using Spark.Importer;
using Android.Content;

namespace Spark.Android
{
    [Activity(Label = "Lanucher", MainLauncher = true)]
    public class MainActivity : SilkActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
        }

        protected override void OnRun()
        {
            var options = ViewOptions.Default;
            options.API = new GraphicsAPI(ContextAPI.OpenGLES, ContextProfile.Core, ContextFlags.Default, new APIVersion(3, 0));

            using var window = Silk.NET.Windowing.Window.GetView(options);

            var Engine = new Engine();
            GL? gl = null;
                // 创建一个摄像机
            var camera1 = Engine.CreateActor<CameraActor>();
            camera1.ClearColor = Color.LightGray;
            // 创建并加载一个模型
            var sma = Engine.CreateActor<StaticMeshActor>();
            StaticMesh mesh = new StaticMesh();
            using (var sr = Assets.Open("Jason.glb"))
            {
                sma.StaticMesh = Engine.ImportStaticMeshFromGLB(sr);
                sma.StaticMesh.Elements.ForEach(element => element.Material.ShaderModel = Spark.Avalonia.Assets.ShaderModel.BlinnPhong);
            }
            sma.Position = camera1.ForwardVector * 50 + camera1.UpVector * -50;
            // 创建一个定向光源
            var light1 = Engine.CreateActor<SpotLightActor>();
            light1.LightColor = Color.LightPink;
            light1.InteriorAngle = 5;
            light1.ExteriorAngle = 10;
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
        }
    }
}