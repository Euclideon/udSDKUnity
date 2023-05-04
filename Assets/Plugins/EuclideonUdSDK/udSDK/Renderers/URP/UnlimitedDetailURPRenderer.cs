using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using udSDK;


public class UnlimitedDetailURPRenderer : ScriptableRendererFeature
{
  public Material postProcessMaterial;

  private URPPostProcessPass postProcessPass;

  public override void Create()
  {
    postProcessPass = new URPPostProcessPass(postProcessMaterial);
    postProcessPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
  }

  public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
  {
    renderer.EnqueuePass(postProcessPass);
  }
}

public class URPPostProcessPass : ScriptableRenderPass
{
  private Material postProcessMaterial;

  private RenderTargetIdentifier source;
  private RenderTargetHandle destination;

  private const string RenderTargetName = "_TempTarget";
  
  private int width = 1280;
  private int height = 1280;
  private Color32[] colourBuffer;
  private Texture2D colourTexture;
  private float[] depthBuffer;
  private Texture2D depthTexture;
  private UDRenderTarget vRenderView;
  
  public URPPostProcessPass(Material postProcessMaterial)
  {
    this.postProcessMaterial = postProcessMaterial;
    destination.Init(RenderTargetName);
    GlobalUDContext.Login();
    InitialiseBuffers(width, height);
    InitialiseTextures();
    vRenderView = new UDRenderTarget();
    vRenderView.Create(GlobalUDContext.uContext, GlobalUDContext.renderer, (uint)width, (uint)height);
    vRenderView.SetTargets(ref colourBuffer, 0, ref depthBuffer);
  }
  
  private void InitialiseTextures()
  {
    if (colourTexture == null)
      colourTexture = new Texture2D(width, height, TextureFormat.BGRA32, false);

    if (depthTexture == null)
      depthTexture = new Texture2D(width, height, TextureFormat.RFloat, false);
  }
  private void InitialiseBuffers(int newWidth, int newHeight)
  {
    width = newWidth;
    height = newHeight;
    colourBuffer = new Color32[width * height];
    depthBuffer = new float[width * height];
  }

  void RebuildBuffers(int newWidth, int newHeight)
  {
    if (colourTexture == null || depthTexture == null)
      InitialiseTextures();
    InitialiseBuffers(newWidth, newHeight);
#if UNITY_2021_3_OR_NEWER
    colourTexture.Reinitialize(width, height, TextureFormat.BGRA32, false);
    depthTexture.Reinitialize(width, height, TextureFormat.RFloat, false);
#else 
    colourTexture.Resize(width, height, TextureFormat.BGRA32, false);
    depthTexture.Resize(width, height, TextureFormat.RFloat, false);
#endif
    vRenderView.Destroy();
    vRenderView.Create(GlobalUDContext.uContext, GlobalUDContext.renderer, (uint)width, (uint)height);
    vRenderView.SetTargets(ref colourBuffer, 0, ref depthBuffer);
  }

  public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
  {
    RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
    descriptor.depthBufferBits = 0;
    
    source = renderingData.cameraData.renderer.cameraColorTarget;

    // CommandBuffer cmd = CommandBufferPool.Get("URP Post-processing");
    cmd.GetTemporaryRT(destination.id, descriptor);
  }

  public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
  {
    Camera cam = renderingData.cameraData.camera;
    cam.depthTextureMode |= DepthTextureMode.Depth;
    if (!GlobalUDContext.isCreated)
      return;
    
    UDCameraOptions optionsContainer = cam.GetComponent<UDCameraOptions>();
    UDRenderSettings options;
    float resolutionScaling;
    if (optionsContainer != null)
    {
      options = optionsContainer.optionsStruct;
      resolutionScaling = optionsContainer.resolutionScaling;
    }
    else
    {
      optionsContainer = null;
      options = new UDRenderSettings();
      resolutionScaling = 1;
    }

    int newWidth = (int)(renderingData.cameraData.cameraTargetDescriptor.width * resolutionScaling);
    int newHeight = (int)(renderingData.cameraData.cameraTargetDescriptor.height * resolutionScaling);
    if (newWidth  != width || newHeight != height)
      RebuildBuffers( newWidth,  newHeight);

    GameObject[] objects = GameObject.FindGameObjectsWithTag("UDSModel");
    udRenderInstance[] modelArray = UDUtilities.getUDSInstances();
    if (modelArray.Length <= 0)
      return;
    
    vRenderView.SetMatrix(udSDK.udRenderTargetMatrix.udRTM_View, UDUtilities.GetUDMatrix(cam.worldToCameraMatrix));
    vRenderView.SetMatrix(udSDK.udRenderTargetMatrix.udRTM_Projection, UDUtilities.GetUDMatrix(cam.projectionMatrix));

    //interface to input render options: this allows setting of render flags, picking and filtering from unity objects attached to the camera
    GlobalUDContext.renderer.Render(vRenderView, modelArray, modelArray.Length, options);

    //pass the depth buffer back to the unity interface for further processing:
    if (optionsContainer != null && optionsContainer.recordDepthBuffer) 
      optionsContainer.setDepthImageFromZ(depthBuffer);//for as yet unimplemented features
            
    //make sure that the textures exist before operating on them
    if (colourTexture == null || depthTexture == null)
      InitialiseTextures();

    colourTexture.SetPixels32(colourBuffer);
    colourTexture.Apply();

    depthTexture.LoadRawTextureData<float>(new Unity.Collections.NativeArray<float>(depthBuffer, Unity.Collections.Allocator.Temp));
    depthTexture.Apply();
    
    postProcessMaterial.SetTexture("_udCol", colourTexture);
    postProcessMaterial.SetTexture("_udDep", depthTexture);
    
    CommandBuffer cmd = CommandBufferPool.Get();
    
    Blit(cmd, source, destination.Identifier(), postProcessMaterial);
    Blit(cmd, destination.Identifier(), source);

    context.ExecuteCommandBuffer(cmd);
    CommandBufferPool.Release(cmd);
  }

  public override void OnCameraCleanup(CommandBuffer cmd)
  {
    cmd.ReleaseTemporaryRT(destination.id);
  }
}