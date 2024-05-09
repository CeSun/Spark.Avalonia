using Spark.Util;

namespace Spark.Actors;

public class PointLightActor : BaseLightActor, IActorCreator<PointLightActor>
{
    public BoundingSphere BoundingSphere { get; protected set; }
    
    public PointLightActor(Engine engine) : base(engine)
    {
        BoundingSphere = new BoundingSphere(this);
    }

    private float _attenuationRadius;
    public float AttenuationRadius 
    { 
        get => _attenuationRadius; 
        set
        {
            _attenuationRadius = value;
            UpdateOctree();
        }
    }

    public float MinThresholdStrong { get; set; } = 0.1f;

    public float AttenuationFactor => (AttenuationRadius * AttenuationRadius) * MinThresholdStrong;

    public override void Initialize()
    {
        base.Initialize();

        Engine.Octree.InsertObject(BoundingSphere);
    }

    public void UpdateOctree()
    {
        Engine.Octree.RemoveObject(BoundingSphere);
        BoundingSphere.Location = this.WorldPosition;
        BoundingSphere.Radius = _attenuationRadius;
        Engine.Octree.InsertObject(BoundingSphere);
    }
    public override void OnTransformChangedTickEnd()
    {
        base.OnTransformChangedTickEnd();
        UpdateOctree();
    }
    public override void UnInitialize()
    {
        Engine.Octree.RemoveObject(BoundingSphere);
        base.UnInitialize();

    }

    public new static PointLightActor Create(Engine engine) => new(engine);
    
}
