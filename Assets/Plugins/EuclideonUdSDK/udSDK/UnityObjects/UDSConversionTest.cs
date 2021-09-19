using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor ; 
#endif

using udSDK;

public struct ConvertedItem
{
    public string filepath;
    public float convertTime;

    public ConvertedItem(string filepath, float convertTime)
    {
        this.filepath = filepath;
        this.convertTime = convertTime; 
    }
}

public class UDSConversionTest : MonoBehaviour
{
    public string inPath;
    public bool overrideDefaultResolution = false; 
    public float resolution;
    public string outPath;

    [HideInInspector] public bool converting = false;
    [HideInInspector] public float convertProgress = 0.0f;
    [HideInInspector] public float convertTime = 0.0f;
    [HideInInspector] public List<ConvertedItem> converted = new List<ConvertedItem>(); 
    
    private udConvertContext convertContext;
    
    [ContextMenu("Convert")]
    public void Convert()
    {
        StartCoroutine(_Convert());
    }

    IEnumerator _Convert()
    {
        // check flags
        converting = true;
        convertProgress = 0f;
        convertTime = 0f; 
        
        // yield return new WaitForSeconds(1.0f);
        
        // reinit
        convertContext = new udConvertContext(); 
        
        // make a context
        convertContext.Create(GlobalUDContext.uContext);
        
        convertProgress += 0.1f;
        convertTime += Time.deltaTime;
        yield return null;

        // add input files
        convertContext.AddItem(inPath);
        
        convertProgress += 0.1f;
        convertTime += Time.deltaTime;
        yield return null;

        // set output filename 
        convertContext.SetOutputFilename(outPath);

        convertProgress += 0.1f;
        convertTime += Time.deltaTime;
        yield return null;
        
        // point res 
        convertContext.SetPointResolution(overrideDefaultResolution, resolution);
        
        convertProgress = 0.75f;
        convertTime += Time.deltaTime;
        yield return null;

        // do convert? 
        convertContext.DoConvert();

        // destroy context 
        convertContext.Destroy();
        
        // uncheck flags
        converting = false;
        convertProgress = 1f;
        convertTime += Time.deltaTime;
        converted.Add(new ConvertedItem(outPath, convertTime)); 
        EditorUtility.DisplayDialog("UDS Convert Complete","Converted file : "+outPath, "Ok", "");
        yield return null;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(UDSConversionTest))]
public class UDConvertGUI : Editor 
{
    UDSConversionTest script;
    GameObject gameObject ; 
    
    void OnEnable() {
        script = (UDSConversionTest) target;
        gameObject = script.gameObject;
    }

    public void ShowExplorer(string itemPath)
    {
        itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
        System.Diagnostics.Process.Start("explorer.exe", "/select,"+itemPath);
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.LabelField("Convert Control", EditorStyles.boldLabel);
        if (!script.converting)
        {
            if (GUILayout.Button("Convert"))
            {
                script.Convert();    
            }    
        }
        else
        {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

            var message = "waiting";
        
            if (script.convertProgress >= 1f)
                message = "converted.";
            else if (script.convertProgress > 0f)
                message = "converting...";

            EditorGUI.ProgressBar(rect, script.convertProgress, message);            
        }

        if (script.converted.Count > 0)
        {
            EditorGUILayout.LabelField("Last Convert Result", EditorStyles.boldLabel);

            var lastResult = script.converted[script.converted.Count - 1];
            EditorGUILayout.LabelField("Path: ", lastResult.filepath);
            EditorGUILayout.LabelField("Convert Time: ", lastResult.convertTime.ToString());

            if (GUILayout.Button("Open in explorer"))
            {
                // file open in explorer?   
                ShowExplorer(lastResult.filepath);
            } 
        }
    }
}
#endif