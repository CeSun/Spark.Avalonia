
using System.Numerics;

namespace Spark.Avalonia.Actors;

public class Actor
{
    public required Engine Engine;
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
    public Matrix4x4 WorldTransform => Transform * ParentActor?.WorldTransform ?? Matrix4x4.Identity;
    public Vector3 WorldPosition => WorldTransform.Translation;
    public Quaternion WorldRotation => WorldTransform.Rotation();
    public Vector3 WorldScale => WorldTransform.Scale();
    public Matrix4x4 Transform => MatrixHelper.CreateTransform(Position, Rotation, Scale);
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Scale { get; set; } = Vector3.One;

    public virtual void Update(float deltaTime)
    {

    }

}
