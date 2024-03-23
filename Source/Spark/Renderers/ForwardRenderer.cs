using SharpGLTF.Schema2;
using Silk.NET.OpenGLES;
using Spark.Actors;
using Spark.Assets;
using Spark.Avalonia.Actors;
using Spark.Avalonia.Assets;
using Spark.Avalonia.Renderers;
using System.Numerics;
using System.Xml.Linq;

namespace Spark.Renderers;

public class ForwardRenderer : IRenderer
{
    RenderFeatures RenderFeatures;
    Dictionary<ShaderModel ,ShaderModelPass> ShaderModelPassMap = new ();
    public ForwardRenderer(RenderFeatures renderFeatures)
    {
        this.RenderFeatures = renderFeatures;
    }

    readonly List<ElementProxy> NeedRenderStaticMeshs = new();
    readonly List<ElementProxy> OpaqueStaticMeshs = new();
    readonly List<ElementProxy> MaskedStaticMeshs = new();
    readonly List<ElementProxy> TranslucentStaticMeshs = new();

    readonly List<ElementProxy> LambertStaticMeshs = new();
    readonly List<ElementProxy> BlinnPhongStaticMeshs = new();
    readonly List<PointLightActor> PointLightActors = new();

    Shader? PreZMaskedShader = null;
    Shader? PreZOpaqueShader = null;
    Shader? BlinnPhongDirectionLightShader = null;
    Shader? BlinnPhongPointLightShader = null;
    Shader? AmbientLightShader = null;
#if DEBUG
    readonly GLDebugGroup PreZGroup = new("PreZ Pass");
    readonly GLDebugGroup OpaqueGroup = new("Opaque Pass");
    readonly GLDebugGroup MaskedGroup = new("Masked Pass");
    readonly GLDebugGroup BasePassGroup = new("Base Pass");
    readonly GLDebugGroup DirectionLightGroup = new("DirectionLight Pass");
    readonly GLDebugGroup PointLightGroup = new("PointLight Pass");
    readonly GLDebugGroup AmbientLightGroup = new("AmbientLight Pass");
#endif
    public void Render(GL gl, CameraActor Camera)
    {
        LightShadowMapPass(gl);
        using (Camera.RenderTarget.Use(gl))
        {
            Filter(Camera);
            Clear(gl, Camera);
            if (RenderFeatures.PreZ == true)
            {
                PreZPass(gl, Camera);
            }
            BasePass(gl, Camera);

        }
    }

    private void Filter(CameraActor Camera)
    {
        OpaqueStaticMeshs.Clear();
        MaskedStaticMeshs.Clear();
        OpaqueStaticMeshs.Clear();
        LambertStaticMeshs.Clear();
        BlinnPhongStaticMeshs.Clear();
        NeedRenderStaticMeshs.Clear();
        PointLightActors.Clear();
        Camera.Engine.Octree.FrustumCulling(NeedRenderStaticMeshs, Camera.GetPlanes());
        Camera.Engine.Octree.FrustumCulling(PointLightActors, Camera.GetPlanes());
        foreach (var proxy in NeedRenderStaticMeshs)
        {
            var element = proxy.Element;
            if (element.Material == null)
                continue;

            // 混合模式
            if (element.Material.BlendMode == Avalonia.Assets.BlendMode.Opaque)
                OpaqueStaticMeshs.Add(proxy);
            else if (element.Material.BlendMode == Avalonia.Assets.BlendMode.Masked)
                MaskedStaticMeshs.Add(proxy);
            else if (element.Material.BlendMode == Avalonia.Assets.BlendMode.Translucent)
                TranslucentStaticMeshs.Add(proxy);

            // 光源模型
            if (element.Material.ShaderModel == Avalonia.Assets.ShaderModel.Lambert)
                LambertStaticMeshs.Add(proxy);
            else if (element.Material.ShaderModel == Avalonia.Assets.ShaderModel.BlinnPhong)
                BlinnPhongStaticMeshs.Add(proxy);
        }
    }

    private void Clear(GL gl, CameraActor Camera)
    {
        if (RenderFeatures.PreZ)
        {
            gl.DepthMask(true);
        }
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
    private void LightShadowMapPass(GL gl)
    {

    }
    public unsafe void PreZPass(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = PreZGroup.PushGroup(gl);
#endif
        gl.Disable(EnableCap.Blend);
        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Less);
        gl.DepthMask(true);
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
                    gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.Diffuse!.TextureId);
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
        gl.BlendEquation(GLEnum.FuncAdd);
        gl.BlendFunc(GLEnum.One, GLEnum.One);
        gl.Enable(EnableCap.Blend);

        DirectionLightPass(gl, Camera);
        PointLightPass(gl, Camera);

    }

    public unsafe void DirectionLightPass(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = DirectionLightGroup.PushGroup(gl);
#endif
        foreach (var directionLight in Camera.Engine.DirectionLightActors)
        {
            using (BlinnPhongDirectionLightShader!.Use(gl))
            {
                BlinnPhongDirectionLightShader!.SetVector3("lightInfo.CameraPosition", Camera.WorldPosition);
                BlinnPhongDirectionLightShader!.SetVector3("lightInfo.Color", directionLight.LightColorVec3);
                BlinnPhongDirectionLightShader!.SetVector3("lightInfo.Direction", directionLight.ForwardVector);

                BlinnPhongDirectionLightShader!.SetMatrix("Projection", Camera.ProjectTransform);
                BlinnPhongDirectionLightShader!.SetMatrix("View", Camera.ViewTransform);
                foreach (var proxy in BlinnPhongStaticMeshs)
                {
                    BlinnPhongDirectionLightShader!.SetMatrix("Model", proxy.ModelTransform);
                    BlinnPhongDirectionLightShader!.SetInt("BaseColor", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.Diffuse!.TextureId);
                    gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                    gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
                }
            }
        }
    }

    readonly List<ElementProxy> tmpStaticMeshs = new();
    readonly List<ElementProxy> tmpBlinnPhongStaticMesh = new();
    public unsafe void PointLightPass(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = PointLightGroup.PushGroup(gl);
#endif
        foreach(var pointLight in PointLightActors)
        {

            tmpStaticMeshs.Clear();
            tmpBlinnPhongStaticMesh.Clear();
            Camera.Engine.Octree.SphereCulling(tmpStaticMeshs, pointLight.BoundingSphere.Sphere);
            using (BlinnPhongPointLightShader!.Use(gl))
            {

                BlinnPhongPointLightShader!.SetVector3("lightInfo.CameraPosition", Camera.WorldPosition);
                BlinnPhongPointLightShader!.SetVector3("lightInfo.Color", pointLight.LightColorVec3);
                BlinnPhongPointLightShader!.SetVector3("lightInfo.LightPosition", pointLight.WorldPosition);
                BlinnPhongPointLightShader!.SetFloat("lightInfo.AttenuationFactor", pointLight.AttenuationFactor);
                
                BlinnPhongPointLightShader!.SetMatrix("Projection", Camera.ProjectTransform);
                BlinnPhongPointLightShader!.SetMatrix("View", Camera.ViewTransform);
                foreach (var proxy in tmpStaticMeshs)
                {
                    if (proxy.Element.Material == null)
                        continue;
                    if (proxy.Element.Material.ShaderModel != Avalonia.Assets.ShaderModel.BlinnPhong)
                    {
                        tmpBlinnPhongStaticMesh.Add(proxy);
                        continue;
                    }
                    BlinnPhongPointLightShader!.SetMatrix("Model", proxy.ModelTransform);
                    BlinnPhongPointLightShader!.SetInt("BaseColor", 0);
                    gl.ActiveTexture(GLEnum.Texture0);
                    gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.Diffuse!.TextureId);
                    gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                    gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
                }
            }
        }
    }

    public unsafe void AmbientLight(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = AmbientLightGroup.PushGroup(gl);
#endif
        using (AmbientLightShader!.Use(gl))
        {
            AmbientLightShader.SetFloat("AmbientStrength", 0.05f);
            AmbientLightShader!.SetMatrix("Projection", Camera.ProjectTransform);
            AmbientLightShader!.SetMatrix("View", Camera.ViewTransform);
            foreach (var proxy in LambertStaticMeshs)
            {
                AmbientLightShader.SetMatrix("Model", proxy.ModelTransform);
                AmbientLightShader.SetInt("BaseColor", 0);
                gl.ActiveTexture(GLEnum.Texture0);
                gl.BindTexture(GLEnum.Texture2D, proxy.Element.Material!.Diffuse!.TextureId);
                gl.BindVertexArray(proxy.Element.VertexArrayObjectIndex);
                gl.DrawElements(GLEnum.Triangles, (uint)proxy.Element.IndicesCount, GLEnum.UnsignedInt, (void*)0);
            }
            foreach (var proxy in BlinnPhongStaticMeshs)
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
    public void Initialize(GL gl)
    {
        PreZMaskedShader = gl.CreateShader("PreZ.vert", "PreZ.frag", new() { "_BLENDMODE_MASKED_" });
        PreZOpaqueShader = gl.CreateShader("PreZ.vert", "PreZ.frag");
        AmbientLightShader = gl.CreateShader("PreZ.vert", "AmbientLight.frag", new() { "_SHADERMODEL_BLINNPHONG_LAMBERT_" });
        BlinnPhongDirectionLightShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_BLINNPHONG_", "_DIRECTIONLIGHT_", "_PREZ_" });
        BlinnPhongPointLightShader = gl.CreateShader("Light.vert", "Light.frag", new() { "_SHADERMODEL_BLINNPHONG_", "_POINTLIGHT_", "_PREZ_" });
    }
    public void Uninitialize(GL gl)
    {
    }
}
