using SharpGLTF.Transforms;
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

    private Plane[] tmpPlanes = new Plane[6];
    public Plane[] GetPlanes()
    {
        GetPlanes(ViewTransform * ProjectTransform, ref tmpPlanes);
        return tmpPlanes;
    }
    public static void GetPlanes(Matrix4x4 ViewTransform, ref Plane[] Planes)
    {
        if (Planes.Length < 6)
        {
            Planes = new Plane[6];
        }

        //左侧  
        Planes[0].Normal.X = ViewTransform[0, 3] + ViewTransform[0, 0];
        Planes[0].Normal.Y = ViewTransform[1, 3] + ViewTransform[1, 0];
        Planes[0].Normal.Z = ViewTransform[2, 3] + ViewTransform[2, 0];
        Planes[0].D = ViewTransform[3, 3] + ViewTransform[3, 0];
        //右侧
        Planes[1].Normal.X = ViewTransform[0, 3] - ViewTransform[0, 0];
        Planes[1].Normal.Y = ViewTransform[1, 3] - ViewTransform[1, 0];
        Planes[1].Normal.Z = ViewTransform[2, 3] - ViewTransform[2, 0];
        Planes[1].D = ViewTransform[3, 3] - ViewTransform[3, 0];
        //上侧
        Planes[2].Normal.X = ViewTransform[0, 3] - ViewTransform[0, 1];
        Planes[2].Normal.Y = ViewTransform[1, 3] - ViewTransform[1, 1];
        Planes[2].Normal.Z = ViewTransform[2, 3] - ViewTransform[2, 1];
        Planes[2].D = ViewTransform[3, 3] - ViewTransform[3, 1];
        //下侧
        Planes[3].Normal.X = ViewTransform[0, 3] + ViewTransform[0, 1];
        Planes[3].Normal.Y = ViewTransform[1, 3] + ViewTransform[1, 1];
        Planes[3].Normal.Z = ViewTransform[2, 3] + ViewTransform[2, 1];
        Planes[3].D = ViewTransform[3, 3] + ViewTransform[3, 1];
        //Near
        Planes[4].Normal.X = ViewTransform[0, 3] + ViewTransform[0, 2];
        Planes[4].Normal.Y = ViewTransform[1, 3] + ViewTransform[1, 2];
        Planes[4].Normal.Z = ViewTransform[2, 3] + ViewTransform[2, 2];
        Planes[4].D = ViewTransform[3, 3] + ViewTransform[3, 2];
        //Far
        Planes[5].Normal.X = ViewTransform[0, 3] - ViewTransform[0, 2];
        Planes[5].Normal.Y = ViewTransform[1, 3] - ViewTransform[1, 2];
        Planes[5].Normal.Z = ViewTransform[2, 3] - ViewTransform[2, 2];
        Planes[5].D = ViewTransform[3, 3] - ViewTransform[3, 2];
    }
}
