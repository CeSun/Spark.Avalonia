﻿using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using Spark.Actors;
using Spark.Assets;
using Spark;
using Spark.Importer;
using System.Drawing;

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

// 创建一个摄像机
var camera1 = Engine.CreateActor<CameraActor>();
camera1.ClearColor = Color.LightGray;
// 创建并加载一个模型
var sma = Engine.CreateActor<StaticMeshActor>();
StaticMesh mesh = new StaticMesh();
using (var sr = new StreamReader("F:\\Spark.Engine\\Source\\Platform\\Resource\\Content\\StaticMesh\\Jason.glb"))
{
    sma.StaticMesh = Engine.ImportStaticMeshFromGLB(sr.BaseStream);
}
sma.Position = camera1.ForwardVector * 50 + camera1.UpVector * -50;
// 创建一个定向光源
var light1 = Engine.CreateActor<SpotLightActor>();
light1.LightColor = Color.LightPink;
light1.InteriorAngle = 5;
light1.ExteriorAngle = 10;
window.Resize += size =>
{
    Engine.DefaultRenderTarget.Resize(size.X, size.Y);
};
window.Load += () =>
{
    Engine.DefaultRenderTarget.Resize(window.Size.X, window.Size.Y);
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