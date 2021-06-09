using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using udSDK;

[System.Serializable]
public struct UDProjectAppearance 
{
    [Tooltip("Control the color of lines and points.")]
    public Color interestColor;
    [Tooltip("Control the size of points.")]
    public float pointSize;
    [Tooltip("Control the size of image media.")]
    public float imageMediaSize;
    [Tooltip("Control the size of lines.")]
    public float lineSize;
}

public class UDProjectUnity : MonoBehaviour
{
 
    public string path;
    public bool isLoaded = false;
    private string geoJSON;
    UDProject proj;

    // this retains an offset, to keep everything close to world origin
    private double[] positionOffset = new double[3];
    private bool positionSet = false; 

    [Tooltip("Modify the visuals of a project when loaded.")]
    public UDProjectAppearance appearance = new UDProjectAppearance{
        interestColor = new Color(1f, 0.42f, 0f),
        pointSize = 50f,
        imageMediaSize = 100f,
        lineSize = 20f
    };

    public double[] CheckPosition(double[] position)
    {
        if(!positionSet)
        {
            SetPosition(position);
            return new double[] {0,0,0}; 
        }
        
        return new double[]
        {
            positionOffset[0] - position[0],
            positionOffset[1] - position[1],
            positionOffset[2] - position[2]
        };
    }

    public void SetPosition(double[] position)
    {
        positionOffset[0] = position[0]; 
        positionOffset[1] = position[1]; 
        positionOffset[2] = position[2];
        
        Debug.Log("Set project position : " + position[0] + ", " + position[1] + ", " + position[2]);
        
        positionSet = true; 
    }
    void LoadFromFile(string path)
    {
        geoJSON = System.IO.File.ReadAllText(path);
        proj = new UDProject(geoJSON);
        this.path = path;
        print("loaded node!");
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLoaded)
        {
            if(!GlobalUDContext.isCreated)
            {
                Debug.Log("Cannot load project before global context.");
                return; 
            }

            try
            {
                LoadFromFile(path);
                GameObject rootNodeGO = new GameObject();
                rootNodeGO.transform.parent = transform;
                var pn = rootNodeGO.AddComponent<udProjectNodeUnity>();
                pn.LoadTree(proj.pRootNode);
            }
            catch(Exception e) 
            {
                Debug.LogError("caught exception loading project: " + e.ToString());
            }
            
            isLoaded = true;
        }
    }
}