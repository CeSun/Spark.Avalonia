using Silk.NET.OpenGLES;
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
    IRenderer Renderer;
    public List<Action<GL>> RenderMethods = new List<Action<GL>>(); 
    public BaseRenderTarget DefaultRenderTarget { get; set; }

    public Octree Octree { get; private set; }
    public Engine() 
    {
        Octree = new Octree();
        Renderer = new ForwardRenderer();
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
        _Actors.Add(actor);
    }

    private void UnregisterActor(Actor actor)
    {
        if (actor is CameraActor cam)
        {
            CameraActors.Remove(cam);
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

    public void Render(GL gl)
    {
        RenderMethods.ForEach(m => m(gl));
        RenderMethods.Clear();
        CameraActors.Sort((left, right) =>
        {
            return left.Order.CompareTo(right.Order);
        });
        CameraActors.ForEach(camera => Renderer.Render(gl, camera));
    }
}
