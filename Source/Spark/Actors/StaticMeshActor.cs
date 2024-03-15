using Spark.Assets;
using Spark.Avalonia.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Actors;

public class StaticMeshActor : Actor
{
    private StaticMesh? _StaticMesh;
    public StaticMesh? StaticMesh
    {
        get => _StaticMesh;
        set
        {
            if (_StaticMesh == value && value != null)
            {
                // updateBox
            }
            else if (_StaticMesh != null && value == null)
            {

            }
            else if (_StaticMesh != null && value != null) 
            {
            
            }
        }
    }
}
