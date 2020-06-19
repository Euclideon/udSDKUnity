using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Vault;
public class vdkCameraOptions : MonoBehaviour
{
    public Camera cam;
    public RenderOptions optionsStruct = new RenderOptions();
    public vdkRenderContextPointMode pointMode = vdkRenderContextPointMode.vdkRCPM_Rectangles;
    public bool recordDepthBuffer = false;
    public bool placeCubesOnPick = true;
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
        (previewCube.GetComponent<Renderer>()).material.color = Color.clear;
        previewCube.GetComponent<Collider>().enabled = false;

    }

    void Update()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        optionsStruct.options.pointMode = pointMode;

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
        optionsStruct.setPick((uint)mp.x, (uint)(cam.pixelHeight - mp.y));
        
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
