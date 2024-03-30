using Spark.Util;
using System.Numerics;

namespace Spark.Actors;

public class SpotLightActor : BaseLightActor
{
    public SpotLightActor()
    {
        BoundingBox = new BoundingBox(this);
        InteriorAngle = 30;
        ExteriorAngle = 30;
        Distance = 50;
    }
    public BoundingBox BoundingBox { get; protected set; }
    public float InteriorAngle { get; set; }
    public float ExteriorAngle 
    { 
        get => _ExteriorAngle; 
        set
        {
            _ExteriorAngle = value;
            UpdateOctree();
        }
    }
    public float Distance 
    {
        get => _Distance;
        set
        {
            _Distance = value;
            UpdateOctree();
        }
    }

    private Plane[] tmpPlanes = new Plane[6];
    public Plane[] GetPlanes()
    {
        CameraActor.GetPlanes(Matrix4x4.CreateLookAt(WorldPosition, WorldPosition + ForwardVector, UpVector) * Matrix4x4.CreatePerspectiveFieldOfView(ExteriorAngle.DegreeToRadians(), 1, 0.01f, Distance), ref tmpPlanes);
        return tmpPlanes;
    }
    public float _ExteriorAngle;
    public float _Distance;
    public override void Initialize()
    {
        base.Initialize();
        UpdateOctree();
    }

    public void UpdateOctree()
    {
        if (Engine == null)
            return;
        var Edge = Distance * (float)Math.Tan(InteriorAngle.DegreeToRadians());
        var point1 = new Vector3(Edge, Edge, -1 * Distance);
        var point2 = new Vector3(-Edge, Edge, -1 * Distance);
        var point3 = new Vector3(-Edge, -Edge, -1 * Distance);
        var point4 = new Vector3(Edge, -Edge, -1 * Distance);
        var point5 = Vector3.Zero;

        Engine.Octree.RemoveObject(BoundingBox);
        BoundingBox.Box.MinPoint = Vector3.Transform(point1, WorldTransform);
        BoundingBox.Box.MaxPoint = BoundingBox.Box.MinPoint;
        BoundingBox.Box += Vector3.Transform(point2, WorldTransform);
        BoundingBox.Box += Vector3.Transform(point3, WorldTransform);
        BoundingBox.Box += Vector3.Transform(point4, WorldTransform);
        BoundingBox.Box += Vector3.Transform(point5, WorldTransform);
        Engine.Octree.InsertObject(BoundingBox);
    }
    public override void Uninitialize()
    {
        base.Uninitialize();
        Engine.Octree.RemoveObject(BoundingBox);
    }
    public override void OnTransformChanged()
    {
        base.OnTransformChanged();
        UpdateOctree();
    }
}
