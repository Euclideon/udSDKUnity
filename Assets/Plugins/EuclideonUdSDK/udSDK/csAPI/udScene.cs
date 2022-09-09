using System;
using System.Runtime.InteropServices;

//! The udScene and udSceneNode objects provide an interface for storing and syncronising geolocated projects between udSDK sessions
//! @note The GeoJSON provided by this implementation is not fully compliant with RFC7946
//! @warning Antimeridian Cutting (Section 3.1.9) and handling the poles (Secion 5.3) are not fully working in this implementation
//! @warning This module does not currently expose the functionality to sync with the server. This will be added in a future release.

namespace udSDK
{
  /// <summary>
  /// These are the geometry types for nodes
  /// </summary>
  public enum udSceneGeometryType
  {
    udPGT_None, //!< There is no geometry associated with this node

    udPGT_Point, //!< pCoordinates is a single 3D position
    udPGT_MultiPoint, //!< Array of udPGT_Point, pCoordinates is an array of 3D positions
    udPGT_LineString, //!< pCoordinates is an array of 3D positions forming an open line
    udPGT_MultiLineString, //!< Array of udPGT_LineString; pCoordinates is NULL and children will be present
    udPGT_Polygon, //!< pCoordinates will be a closed linear ring (the outside), there MAY be children that are interior as pChildren udPGT_MultiLineString items, these should be counted as islands of the external ring.
    udPGT_MultiPolygon, //!< pCoordinates is null, children will be udPGT_Polygon (which still may have internal islands)
    udPGT_GeometryCollection, //!< Array of geometries; pCoordinates is NULL and children may be present of any type

    udPGT_Count, //!< Total number of geometry types. Used internally but can be used as an iterator max when displaying different type modes.

    udPGT_Internal, //!< Used internally when calculating other types. Do not use.
  };
  
  /// <summary>
  /// This represents the type of data stored in the node.
  /// @note The `itemtypeStr` in the udSceneNode is a string version. This enum serves to simplify the reading of standard types. The the string in brackets at the end of the comment is the string.
  /// </summary>
  public enum udSceneNodeType
  {
    udPNT_Custom, //!< Need to check the itemtypeStr string manually

    udPNT_PointCloud, //!< A Euclideon Unlimited Detail Point Cloud file ("UDS")
    udPNT_PointOfInterest, //!< A point, line or region describing a location of interest ("POI")
    udPNT_Folder, //!< A folder of other nodes ("Folder")
    udPNT_Media, //!< An Image, Movie, Audio file or other media object ("Media")
    udPNT_Viewpoint, //!< An Camera Location & Orientation ("Camera")
    udPNT_VisualisationSettings, //!< Visualisation settings (itensity, map height etc) ("VizSet")
    udPNT_I3S, //!< An Indexed 3d Scene Layer (I3S) or Scene Layer Package (SLPK) dataset ("I3S")
    udPNT_Water, //!< A region describing the location of a body of water ("Water")
    udPNT_ViewShed, //!< A point describing where to generate a view shed from ("ViewMap")
    udPNT_Polygon, //!< A polygon model, usually an OBJ or FBX ("Polygon")
    udPNT_QueryFilter, //!< A query filter to be applied to all PointCloud types in the scene ("QFilter")
    udPNT_Places, //!< A collection of places that can be grouped together at a distance ("Places")
    udPNT_HeightMeasurement, //!< A height measurement object ("MHeight")
    udPNT_GTFS, //!< A General Transit Feed Specification object ("GTFS")
    udPNT_LassoNode, //!< A Lasso Selection Folder ("LNode")
    udPNT_QueryGroup, //!< A Group of Query node being represented as one node ("QGroup")
    udPNT_Count //!< Total number of node types. Used internally but can be used as an iterator max when displaying different type modes.
  };
  
  /// <summary>
  /// This represents the camera position in 3D
  /// @note contains x,y,z and heading, pitch  of a camera
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udCameraPosition
  {
    public double x; //!< The x coordiante of the camera
    public double y; //!< The y coordiante of the camera
    public double z; //!< The z coordiante of the camera

    public double heading; //!< The heading of the camera
    public double pitch; //!< The pitch of the camera
  };
  
  /// <summary>
  /// This represents the selected project node of a user
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udSelectedNode
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
    public char[] id; //!< The uuid of the selected node
  };
  
  /// <summary>
  /// This represents the avatar info used for collaboration
  /// </summary>
  public struct udAvatarInfo
  {
    public string pURL; //!< the url of the avatar
    public double offsetX; //!< offset x of the avatar
    public double offsetY; //!< offset y of the avatar
    public double offsetZ; //!< offset z of the avatar
    public double scaleX; //!< scale of the avatar in x
    public double scaleY; //!< scale of the avatar in y
    public double scaleZ; //!< scale of the avatar in z
    public double yaw; //!< The yaw of the avatar
    public double pitch; //!< The pitch of the avatar
    public double roll; //!< The roll of the avatar
  };
  
  /// <summary>
  /// This represents the message sent in project to users
  /// </summary>
  public struct udMessage
  {
    public readonly IntPtr pMessageType; //!< The type of the message
    public readonly IntPtr pMessagePayload; //!< The payload of the message

    public readonly IntPtr pTargetSessionID; //!< The session ID of the message
    public readonly IntPtr pReceivedFromSessionID; //!< The session Id where it's been received
  };
  
  /// <summary>
  /// This represents the user info used for collaboration
  /// </summary>
  public struct udUserPosition
  {
    public readonly string userName; //!< The username of this user
    public readonly string ID; //!< The uuid of the user
    public readonly string pSceneSessionID; //!< THe current scene/session ID this user is log in
    public double lastUpdated; //!< The time its position has been kast updated

    public UInt32 selectedNodesCount; //!< The number of node selected
    public IntPtr selectedNodesList; //!< The selected nodes

    public IntPtr cameraPositionList; //!< The list of positions for each camera

    public udAvatarInfo avatar; //!< The info on the used avatar by this user
  };
  
  /// <summary>
  /// Stores the state of a node.
  /// This struct is exposed to avoid having a huge API of accessor functions but it should be treated as read only with the exception of `pUserData`. Making changes to the internal data will cause issues syncronising data
  /// </summary>
  public struct udSceneNode
  {
    // Node header data
    public readonly int isVisible; //!< Non-zero if the node is visible and should be drawn in the scene
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
    public readonly char[] UUID; //!< Unique identifier for this node "id"
    public readonly double lastUpdate; //!< The last time this node was updated in UTC

    public readonly udSceneNodeType itemtype; //!< The type of this node, see udSceneNodeType for more information
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public readonly char[] itemtypeStr; //!< The string representing the type of node. If its a known type during node creation `itemtype` will be set to something other than udPNT_Custom

    public readonly IntPtr pName; //!< Human readable name of the item
    public readonly IntPtr pURI; //!< The address or filename that the resource can be found.

    public readonly UInt32 hasBoundingBox; //!< Set to not 0 if this nodes boundingBox item is filled out
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public readonly double[] boundingBox; //!< The bounds of this model, ordered as [West, South, Floor, East, North, Ceiling]

    // Geometry Info
    public readonly udSceneGeometryType geomtype; //!< Indicates what geometry can be found in this model. See the udSceneGeometryType documentation for more information.
    public readonly int geomCount; //!< How many geometry items can be found on this model
    public readonly IntPtr pCoordinates; //!< The positions of the geometry of this node (NULL this this node doesn't have points). The format is [X0,Y0,Z0,...Xn,Yn,Zn]

    // Parent node
    public readonly IntPtr pParent; //!< This is the parent item of the current node (NULL if root node)

    // Next nodes
    public readonly IntPtr pNextSibling; //!< This is the next item in the scene (NULL if no further siblings)
    public readonly IntPtr pFirstChild; //!< Some types ("folder", "collection", "polygon"...) have children nodes, NULL if there are no children.

    // Node Data
    public IntPtr pUserDataCleanup; //!< When a project node is deleted, this function is called first
    public IntPtr pUserData; //!< This is application specific user data. The application should traverse the tree to release these before releasing the udScene
    public readonly IntPtr pInternalData; //!< Internal udSDK specific state for this node
  };
  
  /// <summary>
  /// This represents where the scene was loaded from/saved to most recently and where future calls to udScene_Save will go
  /// </summary>
  public enum udSceneLoadSource
  {
    udSceneLoadSource_Memory, //!< The scene source exists in memory; udScene_CreateInMemory, udScene_LoadFromMemory or udScene_SaveToMemory
    udSceneLoadSource_Server, //!< The scene source exists from the server; udScene_CreateInServer, udScene_LoadFromServer or udScene_SaveToServer
    udSceneLoadSource_URI, //!< The scene source exists from a file or URL; udScene_CreateInFile, udScene_LoadFromFile or udScene_SaveToFile

    udSceneLoadSource_Count //!< Total number of source types. Used internally but can be used as an iterator max when displaying different source types.
  };
  
  /// <summary>
  /// This represents the update info given/received to/by udScene_Update
  /// Memory is Freed on next call of udScene_Updte()
  /// </summary>
  public struct udSceneUpdateInfo
  {
    public UInt32 forceSync; //!< If this is non-zero the sync to the server will happen immediately and the update call will block

    public IntPtr pCameraPositions; //!< The position of each camera 
    public UInt32 count; //!< The lenght of pCameraPositions

    public IntPtr pUserList; //!< The list of position for each user on this project
    public UInt32 usersCount; //!< The lenght of pUserList

    public IntPtr pSelectedNodesList; //!< The list of selected nodes
    public UInt32 selectedNodesCount; //!< The length of pSelectedNodesList

    public udAvatarInfo avatar; //!< The info required to display the avatar

    public IntPtr pReceivedMessages; //!< The list of messages
    public UInt32 receivedMessagesCount; //!< The length of pReceivedMessages
  };
  
  public static partial class udScene_f
  {
    /// <summary>
    /// Create an empty, local only, instance of `udScene`.
    /// </summary>
    /// <param name="pContext">The pointer to the udContext of the session to use to creae in memory.</param>
    /// <param name="ppScene">The pointer pointer of the udScene. This will allocate an instance of udScene into `ppScene`.</param>
    /// <param name="pName">The name of the scene, this will be the name of the root item.</param>
    /// <returns>A udError value based on the result of the project creation.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_CreateInMemory")]
    public static extern udError CreateInMemory(IntPtr pContext, ref IntPtr ppScene, string pName);
    
    /// <summary>
    /// Create an empty, local only, instance of `udScene`.
    /// </summary>
    /// <param name="pContext">The pointer to the udContext of the session to use to create ina file.</param>
    /// <param name="ppScene">The pointer pointer of the udScene. This will allocate an instance of udScene into `ppScene`.</param>
    /// <param name="pName">The name of the project, this will be the name of the root item.</param>
    /// <param name="pFilename">The path to create the project at.</param>
    /// <returns>A udError value based on the result of the project creation.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_CreateInFile")]
    public static extern udError CreateInFile(IntPtr pContext, ref IntPtr ppScene, string pName, string pFilename);
    
    /// <summary>
    /// Create an empty project in the server and the local instance of `udScene`.
    /// </summary>
    /// <param name="pContext">The pointer to the udContext of the session to use to load the project, read/write permissions will be based on the current session.</param>
    /// <param name="ppScene">The pointer pointer of the udScene. This will allocate an instance of udScene into `ppScene`.</param>
    /// <param name="pName">The name of the project, this will be the name of the root item.</param>
    /// <param name="pGroupID">The UUID of the group this project will belong to.</param>
    /// <returns>A udError value based on the result of the project creation.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_CreateInServer")]
    public static extern udError CreateInServer(IntPtr pContext, ref IntPtr ppScene, string pName, string pGroupID);
    
    /// <summary>
    /// Create a local only instance of `udScene` filled in with the contents of a GeoJSON string.
    /// </summary>
    /// <param name="pContext">The pointer to the udContext of the session to use to load from memory.</param>
    /// <param name="ppScene">The pointer pointer of the udScene. This will allocate an instance of udScene into `ppScene`.</param>
    /// <param name="pGeoJSON">The GeoJSON string of the project, this will be unpacked into udSceneNode items.</param>
    /// <returns>A udError value based on the result of the project creation.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_LoadFromMemory")]
    public static extern udError LoadFromMemory(IntPtr pContext, ref IntPtr ppScene, string pGeoJSON);
    
    /// <summary>
    /// Create a local only instance of `udScene` filled in with the contents of the specified project file.
    /// </summary>
    /// <param name="pContext">The pointer to the udContext of the session to use to load from a file.</param>
    /// <param name="ppScene">The pointer pointer of the udScene. This will allocate an instance of udScene into `ppScene`.</param>
    /// <param name="pFilename">The project file URL.</param>
    /// <returns>A udError value based on the result of the project creation.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_LoadFromFile")]
    public static extern udError LoadFromFile(IntPtr pContext, ref IntPtr ppScene, string pFilename);
    
    /// <summary>
    /// Create a local instance of `udScene` filled in from the server.
    /// </summary>
    /// <param name="pContext">The pointer to the udContext of the session to use to load the project, read/write permissions will be based on the current session.</param>
    /// <param name="ppScene">The pointer pointer of the udScene. This will allocate an instance of udScene into `ppScene`.</param>
    /// <param name="pSceneUUID">The UUID for the project that is being requested.</param>
    /// <param name="pGroupID">The ID for the workspace/project for udCloud projects (null for udServer projects).</param>
    /// <returns>A udError value based on the result of the project creation.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_LoadFromServer")]
    public static extern udError LoadFromServer(IntPtr pContext, ref IntPtr ppScene, string pSceneUUID, string pGroupID);
    
    /// <summary>
    /// Destroy the instance of the project.
    /// @warning The pUserData item should be already released from every node before calling this.
    /// </summary>
    /// <param name="ppScene">The pointer pointer of the udScene. This will deallocate the instance of the project as well as destroying all nodes.</param>
    /// <returns>A udError value based on the result of the project destruction.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_Release")]
    public static extern udError Release(ref IntPtr ppScene);
    
    /// <summary>
    /// Export a project to where it was loaded from (server or file).
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene that will saved. This will fail immediately for projects loaded from memory.</param>
    /// <returns>A udError value based on the result of the project save.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_Save")]
    public static extern udError Save(IntPtr pScene);
    
    /// <summary>
    /// Update a project to where it was loaded from (server or file).
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene that will saved. This will fail immediately for projects loaded from memory.</param>
    /// <param name="pUpdateInfo">The data that would be updated.</param>
    /// <returns>A udError value based on the result of the project save.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_Update")]
    public static extern udError Update(IntPtr pScene, IntPtr pUpdateInfo);
    
    /// <summary>
    /// Export a project to a GeoJSON string in memory.
    /// @warning The memory is stored in the udScene and subsequent calls will destroy older versions, the buffer is released when the project is released.
    /// </summary>
    /// <param name="pContext">The pointer to the udContext of the session to use to save the project to memory.</param>
    /// <param name="pScene">The pointer to a valid udScene that will be exported as GeoJSON.</param>
    /// <param name="ppMemory">The pointer pointer to a string that will contain the exported GeoJSON.</param>
    /// <returns>A udError value based on the result of the project export.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_SaveToMemory")]
    public static extern udError SaveToMemory(IntPtr pContext, IntPtr pScene, IntPtr ppMemory);
    
    /// <summary>
    /// Create an project in the server from an existing udScene.
    /// </summary>
    /// <param name="pContext">The pointer to the udContext of the session to use to upload the project, write permissions will be based on the current session.</param>
    /// <param name="pScene">The udScene to upload.</param>
    /// <param name="pGroupID">The UUID of the group this project will belong to.</param>
    /// <returns>A udError value based on the result of the project creation.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_SaveToServer")]
    public static extern udError SaveToServer(IntPtr pContext, IntPtr pScene, string pGroupID);
    
    /// <summary>
    /// Create an project in the server from an existing udScene.
    /// </summary>
    /// <param name="pContext">The pointer to the udContext of the session to use to save the project to file.</param>
    /// <param name="pScene">The udScene to save out.</param>
    /// <param name="pPath">The new path to save the project to.</param>
    /// <returns>A udError value based on the result of the project creation.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_SaveToFile")]
    public static extern udError SaveToFile(IntPtr pContext, IntPtr pScene, string pPath);

    /// <summary>
    /// Get the project root node.
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene.</param>
    /// <param name="ppRootNode">The pointer pointer to a node that will contain the node. The node ownership still belongs to the pScene.</param>
    /// <returns>A udError value based on the result of getting the root node.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_GetProjectRoot")]
    public static extern udError GetProjectRoot(IntPtr pScene, ref IntPtr ppRootNode);
    
    /// <summary>
    /// Get the project UUID (for server projects).
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene.</param>
    /// <param name="ppSceneUUID">The pointer pointer to memory that will contain the project UUID. The ownership still belongs to the pScene.</param>
    /// <returns>A udError value based on the result of getting the root node.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_GetProjectUUID")]
    public static extern udError GetProjectUUID(IntPtr pScene, ref IntPtr ppSceneUUID);
    
    /// <summary>
    /// Get the state of unsaved local changes.
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene.</param>
    /// <returns>UdE_Success if there a unsaved changes, udE_NotFound when no local changes exist and other udError values for errors.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_HasUnsavedChanges")]
    public static extern udError HasUnsavedChanges(IntPtr pScene);
    
    /// <summary>
    /// Gets the current source of a given udScene.
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene.</param>
    /// <param name="pSource">The pointer to memory to write the source of pScene to.</param>
    /// <returns>UdE_Success if wrote source, error otherwise.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_GetLoadSource")]
    public static extern udError GetLoadSource(IntPtr pScene, udSceneLoadSource pSource);
  }

  public static class udSceneNode_f
  {
    /// <summary>
    /// Create a node in a project.
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene.</param>
    /// <param name="ppNode">A pointer pointer to the node that will be created. This can be NULL if getting the node back isn't required.</param>
    /// <param name="pParent">The parent node of the item.</param>
    /// <param name="pType">The string representing the type of the item. This can be a defined string provided by udScene_GetTypeName or a custom type. Cannot be NULL.</param>
    /// <param name="pName">A human readable name for the item. If this item is NULL it will attempt to generate a name from the pURI or the pType strings.</param>
    /// <param name="pURI">The URL, filename or other URI containing where to find this item. These should be absolute paths where applicable (preferably HTTPS) to ensure maximum interop with other packages.</param>
    /// <param name="pUserData">A pointer to data provided by the application to store in the `pUserData` item in the udSceneNode.</param>
    /// <returns>A udError value based on the result of the node being created in the project.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_Create")]
    public static extern udError Create(IntPtr pScene, IntPtr ppNode, IntPtr pParent, string pType, string pName, string pURI, IntPtr pUserData);
    
    /// <summary>
    /// Move a node to reorder within the current parent or move to a different parent.
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene.</param>
    /// <param name="pCurrentParent">The current parent of pNode.</param>
    /// <param name="pNewParent">The intended new parent of pNode.</param>
    /// <param name="pNode">The node to move.</param>
    /// <param name="pInsertBeforeChild">The node that will be after pNode after the move. Set as NULL to be the last child of pNewParent.</param>
    /// <returns>A udError value based on the result of the move.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_MoveChild")]
    public static extern udError MoveChild(IntPtr pScene, IntPtr pCurrentParent, IntPtr pNewParent, IntPtr pNode, IntPtr pInsertBeforeChild);
    
    /// <summary>
    /// Remove a node from the project.
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene.</param>
    /// <param name="pParentNode">The parent of the node to be removed.</param>
    /// <param name="pNode">The node to remove from the project.</param>
    /// <returns>A udError value based on the result of removing the node.</returns>
    /// <summary>
    /// @warning Ensure that the pUserData point of pNode has been released before calling this.
    /// </summary>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_RemoveChild")]
    public static extern udError RemoveChild(IntPtr pScene, IntPtr pParentNode, IntPtr pNode);
    
    /// <summary>
    /// Set the human readable name of a node.
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene.</param>
    /// <param name="pNode">The node to change the name of.</param>
    /// <param name="pNodeName">The new name of the node. This is duplicated internally.</param>
    /// <returns>A udError value based on the result of setting the name.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_SetName")]
    public static extern udError SetName(IntPtr pScene, IntPtr pNode, string pNodeName);
    
    /// <summary>
    /// Set the visibility of the node.
    /// </summary>
    /// <param name="pNode">The node to change the visibility.</param>
    /// <param name="visibility">The new visibility of the node (non-zero for visible).</param>
    /// <returns>A udError value based on the result of updating the visibility.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_SetVisibility")]
    public static extern udError SetVisibility(IntPtr pNode, int visibility);
    
    /// <summary>
    /// Set the URI of a node.
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene.</param>
    /// <param name="pNode">The node to change the name of.</param>
    /// <param name="pNodeURI">The new URI of the node. This is duplicated internally.</param>
    /// <returns>A udError value based on the result of setting the URI.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_SetURI")]
    public static extern udError SetURI(IntPtr pScene, IntPtr pNode, IntPtr pNodeURI);
    
    /// <summary>
    /// Set a bounding box for the node.
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene.</param>
    /// <param name="pNode">The node to change the bounding box of.</param>
    /// <param name="boundingBox">An array of doubles representing the bounds of the node, ordered as [West, South, Floor, East, North, Ceiling].</param>
    /// <returns>A udError value based on the result of setting the bounding box.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_SetBoundingBox")]
    public static extern udError SetBoundingBox(IntPtr pScene, IntPtr pNode, double[] boundingBox);
    
    /// <summary>
    /// Set the new geometry of a node.
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene.</param>
    /// <param name="pNode">The node to change the geometry of.</param>
    /// <param name="nodeType">The new type of the geometry.</param>
    /// <param name="geometryCount">How many coordinates are being added.</param>
    /// <param name="pCoordinates">A pointer to the new coordinates (this is an array that should be 3x the length of geometryCount). Ordered in [ longitude0, latitude0, altitude0, ..., longitudeN, latitudeN, altitudeN ] order.</param>
    /// <returns>A udError value based on the result of setting the geometry.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_SetGeometry")]
    public static extern udError SetGeometry(IntPtr pScene, IntPtr pNode, udSceneGeometryType nodeType, int geometryCount, double[] pCoordinates);
    
    /// <summary>
    /// Get a metadata item of a node as an integer.
    /// </summary>
    /// <param name="pNode">The node to get the metadata from.</param>
    /// <param name="pMetadataKey">The name of the metadata key.</param>
    /// <param name="pInt">The pointer to the memory to write the metadata to.</param>
    /// <param name="defaultValue">The value to write to pInt if the metadata item isn't in the udSceneNode or if it isn't of an integer type.</param>
    /// <returns>A udError value based on the result of reading the metadata into pInt.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_GetMetadataInt")]
    public static extern udError GetMetadataInt(IntPtr pNode, string pMetadataKey, ref Int32 pInt, Int32 defaultValue);
    
    /// <summary>
    /// Set a metadata item of a node from an integer.
    /// </summary>
    /// <param name="pNode">The node to get the metadata from.</param>
    /// <param name="pMetadataKey">The name of the metadata key.</param>
    /// <param name="iValue">The integer value to write to the metadata key.</param>
    /// <returns>A udError value based on the result of writing the metadata into the node.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_SetMetadataInt")]
    public static extern udError SetMetadataInt(IntPtr pNode, string pMetadataKey, Int32 iValue);
    
    /// <summary>
    /// Get a metadata item of a node as an unsigned integer.
    /// </summary>
    /// <param name="pNode">The node to get the metadata from.</param>
    /// <param name="pMetadataKey">The name of the metadata key.</param>
    /// <param name="pInt">The pointer to the memory to write the metadata to.</param>
    /// <param name="defaultValue">The value to write to pInt if the metadata item isn't in the udSceneNode or if it isn't of an integer type.</param>
    /// <returns>A udError value based on the result of reading the metadata into pInt.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_GetMetadataUint")]
    public static extern udError GetMetadataUint(IntPtr pNode, string pMetadataKey, ref UInt32 pInt, UInt32 defaultValue);
    
    /// <summary>
    /// Set a metadata item of a node from an unsigned integer.
    /// </summary>
    /// <param name="pNode">The node to get the metadata from.</param>
    /// <param name="pMetadataKey">The name of the metadata key.</param>
    /// <param name="iValue">The unsigned integer value to write to the metadata key.</param>
    /// <returns>A udError value based on the result of writing the metadata into the node.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_SetMetadataUint")]
    public static extern udError SetMetadataUint(IntPtr pNode, string pMetadataKey, UInt32 iValue);
    
    /// <summary>
    /// Get a metadata item of a node as a 64 bit integer.
    /// </summary>
    /// <param name="pNode">The node to get the metadata from.</param>
    /// <param name="pMetadataKey">The name of the metadata key.</param>
    /// <param name="pInt64">The pointer to the memory to write the metadata to.</param>
    /// <param name="defaultValue">The value to write to pInt64 if the metadata item isn't in the udSceneNode or if it isn't of an integer type.</param>
    /// <returns>A udError value based on the result of reading the metadata into pInt64.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_GetMetadataInt64")]
    public static extern udError GetMetadataInt64(IntPtr pNode, string pMetadataKey, ref Int64 pInt64, Int64 defaultValue);
    
    /// <summary>
    /// Set a metadata item of a node from a 64 bit integer.
    /// </summary>
    /// <param name="pNode">The node to get the metadata from.</param>
    /// <param name="pMetadataKey">The name of the metadata key.</param>
    /// <param name="i64Value">The integer value to write to the metadata key.</param>
    /// <returns>A udError value based on the result of writing the metadata into the node.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_SetMetadataInt64")]
    public static extern udError SetMetadataInt64(IntPtr pNode, string pMetadataKey, Int64 i64Value);
    
    /// <summary>
    /// Get a metadata item of a node as a double.
    /// </summary>
    /// <param name="pNode">The node to get the metadata from.</param>
    /// <param name="pMetadataKey">The name of the metadata key.</param>
    /// <param name="pDouble">The pointer to the memory to write the metadata to.</param>
    /// <param name="defaultValue">The value to write to pDouble if the metadata item isn't in the udSceneNode or if it isn't of an integer or floating point type.</param>
    /// <returns>A udError value based on the result of reading the metadata into pDouble.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_GetMetadataDouble")]
    public static extern udError GetMetadataDouble(IntPtr pNode, string pMetadataKey, ref double pDouble, double defaultValue);
    
    /// <summary>
    /// Set a metadata item of a node from a double.
    /// </summary>
    /// <param name="pNode">The node to get the metadata from.</param>
    /// <param name="pMetadataKey">The name of the metadata key.</param>
    /// <param name="doubleValue">The double value to write to the metadata key.</param>
    /// <returns>A udError value based on the result of writing the metadata into the node.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_SetMetadataDouble")]
    public static extern udError SetMetadataDouble(IntPtr pNode, string pMetadataKey, double doubleValue);
    
    /// <summary>
    /// Get a metadata item of a node as a boolean.
    /// </summary>
    /// <param name="pNode">The node to get the metadata from.</param>
    /// <param name="pMetadataKey">The name of the metadata key.</param>
    /// <param name="pBool">The pointer to the memory to write the metadata to (0=false, !0=true).</param>
    /// <param name="defaultValue">The value to write to pBool if the metadata item isn't in the udSceneNode or if it isn't of a boolean type.</param>
    /// <returns>A udError value based on the result of reading the metadata into pBool.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_GetMetadataBool")]
    public static extern udError GetMetadataBool(IntPtr pNode, string pMetadataKey, ref UInt32 pBool, UInt32 defaultValue);
    
    /// <summary>
    /// Set a metadata item of a node from a boolean.
    /// </summary>
    /// <param name="pNode">The node to get the metadata from.</param>
    /// <param name="pMetadataKey">The name of the metadata key.</param>
    /// <param name="boolValue">The boolean value to write to the metadata key (0=false, !0=true).</param>
    /// <returns>A udError value based on the result of writing the metadata into the node.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_SetMetadataBool")]
    public static extern udError SetMetadataBool(IntPtr pNode, string pMetadataKey, UInt32 boolValue);
    
    /// <summary>
    /// Get a metadata item of a node as a string.
    /// </summary>
    /// <param name="pNode">The node to get the metadata from.</param>
    /// <param name="pMetadataKey">The name of the metadata key.</param>
    /// <param name="ppString">The pointer pointer to the memory of the string. This is owned by the udSceneNode and should be copied if required to be stored for a long period.</param>
    /// <param name="pDefaultValue">The value to write to ppString if the metadata item isn't in the udSceneNode or if it isn't of a string type.</param>
    /// <returns>A udError value based on the result of reading the metadata into ppString.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_GetMetadataString")]
    public static extern udError GetMetadataString(IntPtr pNode, string pMetadataKey, IntPtr ppString, string pDefaultValue);
    
    /// <summary>
    /// Set a metadata item of a node from a string.
    /// </summary>
    /// <param name="pNode">The node to get the metadata from.</param>
    /// <param name="pMetadataKey">The name of the metadata key.</param>
    /// <param name="pString">The string to write to the metadata key. This is duplicated internally.</param>
    /// <returns>A udError value based on the result of writing the metadata into the node.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udSceneNode_SetMetadataString")]
    public static extern udError SetMetadataString(IntPtr pNode, string pMetadataKey, string pString);
  }

  public static partial class udScene_f
  {
    /// <summary>
    /// Get the standard type string name for an itemtype.
    /// </summary>
    /// <param name="itemtype">The udSceneNodeType value.</param>
    /// <returns>A string containing the standard name for the item in the udSceneNodeType enum. This is internal and should not be freed.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_GetTypeName")]
    public static extern string GetTypeName(udSceneNodeType itemtype);

    /// <summary>
    /// Deletes a project from the server.
    /// </summary>
    /// <param name="pContext">The udContext to use to communicate with the server.</param>
    /// <param name="pSceneUUID">The UUID of the project to delete.</param>
    /// <param name="pGroupID">The ID for the workspace/project for udCloud projects (null for udServer projects).</param>
    /// <returns>A udError result (udE_Success if it was deleted, other status codes depending on result).</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_DeleteServerProject")]
    public static extern udError DeleteServerProject(IntPtr pContext, IntPtr pSceneUUID, IntPtr pGroupID);

    /// <summary>
    /// Sets the share status of a project on the server.
    /// </summary>
    /// <param name="pContext">The udContext to use to communicate with the server.</param>
    /// <param name="pSceneUUID">The UUID of the project to update the share status of.</param>
    /// <param name="isSharableToAnyoneWithTheLink">Not 0 if the project should be able to be loaded by anyone with the link, 0 otherwise.</param>
    /// <param name="pGroupID">The ID for the workspace/project for udCloud projects (null for udServer projects).</param>
    /// <returns>A udError result (udE_Success if it was updated, other status codes depending on result).</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_SetLinkShareStatus")]
    public static extern udError SetLinkShareStatus(IntPtr pContext, IntPtr pSceneUUID, UInt32 isSharableToAnyoneWithTheLink, IntPtr pGroupID);

    /// <summary>
    /// Get the session ID (for server projects).
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene.</param>
    /// <param name="ppSessionID">The pointer pointer to memory that will contain the session ID. The ownership still belongs to the pScene.</param>
    /// <returns>A udError value based on the result of getting the root node.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_GetSessionID")]
    public static extern udError GetSessionID(IntPtr pScene, ref IntPtr ppSessionID);

    /// <summary>
    /// Queues a message to be sent on next project update (for server projects).
    /// </summary>
    /// <param name="pScene">The pointer to the active udScene.</param>
    /// <param name="pTargetSessionID">The session ID of the user who will receive this message. Passing in 'nullptr' to this parameter indicates all users (except the sender) will receive this message.</param>
    /// <param name="pMessageType">PMessageType User defined message type.</param>
    /// <param name="pMessagePayload">User defined payload.</param>
    /// <returns>A udError result (udE_Success if the message was successfully queued, other status codes depending on result).</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_QueueMessage")]
    public static extern udError QueueMessage(IntPtr pScene, string pTargetSessionID, string pMessageType, string pMessagePayload);

    /// <summary>
    /// Saves a project thumbnail in base64 format to the server (udCloud only).
    /// </summary>
    /// <param name="pScene">The pointer to a valid udScene to save the thumbnail to.</param>
    /// <param name="pImageBase64">The base64 encoded thumbnail.</param>
    /// <returns>A udError value based on the result of the save.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udScene_SaveThumbnail")]
    public static extern udError SaveThumbnail(IntPtr pScene, IntPtr pImageBase64);
  }
}