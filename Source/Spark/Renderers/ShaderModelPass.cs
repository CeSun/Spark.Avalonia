using Silk.NET.OpenGLES;
using Spark.Actors;
using Spark.Assets;
using Spark.Avalonia.Actors;
using Spark.Avalonia.Assets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace Spark.Renderers;

public abstract class ShaderModelPass
{
    public readonly List<ElementProxy> StaticMeshes  = new();
    public readonly List<ElementProxy> TranslucentStaticMeshs = new();

    public Shader? DirectionLightShader { get; set; }
    public Shader? PointLightShader { get; set; }
    public Shader? SpotLightShader { get; set; }
    public abstract void Uninitialize(GL gl);
    public abstract void Initialize(GL gl);
    public abstract ShaderModel ShaderModel { get; }
    public abstract void PreRender(GL gl, CameraActor Camera);
	public abstract void PostRender(GL gl, CameraActor Camera);

	public abstract void SetupDirectionLightInfo(GL gl, Shader Shader, CameraActor Camera, DirectionLightActor DirectionLightActor);
    public abstract void SetupPointLightInfo(GL gl, Shader Shader, CameraActor Camera, PointLightActor DirectionLightActor);
    public abstract void SetupSpotLightInfo(GL gl, Shader Shader, CameraActor Camera, SpotLightActor DirectionLightActor);
    public abstract void RenderStaticMesh(GL gl, Shader Shader, CameraActor Camera, ElementProxy proxy);

}

public interface IStaticShaderModel
{

	public static abstract ShaderModel CreateInstance();
}