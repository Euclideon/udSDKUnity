
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Vault;

static class constants
{
  public const float MAX_RESOLUTION_SCALE = 3;
}
public class vdkCameraOptions : MonoBehaviour
{
    public Camera cam;
    public RenderOptions optionsStruct = new RenderOptions();
    public vdkRenderContextPointMode pointMode = vdkRenderContextPointMode.vdkRCPM_Rectangles;
    public bool showPickMarker = false;
    [Tooltip("Factor by which to scale VDK resolution relative to camera resolution: lower numbers will increase frame rate at the cost of resolution")]
    public float resolutionScaling = 1;

    [System.NonSerialized]
    public bool recordDepthBuffer = false;
    public bool placeCubesOnPick = true;
    [System.NonSerialized]
    public bool placeNext = false;
    GameObject previewCube;


    //depth buffer of the camera for surface estimate calculations
    float[] depthBuffer;
    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) {
            cam = Camera.main;
        }
    }
    
    void Start()
    {
        optionsStruct.setPick(0, 0);
        previewCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        (previewCube.GetComponent<Renderer>()).material.color = Color.black;
        previewCube.GetComponent<Collider>().enabled = false;

    }

    void Update()
    {
    if (resolutionScaling > constants.MAX_RESOLUTION_SCALE)
      resolutionScaling = constants.MAX_RESOLUTION_SCALE;

    if (resolutionScaling <= 0)
      resolutionScaling = 1;
        
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        optionsStruct.options.pointMode = pointMode;
        if (!showPickMarker)
        {
          previewCube.SetActive(false);
        }
        else 
        { 
          previewCube.SetActive(true);
        }

        if (optionsStruct.pickRendered)
        {
            if (placeNext && optionsStruct.Pick.hit == 0 )
                Debug.Log("missed!");
            else
            {
                Vector3 pickCentre = optionsStruct.PickLocation();
                if (placeNext)
                {
                    Debug.Log("Mouse located at " + pickCentre.ToString());
                    if (optionsStruct.Pick.isHighestLOD==0)
                    {
                        Debug.Log("Warning: pick may not represent actual point in cloud");
                    }
                    //Here is an example of how to place an object at the position returned by pick
                    if (placeCubesOnPick)
                    {
                        var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        marker.GetComponent<Renderer>().material.color = Color.red;
                        marker.transform.position = pickCentre;
                    }
                }
                previewCube.transform.position = pickCentre;
            }
            placeNext = false;
        }

        Vector3 mp = Input.mousePosition;
        optionsStruct.setPick((uint)(mp.x * resolutionScaling), (uint)((cam.pixelHeight - mp.y)*resolutionScaling));
        
        if (Input.GetMouseButtonDown(0))
        {
            placeNext = true;
        }
    }

    /*
     * for future implementation
     */
    Vector3 posFromScreenDepth(uint x, uint y)
    {
        return Vector3.zero;
    }

    /*
     *generates and stores a depth image rom a z buffer for use in spacial calculations
     * this is pretty expensive
     */
    public void setDepthImageFromZ(float[] value)
    {
      if (cam == null)
          return;

      if (depthBuffer == null || depthBuffer.Length!=value.Length) {
          depthBuffer = new float[value.Length];
      }

      for (int i= 0; i< depthBuffer.Length; ++i)
      {
          depthBuffer[i] = UDUtilities.zBufferToDepth(depthBuffer[i], cam.nearClipPlane, cam.farClipPlane, false);
      }
  }
  public float[] DepthBuffer
  {
      get
      {
          return depthBuffer;
      }
  }
}
