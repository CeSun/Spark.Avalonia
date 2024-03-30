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
    public BaseLightActor()
    {
        LightColor = Color.White;
    }
    public Color LightColor 
    {
        get => _LightColor;
        set
        {
            _LightColor = value;
            _LightColorVec3 = new Vector3(value.R / 255f, value.G / 255f, value.B / 255f);
        }
    }
    public Vector3 LightColorVec3 
    {
        get => _LightColorVec3;
        set
        {
            _LightColorVec3 = value;
            _LightColor = Color.FromArgb(255, (int)(value.X * 255), (int)(value.Y * 255), (int)(value.Z * 255));
        }
    }
    private Color _LightColor;
    private Vector3 _LightColorVec3;
}
