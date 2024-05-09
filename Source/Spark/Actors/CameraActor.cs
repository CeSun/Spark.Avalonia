using Spark.RenderTarget;
using Spark.Util;
using System.Drawing;
using System.Numerics;

namespace Spark.Actors;

[Flags]
public enum CameraClearFlag
{
    ColorFlag = (1 << 0),
    DepthFlag = (1 << 1),
    Skybox = (1 << 2),
}
public class CameraActor(Engine engine) : Actor(engine), IActorCreator<CameraActor>
{
    public float NearPlane = 10;

    public float FarPlane = 100;

    public float FieldOfView = 90f;

    public CameraClearFlag ClearFlag = CameraClearFlag.ColorFlag | CameraClearFlag.DepthFlag;

    public Color ClearColor = Color.Blue;
    public Matrix4x4 ViewTransform => Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView.DegreeToRadians(), RenderTarget.Width / (float)RenderTarget.Height, NearPlane, FarPlane);

    public int Order = 1;
    public Matrix4x4 ProjectTransform => Matrix4x4.CreateLookAt(WorldPosition, WorldPosition + ForwardVector, UpVector);

    public BaseRenderTarget RenderTarget = engine.DefaultRenderTarget;
   

    private Plane[] _tmpPlanes = new Plane[6];
    public Plane[] GetPlanes()
    {
        GetPlanes(ViewTransform * ProjectTransform, ref _tmpPlanes);
        return _tmpPlanes;
    }
    public static void GetPlanes(Matrix4x4 viewTransform, ref Plane[] planes)
    {
        if (planes.Length < 6)
        {
            planes = new Plane[6];
        }

        //左侧  
        planes[0].Normal.X = viewTransform[0, 3] + viewTransform[0, 0];
        planes[0].Normal.Y = viewTransform[1, 3] + viewTransform[1, 0];
        planes[0].Normal.Z = viewTransform[2, 3] + viewTransform[2, 0];
        planes[0].D = viewTransform[3, 3] + viewTransform[3, 0];
        //右侧
        planes[1].Normal.X = viewTransform[0, 3] - viewTransform[0, 0];
        planes[1].Normal.Y = viewTransform[1, 3] - viewTransform[1, 0];
        planes[1].Normal.Z = viewTransform[2, 3] - viewTransform[2, 0];
        planes[1].D = viewTransform[3, 3] - viewTransform[3, 0];
        //上侧
        planes[2].Normal.X = viewTransform[0, 3] - viewTransform[0, 1];
        planes[2].Normal.Y = viewTransform[1, 3] - viewTransform[1, 1];
        planes[2].Normal.Z = viewTransform[2, 3] - viewTransform[2, 1];
        planes[2].D = viewTransform[3, 3] - viewTransform[3, 1];
        //下侧
        planes[3].Normal.X = viewTransform[0, 3] + viewTransform[0, 1];
        planes[3].Normal.Y = viewTransform[1, 3] + viewTransform[1, 1];
        planes[3].Normal.Z = viewTransform[2, 3] + viewTransform[2, 1];
        planes[3].D = viewTransform[3, 3] + viewTransform[3, 1];
        //Near
        planes[4].Normal.X = viewTransform[0, 3] + viewTransform[0, 2];
        planes[4].Normal.Y = viewTransform[1, 3] + viewTransform[1, 2];
        planes[4].Normal.Z = viewTransform[2, 3] + viewTransform[2, 2];
        planes[4].D = viewTransform[3, 3] + viewTransform[3, 2];
        //Far
        planes[5].Normal.X = viewTransform[0, 3] - viewTransform[0, 2];
        planes[5].Normal.Y = viewTransform[1, 3] - viewTransform[1, 2];
        planes[5].Normal.Z = viewTransform[2, 3] - viewTransform[2, 2];
        planes[5].D = viewTransform[3, 3] - viewTransform[3, 2];
    }


    public new static CameraActor Create(Engine engine) => new(engine);
}
