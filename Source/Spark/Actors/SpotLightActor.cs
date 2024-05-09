using Spark.Util;
using System.Numerics;

namespace Spark.Actors;

public class SpotLightActor : BaseLightActor, IActorCreator<SpotLightActor>
{
    public SpotLightActor(Engine engine) : base(engine)
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
        get => _exteriorAngle; 
        set
        {
            _exteriorAngle = value;
            UpdateOctree();
        }
    }
    public float Distance 
    {
        get => _distance;
        set
        {
            _distance = value;
            UpdateOctree();
        }
    }

    private Plane[] _tmpPlanes = new Plane[6];
    public Plane[] GetPlanes()
    {
        CameraActor.GetPlanes(Matrix4x4.CreateLookAt(WorldPosition, WorldPosition + ForwardVector, UpVector) * Matrix4x4.CreatePerspectiveFieldOfView(ExteriorAngle.DegreeToRadians(), 1, 0.01f, Distance), ref _tmpPlanes);
        return _tmpPlanes;
    }
    private float _exteriorAngle;
    private float _distance;
    public override void Initialize()
    {
        base.Initialize();
        UpdateOctree();
    }

    public void UpdateOctree()
    {
        var edge = Distance * (float)Math.Tan(InteriorAngle.DegreeToRadians());
        var point1 = new Vector3(edge, edge, -1 * Distance);
        var point2 = new Vector3(-edge, edge, -1 * Distance);
        var point3 = new Vector3(-edge, -edge, -1 * Distance);
        var point4 = new Vector3(edge, -edge, -1 * Distance);
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
    public override void UnInitialize()
    {
        base.UnInitialize();
        Engine.Octree.RemoveObject(BoundingBox);
    }
    public override void OnTransformChangedTickEnd()
    {
        base.OnTransformChangedTickEnd();
        UpdateOctree();
    }

    public new static SpotLightActor Create(Engine engine) => new(engine);

}
