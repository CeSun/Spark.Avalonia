using Silk.NET.OpenGLES;
using Spark.RenderTarget;
using Spark.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Avalonia.Actors;

public enum CameraClearFlag
{
    ColorFlag = (1 << 0),
    DepthFlag = (1 << 1),
    Skybox = (1 << 2),
}
public class CameraActor : Actor
{
    public float NearPlane = 10;

    public float FarPlane = 100;

    public float FieldOfView = 90f;

    public CameraClearFlag ClearFlag = CameraClearFlag.ColorFlag | CameraClearFlag.DepthFlag;

    public Color ClearColor = Color.Blue;
    public Matrix4x4 ViewTransform => Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView.DegreeToRadians(), RenderTarget.Width / (float)RenderTarget.Height, NearPlane, FarPlane);

    public int Order = 1;
    public Matrix4x4 ProjectTransform => Matrix4x4.CreateLookAt(WorldPosition, WorldPosition + ForwardVector, UpVector);

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    public BaseRenderTarget RenderTarget;
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    public override void Initialize()
    {
        base.Initialize();
        RenderTarget = Engine.DefaultRenderTarget;
    }
}
