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
    public List<ElementProxy> MaskedStaticMeshes { get; private set; } = new List<ElementProxy>();
    public List<ElementProxy> OpaqueStaticMeshes { get; private set; } = new List<ElementProxy>();

    public abstract ShaderModel ShaderModel { get; }
    public abstract void PreRender(GL gl, Shader Shader, CameraActor Camera);
	public abstract void PostRender(GL gl, Shader Shader, CameraActor Camera);

	public abstract void SetupDirectionLightInfo(GL gl, Shader Shader, CameraActor Camera, DirectionLightActor DirectionLightActor);
    public abstract void SetupPointLightInfo(GL gl, Shader Shader, CameraActor Camera, PointLightActor DirectionLightActor);
    public abstract void SetupSpotLightInfo(GL gl, Shader Shader, CameraActor Camera, SpotLightActor DirectionLightActor);
    public abstract void RenderMaskedStaticMesh(GL gl, Shader Shader, CameraActor Camera, ElementProxy proxy);
	public abstract void RenderOpaqueStaticMesh(GL gl, Shader Shader, CameraActor Camera, ElementProxy proxy);

}

public interface IStaticShaderModel
{

	public static abstract ShaderModel CreateInstance();
}