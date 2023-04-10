using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthOutLine : ScriptableRendererFeature
{
    [System.Serializable]
    public class OutLineSettings {

        public Material outLineMaterial = null;
        [Range(0.0f,20.0f)]
        public float LineWidth=1.0f;
        public float speed=1.0f;
        //public float range
        public string textureId = "_ScreenTex";
        public float rangeMin=0.0f;
        public float rangeMax=150.0f;
        public Vector4 center = Vector4.zero;
    }
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingTransparents;

    public OutLineSettings settings = new OutLineSettings();

    RenderTargetHandle renderTargetHandle;

    DepthOutlinePass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new DepthOutlinePass(settings.outLineMaterial,settings.speed,settings.LineWidth,settings.rangeMin,settings.rangeMax,settings.center);
        m_ScriptablePass.renderPassEvent = Event;
        renderTargetHandle.Init(settings.textureId);
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.SetUp(renderer.cameraColorTarget, renderTargetHandle);
        renderer.EnqueuePass(m_ScriptablePass);
    }
    //void Update() { }
}
public class DepthOutlinePass : ScriptableRenderPass
{
    string q_RenderTag = "Depth Scan PostProcessing";
    private Material m_material;
    private RenderTargetHandle shaderPropertyHandle;
    private float speed;
    private float width;
    private float rangeMin;
    private float rangeMax;
    private Vector4 center;
    
    private Camera camera;
    private Transform cameraTrans;
    //private Color m_Color;

    private RenderTargetIdentifier source;

    private RenderTextureDescriptor m_OpaqueDesc;
    public DepthOutlinePass(Material material,float speed,float width,float rangeMin,float rangeMax,Vector4 center) {
        this.m_material = material;
        this.speed = speed;
        this.width = width;
        this.rangeMin = rangeMin;
        this.rangeMax = rangeMax;
        this.center = center;
    }
    public void SetUp(RenderTargetIdentifier source,RenderTargetHandle rth) {
        this.source = source;
        shaderPropertyHandle = rth;
        camera = Camera.main;
        cameraTrans = camera.transform;

    }
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cameraTextureDescriptor.msaaSamples = 1;
        m_OpaqueDesc = cameraTextureDescriptor;
        //cmd.GetTemporaryRT(m_colorHandle.id,cameraTextureDescriptor,FilterMode.Bilinear);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (m_material == null)
            return;


        Matrix4x4 farFarClipPos=Matrix4x4.identity;

        //计算远平面 这里先只处理透视相机
        float fov = camera.fieldOfView;
        float near = camera.nearClipPlane;
        float far = camera.farClipPlane;
        float aspect = camera.aspect;//视锥宽高比

        float halfHeight = far * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * aspect;
        Vector3 toRight = halfWidth * camera.transform.right;
        Vector3 toTop = halfHeight * camera.transform.up;


        Vector3 farCenter = camera.transform.forward * far;
        Vector3 farTopLeft = farCenter + toTop - toRight;
        Vector3 farTopRight = farCenter + toTop + toRight;
        Vector3 farBottomLeft = farCenter - toTop - toRight;
        Vector3 farBottomRight = farCenter - toTop + toRight;

        farFarClipPos.SetRow(0, farBottomLeft);
        farFarClipPos.SetRow(1, farBottomRight);
        farFarClipPos.SetRow(2, farTopRight);
        farFarClipPos.SetRow(3, farTopLeft);

        m_material.SetMatrix("_FrustumCornersRay", farFarClipPos);
        // m_material.SetMatrix("_ViewProjectionInverseMatrix", (camera.projectionMatrix * camera.worldToCameraMatrix).inverse);

        //Matrix4x4 frustumCorners=
        CommandBuffer buffer = CommandBufferPool.Get(q_RenderTag);
        int screenCopyID = Shader.PropertyToID("_ScreenCopyTex");
        buffer.GetTemporaryRT(screenCopyID,m_OpaqueDesc,FilterMode.Bilinear);
        buffer.Blit(source,screenCopyID);
        m_material.SetFloat("_Speed",speed);
        m_material.SetFloat("_DepthWidth", width);
        m_material.SetFloat("_RangeMin", rangeMin);
        m_material.SetFloat("_RangeMax", rangeMax);
        m_material.SetVector("center",center);

        buffer.Blit(screenCopyID,source,m_material);

        buffer.ReleaseTemporaryRT(screenCopyID);
        context.ExecuteCommandBuffer(buffer);
        CommandBufferPool.Release(buffer);
    }
}
