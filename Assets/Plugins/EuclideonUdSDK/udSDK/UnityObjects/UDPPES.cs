using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

using udSDK;
using System.Runtime.InteropServices;

[Serializable]
[PostProcess(typeof(UDPPER), PostProcessEvent.BeforeTransparent, "UD/UDPPES")]
public sealed class UDPPES : PostProcessEffectSettings
{
    [Tooltip("Apply the depth pass from udShader to _CameraDepthTexture")]
    public BoolParameter depthPass = new BoolParameter { value = true };
}
public sealed class UDPPER : PostProcessEffectRenderer<UDPPES>
{
    private int width = 1280;
    private int height = 1280;

    private Color32[] colourBuffer;
    private Texture2D colourTexture;

    private float[] depthBuffer;
    private Texture2D depthTexture;

    private udRenderTarget vRenderView;
    public override void Init()
    {
        try
        {
            GlobalUDContext.Login();
            InitialiseBuffers(width, height);
            InitialiseTextures();
            vRenderView = new udRenderTarget();
            vRenderView.Create(GlobalUDContext.uContext, GlobalUDContext.renderer, (uint)width, (uint)height);
            vRenderView.SetTargets(ref colourBuffer, 0, ref depthBuffer);
        }
        catch
        {
            System.Diagnostics.Debug.WriteLine("Failed to Init");
        }
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
        InitialiseBuffers(newWidth, newHeight);
        colourTexture.Resize(width, height, TextureFormat.BGRA32, false);
        depthTexture.Resize(width, height, TextureFormat.RFloat, false);
        vRenderView.Destroy();
        vRenderView.Create(GlobalUDContext.uContext, GlobalUDContext.renderer, (uint)width, (uint)height);
        vRenderView.SetTargets(ref colourBuffer, 0, ref depthBuffer);
    }

    public override void Render(PostProcessRenderContext context)
    {
        Camera cam = context.camera;
        cam.depthTextureMode |= DepthTextureMode.Depth;
        if (!GlobalUDContext.isCreated)
            return;

        UDCameraOptions optionsContainer = cam.GetComponent<UDCameraOptions>();
        RenderOptions options;
        float resolutionScaling;
        if (optionsContainer != null)
        {
            options = optionsContainer.optionsStruct;
            resolutionScaling = optionsContainer.resolutionScaling;
        }
        else
        {
            optionsContainer = null;
            options = new RenderOptions();
            resolutionScaling = 1;
        }

        if ( (int)context.width*resolutionScaling != width ||  (int)context.height*resolutionScaling != height)
            RebuildBuffers( (int)(context.width*resolutionScaling),  (int)(context.height*resolutionScaling));

        GameObject[] objects = GameObject.FindGameObjectsWithTag("UDSModel");
        udRenderInstance[] modelArray = UDUtilities.getUDSInstances();
        if (modelArray.Length > 0)
        {
            vRenderView.SetMatrix(udSDK.udRenderTargetMatrix.View, UDUtilities.GetUDMatrix(cam.worldToCameraMatrix));
            vRenderView.SetMatrix(udSDK.udRenderTargetMatrix.Projection, UDUtilities.GetUDMatrix(cam.projectionMatrix));

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

            var sheet = context.propertySheets.Get(Shader.Find("Hidden/UDSDK/UDSDKShader"));
            sheet.properties.SetTexture("_udCol", colourTexture);
            sheet.properties.SetTexture("_udDep", depthTexture);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

            if(settings.depthPass)
                context.command.BlitFullscreenTriangle(context.source, BuiltinRenderTextureType.Depth, sheet, 0);
        }
    }
}

