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

public class ForwardRenderer : BaseRenderer
{
    RenderFeatures RenderFeatures;
    public ForwardRenderer(RenderFeatures renderFeatures)
    {
        this.RenderFeatures = renderFeatures;
        this.ShaderModelPassMap.Add(ShaderModel.BlinnPhong, new BlinnPhongRenderPass());
    }


    readonly List<ElementProxy> LambertStaticMeshs = new();
    readonly List<ElementProxy> BlinnPhongStaticMeshs = new();

    Shader? PreZMaskedShader = null;
    Shader? PreZOpaqueShader = null;
    Shader? AmbientLightShader = null;
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
        foreach(var (_, pass) in ShaderModelPassMap)
        {
            pass.PreRender(gl, Camera);
        }
        gl.BlendEquation(GLEnum.FuncAdd);
        gl.BlendFunc(GLEnum.One, GLEnum.One);
        gl.Enable(EnableCap.Blend);

        DirectionLightPass(gl, Camera);
        PointLightPass(gl, Camera);
        SpotLightPass(gl, Camera);

        foreach (var (_, pass) in ShaderModelPassMap)
        {
            pass.PostRender(gl, Camera);
        }
    }

    public unsafe void DirectionLightPass(GL gl, CameraActor Camera)
    {
#if DEBUG
        using var _ = DirectionLightGroup.PushGroup(gl);
#endif
        foreach (var directionLight in Camera.Engine.DirectionLightActors)
        {
            foreach (var (_, pass) in ShaderModelPassMap)
            {
                using (pass.DirectionLightShader!.Use(gl))
                {
                    pass.SetupDirectionLightInfo(gl, pass.DirectionLightShader, Camera, directionLight);
                    foreach (var proxy in pass.StaticMeshes)
                    {
                        pass.RenderStaticMesh(gl, pass.DirectionLightShader, Camera, proxy);
                    }
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
            foreach (var (_, pass) in ShaderModelPassMap)
            {
                using (pass.SpotLightShader!.Use(gl))
                {
                    pass.SetupSpotLightInfo(gl, pass.SpotLightShader, Camera, spotLightActor);
                    foreach (var proxy in pass.StaticMeshes)
                    {
                        pass.RenderStaticMesh(gl, pass.SpotLightShader, Camera, proxy);
                    }
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
            foreach (var (model, pass) in ShaderModelPassMap)
            {
                using (pass.PointLightShader!.Use(gl))
                {
                    pass.SetupPointLightInfo(gl, pass.PointLightShader, Camera, pointLight);
                    tmpBlinnPhongStaticMesh.Clear();
                    foreach (var proxy in pass.StaticMeshes)
                    {
                        if (proxy.Element.Material == null)
                            continue;
                        if (proxy.Element.Material.ShaderModel != model)
                        {
                            tmpBlinnPhongStaticMesh.Add(proxy);
                            continue;
                        }
                        pass.RenderStaticMesh(gl, pass.PointLightShader, Camera, proxy);
                    }
                    var tmp = tmpStaticMeshs;
                    tmpStaticMeshs = tmpBlinnPhongStaticMesh;
                    tmpBlinnPhongStaticMesh = tmpStaticMeshs;
                }
            }
        }
    }

    public override void Initialize(GL gl)
    {
        base.Initialize(gl);
        PreZMaskedShader = gl.CreateShader("PreZ.vert", "PreZ.frag", new() { "_BLENDMODE_MASKED_" });
        PreZOpaqueShader = gl.CreateShader("PreZ.vert", "PreZ.frag");
    }
    public override void Uninitialize(GL gl)
    {
        base.Uninitialize(gl);
    }
}
