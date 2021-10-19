using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor ; 
#endif

using udSDK;

public class UDSModel : MonoBehaviour
{
    [System.NonSerialized]
    public udPointCloud udModel = new udPointCloud();
    [System.NonSerialized]
    public bool isLoaded = false;
    [System.NonSerialized]
    public Matrix4x4 modelScale;

    [System.NonSerialized]
    public Matrix4x4 modelToPivot; //This represents the transformation between the file representation and the coordinate system centred at pivot
    public udPointCloudHeader header = new udPointCloudHeader();


    [System.NonSerialized]
    public Matrix4x4 storedMatrix;
    [System.NonSerialized]
    public Vector3 fileScale;
    public Vector3 geolocationOffset = Vector3.zero;
    public string path = "";
    public bool geolocate = false;
    
    [HideInInspector]
    public UnityEvent m_OnLoaded;
    
    public string Path
    {
        get { return path; }
        set
        {
            path = value;
            if (isLoaded == true)
                udModel.Unload();

            isLoaded = false;
        }
    }
    private string resolvedPath; // This resolves any issues with local streamable assets 

    private void Awake()
    {
        // adding this init in Awake so that it happens at the earliest possible time 
        m_OnLoaded = new UnityEvent();
    }

    private void Update()
    {
        if (geolocate)
        {
            this.transform.localPosition =  UDUtilities.UDtoGL *
                new Vector3(
                    (float)(header.baseOffset[0] + header.pivot[0] * header.scaledRange),
                    (float)(header.baseOffset[1] + header.pivot[1] * header.scaledRange),
                    (float)(header.baseOffset[2] + header.pivot[2] * header.scaledRange)
                    );
            transform.localPosition += geolocationOffset;
            geolocate = false;
        }
    }
    public Matrix4x4 getStoredMatrix() {
        return new Matrix4x4(
                new Vector4((float)header.storedMatrix[0], (float)header.storedMatrix[1], (float)header.storedMatrix[2], (float)header.storedMatrix[3]),
                new Vector4((float)header.storedMatrix[4], (float)header.storedMatrix[5], (float)header.storedMatrix[6], (float)header.storedMatrix[7]),
                new Vector4((float)header.storedMatrix[8], (float)header.storedMatrix[9], (float)header.storedMatrix[10], (float)header.storedMatrix[11]),
                new Vector4((float)header.storedMatrix[12], (float)header.storedMatrix[13], (float)header.storedMatrix[14], (float)header.storedMatrix[15])
                ) ;
    }

    // This gets called by getUDSInstances if it isn't loaded already
    public void LoadModel()
    {
        // if not ready for loading, fail
        if (!GlobalUDContext.isCreated || isLoaded || Path == "" || Path == null)
            return;

        // handle cases for local paths 
        if (resolvedPath == "" || resolvedPath == null)
        {
            // while we are here, trim for convenience
            // fyi windows copy as path tool always has quotes either side
            char[] charsToTrim = {' ', '"'};
            var trimmedString = Path.Trim(charsToTrim);
            
            // need to determine if we are using a url, an absolute path, or a local path 
            if (trimmedString.StartsWith("Assets/StreamingAssets"))
            {
                // Warning, the path reference by streamingAssetsPath varies per platform 
                // More details here : https://docs.unity3d.com/Manual/StreamingAssets.html
                var pathChunks = trimmedString.Split(new[] {"StreamingAssets",}, StringSplitOptions.None);
                string relativePath = pathChunks[pathChunks.Length-1];
                List<RuntimePlatform> defaultPlatforms = new List<RuntimePlatform>() {
                    RuntimePlatform.WindowsPlayer, 
                    RuntimePlatform.WindowsEditor,
                    RuntimePlatform.LinuxPlayer,
                    RuntimePlatform.LinuxEditor,
                    RuntimePlatform.OSXEditor,
                    RuntimePlatform.OSXPlayer,
                };
                if (defaultPlatforms.Contains(Application.platform))
                    resolvedPath = Application.streamingAssetsPath + relativePath;
                else
                    resolvedPath = trimmedString;
            }
            else
            {
                // this is the default original case 
                resolvedPath = trimmedString;
            } 
        }

        // attempt to load via udsdk
        try
        {
            udModel.Load(GlobalUDContext.uContext, resolvedPath, ref header);
            storedMatrix = getStoredMatrix();
            double maxDim = 0;
            for (int i = 0; i < 3; i++) {
                if(maxDim<header.boundingBoxExtents[i])
                  maxDim = header.boundingBoxExtents[i];
            }
            float s = (float)(1 / (2* maxDim));//bounding box extents are relative to centre -> size of model is double the extents
            fileScale = new Vector3(s,s,s);
            modelScale = Matrix4x4.Scale(new Vector3((float) header.scaledRange/s, (float) header.scaledRange/s, (float) header.scaledRange/s));
            modelToPivot = UDUtilities.UDtoGL *
                        Matrix4x4.Scale(new Vector3(
                            (float)header.scaledRange,
                            (float)header.scaledRange,
                            (float)header.scaledRange
                            )
                        ) *
                        Matrix4x4.Translate(new Vector3(
                            (float)(-header.pivot[0]),
                            (float)(-header.pivot[1]),
                            (float)(-header.pivot[2])
                          )
                        );
            isLoaded = true;
            
            m_OnLoaded.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError("Could not open UDS: " + resolvedPath + " " + e.Message);
        }
    }
    
    [ContextMenu("Reload")]
    public void Reload()
    {
        // we don't force a reload, only ensure reload happens at the next opportunity
        isLoaded = false;
        udModel.Unload();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UDSModel))]
public class UDSModelGUI : Editor 
{
    UDSModel script;
    GameObject gameObject ; 
    
    void OnEnable() {
        script = (UDSModel) target;
        gameObject = script.gameObject;
    }

    public override void OnInspectorGUI()
    {
        if(gameObject.tag != "UDSModel")
            EditorGUILayout.HelpBox("This is not displayed because gameObject tag is not 'UDSModel'", MessageType.Warning);

        DrawDefaultInspector();
    }
}
#endif