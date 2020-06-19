using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vault;
/*
 *Passes filter to 
 */
public class VDKFilter : MonoBehaviour
{
    private vdkQueryFilter vFilter = new vdkQueryFilter();
    public Camera cam;
    public double[] centrePoint;
    public double[] yawPitchRoll;
    public double[] halfsize = new double[3] { 1, 1, 1 };
    public GameObject targetUDS = null;
    public bool inverted = false;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.current;
    }

    // Update is called once per frame
    void Update()
    {
        centrePoint  = new double[3] {(double)this.transform.position.x,(double)this.transform.position.y,(double)this.transform.position.z};
        yawPitchRoll = new double[3]
        {
            (double)this.transform.eulerAngles.z/180*(double)Mathf.PI,
            (double)this.transform.eulerAngles.x/180*(double)Mathf.PI,
            (double)this.transform.eulerAngles.y/180*(double)Mathf.PI
        };
        halfsize = new double[3] { (double)this.transform.localScale.x/2, (double)this.transform.localScale.y/2, (double)this.transform.localScale.z/2 };
        vFilter.SetAsBox(centrePoint, halfsize, yawPitchRoll);
        vFilter.SetInverted(inverted);
        cam = Camera.main;
        if (cam == null)
            return;
        vdkCameraOptions optionsContainer = null;
        optionsContainer = cam.GetComponent<vdkCameraOptions>();
        if (optionsContainer != null)
        {
          optionsContainer.optionsStruct.options.pFilter = this.vFilter.pQueryFilter;
        }
        
        
    }
}

