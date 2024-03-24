using Silk.NET.OpenGLES;
using Spark.Actors;
using Spark.Avalonia.Actors;
using Spark.Avalonia.Renderers;
using Spark.Renderers;
using Spark.RenderTarget;
using Spark.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Avalonia;

public class Engine
{
    BaseRenderer Renderer;
    private readonly List<Action<GL>> RenderMethods = new();
    private readonly List<Action<GL>> TmpRenderMethods = new();
    public BaseRenderTarget DefaultRenderTarget { get; set; }

    private readonly List<DirectionLightActor> _DirectionLightActors = new();
    public IReadOnlyList<DirectionLightActor> DirectionLightActors => _DirectionLightActors;

    private readonly List<PointLightActor> _PointLightActors = new();
    public IReadOnlyList<PointLightActor> PointLightActors => _PointLightActors;

    public Octree Octree { get; private set; }
    public Engine() 
    {
        Octree = new Octree();
        Renderer = new ForwardRenderer(new RenderFeatures { PreZ = true });
        DefaultRenderTarget = new CanvasRenderTarget();
    }

    public T CreateActor<T>() where T : Actor, new()
    {
        var actor = new T();
        actor.Engine = this;
        RegisterActor(actor);
        actor.Initialize();
        return actor;
    }

    public void RemoveActor(Actor actor)
    {
        actor.Uninitialize();
        UnregisterActor(actor);
    }

    public bool Contains(Actor actor)
    {
        return Actors.Contains(actor);
    }
    private void RegisterActor(Actor actor)
    {
        if (actor is CameraActor camera)
        {
            CameraActors.Add(camera);
        }
        else if (actor is DirectionLightActor directionLight)
        {
             _DirectionLightActors.Add(directionLight);
        }
        else if (actor is PointLightActor pointLight)
        {
            _PointLightActors.Add(pointLight);
        }
        _Actors.Add(actor);
    }

    private void UnregisterActor(Actor actor)
    {
        if (actor is CameraActor cam)
        {
            CameraActors.Remove(cam);
        }
        else if (actor is DirectionLightActor directionLight)
        {
            _DirectionLightActors.Remove(directionLight);
        }
        else if (actor is PointLightActor pointLightActor)
        {
            _PointLightActors.Remove(pointLightActor);
        }
        _Actors.Remove(actor);
    }

    private List<CameraActor> CameraActors = new List<CameraActor>();
    private List<Actor> _Actors { get; set; } = new List<Actor>();
    
    public IReadOnlyList<Actor> Actors => _Actors;
    public void Update(float DeltaTime)
    {
        _Actors.ToList().ForEach(actor => actor.Update(DeltaTime));
    }

    public void AddRenderTask(Action<GL> action)
    {
        lock(RenderMethods)
        {
            RenderMethods.Add(action);
        }
    }
    public void Render(GL gl)
    {
        TmpRenderMethods.Clear();
        lock(RenderMethods)
        {
            TmpRenderMethods.AddRange(RenderMethods);
            RenderMethods.Clear();
        }
        TmpRenderMethods.ForEach(m => m(gl));
        CameraActors.Sort((left, right) =>
        {
            return left.Order.CompareTo(right.Order);
        });
        CameraActors.ForEach(camera => Renderer.Render(gl, camera));
    }

    public void Initialize(GL gl)
    {
        Renderer.Initialize(gl);
    }

    public void Uninitialize(GL gl)
    {
        Renderer.Uninitialize(gl);
    }
}
