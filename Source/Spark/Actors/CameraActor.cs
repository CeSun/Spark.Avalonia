using Silk.NET.OpenGLES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Avalonia.Actors;

public class CameraActor : Actor
{
    public float NearPlane = 10;

    public float FarPlane = 100;

    public float FieldOfView = 90f;

    public Matrix4x4 ViewTransform;

    public Matrix4x4 ProjectTransform;


    public void RenderSence(GL gl)
    {

    }

}
