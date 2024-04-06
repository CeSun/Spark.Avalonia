using SharpGLTF.Schema2;
using Silk.NET.OpenGLES;
using Spark.Actors;
using Spark.Assets;
using Spark.RenderTarget;
using Spark.Util;
using System.Drawing;
using System.Numerics;
using System.Xml.Linq;

namespace Spark.Renderers;

public class ForwardRenderer : BaseRenderer
{
    RenderFeatures RenderFeatures;
    CustomRenderTarget BaseRenderTarget = new CustomRenderTarget(0, 0) { IsHdr = true, HasStencil = false };
    CustomRenderTarget PostProcessRenderTarget = new CustomRenderTarget(0, 0) { IsHdr = true, HasStencil = false, Filter = TextureFilter.Liner };
    public ForwardRenderer(RenderFeatures renderFeatures)
    {
        this.RenderFeatures = renderFeatures;
    }

    Shader? AmbientLightOpaqueShader = null;
    Shader? DirectionLightOpaqueShader = null;
    Shader? PointLightOpaqueShader = null;
    Shader? SpotLightOpaqueShader = null;

    Shader? AmbientLightMaskedShader = null;
    Shader? DirectionLightMaskedShader = null;
    Shader? PointLightMaskedShader = null;
    Shader? SpotLightMaskedShader = null;
    Shader? TranslucentShader = null;

    Shader? PostProcessShader = null;
    Shader? FXAAShader = null;

    (uint Vao, uint Vbo, uint Ebo) PostProcessElement;
#if DEBUG
    readonly GLDebugGroup AmbientLightGroup = new("AmbientLight Pass");
    readonly GLDebugGroup OpaqueGroup = new("Opaque Pass");
    readonly GLDebugGroup MaskedGroup = new("Masked Pass");
    readonly GLDebugGroup BasePassGroup = new("Base Pass");
    readonly GLDebugGroup DirectionLightGroup = new("DirectionLight Pass");
    readonly GLDebugGroup PointLightGroup = new("PointLight Pass");
    readonly GLDebugGroup SpotLightGroup = new("SpotLight Pass");
    readonly GLDebugGroup PostProcessGroup = new("PostProcess Pass");
#endif
    public override void Render(GL gl, CameraActor Camera)
    {
        gl.Viewport(new Size(Camera.RenderTarget.Width, Camera.RenderTarget.Height));
        RenderTargetRezie(Camera);
        base.Render(gl, Camera);
        using (BaseRenderTarget.Use(gl))
        {
            Clear(gl, Camera);
            BasePass(gl, Camera);
            TranslucentPass(gl, Camera);
        }
        PostProcessPass(gl, Camera);
    }

    private void RenderTargetRezie(CameraActor Camera)
    {
        if (Camera.RenderTarget.Width > BaseRenderTarget.Width) 
        {
            BaseRenderTarget.Resize(Camera.RenderTarget.Width, BaseRenderTarget.Height);
            PostProcessRenderTarget.Resize(Camera.RenderTarget.Width, BaseRenderTarget.Height);
        }
        if (Camera.RenderTarget.Height > BaseRenderTarget.Height)
        {
            BaseRenderTarget.Resize(BaseRenderTarget.Width, Camera.RenderTarget.Height);
            PostProcessRenderTarget.Resize(Camera.RenderTarget.Width, BaseRenderTarget.Height);
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
    public void RenderSkybox(GL gl, CameraActor Camera)
    {

    }

    private unsafe void PostProcessPass(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = PostProcessGroup.PushGroup(gl);
#endif
        using(PostProcessRenderTarget.Use(gl))
        {
            gl.DepthMask(false);
            gl.Disable(EnableCap.Blend);
            gl.Disable(EnableCap.DepthTest);
            using (PostProcessShader!.Use(gl))
            {
                PostProcessShader!.SetInt("ColorTexture", 0);
                PostProcessShader.SetVector2("RealRenderTargetSize", new Vector2(BaseRenderTarget.Width, BaseRenderTarget.Height));
                PostProcessShader.SetVector2("CameraRenderTargetSize", new Vector2(Camera.RenderTarget.Width, Camera.RenderTarget.Height));
                gl.ActiveTexture(GLEnum.Texture0);
                gl.BindTexture(GLEnum.Texture2D, BaseRenderTarget.ColorId);
                gl.BindVertexArray(PostProcessElement.Vao);
                gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
            }
        }

        using (Camera.RenderTarget.Use(gl))
        {
            using (FXAAShader!.Use(gl))
            {
                FXAAShader.SetInt("ColorTexture", 0);
                FXAAShader.SetVector2("RealRenderTargetSize", new Vector2(PostProcessRenderTarget.Width, PostProcessRenderTarget.Height));
                FXAAShader.SetVector2("CameraRenderTargetSize", new Vector2(Camera.RenderTarget.Width, Camera.RenderTarget.Height));
                gl.ActiveTexture(GLEnum.Texture0);
                gl.BindTexture(GLEnum.Texture2D, PostProcessRenderTarget.ColorId);
                gl.BindVertexArray(PostProcessElement.Vao);
                gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
            }
        }
    }
    public void BasePass(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = BasePassGroup.PushGroup(gl);
#endif
        gl.Disable(EnableCap.Blend);
        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Less);
        AmbientLight(gl, Camera);

        gl.Disable(EnableCap.AlphaTest);
        gl.DepthMask(false);
        gl.DepthFunc(DepthFunction.Equal);
        gl.Enable(EnableCap.DepthTest);
        gl.Disable(EnableCap.Blend);

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
            using (DirectionLightOpaqueShader!.Use(gl))
            {
                DirectionLightOpaqueShader!.SetVector3("Light.CameraPosition", Camera.WorldPosition);
                DirectionLightOpaqueShader!.SetVector3("Light.Color", directionLight.LightColorVec3);
                DirectionLightOpaqueShader!.SetVector3("Light.Direction", directionLight.ForwardVector);
                DirectionLightOpaqueShader!.SetMatrix("Projection", Camera.ProjectTransform);
                DirectionLightOpaqueShader!.SetMatrix("View", Camera.ViewTransform);

                foreach (var proxy in OpaqueStaticMeshs)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, DirectionLightOpaqueShader, Camera, proxy);
                }
            }

            using (DirectionLightMaskedShader!.Use(gl))
            {
                DirectionLightMaskedShader!.SetVector3("Light.CameraPosition", Camera.WorldPosition);
                DirectionLightMaskedShader!.SetVector3("Light.Color", directionLight.LightColorVec3);
                DirectionLightMaskedShader!.SetVector3("Light.Direction", directionLight.ForwardVector);
                DirectionLightMaskedShader!.SetMatrix("Projection", Camera.ProjectTransform);
                DirectionLightMaskedShader!.SetMatrix("View", Camera.ViewTransform);
                foreach (var proxy in MaskedStaticMeshs)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, DirectionLightMaskedShader, Camera, proxy);
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
            using (SpotLightOpaqueShader!.Use(gl))
            {
                SpotLightOpaqueShader!.SetVector3("Light.CameraPosition", Camera.WorldPosition);
                SpotLightOpaqueShader!.SetVector3("Light.LightPosition", spotLightActor.WorldPosition);
                SpotLightOpaqueShader!.SetVector3("Light.Direction", spotLightActor.ForwardVector);
                SpotLightOpaqueShader!.SetFloat("Light.Distance", spotLightActor.Distance);
                SpotLightOpaqueShader!.SetVector3("Light.Color", spotLightActor.LightColorVec3);
                SpotLightOpaqueShader!.SetFloat("Light.InteriorCosine", MathF.Cos(spotLightActor.InteriorAngle.DegreeToRadians()));
                SpotLightOpaqueShader!.SetFloat("Light.ExteriorCosine", MathF.Cos(spotLightActor.ExteriorAngle.DegreeToRadians()));
                SpotLightOpaqueShader!.SetMatrix("Projection", Camera.ProjectTransform);
                SpotLightOpaqueShader!.SetMatrix("View", Camera.ViewTransform);
                foreach (var proxy in OpaqueStaticMeshs)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, SpotLightOpaqueShader, Camera, proxy);
                }
            }

            using (SpotLightMaskedShader!.Use(gl))
            {
                SpotLightMaskedShader!.SetVector3("Light.CameraPosition", Camera.WorldPosition);
                SpotLightMaskedShader!.SetVector3("Light.LightPosition", spotLightActor.WorldPosition);
                SpotLightMaskedShader!.SetVector3("Light.Direction", spotLightActor.ForwardVector);
                SpotLightMaskedShader!.SetFloat("Light.Distance", spotLightActor.Distance);
                SpotLightMaskedShader!.SetVector3("Light.Color", spotLightActor.LightColorVec3);
                SpotLightMaskedShader!.SetFloat("Light.InteriorCosine", MathF.Cos(spotLightActor.InteriorAngle.DegreeToRadians()));
                SpotLightMaskedShader!.SetFloat("Light.ExteriorCosine", MathF.Cos(spotLightActor.ExteriorAngle.DegreeToRadians()));
                SpotLightMaskedShader!.SetMatrix("Projection", Camera.ProjectTransform);
                SpotLightMaskedShader!.SetMatrix("View", Camera.ViewTransform);
                foreach (var proxy in MaskedStaticMeshs)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, SpotLightMaskedShader, Camera, proxy);
                }
            }
        }
    }
    public unsafe void PointLightPass(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = PointLightGroup.PushGroup(gl);
#endif
        foreach(var pointLight in PointLightActors)
        {
            using (PointLightOpaqueShader!.Use(gl))
            {
                PointLightOpaqueShader!.SetVector3("Light.CameraPosition", Camera.WorldPosition);
                PointLightOpaqueShader!.SetVector3("Light.Color", pointLight.LightColorVec3);
                PointLightOpaqueShader!.SetVector3("Light.LightPosition", pointLight.WorldPosition);
                PointLightOpaqueShader!.SetFloat("Light.AttenuationFactor", pointLight.AttenuationFactor);
                PointLightOpaqueShader!.SetMatrix("Projection", Camera.ProjectTransform);
                PointLightOpaqueShader!.SetMatrix("View", Camera.ViewTransform);
                foreach (var proxy in OpaqueStaticMeshs)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, PointLightOpaqueShader, Camera, proxy);
                }
            }
            using (PointLightMaskedShader!.Use(gl))
            {
                PointLightMaskedShader!.SetVector3("Light.CameraPosition", Camera.WorldPosition);
                PointLightMaskedShader!.SetVector3("Light.Color", pointLight.LightColorVec3);
                PointLightMaskedShader!.SetVector3("Light.LightPosition", pointLight.WorldPosition);
                PointLightMaskedShader!.SetFloat("Light.AttenuationFactor", pointLight.AttenuationFactor);
                PointLightMaskedShader!.SetMatrix("Projection", Camera.ProjectTransform);
                PointLightMaskedShader!.SetMatrix("View", Camera.ViewTransform);
                foreach (var proxy in MaskedStaticMeshs)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, PointLightOpaqueShader, Camera, proxy);
                }
            }
        }
    }
    private unsafe void RenderStaticMesh(GL gl, Shader Shader, CameraActor Camera, ElementProxy proxy)
    {
        Shader!.SetMatrix("Model", proxy.ModelTransform);
        if (proxy.Element.Material?.BaseColor != null)
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
        PostProcessElement = CreateQuad(gl);
        DirectionLightOpaqueShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_BLINNPHONG_", "_DIRECTIONLIGHT_" });
        PointLightOpaqueShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_BLINNPHONG_", "_POINTLIGHT_"});
        SpotLightOpaqueShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_BLINNPHONG_", "_SPOTLIGHT_" });
        AmbientLightOpaqueShader = gl.CreateShader("AmbientLight.vert", "AmbientLight.frag", new() { "_SHADERMODEL_BLINNPHONG_LAMBERT_" });


        AmbientLightMaskedShader = gl.CreateShader("AmbientLight.vert", "AmbientLight.frag", new() { "_SHADERMODEL_BLINNPHONG_LAMBERT_" , "_BLENDMODE_MASKED_" });
        DirectionLightMaskedShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_BLINNPHONG_", "_DIRECTIONLIGHT_", "_BLENDMODE_MASKED_" });
        PointLightMaskedShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_BLINNPHONG_", "_POINTLIGHT_", "_BLENDMODE_MASKED_" });
        SpotLightMaskedShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_BLINNPHONG_", "_SPOTLIGHT_", "_BLENDMODE_MASKED_" });


        TranslucentShader = gl.CreateShader("AmbientLight.vert", "Translucent.frag", new() { "_SHADERMODEL_BLINNPHONG_LAMBERT_" });
        PostProcessShader = gl.CreateShader("PostProcess.vert", "PostProcess.frag");

        FXAAShader = gl.CreateShader("PostProcess.vert", "FXAA.frag");
    }
    public unsafe void AmbientLight(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = AmbientLightGroup.PushGroup(gl);
#endif
        using (AmbientLightOpaqueShader!.Use(gl))
        {
            AmbientLightOpaqueShader.SetFloat("AmbientStrength", 0.05f);
            AmbientLightOpaqueShader!.SetMatrix("Projection", Camera.ProjectTransform);
            AmbientLightOpaqueShader!.SetMatrix("View", Camera.ViewTransform);
            foreach (var proxy in OpaqueStaticMeshs)
            {
                AmbientLightOpaqueShader.SetMatrix("Model", proxy.ModelTransform);

                if (proxy.Element.Material?.BaseColor != null)
                {
                    AmbientLightOpaqueShader!.SetFloat("HasBaseColor", 1);
                    AmbientLightOpaqueShader!.SetInt("BaseColorTexture", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.BaseColor!.TextureId);
                }
                else
                {

                    AmbientLightOpaqueShader!.SetFloat("HasBaseColor", 0);
                    AmbientLightOpaqueShader!.SetInt("BaseColorTexture", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, 0);
                }

                gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
            }
        }
        using(AmbientLightMaskedShader!.Use(gl))
        {
            AmbientLightMaskedShader.SetFloat("AmbientStrength", 0.05f);
            AmbientLightMaskedShader.SetMatrix("Projection", Camera.ProjectTransform);
            AmbientLightMaskedShader.SetMatrix("View", Camera.ViewTransform);
            foreach (var proxy in MaskedStaticMeshs)
            {
                AmbientLightMaskedShader.SetMatrix("Model", proxy.ModelTransform);
                if (proxy.Element.Material?.BaseColor != null)
                {
                    AmbientLightMaskedShader!.SetFloat("HasBaseColor", 1);
                    AmbientLightMaskedShader!.SetInt("BaseColorTexture", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.BaseColor!.TextureId);
                }
                else
                {
                    AmbientLightMaskedShader!.SetFloat("HasBaseColor", 0);
                    AmbientLightMaskedShader!.SetInt("BaseColorTexture", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, 0);
                }
                gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
            }
        }
    }

    private unsafe void TranslucentPass(GL gl, CameraActor Camera)
    {
        gl.Enable(EnableCap.Blend);
        gl.BlendEquation(GLEnum.FuncAdd);
        gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Less);
        TranslucentStaticMeshs.Sort((left, right) =>
        {
            if ((left.ModelTransform.Translation - Camera.WorldPosition).LengthSquared() < (left.ModelTransform.Translation - Camera.WorldPosition).LengthSquared())
                return 1;
            else return -1;
        });
        using(TranslucentShader!.Use(gl))
        {
            TranslucentShader.SetFloat("AmbientStrength", 0.05f);
            TranslucentShader.SetMatrix("Projection", Camera.ProjectTransform);
            TranslucentShader.SetMatrix("View", Camera.ViewTransform);
            foreach (var proxy in TranslucentStaticMeshs)
            {
                TranslucentShader.SetMatrix("Model", proxy.ModelTransform);
                if (proxy.Element.Material?.BaseColor != null)
                {
                    TranslucentShader!.SetFloat("HasBaseColor", 1);
                    TranslucentShader!.SetInt("BaseColorTexture", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.BaseColor!.TextureId);
                }
                else
                {
                    TranslucentShader!.SetFloat("HasBaseColor", 0);
                    TranslucentShader!.SetInt("BaseColorTexture", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, 0);
                }
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
