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
    bool tryLoggingIn = false;
    bool tryingToLogIn = false;
    Vector2 scrollPosition;

    static void UnloadServerProjectsInScene()
    {
        UDProjectUnity oldProjectToUnload = GameObject.FindObjectOfType<UDProjectUnity>();

        if (oldProjectToUnload)
            Destroy(oldProjectToUnload.gameObject);
    }

    static void LoadServerProject(UDCloudNode sceneNode)
    {
        UnloadServerProjectsInScene();
        GameObject udCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (!udCamera)
        {
            udCamera = Instantiate(Resources.Load("udSDKCamera")) as GameObject;
            udCamera.name = "udSDKCamera"; //so its not called (clone).
        }
        GameObject projectGO = new GameObject();
        UDProjectUnity projectComponent = projectGO.AddComponent<UDProjectUnity>();
        projectComponent.LoadFromServer(sceneNode.uuid, sceneNode.parent.uuid, sceneNode.parent.parent.uuid);
        DrawLoadingAnimation(projectComponent);
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

        if (path.ToCharArray()[0] == '"' && path.ToCharArray()[path.Length] == '"') //removes quotes from a path, often neccessary from a 'copy as path' generated string
            path = path.Substring(1, path.Length - 2);

        projectGO.GetComponent<UDProjectUnity>().LoadFromFile(path);
    }

    static void OpenProjectInWeb(UDCloudNode sceneNode)
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

    ///<summary>
    /// Draws a small loading animaton to ease users.
    ///</summary>
    public static void DrawLoadingAnimation(UDProjectUnity proj)
    {
        if(proj.isLoaded && !proj.allModelsLoaded)
        {

        }
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
        {
            LoadServerProject(node);         
        }          

        if (GUILayout.Button("View in ud<b><i>Stream</i></b> Web", style))
            OpenProjectInWeb(node);
    }

    /// <summary>
    /// Draws the heirarchy in the inspector using a foldout boolean array whose size is equal to the tree size at the node.
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
        buttonStyle.richText = true;
        labelStyle.alignment = TextAnchor.MiddleCenter;

        if (Application.isPlaying) //Runs only in Play mode
        {
            if (!GlobalUDContext.isCreated && !GlobalUDContext.isPartiallyCreated) //step 1
            {
                tryLoggingIn = false;
                tryingToLogIn = true;

                BeginGUICentre(true, true);
                string buttonName = "Go to ud<b><i>Cloud</i></b>";
                float buttonWidth = labelStyle.CalcSize(new GUIContent(buttonName)).x + 10f;
                if (GUILayout.Button(buttonName, buttonStyle, GUILayout.Width(buttonWidth)))
                    GlobalUDContext.StartLogin_Cloud();
                EndGUICentre(true, true);

                if (!GlobalUDContext.triedResume)
                    GlobalUDContext.Resume();
            }

            if (!GlobalUDContext.isCreated && GlobalUDContext.isPartiallyCreated) //step 2
            {
                foldouts = null;
                pathText = "";
                scrollPosition = Vector2.right;            

                if (tryLoggingIn)
                {
                    BeginGUICentre(true, true);
                    EditorGUILayout.LabelField("Logging in...", labelStyle);
                }
                else
                {
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.LabelField("Once succesfully authorised ", labelStyle);
                    BeginGUICentre(true, false);
                    string buttonName = "click here";
                    float buttonWidth = labelStyle.CalcSize(new GUIContent(buttonName)).x + 10f;
                    if (GUILayout.Button(buttonName, buttonStyle, GUILayout.Width(buttonWidth)))
                        tryLoggingIn = true;
                }
                EndGUICentre(true, true);

                if (tryLoggingIn && Event.current.type == EventType.Repaint)
                {
                    GlobalUDContext.CompleteLogin_Cloud();
                    tryLoggingIn = false;       
                }
            }
            else if (GlobalUDContext.isCreated && !GlobalUDContext.isPartiallyCreated) //step 3
            {
                if (!GlobalUDContext.loadedCloudData)
                    GlobalUDContext.LoadCloudData();

                UDCloudNode root = GlobalUDContext.udCloudRootNode;

                if (foldouts == null)
                {
                    foldouts = new bool[root.treeSize];
                    for (int i = 0; i < foldouts.Length; i++)
                        foldouts[i] = false;
                }

                UDCloudGUI.DrawCloudUserBox(root);
                DrawGUILine(Color.gray);

                GUILayout.FlexibleSpace();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DrawCloudHeirarchy(root, foldouts);
                EditorGUILayout.EndScrollView();

                GUILayout.FlexibleSpace();

                DrawGUILine(Color.gray);
                EditorGUILayout.Separator();
                BeginGUICentre(true, false);
                GUILayout.Label("Local project path: ");
                pathText = EditorGUILayout.TextField(pathText);
                if (GUILayout.Button("Load scene", buttonStyle))
                    LoadLocalProject(pathText);
                EndGUICentre(true, false);
                EditorGUILayout.Separator();
            }
        }
        else //Info while in edit mode
        {
            BeginGUICentre(true, true);
            EditorGUILayout.LabelField("Enter play mode to use ud<b><i>Cloud</i></b>", labelStyle);
            EndGUICentre(true, true);
        }
    }

    private void Update()
    {
        Repaint(); //Repaints GUI every frame
    }
}
#endif