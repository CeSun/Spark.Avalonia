using Spark.Actors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Actors;

public abstract class BaseLightActor : Actor
{
    protected BaseLightActor(Engine engine) : base(engine)
    {
        LightColor = Color.White;
    }
    public Color LightColor 
    {
        get => _lightColor;
        set
        {
            _lightColor = value;
            _lightColorVec3 = new Vector3(value.R / 255f, value.G / 255f, value.B / 255f);
        }
    }
    public Vector3 LightColorVec3 
    {
        get => _lightColorVec3;
        set
        {
            _lightColorVec3 = value;
            _lightColor = Color.FromArgb(255, (int)(value.X * 255), (int)(value.Y * 255), (int)(value.Z * 255));
        }
    }
    private Color _lightColor;
    private Vector3 _lightColorVec3;
    
}
