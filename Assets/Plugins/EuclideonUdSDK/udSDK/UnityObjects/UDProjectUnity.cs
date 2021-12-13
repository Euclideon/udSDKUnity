using System;
using UnityEngine;
using System.Collections;
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
    [ReadOnly] public string path;
    public bool isLoaded = false;
    public udSDK.udProject proj = new udSDK.udProject();
    [ReadOnly] public string udCloudSceneID;
    [ReadOnly] public string udCloudProjectID;
    [ReadOnly] public string udCloudWorkspaceID;
    private UDSModel model;

    // this retains an offset, to keep everything close to world origin
    private double[] positionOffset = new double[3];
    private bool positionSet = false;

    [Tooltip("Modify the visuals of a project when loaded.")]
    public UDProjectAppearance appearance = new UDProjectAppearance {
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
        
        positionSet = true; 
    }
    public void LoadFromFile(string projectPath)
    {
        LoadFromMemory(System.IO.File.ReadAllText(projectPath));
        this.path = projectPath;
    }

    public void LoadFromMemory (string geoJSON)
    {
        if (!GlobalUDContext.isCreated)
        {
            Debug.Log("Global context not loaded, cannot load project.");
            return;
        }

        if (isLoaded)
        {
            Debug.Log("Project already loaded.");
            return;
        }

        proj.LoadFromMemory(GlobalUDContext.uContext, geoJSON);
        proj.GetProjectRoot();
        isLoaded = true;

        UDProjectNodeUnity pn = gameObject.AddComponent<UDProjectNodeUnity>();
        pn.LoadTree(proj.pRootNode);
    }

    public void LoadFromServer(string sid, string pid, string wid)
    {
        if (!GlobalUDContext.isCreated)
        {
            Debug.Log("Global context not loaded, cannot load project.");
            return;
        }

        if (isLoaded)
        {
            Debug.Log("Project already loaded.");
            return;
        }

        proj.LoadFromServer(GlobalUDContext.uContext, sid, wid + "/" + pid);
        proj.GetProjectRoot();
        Debug.Log("Project loaded from server.");
        isLoaded = true;

        UDProjectNodeUnity pn = gameObject.AddComponent<UDProjectNodeUnity>();
        pn.LoadTree(proj.pRootNode);
    }
}