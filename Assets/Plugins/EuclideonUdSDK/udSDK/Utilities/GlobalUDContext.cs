using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

namespace udSDK
{
    /// <summary>
    /// The global context used by the integration.
    /// </summary>
    public static class GlobalUDContext
    {
        private static bool _isCreated = false;
        private static bool _isPartiallyCreated = false;
        private static bool _loadedCloudData = false;
        public static bool triedResume = false;
        public static udContext uContext = new udContext();
        public static udRenderContext renderer = new udRenderContext();
        public static UDSessionThreadManager sessionKeeper = new UDSessionThreadManager();
        public static udServerAPI serverAPI = new udServerAPI();
        public static UDCloudNode udCloudRootNode = new UDCloudNode();
        public static string cloudServer = "https://udcloud.euclideon.com";
        public static string legacyServer = "https://udstream.euclideon.com";
        public static string username = "";
        
        // For validating the version 
        public static UDVersion sdkVersion; // the version of the sdk currently linked to the project
        public static UDVersion wrapperVersion = new UDVersion(2, 2, 0); // the latest version of the sdk supported by the integration

        /// <summary>
        /// Returns the read-only state of the context.
        /// </summary>
        public static bool isCreated { get { return _isCreated; } }

        /// <summary>
        /// Returns the read-only state of the partial context.
        /// </summary>
        public static bool isPartiallyCreated { get { return _isPartiallyCreated; } }

        /// <summary>
        /// Returns the read-only state of the cloud data.
        /// </summary>
        public static bool loadedCloudData { get { return _loadedCloudData; } }

        /// <summary>
        /// Starts the login process to connect with <i><b>udCloud</b></i>.
        /// </summary>
        public static void StartLogin_Cloud()
        {
            try
            {
                UDCloudApproval cloudApproval = GlobalUDContext.uContext.ConnectStart(cloudServer, "Unity", wrapperVersion.Format());
                _isPartiallyCreated = true;
                string approvePath = cloudApproval.path;
                string approveCode = cloudApproval.code;

                Debug.Log("Opening <i><color=#008b8b>" + approvePath + "</color></i> in a browser window.");
                System.Diagnostics.Process.Start(approvePath);
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        /// <summary>
        /// Cancels the login process to connect with <i><b>udCloud</b></i>.
        /// </summary>
        public static void CancelLogin_Cloud()
        {
            try
            {
                GlobalUDContext.uContext.ConnectCancel();
                _isCreated = false;
                _isPartiallyCreated = false;
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        /// <summary>
        /// Completes the login process to connect with ud<i><b>Cloud</b></i>. 
        /// </summary>
        public static void CompleteLogin_Cloud()
        {
            if (_isPartiallyCreated)
            {
                try
                {                   
                    GlobalUDContext.uContext.ConnectComplete();
                    _isCreated = true;
                }
                catch (System.Exception e)
                {
                    if (e.ToString().Contains(Enum.GetName(typeof(udError), 2))) //udE_NothingToDo
                    {
                        Debug.Log("Waiting for browser to log in.");
                    }
                    else
                    {
                        Debug.Log(e.ToString());
                        GlobalUDContext.CancelLogin_Cloud(); // For safety, but could be removed.
                    }
                }
            }

            if (_isCreated)
            {
                if(_isPartiallyCreated == true)
                    GlobalUDContext.renderer.Create(GlobalUDContext.uContext);

                _isPartiallyCreated = false;
            }
        }

        /// <summary>
        /// Uses the type of <paramref name="node"/> to get the <i><b>udCloud</b></i> directory of the JSON that lists the children of this node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static string GetUDCloudDirectory(UDCloudNode node)
        {

            if (node.type == UDCloudNodeType.udCloud_User)
            {
                if (node.uuid == null)
                    return "_user/whoami";
                else
                    return "org/list";
            }
            else if (node.type == UDCloudNodeType.udCloud_Workspace)
                return node.uuid + "/_project/list";
            else if (node.type == UDCloudNodeType.udCloud_Project)
                return node.parent.uuid + "/" + node.uuid + "/_scene/list";

            return "";
        }


        /// <summary>
        /// Loads the data from the cloud, once.
        /// </summary>
        public static void LoadCloudData()
        {
            if (_loadedCloudData)
                return;

            GlobalUDContext.LoadCloudTree(udCloudRootNode);
            _loadedCloudData = true;
        }

        /// <summary>
        /// Loads the <i><b>udCloud</b></i> heirarchy in a recursive fashion, with <paramref name="node"/> as the root.
        /// </summary>
        /// <param name="node">The node to build from.</param>
        /// <param name="depthLimit">The limit at which the tree will stop growing.</param>
        public static void LoadCloudTree(UDCloudNode node, int depthLimit = 3)
        {
            int typeAsInt = (int) node.type;

            if (typeAsInt < depthLimit)
            {
                string query = GlobalUDContext.serverAPI.Query(GlobalUDContext.uContext, GetUDCloudDirectory(node), null);

                if (node.uuid == null) //if rootnode is empty, write info to it.
                {
                    UDCloudJSON jsonObject = JsonUtility.FromJson<UDCloudJSON>(query);

                    node.uuid = jsonObject.id;
                    node.name = jsonObject.name;
                    username = node.name;

                    GlobalUDContext.LoadCloudTree(node); //loops back now with informed root node
                }
                else //now we create children nodes for workspaces(orgs), then projects, then scenes.
                {
                    UDCloudQuery cloudQuery = JsonUtility.FromJson<UDCloudQuery>(query);
                    UDCloudJSON[] jsonObjects = null;

                    if (typeAsInt == (int)UDCloudNodeType.udCloud_User)
                        jsonObjects = cloudQuery.organisations;
                    else if (typeAsInt == (int)UDCloudNodeType.udCloud_Workspace)
                        jsonObjects = cloudQuery.projects;
                    else if (typeAsInt == (int)UDCloudNodeType.udCloud_Project)
                        jsonObjects = cloudQuery.scenes;

                    if (jsonObjects.Length > 0)
                    {
                        node.children = new UDCloudNode[jsonObjects.Length];

                        for (int i = 0; i < node.children.Length; i++)
                        {
                            node.children[i] = new UDCloudNode();
                            node.children[i].parent = node;
                            node.children[i].uuid = jsonObjects[i].id;
                            node.children[i].name = jsonObjects[i].name;

                            GlobalUDContext.LoadCloudTree(node.children[i]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tries to resume the context.
        /// </summary>
        public static void Resume()
        {
            try
            {
                uContext.Try_Resume(cloudServer, "Unity", username, false);
                _isCreated = true;
            }
            catch (System.Exception e)
            {
                if (e.ToString().Contains(Enum.GetName(typeof(udError), 10)) == false) //udE_NotFound
                    Debug.Log(e.ToString());
            }

            triedResume = true;
        }

        /// <summary>
        /// Attempts to disconnect the context. All references to the context must be desstroyed beforehand.
        /// </summary>
        public static void Logout()
        {
            try
            {
                GlobalUDContext.uContext.Disconnect(false);
                _isCreated = false;
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
        
        /// <summary>
        /// Checks the current SDK version in use by the integration.
        /// </summary>
        public static void CheckSDKVersion()
        {
          if(sdkVersion == null)
            GlobalUDContext.sdkVersion = new UDVersion();

          if (GlobalUDContext.sdkVersion.Equals(wrapperVersion))
            return ; 

          // identify mismatches that will likely affect the functionality (major, and minor versions, ignoring patch)
          if(wrapperVersion.GreaterThan(sdkVersion, 1) )
          {
            Debug.LogWarning("udSDK "+sdkVersion.Format()+" is deprecated, and does not match the integration. "+
                             "Please update to the supported version "+wrapperVersion.Format()+".");
          }
          else if(sdkVersion.GreaterThan(wrapperVersion, 1) )
          {
            Debug.LogWarning("udSDK "+sdkVersion.Format()+" is newer than the integration. "+
                             "The integration should be updated, as certain features may not be working correctly.");
          }
        }
    }
}

