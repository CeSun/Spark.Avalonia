using Spark.Assets;
using Spark.Util;
using System.Numerics;

namespace Spark.Actors;

public class StaticMeshActor(Engine engine) : Actor(engine), IActorCreator<StaticMeshActor>
{
    private StaticMesh? _staticMesh;
    public StaticMesh? StaticMesh
    {
        get => _staticMesh;
        set
        {
            if (_staticMesh == value)
                return;
            if (_staticMesh != null)
            {
                UnregisterFromOctree();
                _staticMesh = null;
            }
            if (value != null)
            {
                _staticMesh = value;
                RegisterToOctree();
            }
        }
    }
    List<BoundingBox> BoundingBoxes { get; set; } = new List<BoundingBox>();
    private void RegisterToOctree()
    {
        if (_staticMesh == null)
            return;
        var matrix = this.WorldTransform;
        foreach (var element in _staticMesh.Elements)
        {
            var proxy = new ElementProxy(element);
            var box = new BoundingBox(proxy);
            int i = 0;
            foreach(var p in element.ConvexHull)
            {
                if (i == 0)
                {
                    box.Box.MinPoint = Vector3.Transform(p, matrix);
                    box.Box.MinPoint = box.Box.MaxPoint;
                }
                else
                {
                    box.Box += Vector3.Transform(p, matrix);
                }
                i++;
            }
            BoundingBoxes.Add(box);
            proxy.ModelTransform = matrix;
        }
        AddToOctree();
    }

    private void UnregisterFromOctree()
    {
        RemoveFromOctree();
        BoundingBoxes.Clear();
    }
    private void RemoveFromOctree()
    {
        foreach(var box in BoundingBoxes)
        {
            Engine.Octree.RemoveObject(box);
        }
    }

    private void AddToOctree()
    {
        foreach (var box in BoundingBoxes)
        {
            Engine.Octree.InsertObject(box);
        }
    }

    private void UpdateOctree()
    {
        if (_staticMesh == null)
            return;
        RemoveFromOctree();
        var matrix = this.WorldTransform;
        foreach (var box in BoundingBoxes)
        {
            var proxy = (ElementProxy)box.Object;
            box.Box.MinPoint = Vector3.Zero;
            box.Box.MaxPoint = Vector3.Zero;
            int i = 0;
            foreach(var p in proxy.Element.ConvexHull)
            {
                if (i == 0)
                {
                    box.Box.MinPoint = Vector3.Transform(p, matrix);
                    box.Box.MinPoint = box.Box.MaxPoint;
                }
                else
                {
                    box.Box += Vector3.Transform(p, matrix);
                }
                i++;
            }
            proxy.ModelTransform = matrix;
        }
        AddToOctree();
    }
    public override void OnTransformChangedTickEnd()
    {
        base.OnTransformChangedTickEnd();
        UpdateOctree();
    }

    public override void UnInitialize()
    {
        RemoveFromOctree();
        base.UnInitialize();
    }

    public new static StaticMeshActor Create(Engine engine) => new(engine);
}
