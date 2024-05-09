
using System.Numerics;

namespace Spark.Actors;

public class Actor (Engine engine) : IActorCreator<Actor>
{
    public Engine Engine = engine;

    private Actor? _parentActor;
    public Actor? ParentActor
    {
        get => _parentActor;
        set
        {
            if (value == null)
            {
                if (_parentActor != null && _children.Contains(_parentActor))
                {
                    _children.Remove(_parentActor);
                }
            }
            else
            {
                _children.Add(value);
            }
            _parentActor = value;
        }
    }
    private readonly List<Actor> _children  = [];
    public IReadOnlyList<Actor> Children => _children;
    public Matrix4x4 WorldTransform
    {
        get
        {
            if (_parentActor == null)
                return Transform;
            return Transform * _parentActor.WorldTransform;
        }
    }
    public Vector3 WorldPosition => WorldTransform.Translation;
    public Quaternion WorldRotation => WorldTransform.Rotation();
    public Vector3 WorldScale => WorldTransform.Scale();
    public Matrix4x4 Transform => MatrixHelper.CreateTransform(Position, Rotation, Scale);

    public bool IsDirty 
    {
        get => _isDirty;
        set
        {
            _isDirty = value;
            if (_isDirty || value == false)
                return;
            foreach (var child in Children)
            {
                child.IsDirty = true;
            }
        }
    }

    private bool _isDirty;
    public Vector3 Position 
    {
        get => _position;
        set 
        {
            IsDirty = true;
            _position = value;
        } 
    }
    public Quaternion Rotation 
    { 
        get => _rotation; 
        set
        {
            IsDirty = true;
            _rotation = value;
        }
    }
    public Vector3 Scale 
    {
        get => _scale;
        set
        {
            IsDirty = true;
            _scale = value;
        }
    }

    public Vector3 ForwardVector => Vector3.Transform(new Vector3(0, 0, -1), WorldRotation);
    public Vector3 RightVector => Vector3.Transform(new Vector3(1, 0, 0), WorldRotation);
    public Vector3 UpVector => Vector3.Transform(new Vector3(0, 1, 0), WorldRotation);

    public virtual void Update(float deltaTime)
    {
        if (IsDirty != true) 
            return;
        OnTransformChangedTickEnd();
        IsDirty = false;
    }
    public virtual void OnTransformChangedTickEnd()
    {

    }

    public virtual void Initialize()
    {

    }

    public virtual void UnInitialize()
    {

    }

    private Vector3 _position;
    private Quaternion _rotation;
    private Vector3 _scale = Vector3.One;


    public new static Actor Create(Engine engine) => new(engine);
}


public interface IActorCreator<T> where T: Actor
{
    public static abstract T Create(Engine engine);

}