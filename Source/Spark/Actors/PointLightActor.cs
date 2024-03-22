using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Actors;

public class PointLightActor : BaseLightActor
{
    public float AttenuationRatius { get; set; } = 1.0f;

}
