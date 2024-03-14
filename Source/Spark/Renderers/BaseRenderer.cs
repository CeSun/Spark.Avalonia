using Silk.NET.OpenGLES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Avalonia.Renderers
{
    public class BaseRenderer
    {
        protected Engine Engine;
        public BaseRenderer(Engine engine)
        {
            Engine = engine;
        }

        public virtual void Begin(GL gl)
        {

        }

        public virtual void Render(GL gl)
        {

        }

        public virtual void End(GL gl)
        {

        }

    }
}
