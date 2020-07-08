using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

using Vault;
using System.Runtime.InteropServices;

[Serializable]
[PostProcess(typeof(VDKPPER), PostProcessEvent.BeforeTransparent, "VDK/VDKPPES")]
public sealed class VDKPPES : PostProcessEffectSettings
{
}
public sealed class VDKPPER : PostProcessEffectRenderer<VDKPPES>
{
    private int width = 1280;
    private int height = 1280;

    private Color32[] colourBuffer;
    private Texture2D colourTexture;

    private float[] depthBuffer;
    private Texture2D depthTexture;

    private vdkRenderView vRenderView;
    public override void Init()
    {
        try
        {
            GlobalVDKContext.Login();
            InitialiseBuffers(width, height);
            InitialiseTextures();
            vRenderView = new vdkRenderView();
            vRenderView.Create(GlobalVDKContext.vContext, GlobalVDKContext.renderer, (uint)width, (uint)height);
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
        vRenderView.Create(GlobalVDKContext.vContext, GlobalVDKContext.renderer, (uint)width, (uint)height);
        vRenderView.SetTargets(ref colourBuffer, 0, ref depthBuffer);
    }

    public override void Render(PostProcessRenderContext context)
    {
        Camera cam = context.camera;
        cam.depthTextureMode |= DepthTextureMode.Depth;
        if (!GlobalVDKContext.isCreated)
            return;

        if (context.width != width || context.height != height)
            RebuildBuffers(context.width, context.height);

        GameObject[] objects = GameObject.FindGameObjectsWithTag("UDSModel");
        vdkRenderInstance[] modelArray = UDUtilities.getUDSInstances();
        if (modelArray.Length > 0)
        {
            vRenderView.SetMatrix(Vault.RenderViewMatrix.View, UDUtilities.GetUDMatrix(cam.worldToCameraMatrix));
            vRenderView.SetMatrix(Vault.RenderViewMatrix.Projection, UDUtilities.GetUDMatrix(cam.projectionMatrix));

            //interface to input render options: this allows setting of render flags, picking and filtering from unity objects attached to the camera
            vdkCameraOptions optionsContainer = cam.GetComponent<vdkCameraOptions>();
            RenderOptions options;
            if (optionsContainer != null)
            {
                options = optionsContainer.optionsStruct;
            }
            else
            {
                options = new RenderOptions();

                // The above comment block has been left for safety, and without user defined tags will always execute the following 2 lines
                optionsContainer = null;
                options = new RenderOptions();
            }

            GlobalVDKContext.renderer.Render(vRenderView, modelArray, modelArray.Length, options);

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

            var sheet = context.propertySheets.Get(Shader.Find("Hidden/VDK/VDKShader"));
            sheet.properties.SetTexture("_udCol", colourTexture);
            sheet.properties.SetTexture("_udDep", depthTexture);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}

