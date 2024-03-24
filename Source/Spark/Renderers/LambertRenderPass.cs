using Silk.NET.OpenGLES;
using Spark.Actors;
using Spark.Assets;
using Spark.Avalonia.Actors;
using Spark.Avalonia.Assets;
using Spark.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Renderers;

public class LambertRenderPass : ShaderModelPass
{
    public override ShaderModel ShaderModel => ShaderModel.Lambert;

    Shader? AmbientLightShader = null;
    public override void Initialize(GL gl)
    {
        DirectionLightShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_LAMBERT_", "_DIRECTIONLIGHT_", "_PREZ_" });
        PointLightShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_LAMBERT_", "_POINTLIGHT_", "_PREZ_" });
        SpotLightShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_LAMBERT_", "_SPOTLIGHT_", "_PREZ_" });
        AmbientLightShader = gl.CreateShader("PreZ.vert", "AmbientLight.frag", new() { "_SHADERMODEL_BLINNPHONG_LAMBERT_" });
    }

    public override void PostRender(GL gl, CameraActor Camera)
    {
    }

    public override unsafe void PreRender(GL gl,  CameraActor Camera)
    {
        using (AmbientLightShader!.Use(gl))
        {
            AmbientLightShader.SetFloat("AmbientStrength", 0.05f);
            AmbientLightShader!.SetMatrix("Projection", Camera.ProjectTransform);
            AmbientLightShader!.SetMatrix("View", Camera.ViewTransform);
            foreach (var proxy in StaticMeshes)
            {
                AmbientLightShader.SetMatrix("Model", proxy.ModelTransform);
                AmbientLightShader.SetInt("BaseColor", 0);
                gl.ActiveTexture(GLEnum.Texture0);
                gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.Diffuse!.TextureId);
                gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
            }
        }
    }

    public override unsafe void RenderStaticMesh(GL gl, Shader Shader, CameraActor Camera, ElementProxy proxy)
    {
        Shader!.SetMatrix("Model", proxy.ModelTransform);
        Shader!.SetInt("BaseColor", 0);
        gl.ActiveTexture(GLEnum.Texture0);
        gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.Diffuse!.TextureId);

        Shader!.SetInt("NormalTexture", 1);
        gl.ActiveTexture(GLEnum.Texture1);
        gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.Normal!.TextureId);

        gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
        gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
    }

    public override void SetupDirectionLightInfo(GL gl, Shader Shader, CameraActor Camera, DirectionLightActor DirectionLightActor)
    {
        Shader!.SetVector3("lightInfo.CameraPosition", Camera.WorldPosition);
        Shader!.SetVector3("lightInfo.Color", DirectionLightActor.LightColorVec3);
        Shader!.SetVector3("lightInfo.Direction", DirectionLightActor.ForwardVector);

        Shader!.SetMatrix("Projection", Camera.ProjectTransform);
        Shader!.SetMatrix("View", Camera.ViewTransform);
    }

    public override void SetupPointLightInfo(GL gl, Shader Shader, CameraActor Camera, PointLightActor PointLightActor)
    {
        Shader!.SetVector3("lightInfo.CameraPosition", Camera.WorldPosition);
        Shader!.SetVector3("lightInfo.Color", PointLightActor.LightColorVec3);
        Shader!.SetVector3("lightInfo.LightPosition", PointLightActor.WorldPosition);
        Shader!.SetFloat("lightInfo.AttenuationFactor", PointLightActor.AttenuationFactor);
        Shader!.SetMatrix("Projection", Camera.ProjectTransform);
        Shader!.SetMatrix("View", Camera.ViewTransform);
    }

    public override void SetupSpotLightInfo(GL gl, Shader Shader, CameraActor Camera, SpotLightActor SpotLightActor)
    {
        Shader!.SetVector3("lightInfo.CameraPosition", Camera.WorldPosition);
        Shader!.SetVector3("lightInfo.LightPosition", SpotLightActor.WorldPosition);
        Shader!.SetVector3("lightInfo.Direction", SpotLightActor.ForwardVector);
        Shader!.SetFloat("lightInfo.Distance", SpotLightActor.Distance);
        Shader!.SetVector3("lightInfo.Color", SpotLightActor.LightColorVec3);
        Shader!.SetFloat("lightInfo.InteriorCosine", MathF.Cos(SpotLightActor.InteriorAngle.DegreeToRadians()));
        Shader!.SetFloat("lightInfo.ExteriorCosine", MathF.Cos(SpotLightActor.ExteriorAngle.DegreeToRadians()));

        Shader!.SetMatrix("Projection", Camera.ProjectTransform);
        Shader!.SetMatrix("View", Camera.ViewTransform);
    }

    public override void Uninitialize(GL gl)
    {
    }
}
