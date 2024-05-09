using Silk.NET.OpenGLES;
using Spark.Actors;
using Spark.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Renderers;

public class DeferredRenderer : BaseRenderer
{
    public override void Initialize(GL gl)
    {
        base.Initialize(gl);
    }

    public override void Render(GL gl, CameraActor camera)
    {
        base.Render(gl, camera);

    }
    public override void UnInitialize(GL gl)
    {
        base.UnInitialize(gl);
    }
}
