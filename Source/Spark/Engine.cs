using Silk.NET.OpenGLES;
using Spark.Actors;
using Spark.Renderers;
using Spark.RenderTarget;
using Spark.Util;

namespace Spark;

public class Engine
{
    private readonly BaseRenderer _renderer = new ForwardRenderer();
    private readonly List<Action<GL>> _renderMethods = [];
    private readonly List<Action<GL>> _tmpRenderMethods = [];
    public BaseRenderTarget DefaultRenderTarget { get; set; } = new CanvasRenderTarget();

    private readonly List<DirectionLightActor> _directionLightActors = [];
    public IReadOnlyList<DirectionLightActor> DirectionLightActors => _directionLightActors;

    private readonly List<PointLightActor> _pointLightActors = [];
    public IReadOnlyList<PointLightActor> PointLightActors => _pointLightActors;

    public Octree Octree { get; private set; } = new();

    public T CreateActor<T>() where T : Actor, IActorCreator<T>
    {
        var actor = T.Create(this);
        RegisterActor(actor);
        actor.Initialize();
        return actor;
    }

    public void RemoveActor(Actor actor)
    {
        actor.UnInitialize();
        UnregisterActor(actor);
    }

    public bool Contains(Actor actor)
    {
        return Actors.Contains(actor);
    }
    private void RegisterActor(Actor actor)
    {
        switch (actor)
        {
            case CameraActor camera:
                _cameraActors.Add(camera);
                break;
            case DirectionLightActor directionLight:
                _directionLightActors.Add(directionLight);
                break;
            case PointLightActor pointLight:
                _pointLightActors.Add(pointLight);
                break;
        }
        _actors.Add(actor);
    }

    private void UnregisterActor(Actor actor)
    {
        switch (actor)
        {
            case CameraActor cam:
                _cameraActors.Remove(cam);
                break;
            case DirectionLightActor directionLight:
                _directionLightActors.Remove(directionLight);
                break;
            case PointLightActor pointLightActor:
                _pointLightActors.Remove(pointLightActor);
                break;
        }
        _actors.Remove(actor);
    }

    private readonly List<CameraActor> _cameraActors = [];

    private readonly List<Actor> _actors = [];
    
    public IReadOnlyList<Actor> Actors => _actors;
    public void Update(float deltaTime)
    {
        _actors.ToList().ForEach(actor => actor.Update(deltaTime));
    }

    public void AddRenderTask(Action<GL> action)
    {
        lock(_renderMethods)
        {
            _renderMethods.Add(action);
        }
    }
    public void Render(GL gl)
    {
        _tmpRenderMethods.Clear();
        lock(_renderMethods)
        {
            _tmpRenderMethods.AddRange(_renderMethods);
            _renderMethods.Clear();
        }
        _tmpRenderMethods.ForEach(m => m(gl));
        _cameraActors.Sort((left, right) => left.Order.CompareTo(right.Order));
        _cameraActors.ForEach(camera => _renderer.Render(gl, camera));
    }

    public void Initialize(GL gl)
    {
        _renderer.Initialize(gl);
    }

    public void UnInitialize(GL gl)
    {
        _renderer.UnInitialize(gl);
    }
}
