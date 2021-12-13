using System;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif

using udSDK;

#if UNITY_EDITOR
public class UDUserInfoEditor : EditorWindow
{
    [MenuItem("Euclideon/udCloud")]
    public static void ShowWindow()
    {
        Texture icon = Resources.Load("Images/EuclideonIcon") as Texture;
        String title = "ud<b><i>Cloud</i></b>";
        EditorWindow window = GetWindow(typeof(UDCloudGUI));
        window.titleContent = new GUIContent(title, icon);
    }
}

public class UDCloudGUI : EditorWindow
{
    bool[] foldouts = null;
    string pathText = "";

    static void UnloadServerProjectsInScene()
    {
        UDProjectUnity oldProjectToUnload = GameObject.FindObjectOfType<UDProjectUnity>();
        if (oldProjectToUnload)
            Destroy(oldProjectToUnload.gameObject);

        GameObject udCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (udCamera)
            Destroy(udCamera);
    }

    static void LoadServerProject(UDCloudNode sceneNode)
    {
        UnloadServerProjectsInScene();
        GameObject udCamera = Instantiate(Resources.Load("udSDKCamera")) as GameObject;
        udCamera.name = "udSDKCamera"; //so its not called (clone).
        GameObject projectGO = new GameObject();
        projectGO.AddComponent<UDProjectUnity>();
        projectGO.GetComponent<UDProjectUnity>().LoadFromServer(sceneNode.uuid, sceneNode.parent.uuid, sceneNode.parent.parent.uuid);
    }

    static void LoadLocalProject(string path)
    {
        GameObject oldudCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (!oldudCamera)
        {
            GameObject udCamera = Instantiate(Resources.Load("udSDKCamera")) as GameObject;
            udCamera.name = "udSDKCamera"; //so its not called (clone).
        }
        GameObject projectGO = new GameObject();
        projectGO.AddComponent<UDProjectUnity>();
        projectGO.GetComponent<UDProjectUnity>().LoadFromFile(path);
    }

    static void OpenProjectInCloud(UDCloudNode sceneNode)
    {
        string link = GlobalUDContext.cloudServer + "/api/" + sceneNode.parent.parent.uuid + "/" + sceneNode.parent.uuid + "/" + sceneNode.uuid + "/_scene/view";
        System.Diagnostics.Process.Start(link);
    }

    //Begins centrering of Gui Content in window, where x centres the horizontal, y the vertical
    static void BeginGUICentre(bool x, bool y)
    {
        if (y)
            GUILayout.FlexibleSpace();

        if (x)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
        }
    }

    //Ends centering of GUI Content in window,, where x centres the horizontal, y the vertical
    static void EndGUICentre(bool x, bool y)
    {
        if (x)
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        if (y)
            GUILayout.FlexibleSpace();
    }

    //Begins Gui Indent
    static void BeginGUIIndent(float indentWidth, int level)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(indentWidth * level);
    }

    //Ends Gui Indent
    static void EndGUIIndent()
    {
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draws a horizontal line in the GUI
    /// </summary>
    /// <param name="color"></param>
    /// <param name="thickness"></param>
    /// <param name="padding"></param>
    public static void DrawGUILine(Color color, int thickness = 2, int padding = 3)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }

    /// <summary>
    /// Draws the welcome user header box.
    /// </summary>
    /// <param name="userNode"></param>
    static void DrawCloudUserBox(UDCloudNode userNode)
    {
        GUIStyle style = GUI.skin.GetStyle("Label");
        style.richText = true;

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Welcome <b>" + userNode.name + "</b>", style);
        EditorGUILayout.Separator();

        DrawGUILine(Color.gray);
    }

    /// <summary>
    /// Draws the buttons related to the scene node.
    /// </summary>
    /// <param name="node"></param>
    static void DrawCloudSceneOptions(UDCloudNode node)
    {
        GUIStyle style = GUI.skin.GetStyle("Button");
        style.richText = true;

        if (GUILayout.Button("Load in scene", style))
            LoadServerProject(node);

        if (GUILayout.Button("View in ud<b><i>Stream</i></b> Web", style))
            OpenProjectInCloud(node);
    }

    /// <summary>
    /// Draws the heirarchy in the inspector using a foldout boolean array equal to the tree size.
    /// </summary>
    /// <param name="node"></param>
    void DrawCloudHeirarchy(UDCloudNode node, bool[] foldouts, float indentWidth = 10)
    {
        if (node.depth < 3)
        {              
            if (node.children != null)
            {
                for (int i = 0; i < node.children.Length; i++)
                {
                    BeginGUIIndent(indentWidth, node.depth);
                    foldouts[node.children[i].rootIndex] = EditorGUILayout.Foldout(foldouts[node.children[i].rootIndex], new GUIContent(node.children[i].name), true);
                    EndGUIIndent();

                    if (foldouts[node.children[i].rootIndex])
                        DrawCloudHeirarchy(node.children[i], foldouts);
                }
            }
        }
        else
        {
            BeginGUIIndent(indentWidth, node.depth);
            DrawCloudSceneOptions(node);
            EndGUIIndent();
        }
    }

    //GUI Elements.
    private void OnGUI()
    {
        GUIStyle buttonStyle = GUI.skin.GetStyle("Button");
        GUIStyle labelStyle = GUI.skin.GetStyle("Label");
        labelStyle.richText = true;
        buttonStyle.richText= true;
        labelStyle.alignment = TextAnchor.MiddleCenter;

        if (Application.isPlaying) //Runs only in Play mode
        {
            if (!GlobalUDContext.isCreated && !GlobalUDContext.isPartiallyCreated) //step 1
            {
                BeginGUICentre(true, true);
                string buttonName = "Go to ud<b><i>Cloud</i></b>";
                float buttonWidth = labelStyle.CalcSize(new GUIContent(buttonName)).x + 10f;
                if (GUILayout.Button(buttonName, buttonStyle, GUILayout.Width(buttonWidth)))
                    GlobalUDContext.StartLogin_Cloud();
                EndGUICentre(true, true);

                if(!GlobalUDContext.triedResume)
                    GlobalUDContext.Resume();
            }

            if (!GlobalUDContext.isCreated && GlobalUDContext.isPartiallyCreated) //step 2
            {
                foldouts = null;
                pathText = "";

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Once succesfuly authorised ", labelStyle);
                BeginGUICentre(true, false);
                string buttonName = "Click here";
                float buttonWidth = labelStyle.CalcSize(new GUIContent(buttonName)).x + 10f;
                if (GUILayout.Button(buttonName, buttonStyle, GUILayout.Width(buttonWidth)))
                    GlobalUDContext.CompleteLogin_Cloud();
                EndGUICentre(true, true);
            }

            if (GlobalUDContext.isCreated && !GlobalUDContext.isPartiallyCreated) //step 3
            {
                
                UDCloudNode root = GlobalUDContext.udCloudRootNode;

                if (foldouts == null)
                {
                    foldouts = new bool[root.treeSize];
                    for (int i = 0; i < foldouts.Length; i++)
                    {
                        foldouts[i] = false;
                    }
                        
                }

                UDCloudGUI.DrawCloudUserBox(root);
                GUILayout.FlexibleSpace();
                DrawCloudHeirarchy(root, foldouts);
                GUILayout.FlexibleSpace();
                DrawGUILine(Color.gray);
                BeginGUICentre(true, false);
                GUILayout.Label("Local project: ");
                pathText = EditorGUILayout.TextField(pathText);
                if (GUILayout.Button("Load scene", buttonStyle))
                    LoadLocalProject(pathText);
                EndGUICentre(true, false);
                DrawGUILine(Color.gray);
                EditorGUILayout.Separator();
                TimeSpan expiration = TimeSpan.FromSeconds(GlobalUDContext.uContext.GetSessionInfo().expiresTimestamp);
                String humanReadableExpiration = string.Format("{0:D2}:{1:D2}:{2:D2}", expiration.Hours, expiration.Minutes, expiration.Seconds);
                EditorGUILayout.LabelField("Session expires in: <b>" + humanReadableExpiration + "</b>", labelStyle);
                EditorGUILayout.Separator();
            }
        }
        else //Info while in edit mode
        {
            BeginGUICentre(true, true);
            EditorGUILayout.LabelField("Enter play mode to use ud<b><i>Cloud</i></b>", labelStyle);
            EndGUICentre(true, true);
        }
        Repaint(); //Repaints GUI every frame
    }
}
#endif