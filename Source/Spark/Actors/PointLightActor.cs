using Spark.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Actors;

public class PointLightActor : BaseLightActor
{
    public BoundingSphere BoundingSphere { get; protected set; }
    
    public PointLightActor() : base()
    {
        BoundingSphere = new(this);
    }
    private float _AttenuationRatius;
    public float AttenuationRatius 
    { 
        get => _AttenuationRatius; 
        set
        {
            _AttenuationRatius = value;
            UpdateOctree();
        }
    }

    public float MinThresholdStrong { get; set; } = 0.1f;

    public float AttenuationFactor => (AttenuationRatius * AttenuationRatius) * MinThresholdStrong;

    public override void Initialize()
    {
        base.Initialize();

        Engine.Octree.InsertObject(BoundingSphere);
    }

    public void UpdateOctree()
    {
        Engine.Octree.RemoveObject(BoundingSphere);
        BoundingSphere.Location = this.WorldPosition;
        BoundingSphere.Radius = _AttenuationRatius;
        Engine.Octree.InsertObject(BoundingSphere);
    }
    public override void OnTransformChanged()
    {
        base.OnTransformChanged();
        UpdateOctree();
    }
    public override void Uninitialize()
    {
        Engine.Octree.RemoveObject(BoundingSphere);
        base.Uninitialize();

    }
}
