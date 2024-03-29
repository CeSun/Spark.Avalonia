using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Spark.Actors;
using Spark.Assets;
using Spark.Avalonia;
using Spark.Avalonia.Actors;
using Spark.Importer;
using Spark.Util;
using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace Example.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }


    CameraActor? CameraActor;
    StaticMeshActor? StaticMeshActor;
    StaticMesh? Person;
    StaticMesh? Cube;
    StaticMesh? Wpn;

    private async Task LoadAsset(Engine engine)
    {
        var t1 = Task.Run(() =>
        {
            using (var sr = AssetLoader.Open(new Uri("avares://Example/Assets/Jason.glb")))
            {
                return engine.ImportStaticMeshFromGLB(sr);
            }
        });
        var t2 = Task.Run(() =>
        {
            using (var sr = AssetLoader.Open(new Uri("avares://Example/Assets/sofa.glb")))
            {
                return engine.ImportStaticMeshFromGLB(sr);
            }
        });
        var t3 = Task.Run(() =>
        {
            using (var sr = AssetLoader.Open(new Uri("avares://Example/Assets/AK47.glb")))
            {
                return engine.ImportStaticMeshFromGLB(sr);
            }
        });
        Cube = await t2;
        Wpn = await t3;
        Person = await t1;


    }

    public void ChangePerson(object sender, RoutedEventArgs args)
    {
        StaticMeshActor!.StaticMesh = Person;
        StaticMeshActor.Position = CameraActor!.ForwardVector * 70 + CameraActor.UpVector * -50;
        StaticMeshActor.Scale = Vector3.One;
    }

    public void ChangeWpn(object sender, RoutedEventArgs args)
    {
        StaticMeshActor!.StaticMesh = Wpn;
        StaticMeshActor!.Position = CameraActor!.ForwardVector * 50;
        StaticMeshActor!.Scale = Vector3.One;
    }

    public void ChangeCube(object sender, RoutedEventArgs args)
    {
        StaticMeshActor!.StaticMesh = Cube;
        StaticMeshActor.Position = CameraActor!.ForwardVector * 50 + CameraActor.UpVector * -5;
        StaticMeshActor.Scale = Vector3.One * 20;
    }

    public async void OnWorldBeginPlay(object source, RoutedEventArgs args)
    {
        if (source is SparkCanvas canvas == false)
            return;
        var Engine = canvas.Engine;
        // 创建一个摄像机
        CameraActor = Engine.CreateActor<CameraActor>();
        CameraActor.ClearColor = Color.White;
        // 创建并加载一个模型
        var sma = Engine.CreateActor<StaticMeshActor>();
        StaticMeshActor = sma;
        float yaw = 0;
        var f = async () =>
        {
            while (true)
            {
                if (yaw ++ > 360)
                {
                    yaw = 0;
                }
                sma.Rotation = Quaternion.CreateFromYawPitchRoll(yaw.DegreeToRadians(), 0, 0);
                await Task.Delay(10);
            }
        };
        _ = f();
        // 创建一个定向光源
        var light1 = Engine.CreateActor<SpotLightActor>();
        light1.LightColor = Color.LightPink;
        light1.InteriorAngle = 5;
        light1.ExteriorAngle = 10;


        var light2 = Engine.CreateActor<DirectionLightActor>();
        light2.LightColor = Color.Gray;
        light2.Rotation = Quaternion.CreateFromYawPitchRoll(0, -30f.DegreeToRadians(), 0);

        await LoadAsset(Engine);
    }
}
