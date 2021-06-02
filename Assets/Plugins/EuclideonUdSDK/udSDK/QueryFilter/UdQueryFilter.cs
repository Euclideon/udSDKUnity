using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using udSDK;
#if UNITY_EDITOR
using UnityEditor; 
#endif

/*
 *Passes filter to 
 */
public class UdQueryFilter : MonoBehaviour
{
    private udQueryFilter vFilter = new udQueryFilter();
    public Camera cam = null;
    [System.NonSerialized]
    public double[] centrePoint;
    [System.NonSerialized]
    public double[] yawPitchRoll;
    [System.NonSerialized]
    public double[] halfsize = new double[3] { 1, 1, 1 };
    public bool inverted = false;
    // Start is called before the first frame update
    void Start()
    {
    }
    
  void StopRendering()
  {
    if (cam != null) 
    {
      UDCameraOptions opts = cam.GetComponent<UDCameraOptions>();
      if (opts != null) {
        opts.optionsStruct.options.pFilter = IntPtr.Zero;
      }
    }
  }

  public void OnDestroy()
  {
    StopRendering();
  }
  public void OnDisable()
  {
    StopRendering();
  }

    // Update is called once per frame
    void Update()
    {
        centrePoint  = new double[3] {(double)this.transform.position.x,(double)this.transform.position.y,(double)this.transform.position.z};
    yawPitchRoll = new double[3]
    {
            -(double)this.transform.eulerAngles.y/180*(double)Mathf.PI,
            -(double)this.transform.eulerAngles.x/180*(double)Mathf.PI,
            -(double)this.transform.eulerAngles.z/180*(double)Mathf.PI,
        };
    halfsize = new double[3]
    {
          (double)this.transform.localScale.x/2,
          (double)this.transform.localScale.z /2,
          (double)this.transform.localScale.y/2,
        };
        vFilter.SetAsBox(centrePoint, halfsize, yawPitchRoll);
        vFilter.SetInverted(inverted);
        cam = Camera.main;
        if (cam == null)
            return;
        UDCameraOptions optionsContainer = null;
        optionsContainer = cam.GetComponent<UDCameraOptions>();
        if (optionsContainer != null)
        {
          optionsContainer.optionsStruct.options.pFilter = this.vFilter.pQueryFilter;
        }
        
        
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UdQueryFilter))]
public class UdQueryFilterGUI : Editor 
{
    UdQueryFilter script;
    GameObject gameObject ; 
 
    void OnEnable() {
        script = (UdQueryFilter) target;
        gameObject = script.gameObject;
    }

    public override void OnInspectorGUI()
    {
        if(!GlobalUDContext.uContext.GetSessionInfo().isPremium)
            EditorGUILayout.HelpBox("This is a 'Premium' feature; the current context is not 'Premium', and renders in the current context will fail.", MessageType.Warning);

        DrawDefaultInspector();
    }
}
#endif