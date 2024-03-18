﻿using Spark.Assets;
using Spark.Avalonia.Actors;
using Spark.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Actors;

public class StaticMeshActor : Actor
{
    private StaticMesh? _StaticMesh;
    public StaticMesh? StaticMesh
    {
        get => _StaticMesh;
        set
        {
            if (_StaticMesh == value)
                return;
            if (_StaticMesh != null)
            {
                UnregisterFromOctree();
                _StaticMesh = null;
            }
            if (value != null)
            {
                _StaticMesh = value;
                RegisterToOctree();
            }
        }
    }
    List<BoundingBox> BoundingBoxes { get; set; } = new List<BoundingBox>();
    private void RegisterToOctree()
    {
        if (_StaticMesh == null)
            return;
        var matrix = this.WorldTransform;
        foreach (var element in _StaticMesh.Elements)
        {
            var box = new BoundingBox(element);
            foreach(var p in element.ConvexHull)
            {
                box.Box += Vector3.Transform(p, matrix);
            }
            BoundingBoxes.Add(box);
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
        if (_StaticMesh == null)
            return;
        RemoveFromOctree();
        var matrix = this.WorldTransform;
        foreach (var box in BoundingBoxes)
        {
            box.Box.MinPoint = Vector3.Zero;
            box.Box.MaxPoint = Vector3.Zero;
            foreach(var p in ((Element)box.Object).ConvexHull)
            {
                box.Box += Vector3.Transform(p, matrix);
            }
        }
        AddToOctree();
    }
    public override void OnTransformChanged()
    {
        base.OnTransformChanged();
        UpdateOctree();
    }

}
