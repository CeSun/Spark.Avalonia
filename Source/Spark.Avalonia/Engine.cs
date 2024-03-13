using Silk.NET.OpenGLES;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Avalonia
{
    internal class Engine
    {
        public Engine() { }

        public void Update(float DeltaTime)
        {
            Console.WriteLine(DeltaTime);
        }

        public void Render(GL gl)
        {
            gl.ClearColor(Color.Blue);
            gl.Clear(ClearBufferMask.ColorBufferBit);

        }
    }
}
