using Avalonia.Controls;
using Avalonia.Interactivity;
using Spark.Actors;
using Spark.Assets;
using Spark.Avalonia;
using Spark.Avalonia.Actors;
using Spark.Importer;
using Spark.Util;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace Example.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    public async void OnWorldBeginPlay(object source, RoutedEventArgs args)
    {
        if (source is SparkCanvas canvas == false)
            return;
        var Engine = canvas.Engine;
        // 创建一个摄像机
        var camera1 = Engine.CreateActor<CameraActor>();
        camera1.ClearColor = Color.LightGray;
        // 创建并加载一个模型
        var sma = Engine.CreateActor<StaticMeshActor>();
        StaticMesh mesh = new StaticMesh();
        using (var sr = new StreamReader("E:\\Spark.Engine\\Source\\Platform\\Resource\\Content\\StaticMesh\\Jason.glb"))
        {
            sma.StaticMesh = await Task.Run(() => Engine.ImportStaticMeshFromGLB(sr.BaseStream));
            sma.StaticMesh.Elements.ForEach(element => element.Material.ShaderModel = Spark.Avalonia.Assets.ShaderModel.Lambert);
        }
        sma.Position = camera1.ForwardVector * 50 + camera1.UpVector * -50;
        // 创建一个定向光源
        var light1 = Engine.CreateActor<SpotLightActor>();
        light1.LightColor = Color.LightPink;
        light1.LightColorVec3 /= 3;
        light1.InteriorAngle = 5;
        light1.ExteriorAngle = 10;

        light1.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0f.DegreeToRadians(), 0);
    }
}
