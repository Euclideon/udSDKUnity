using System;
using System.Runtime.InteropServices;
namespace udSDK
{
  public enum udProjectGeometryType { 
    //These are the geometry types for nodes 
    udPGT_None, //!<There is no geometry associated with this node.
    udPGT_Point, //!<pCoordinates is a single 3D position
    udPGT_MultiPoint, //!<Array of udPGT_Point, pCoordinates is an array of 3D positions.
    udPGT_LineString, //!<pCoordinates is an array of 3D positions forming an open line
    udPGT_MultiLineString, //!<Array of udPGT_LineString; pCoordinates is NULL and children will be present.
    udPGT_Polygon, //!<pCoordinates will be a closed linear ring (the outside), there MAY be children that are interior as pChildren udPGT_MultiLineString items, these should be counted as islands of the external ring.
    udPGT_MultiPolygon, //!<pCoordinates is null, children will be udPGT_Polygon (which still may have internal islands)
    udPGT_GeometryCollection, //!<Array of geometries; pCoordinates is NULL and children may be present of any type.
    udPGT_Count, //!<Total number of geometry types. Used internally but can be used as an iterator max when displaying different type modes.
    udPGT_Internal, //!<Used internally when calculating other types. Do not use.
	}
 public enum udProjectNodeType
  {
    /*
    This represents the type of data stored in the node.

    Note

    The itemtypeStr in the udProjectNode is a string version. This enum serves to simplify the reading of standard types. The the string in brackets at the end of the comment is the string.
     */
    udPNT_Custom, //!<Need to check the itemtypeStr string manually.
    udPNT_PointCloud, //!<A Euclideon Unlimited Detail Point Cloud file (“UDS”)
    udPNT_PointOfInterest, //!<A point, line or region describing a location of interest (“POI”)
    udPNT_Folder, //!<A folder of other nodes (“Folder”)
    udPNT_LiveFeed, //!<A Euclideon udSDK live feed container (“IOT”)
    udPNT_Media, //!<An Image, Movie, Audio file or other media object (“Media”)
    udPNT_Viewpoint, //!<An Camera Location & Orientation (“Camera”)
    udPNT_VisualisationSettings, //!<Visualisation settings (itensity, map height etc) (“VizSet”)
    udPNT_Count, //!<Total number of node types. Used internally but can be used as an iterator max when displaying different type modes.
  }

    [StructLayout(LayoutKind.Sequential)]
  public struct udProjectNode
  {
    /*
    Stores the state of a node.

    Warning

        This struct is exposed to avoid having a huge API of accessor functions but it should be treated as read only with the exception of pUserData. Making changes to the internal data will cause issues syncronising data

    */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
    public readonly char[] UUID; //!<Unique identifier for this node “id”.

    public readonly double lastUpdate; //!<The last time this node was updated in UTC.

    public readonly udProjectNodeType itemtype; //!<The type of this node, see udProjectNodeType for more information.

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public readonly char[] itemtypeStr; //!<The string representing the type of node. If its a known type during node creation itemtype will be set to something other than udPNT_Custom.

    public readonly IntPtr pName; //!<Human readable name of the item.

    public readonly IntPtr pURI; //!<The address or filename that the resource can be found.

    public readonly bool hasBoundingBox; //!<Set to true if this nodes boundingBox item is filled out.

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public readonly double[] boundingBox; //!<The bounds of this model, ordered as [West, South, Floor, East, North, Ceiling].

    public readonly udProjectGeometryType geomtype; //!<Indicates what geometry can be found in this model. See the udProjectGeometryType documentation for more information.

    public readonly int geomCount; //!<How many geometry items can be found on this model.

    public readonly IntPtr pCoordinates; //!<The positions of the geometry of this node (NULL this this node doesn’t have points). The format is [X0,Y0,Z0,…Xn,Yn,Zn].

    public readonly IntPtr pNextSibling; //!<This is the next item in the project (NULL if no further siblings)

    public readonly IntPtr pFirstChild; //!<Some types (“folder”, “collection”, “polygon”…) have children nodes, NULL if there are no children.

    IntPtr pUserData; //!<This is application specific user data. The application should traverse the tree to release these before releasing the udProject.

    public readonly IntPtr pInternalData; //!<Internal udSDK specific state for this node.

  }
	public class UDProject
	{
    IntPtr pudProject;
    public IntPtr pRootNode;

		public UDProject(string geoJSON)
		{
      udError err = udProject_LoadFromMemory(ref pudProject, geoJSON);

      if(err != udError.udE_Success)
        throw new Exception("project load failed: "+ err.ToString());

      pRootNode = IntPtr.Zero;
      udProject_GetProjectRoot(pudProject, ref pRootNode);
		}

    ~UDProject()
    {
      udProject_Release(ref pudProject);
    }


    //Create an empty, local only, instance of udProject.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProject_CreateLocal(ref IntPtr ppProject, string pName);
    //Create a local only instance of udProject filled in with the contents of a GeoJSON string.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProject_LoadFromMemory(ref IntPtr ppProject, string pGeoJSON);
    //Destroy the instance of the project.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProject_Release(ref IntPtr ppProject);
    //Export a project to a GeoJSON string in memory.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProject_WriteToMemory(IntPtr pProject, ref IntPtr ppMemory);
    //Get the project root node.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProject_GetProjectRoot(IntPtr pProject, ref IntPtr ppRootNode);
    //Get the state of unsaved local changes
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProject_HasUnsavedChanges(IntPtr pProject);
    [DllImport(UDSDKLibrary.name)]
    private static extern string udProject_GetTypeName(udProjectNodeType itemtype);


	}

  public class UDProjectNode
  {
    public IntPtr pNode;
    public udProjectNode nodeData;
    public UDProjectNode(IntPtr nodeAddr)
    {
      pNode = nodeAddr;
      this.nodeData = (udProjectNode) Marshal.PtrToStructure(nodeAddr, typeof(udProjectNode));
    }


    //Create a node in a project
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_Create(IntPtr pProject, IntPtr ppNode, ref udProjectNode pParent, string pType, string pName, string pURI, IntPtr pUserData);
    //Move a node to reorder within the current parent or move to a different parent.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_MoveChild(IntPtr pProject, ref udProjectNode pCurrentParent, ref udProjectNode pNewParent, ref udProjectNode pNode, ref udProjectNode pInsertBeforeChild);
    //Remove a node from the project.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_RemoveChild(IntPtr pProject, ref udProjectNode pParentNode, ref udProjectNode pNode);
    //Set the human readable name of a node.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_SetName(IntPtr pProject, ref udProjectNode pNode, string pNodeName);
    //Set the URI of a node.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_SetURI(IntPtr pProject, ref udProjectNode pNode, string pNodeURI);
    //Set the new geometry of a node.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_SetGeometry(IntPtr pProject, ref udProjectNode pNode, udProjectGeometryType nodeType, int geometryCount, ref double pCoordinates);
    //Get a metadata item of a node as an integer.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_GetMetadataInt(ref udProjectNode pNode, string pMetadataKey, ref Int32 pInt, Int32 defaultValue);
    //Set a metadata item of a node from an integer.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_SetMetadataInt(ref udProjectNode pNode, string pMetadataKey, Int32 iValue);
    //Get a metadata item of a node as an unsigned integer.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_GetMetadataUint(ref udProjectNode pNode, string pMetadataKey, ref UInt32 pInt, UInt32 defaultValue);
    //Set a metadata item of a node from an unsigned integer.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_SetMetadataUint(ref udProjectNode pNode, string pMetadataKey, UInt32 iValue);
    //Get a metadata item of a node as a 64 bit integer.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_GetMetadataInt64(ref udProjectNode pNode, string pMetadataKey, ref Int64 pInt64, Int64 defaultValue);
    //Set a metadata item of a node from a 64 bit integer.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_SetMetadataInt64(ref udProjectNode pNode, string pMetadataKey, Int64 i64Value);
    //Get a metadata item of a node as a double.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_GetMetadataDouble(ref udProjectNode pNode, string pMetadataKey, ref double pDouble, double defaultValue);
    //Set a metadata item of a node from a double.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_SetMetadataDouble(ref udProjectNode pNode, string pMetadataKey, double doubleValue);
    //Get a metadata item of a node as an boolean.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_GetMetadataBool(ref udProjectNode pNode, string pMetadataKey, ref bool pBool, bool defaultValue);
    //Set a metadata item of a node from an boolean.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_SetMetadataBool(ref udProjectNode pNode, string pMetadataKey, bool boolValue);
    //Get a metadata item of a node as a string.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_GetMetadataString(ref udProjectNode pNode, string pMetadataKey, ref string ppString, string pDefaultValue);
    //Set a metadata item of a node from a string.
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udProjectNode_SetMetadataString(ref udProjectNode pNode, string pMetadataKey, string pString);
    //Get the standard type string name for an itemtype
  }
}
