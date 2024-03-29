using SharpGLTF.Schema2;
using Silk.NET.OpenGLES;
using Spark.Actors;
using Spark.Assets;
using Spark.Avalonia.Actors;
using Spark.Avalonia.Assets;
using Spark.Avalonia.Renderers;
using Spark.Util;
using System.Numerics;
using System.Xml.Linq;

namespace Spark.Renderers;

public class ForwardRenderer : BaseRenderer
{
    RenderFeatures RenderFeatures;
    public ForwardRenderer(RenderFeatures renderFeatures)
    {
        this.RenderFeatures = renderFeatures;
    }


    readonly List<ElementProxy> LambertStaticMeshs = new();
    readonly List<ElementProxy> BlinnPhongStaticMeshs = new();

    Shader? PreZMaskedShader = null;
    Shader? PreZOpaqueShader = null;
    Shader? AmbientLightShader = null;
    Shader? DirectionLightShader = null;
    Shader? PointLightShader = null;
    Shader? SpotLightShader = null;
#if DEBUG
    readonly GLDebugGroup PreZGroup = new("PreZ Pass");
    readonly GLDebugGroup OpaqueGroup = new("Opaque Pass");
    readonly GLDebugGroup MaskedGroup = new("Masked Pass");
    readonly GLDebugGroup BasePassGroup = new("Base Pass");
    readonly GLDebugGroup DirectionLightGroup = new("DirectionLight Pass");
    readonly GLDebugGroup PointLightGroup = new("PointLight Pass");
    readonly GLDebugGroup SpotLightGroup = new("SpotLight Pass");
#endif
    public override void Render(GL gl, CameraActor Camera)
    {
        base.Render(gl, Camera);
        using (Camera.RenderTarget.Use(gl))
        {
            Clear(gl, Camera);
            PreZPass(gl, Camera);
            BasePass(gl, Camera);

        }
    }


    private void Clear(GL gl, CameraActor Camera)
    {
        gl.DepthMask(true);
        ClearBufferMask ClearFlag = ClearBufferMask.None;
        if ((Camera.ClearFlag & CameraClearFlag.DepthFlag) == CameraClearFlag.DepthFlag)
            ClearFlag |= ClearBufferMask.DepthBufferBit;
        if ((Camera.ClearFlag & CameraClearFlag.ColorFlag) == CameraClearFlag.ColorFlag)
        {
            gl.ClearColor(Camera.ClearColor);
            ClearFlag |= ClearBufferMask.ColorBufferBit;
        }
        if ((Camera.ClearFlag & CameraClearFlag.Skybox) == CameraClearFlag.Skybox)
            ClearFlag = ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit;
        gl.Clear(ClearFlag);
        if ((Camera.ClearFlag & CameraClearFlag.Skybox) == CameraClearFlag.Skybox)
            RenderSkybox(gl, Camera);
    }
    public unsafe void PreZPass(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = PreZGroup.PushGroup(gl);
#endif
        gl.Disable(EnableCap.Blend);
        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Less);
#if DEBUG
        using (OpaqueGroup.PushGroup(gl))
#endif
        { 
            using (PreZOpaqueShader!.Use(gl))
            {
                PreZOpaqueShader.SetMatrix("Projection", Camera.ProjectTransform);
                PreZOpaqueShader.SetMatrix("View", Camera.ViewTransform);
                foreach (var proxy in OpaqueStaticMeshs)
                {
                    PreZOpaqueShader.SetMatrix("Model", proxy.ModelTransform);
                    gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                    gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
                }
            }
        }
#if DEBUG
        using (MaskedGroup.PushGroup(gl))
#endif
        {
            gl.Enable(EnableCap.AlphaTest);
            using (PreZMaskedShader!.Use(gl))
            {
                PreZMaskedShader.SetMatrix("Projection", Camera.ProjectTransform);
                PreZMaskedShader.SetMatrix("View", Camera.ViewTransform);
                foreach (var proxy in MaskedStaticMeshs)
                {
                    PreZMaskedShader.SetMatrix("Model", proxy.ModelTransform);
                    PreZMaskedShader.SetInt("BaseColor", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.BaseColor!.TextureId);
                    gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                    gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
                }
            }
        }
        

    }

    public void RenderSkybox(GL gl, CameraActor Camera)
    {

    }

    public void BasePass(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = BasePassGroup.PushGroup(gl);
#endif
        
        gl.Disable(EnableCap.AlphaTest);
        gl.DepthMask(false);
        gl.DepthFunc(DepthFunction.Equal);
        gl.Enable(EnableCap.DepthTest);
        gl.Disable(EnableCap.Blend);

        AmbientLight(gl, Camera);
        gl.Enable(EnableCap.Blend);
        gl.BlendEquation(GLEnum.FuncAdd);
        gl.BlendFunc(GLEnum.One, GLEnum.One);
        DirectionLightPass(gl, Camera);
        PointLightPass(gl, Camera);
        SpotLightPass(gl, Camera);

    }

    public unsafe void DirectionLightPass(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = DirectionLightGroup.PushGroup(gl);
#endif
        foreach (var directionLight in Camera.Engine.DirectionLightActors)
        {
            using (DirectionLightShader!.Use(gl))
            {
                DirectionLightShader!.SetVector3("Light.CameraPosition", Camera.WorldPosition);
                DirectionLightShader!.SetVector3("Light.Color", directionLight.LightColorVec3);
                DirectionLightShader!.SetVector3("Light.Direction", directionLight.ForwardVector);
                DirectionLightShader!.SetMatrix("Projection", Camera.ProjectTransform);
                DirectionLightShader!.SetMatrix("View", Camera.ViewTransform);

                foreach (var proxy in OpaqueStaticMeshs)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, DirectionLightShader, Camera, proxy);
                }
                foreach (var proxy in MaskedStaticMeshs)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, DirectionLightShader, Camera, proxy);
                }
            }
        }
    }

    public unsafe void SpotLightPass(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = SpotLightGroup.PushGroup(gl);
#endif
        foreach (var spotLightActor in SpotLightActors)
        {
            using (SpotLightShader!.Use(gl))
            {
                SpotLightShader!.SetVector3("Light.CameraPosition", Camera.WorldPosition);
                SpotLightShader!.SetVector3("Light.LightPosition", spotLightActor.WorldPosition);
                SpotLightShader!.SetVector3("Light.Direction", spotLightActor.ForwardVector);
                SpotLightShader!.SetFloat("Light.Distance", spotLightActor.Distance);
                SpotLightShader!.SetVector3("Light.Color", spotLightActor.LightColorVec3);
                SpotLightShader!.SetFloat("Light.InteriorCosine", MathF.Cos(spotLightActor.InteriorAngle.DegreeToRadians()));
                SpotLightShader!.SetFloat("Light.ExteriorCosine", MathF.Cos(spotLightActor.ExteriorAngle.DegreeToRadians()));
                SpotLightShader!.SetMatrix("Projection", Camera.ProjectTransform);
                SpotLightShader!.SetMatrix("View", Camera.ViewTransform);
                foreach (var proxy in OpaqueStaticMeshs)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, SpotLightShader, Camera, proxy);
                }
                foreach (var proxy in MaskedStaticMeshs)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, SpotLightShader, Camera, proxy);
                }
            }
        }
    }

    List<ElementProxy> tmpStaticMeshs = new();
    List<ElementProxy> tmpBlinnPhongStaticMesh = new();
    public unsafe void PointLightPass(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = PointLightGroup.PushGroup(gl);
#endif
        foreach(var pointLight in PointLightActors)
        {
            
            tmpStaticMeshs.Clear();
            Camera.Engine.Octree.SphereCulling(tmpStaticMeshs, pointLight.BoundingSphere.Sphere);
            using (PointLightShader!.Use(gl))
            {
                PointLightShader!.SetVector3("Light.CameraPosition", Camera.WorldPosition);
                PointLightShader!.SetVector3("Light.Color", pointLight.LightColorVec3);
                PointLightShader!.SetVector3("Light.LightPosition", pointLight.WorldPosition);
                PointLightShader!.SetFloat("Light.AttenuationFactor", pointLight.AttenuationFactor);
                PointLightShader!.SetMatrix("Projection", Camera.ProjectTransform);
                PointLightShader!.SetMatrix("View", Camera.ViewTransform);
                tmpBlinnPhongStaticMesh.Clear();
                foreach (var proxy in OpaqueStaticMeshs)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, PointLightShader, Camera, proxy);
                }
                foreach (var proxy in MaskedStaticMeshs)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, PointLightShader, Camera, proxy);
                }
                var tmp = tmpStaticMeshs;
                tmpStaticMeshs = tmpBlinnPhongStaticMesh;
                tmpBlinnPhongStaticMesh = tmpStaticMeshs;
            }
        }
    }
    private unsafe void RenderStaticMesh(GL gl, Shader Shader, CameraActor Camera, ElementProxy proxy)
    {
        Shader!.SetMatrix("Model", proxy.ModelTransform);
        if (proxy.Element.Material?.Normal != null)
        {
            Shader!.SetFloat("HasBaseColor", 1);
            Shader!.SetInt("BaseColorTexture", 0);
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.BaseColor!.TextureId);
        }
        else
        {

            Shader!.SetFloat("HasBaseColor", 0);
            Shader!.SetInt("BaseColorTexture", 0);
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, 0);
        }

        if (proxy.Element.Material?.Normal != null)
        {
            Shader!.SetFloat("HasNormal", 1);
            Shader!.SetInt("NormalTexture", 1);
            gl.ActiveTexture(GLEnum.Texture1);
            gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.Normal!.TextureId);
        }
        else
        {

            Shader!.SetFloat("HasNormal", 0);
            Shader!.SetInt("NormalTexture", 1);
            gl.ActiveTexture(GLEnum.Texture1);
            gl.BindTexture(GLEnum.Texture2D, 0);
        }

        gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
        gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
    }
    public override void Initialize(GL gl)
    {
        base.Initialize(gl);
        PreZMaskedShader = gl.CreateShader("PreZ.vert", "PreZ.frag", new() { "_BLENDMODE_MASKED_" });
        PreZOpaqueShader = gl.CreateShader("PreZ.vert", "PreZ.frag");
        DirectionLightShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_BLINNPHONG_", "_DIRECTIONLIGHT_", "_PREZ_" });
        PointLightShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_BLINNPHONG_", "_POINTLIGHT_", "_PREZ_" });
        SpotLightShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_BLINNPHONG_", "_SPOTLIGHT_", "_PREZ_" });
        AmbientLightShader = gl.CreateShader("PreZ.vert", "AmbientLight.frag", new() { "_SHADERMODEL_BLINNPHONG_LAMBERT_" });
    }
    public unsafe void AmbientLight(GL gl, CameraActor Camera)
    {
        using (AmbientLightShader!.Use(gl))
        {
            AmbientLightShader.SetFloat("AmbientStrength", 0.05f);
            AmbientLightShader!.SetMatrix("Projection", Camera.ProjectTransform);
            AmbientLightShader!.SetMatrix("View", Camera.ViewTransform);
            foreach (var proxy in OpaqueStaticMeshs)
            {
                AmbientLightShader.SetMatrix("Model", proxy.ModelTransform);
                AmbientLightShader.SetInt("BaseColor", 0);
                gl.ActiveTexture(GLEnum.Texture0);
                gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.BaseColor!.TextureId);
                gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
            }
            foreach (var proxy in MaskedStaticMeshs)
            {
                AmbientLightShader.SetMatrix("Model", proxy.ModelTransform);
                AmbientLightShader.SetInt("BaseColor", 0);
                gl.ActiveTexture(GLEnum.Texture0);
                gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.BaseColor!.TextureId);
                gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
            }
        }
    }
    public override void Uninitialize(GL gl)
    {
        base.Uninitialize(gl);
    }
}
