
using System.Numerics;

namespace Spark.Avalonia.Actors;

public class Actor
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    public Engine Engine;
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    private Actor? _ParentActor { get; set; }
    public Actor? ParentActor
    {
        get => _ParentActor;
        set
        {
            if (value == null)
            {
                if (_ParentActor != null && _Children.Contains(_ParentActor))
                {
                    _Children.Remove(_ParentActor);
                }
            }
            else
            {
                _Children.Add(value);
            }
            _ParentActor = value;
        }
    }
    private List<Actor> _Children { get; set; } = new List<Actor>();
    public IReadOnlyList<Actor> Children => _Children;
    public Matrix4x4 WorldTransform
    {
        get
        {
            if (_ParentActor == null)
                return Transform;
            else
                return Transform * _ParentActor.WorldTransform;
        }
    }
    public Vector3 WorldPosition => WorldTransform.Translation;
    public Quaternion WorldRotation => WorldTransform.Rotation();
    public Vector3 WorldScale => WorldTransform.Scale();
    public Matrix4x4 Transform => MatrixHelper.CreateTransform(Position, Rotation, Scale);
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Scale { get; set; } = Vector3.One;

    public Vector3 ForwardVector => Vector3.Transform(new Vector3(0, 0, -1), WorldRotation);
    public Vector3 RightVector => Vector3.Transform(new Vector3(1, 0, 0), WorldRotation);
    public Vector3 UpVector => Vector3.Transform(new Vector3(0, 1, 0), WorldRotation);

    public virtual void Update(float deltaTime)
    {

    }

    public virtual void Initialize()
    {

    }

    public virtual void Uninitialize()
    {

    }

}
