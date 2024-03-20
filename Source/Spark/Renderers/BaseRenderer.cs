using Silk.NET.OpenGLES;
using Spark.Avalonia.Actors;

namespace Spark.Avalonia.Renderers
{
    public interface IRenderer
    {
        void Initialize(GL gl);
        void Render(GL gl, CameraActor Camera);
        void Uninitialize(GL gl);

    }
}
