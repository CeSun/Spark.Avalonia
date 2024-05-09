using Silk.NET.OpenGLES;
using Spark.Actors;
using Spark.Assets;
using Spark.RenderTarget;
using Spark.Util;
using System.Drawing;
using System.Numerics;

namespace Spark.Renderers;

public class ForwardRenderer : BaseRenderer
{
    private readonly CustomRenderTarget _baseRenderTarget = new CustomRenderTarget(0, 0) { IsHdr = true, HasStencil = false };
    private readonly CustomRenderTarget _postProcessRenderTarget = new CustomRenderTarget(0, 0) { IsHdr = true, HasStencil = false, Filter = TextureFilter.Liner };

    private Shader? _ambientLightOpaqueShader;
    private Shader? _directionLightOpaqueShader;
    private Shader? _pointLightOpaqueShader;
    private Shader? _spotLightOpaqueShader;

    private Shader? _ambientLightMaskedShader;
    private Shader? _directionLightMaskedShader;
    private Shader? _pointLightMaskedShader;
    private Shader? _spotLightMaskedShader;
    private Shader? _translucentShader;

    private Shader? _postProcessShader;
    private Shader? _fxaaShader;

    private (uint Vao, uint Vbo, uint Ebo) _postProcessElement;
#if DEBUG
    private readonly GlDebugGroup _ambientLightGroup = new("AmbientLight Pass");
    private readonly GlDebugGroup _basePassGroup = new("Base Pass");
    private readonly GlDebugGroup _directionLightGroup = new("DirectionLight Pass");
    private readonly GlDebugGroup _pointLightGroup = new("PointLight Pass");
    private readonly GlDebugGroup _spotLightGroup = new("SpotLight Pass");
    private readonly GlDebugGroup _postProcessGroup = new("PostProcess Pass");
#endif
    public override void Render(GL gl, CameraActor camera)
    {
        gl.Viewport(new Size(camera.RenderTarget.Width, camera.RenderTarget.Height));
        RenderTargetResize(camera);
        base.Render(gl, camera);
        using (_baseRenderTarget.Use(gl))
        {
            Clear(gl, camera);
            BasePass(gl, camera);
            TranslucentPass(gl, camera);
        }
        PostProcessPass(gl, camera);
    }

    private void RenderTargetResize(CameraActor camera)
    {
        if (camera.RenderTarget.Width > _baseRenderTarget.Width) 
        {
            _baseRenderTarget.Resize(camera.RenderTarget.Width, _baseRenderTarget.Height);
            _postProcessRenderTarget.Resize(camera.RenderTarget.Width, _baseRenderTarget.Height);
        }
        if (camera.RenderTarget.Height > _baseRenderTarget.Height)
        {
            _baseRenderTarget.Resize(_baseRenderTarget.Width, camera.RenderTarget.Height);
            _postProcessRenderTarget.Resize(camera.RenderTarget.Width, _baseRenderTarget.Height);
        }
    }

    private void Clear(GL gl, CameraActor camera)
    {
        gl.DepthMask(true);
        var clearFlag = ClearBufferMask.None;
        if ((camera.ClearFlag & CameraClearFlag.DepthFlag) == CameraClearFlag.DepthFlag)
            clearFlag |= ClearBufferMask.DepthBufferBit;
        if ((camera.ClearFlag & CameraClearFlag.ColorFlag) == CameraClearFlag.ColorFlag)
        {
            gl.ClearColor(camera.ClearColor);
            clearFlag |= ClearBufferMask.ColorBufferBit;
        }
        if ((camera.ClearFlag & CameraClearFlag.Skybox) == CameraClearFlag.Skybox)
            clearFlag = ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit;
        gl.Clear(clearFlag);
        if ((camera.ClearFlag & CameraClearFlag.Skybox) == CameraClearFlag.Skybox)
            RenderSkybox(gl, camera);
    }
    public void RenderSkybox(GL gl, CameraActor camera)
    {

    }

    private unsafe void PostProcessPass(GL gl, CameraActor camera)
    {
#if DEBUG
        using var _ = _postProcessGroup.PushGroup(gl);
#endif
        using(_postProcessRenderTarget.Use(gl))
        {
            gl.DepthMask(false);
            gl.Disable(EnableCap.Blend);
            gl.Disable(EnableCap.DepthTest);
            using (_postProcessShader!.Use(gl))
            {
                _postProcessShader!.SetInt("ColorTexture", 0);
                _postProcessShader.SetVector2("RealRenderTargetSize", new Vector2(_baseRenderTarget.Width, _baseRenderTarget.Height));
                _postProcessShader.SetVector2("CameraRenderTargetSize", new Vector2(camera.RenderTarget.Width, camera.RenderTarget.Height));
                gl.ActiveTexture(GLEnum.Texture0);
                gl.BindTexture(GLEnum.Texture2D, _baseRenderTarget.ColorId);
                gl.BindVertexArray(_postProcessElement.Vao);
                gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
            }
        }

        using (camera.RenderTarget.Use(gl))
        {
            using (_fxaaShader!.Use(gl))
            {
                _fxaaShader.SetInt("ColorTexture", 0);
                _fxaaShader.SetVector2("RealRenderTargetSize", new Vector2(_postProcessRenderTarget.Width, _postProcessRenderTarget.Height));
                _fxaaShader.SetVector2("CameraRenderTargetSize", new Vector2(camera.RenderTarget.Width, camera.RenderTarget.Height));
                gl.ActiveTexture(GLEnum.Texture0);
                gl.BindTexture(GLEnum.Texture2D, _postProcessRenderTarget.ColorId);
                gl.BindVertexArray(_postProcessElement.Vao);
                gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
            }
        }
    }
    public void BasePass(GL gl, CameraActor camera)
    {
#if DEBUG
        using var _ = _basePassGroup.PushGroup(gl);
#endif
        gl.Disable(EnableCap.Blend);
        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Less);
        AmbientLight(gl, camera);

        gl.Disable(EnableCap.AlphaTest);
        gl.DepthMask(false);
        gl.DepthFunc(DepthFunction.Equal);
        gl.Enable(EnableCap.DepthTest);
        gl.Disable(EnableCap.Blend);

        gl.Enable(EnableCap.Blend);
        gl.BlendEquation(GLEnum.FuncAdd);
        gl.BlendFunc(GLEnum.One, GLEnum.One);
        DirectionLightPass(gl, camera);
        PointLightPass(gl, camera);
        SpotLightPass(gl, camera);

    }

    public void DirectionLightPass(GL gl, CameraActor camera)
    {
#if DEBUG
        using var _ = _directionLightGroup.PushGroup(gl);
#endif
        foreach (var directionLight in camera.Engine.DirectionLightActors)
        {
            using (_directionLightOpaqueShader!.Use(gl))
            {
                _directionLightOpaqueShader!.SetVector3("Light.CameraPosition", camera.WorldPosition);
                _directionLightOpaqueShader!.SetVector3("Light.Color", directionLight.LightColorVec3);
                _directionLightOpaqueShader!.SetVector3("Light.Direction", directionLight.ForwardVector);
                _directionLightOpaqueShader!.SetMatrix("Projection", camera.ProjectTransform);
                _directionLightOpaqueShader!.SetMatrix("View", camera.ViewTransform);

                foreach (var proxy in OpaqueStaticMeshes)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, _directionLightOpaqueShader, proxy);
                }
            }

            using (_directionLightMaskedShader!.Use(gl))
            {
                _directionLightMaskedShader!.SetVector3("Light.CameraPosition", camera.WorldPosition);
                _directionLightMaskedShader!.SetVector3("Light.Color", directionLight.LightColorVec3);
                _directionLightMaskedShader!.SetVector3("Light.Direction", directionLight.ForwardVector);
                _directionLightMaskedShader!.SetMatrix("Projection", camera.ProjectTransform);
                _directionLightMaskedShader!.SetMatrix("View", camera.ViewTransform);
                foreach (var proxy in MaskedStaticMeshes)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, _directionLightMaskedShader, proxy);
                }
            }
        }
    }

    public void SpotLightPass(GL gl, CameraActor camera)
    {
#if DEBUG
        using var _ = _spotLightGroup.PushGroup(gl);
#endif
        foreach (var spotLightActor in SpotLightActors)
        {
            using (_spotLightOpaqueShader!.Use(gl))
            {
                _spotLightOpaqueShader!.SetVector3("Light.CameraPosition", camera.WorldPosition);
                _spotLightOpaqueShader!.SetVector3("Light.LightPosition", spotLightActor.WorldPosition);
                _spotLightOpaqueShader!.SetVector3("Light.Direction", spotLightActor.ForwardVector);
                _spotLightOpaqueShader!.SetFloat("Light.Distance", spotLightActor.Distance);
                _spotLightOpaqueShader!.SetVector3("Light.Color", spotLightActor.LightColorVec3);
                _spotLightOpaqueShader!.SetFloat("Light.InteriorCosine", MathF.Cos(spotLightActor.InteriorAngle.DegreeToRadians()));
                _spotLightOpaqueShader!.SetFloat("Light.ExteriorCosine", MathF.Cos(spotLightActor.ExteriorAngle.DegreeToRadians()));
                _spotLightOpaqueShader!.SetMatrix("Projection", camera.ProjectTransform);
                _spotLightOpaqueShader!.SetMatrix("View", camera.ViewTransform);
                foreach (var proxy in OpaqueStaticMeshes)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, _spotLightOpaqueShader, proxy);
                }
            }

            using (_spotLightMaskedShader!.Use(gl))
            {
                _spotLightMaskedShader!.SetVector3("Light.CameraPosition", camera.WorldPosition);
                _spotLightMaskedShader!.SetVector3("Light.LightPosition", spotLightActor.WorldPosition);
                _spotLightMaskedShader!.SetVector3("Light.Direction", spotLightActor.ForwardVector);
                _spotLightMaskedShader!.SetFloat("Light.Distance", spotLightActor.Distance);
                _spotLightMaskedShader!.SetVector3("Light.Color", spotLightActor.LightColorVec3);
                _spotLightMaskedShader!.SetFloat("Light.InteriorCosine", MathF.Cos(spotLightActor.InteriorAngle.DegreeToRadians()));
                _spotLightMaskedShader!.SetFloat("Light.ExteriorCosine", MathF.Cos(spotLightActor.ExteriorAngle.DegreeToRadians()));
                _spotLightMaskedShader!.SetMatrix("Projection", camera.ProjectTransform);
                _spotLightMaskedShader!.SetMatrix("View", camera.ViewTransform);
                foreach (var proxy in MaskedStaticMeshes)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, _spotLightMaskedShader, proxy);
                }
            }
        }
    }
    public void PointLightPass(GL gl, CameraActor camera)
    {
#if DEBUG
        using var _ = _pointLightGroup.PushGroup(gl);
#endif
        foreach(var pointLight in PointLightActors)
        {
            using (_pointLightOpaqueShader!.Use(gl))
            {
                _pointLightOpaqueShader!.SetVector3("Light.CameraPosition", camera.WorldPosition);
                _pointLightOpaqueShader!.SetVector3("Light.Color", pointLight.LightColorVec3);
                _pointLightOpaqueShader!.SetVector3("Light.LightPosition", pointLight.WorldPosition);
                _pointLightOpaqueShader!.SetFloat("Light.AttenuationFactor", pointLight.AttenuationFactor);
                _pointLightOpaqueShader!.SetMatrix("Projection", camera.ProjectTransform);
                _pointLightOpaqueShader!.SetMatrix("View", camera.ViewTransform);
                foreach (var proxy in OpaqueStaticMeshes)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, _pointLightOpaqueShader, proxy);
                }
            }
            using (_pointLightMaskedShader!.Use(gl))
            {
                _pointLightMaskedShader!.SetVector3("Light.CameraPosition", camera.WorldPosition);
                _pointLightMaskedShader!.SetVector3("Light.Color", pointLight.LightColorVec3);
                _pointLightMaskedShader!.SetVector3("Light.LightPosition", pointLight.WorldPosition);
                _pointLightMaskedShader!.SetFloat("Light.AttenuationFactor", pointLight.AttenuationFactor);
                _pointLightMaskedShader!.SetMatrix("Projection", camera.ProjectTransform);
                _pointLightMaskedShader!.SetMatrix("View", camera.ViewTransform);
                foreach (var proxy in MaskedStaticMeshes)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    RenderStaticMesh(gl, _pointLightOpaqueShader, proxy);
                }
            }
        }
    }
    private unsafe void RenderStaticMesh(GL gl, Shader shader, ElementProxy proxy)
    {
        shader.SetMatrix("Model", proxy.ModelTransform);
        if (proxy.Element.Material?.BaseColor != null)
        {
            shader.SetFloat("HasBaseColor", 1);
            shader.SetInt("BaseColorTexture", 0);
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.BaseColor!.TextureId);
        }
        else
        {

            shader.SetFloat("HasBaseColor", 0);
            shader.SetInt("BaseColorTexture", 0);
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, 0);
        }

        if (proxy.Element.Material?.Normal != null)
        {
            shader.SetFloat("HasNormal", 1);
            shader.SetInt("NormalTexture", 1);
            gl.ActiveTexture(GLEnum.Texture1);
            gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.Normal!.TextureId);
        }
        else
        {

            shader.SetFloat("HasNormal", 0);
            shader.SetInt("NormalTexture", 1);
            gl.ActiveTexture(GLEnum.Texture1);
            gl.BindTexture(GLEnum.Texture2D, 0);
        }

        gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
        gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
    }
    public override void Initialize(GL gl)
    {
        base.Initialize(gl);
        _postProcessElement = CreateQuad(gl);
        _directionLightOpaqueShader = gl.CreateShader("Light.vert", "Light.frag", ["_SHADERMODEL_BLINNPHONG_", "_DIRECTIONLIGHT_"]);
        _pointLightOpaqueShader = gl.CreateShader("Light.vert", "Light.frag", ["_SHADERMODEL_BLINNPHONG_", "_POINTLIGHT_"]);
        _spotLightOpaqueShader = gl.CreateShader("Light.vert", "Light.frag", ["_SHADERMODEL_BLINNPHONG_", "_SPOTLIGHT_"]);
        _ambientLightOpaqueShader = gl.CreateShader("AmbientLight.vert", "AmbientLight.frag", ["_SHADERMODEL_BLINNPHONG_LAMBERT_"]);


        _ambientLightMaskedShader = gl.CreateShader("AmbientLight.vert", "AmbientLight.frag", ["_SHADERMODEL_BLINNPHONG_LAMBERT_" , "_BLENDMODE_MASKED_"]);
        _directionLightMaskedShader = gl.CreateShader("Light.vert", "Light.frag", ["_SHADERMODEL_BLINNPHONG_", "_DIRECTIONLIGHT_", "_BLENDMODE_MASKED_"]);
        _pointLightMaskedShader = gl.CreateShader("Light.vert", "Light.frag", ["_SHADERMODEL_BLINNPHONG_", "_POINTLIGHT_", "_BLENDMODE_MASKED_"]);
        _spotLightMaskedShader = gl.CreateShader("Light.vert", "Light.frag", ["_SHADERMODEL_BLINNPHONG_", "_SPOTLIGHT_", "_BLENDMODE_MASKED_"]);


        _translucentShader = gl.CreateShader("AmbientLight.vert", "Translucent.frag", ["_SHADERMODEL_BLINNPHONG_LAMBERT_"]);
        _postProcessShader = gl.CreateShader("PostProcess.vert", "PostProcess.frag");

        _fxaaShader = gl.CreateShader("PostProcess.vert", "FXAA.frag");
    }
    public unsafe void AmbientLight(GL gl, CameraActor camera)
    {
#if DEBUG
        using var _ = _ambientLightGroup.PushGroup(gl);
#endif
        using (_ambientLightOpaqueShader!.Use(gl))
        {
            _ambientLightOpaqueShader.SetFloat("AmbientStrength", 0.05f);
            _ambientLightOpaqueShader!.SetMatrix("Projection", camera.ProjectTransform);
            _ambientLightOpaqueShader!.SetMatrix("View", camera.ViewTransform);
            foreach (var proxy in OpaqueStaticMeshes)
            {
                _ambientLightOpaqueShader.SetMatrix("Model", proxy.ModelTransform);

                if (proxy.Element.Material?.BaseColor != null)
                {
                    _ambientLightOpaqueShader!.SetFloat("HasBaseColor", 1);
                    _ambientLightOpaqueShader!.SetInt("BaseColorTexture", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.BaseColor!.TextureId);
                }
                else
                {

                    _ambientLightOpaqueShader!.SetFloat("HasBaseColor", 0);
                    _ambientLightOpaqueShader!.SetInt("BaseColorTexture", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, 0);
                }

                gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
            }
        }
        using(_ambientLightMaskedShader!.Use(gl))
        {
            _ambientLightMaskedShader.SetFloat("AmbientStrength", 0.05f);
            _ambientLightMaskedShader.SetMatrix("Projection", camera.ProjectTransform);
            _ambientLightMaskedShader.SetMatrix("View", camera.ViewTransform);
            foreach (var proxy in MaskedStaticMeshes)
            {
                _ambientLightMaskedShader.SetMatrix("Model", proxy.ModelTransform);
                if (proxy.Element.Material?.BaseColor != null)
                {
                    _ambientLightMaskedShader!.SetFloat("HasBaseColor", 1);
                    _ambientLightMaskedShader!.SetInt("BaseColorTexture", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.BaseColor!.TextureId);
                }
                else
                {
                    _ambientLightMaskedShader!.SetFloat("HasBaseColor", 0);
                    _ambientLightMaskedShader!.SetInt("BaseColorTexture", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, 0);
                }
                gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
            }
        }
    }

    private unsafe void TranslucentPass(GL gl, CameraActor camera)
    {
        gl.Enable(EnableCap.Blend);
        gl.BlendEquation(GLEnum.FuncAdd);
        gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Less);
        TranslucentStaticMeshes.Sort((left, _) =>
        {
            if ((left.ModelTransform.Translation - camera.WorldPosition).LengthSquared() <
                (left.ModelTransform.Translation - camera.WorldPosition).LengthSquared())
                return 1;
            else return -1;
        });
        using(_translucentShader!.Use(gl))
        {
            _translucentShader.SetFloat("AmbientStrength", 0.05f);
            _translucentShader.SetMatrix("Projection", camera.ProjectTransform);
            _translucentShader.SetMatrix("View", camera.ViewTransform);
            foreach (var proxy in TranslucentStaticMeshes)
            {
                _translucentShader.SetMatrix("Model", proxy.ModelTransform);
                if (proxy.Element.Material?.BaseColor != null)
                {
                    _translucentShader!.SetFloat("HasBaseColor", 1);
                    _translucentShader!.SetInt("BaseColorTexture", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.BaseColor!.TextureId);
                }
                else
                {
                    _translucentShader!.SetFloat("HasBaseColor", 0);
                    _translucentShader!.SetInt("BaseColorTexture", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, 0);
                }
                gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
            }
        }
    }
}
