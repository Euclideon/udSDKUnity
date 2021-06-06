using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using udSDK;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(UDCameraOptions))]
[RequireComponent(typeof(DepthOfFieldEffector))]
public class udPickToDepth : MonoBehaviour
{
    Camera cam ;
    UDCameraOptions camOptions ; 
    DepthOfFieldEffector depthEffect ; 

    bool setNext ; 

    void Start()
    {
        cam = GetComponent<Camera>();
        depthEffect = GetComponent<DepthOfFieldEffector>();
        camOptions = GetComponent<UDCameraOptions>();
    }

    void Update()
    {
        if (setNext)
        {
            udPick pick = camOptions.lastPick; 
            depthEffect.SetFocusDistance(Vector3.Distance(pick.pointCenter, cam.transform.position));
            setNext = false; 
        }
        if(Input.GetMouseButtonDown(0))
        {
            setNext = true; 
            Vector3 mp = Input.mousePosition;
            camOptions.optionsStruct.setPick((uint)(mp.x), (uint)((cam.pixelHeight - mp.y)));
        }
    }
}
