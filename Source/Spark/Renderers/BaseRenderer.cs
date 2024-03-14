using Silk.NET.OpenGLES;
using Spark.Avalonia.Actors;

namespace Spark.Avalonia.Renderers
{
    public interface IRenderer
    {

        void Render(GL gl, CameraActor Camera);

    }
}
