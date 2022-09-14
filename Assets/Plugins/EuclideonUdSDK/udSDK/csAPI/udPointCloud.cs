using System;
using System.Runtime.InteropServices;

//! The **udPointCloud** object provides an interface to load a Euclideon Unlimited Detail model.
//! Once instantiated, the **udPointCloud** can be queried for metadata information, and rendered with the udRenderContext functions.
//! Future releases will allow users to also query the pointcloud data itself, providing the ability to export to LAS or render sub-sections of the pointcloud.

namespace udSDK
{
    /// <summary>
    /// Combines the traverse context and node index to uniquely identify a node
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct udVoxelID
    {
        public UInt64 index; 
        public IntPtr pTrav; 
        public IntPtr pRenderInfo; 
    }

    /// <summary>
    /// Stores basic information about a udPointCloud
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct udPointCloudHeader
    {
        public double scaledRange; //!< The point cloud's range multiplied by the voxel size
        public double unitMeterScale; //!< The scale to apply to convert to/from metres (after scaledRange is applied to the unitCube)
        public UInt32 totalLODLayers; //!< The total number of LOD layers in this octree
        public double convertedResolution; //!< The resolution this model was converted at
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public double[] storedMatrix; //!< This matrix is the 'default' internal matrix to go from a unit cube to the full size

        public udAttributeSet attributes;   //!< The attributes contained in this pointcloud

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] baseOffset; //!< The offset to the root of the pointcloud in unit cube space
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] pivot; //!< The pivot point of the model, in unit cube space
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] boundingBoxCenter; //!< The center of the bounding volume, in unit cube space
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] boundingBoxExtents; //!< The extents of the bounding volume, in unit cube space  }
    }
    
    /// <summary>
    /// Contains additional loading options passed to `udPointCloud_LoadAdv` 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct udPointCloudLoadOptions
    {
        public UInt32 numberAttributesLimited; //!< indicates whether to limit attributes uing the limitAttributes array
        public UInt32[] pLimitedAttributes; //!< array of booleans corresponding to the pAttributes array in the models original udAttributeSet indicating which attributes to load from the pointcloud
    };
    
    public static class udPointCloud_f
    {
        /// <summary>
        /// Load a udPointCloud from `modelLocation`.
        /// </summary>
        /// <param name="pContext">The context to be used to load the model.</param>
        /// <param name="ppModel">The pointer pointer of the udPointCloud. This will allocate an instance of `udPointCloud` into `ppModel`.</param>
        /// <param name="modelLocation">The location to load the model from. This may be a file location, or a supported protocol (HTTP, HTTPS, FTP).</param>
        /// <param name="pHeader"> If non-null, the provided udPointCloudHeader structure will be writen to</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udPointCloud_Load")]
        public static extern udError Load(IntPtr pContext, ref IntPtr ppModel, string modelLocation, ref udPointCloudHeader pHeader);

        /// <summary>
        /// Load a udPointCloud from `modelLocation` with additional load options.
        /// The application should call **udPointCloud_Unload** with `ppModel` to destroy the object once it's no longer needed.
        /// </summary>
        /// <param name="pContext">The context to be used to load the model.</param>
        /// <param name="ppModel">The pointer pointer of the udPointCloud. This will allocate an instance of `udPointCloud` into `ppModel`.</param>
        /// <param name="modelLocation">The location to load the model from. This may be a file location, or a supported protocol (HTTP, HTTPS, FTP).</param>
        /// <param name="pHeader">If non-null, the provided udPointCloudHeader structure will be writen to</param>
        /// <param name="pOptions">If non-null, the options to be applied when loading the model.</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udPointCloud_LoadAdv")]
        public static extern udError LoadAdv(IntPtr pContext, ref IntPtr ppModel, string modelLocation, ref udPointCloudHeader pHeader, [In, Out] udPointCloudLoadOptions pOptions);

        /// <summary>
        /// Destroys the udPointCloud.
        /// </summary>
        /// <param name="ppModel">The pointer pointer of the udPointCloud. This will deallocate any internal memory used. It may take a few frames before the streamer releases the internal memory.</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udPointCloud_Unload")]
        public static extern udError Unload(ref IntPtr ppModel);
        
        /// <summary>
        /// Get the metadata associated with this object.
        /// </summary>
        /// <param name="pPointCloud">The point cloud model to get the metadata from.</param>
        /// <param name="ppJSONMetadata">The metadata(in JSON) from the model.</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udPointCloud_GetMetadata")]
        public static extern udError GetMetadata(IntPtr pPointCloud, ref string ppJSONMetadata);

        /// <summary>
        /// Get the matrix of this model.
        /// </summary>
        /// <param name="pModel">The point cloud model to get the matrix from.</param>
        /// <param name="pHeader">The header structure to fill out</param>
        /// <returns></returns>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udPointCloud_GetHeader")]
        public static extern udError GetHeader(IntPtr pPointCloud, ref udPointCloudHeader pHeader);
        
        /// <summary>
        /// Exports a point cloud
        /// </summary>
        /// <param name="pModel">The loaded pointcloud to export.</param>
        /// <param name="pExportFilename">The path and filename to export the point cloud to. This should be a file location with write permissions. Supported formats are .UDS and .LAS.</param>
        /// <param name="pFilter">If non-NULL this filter will be applied on the export to export a subsection</param>
        /// <param name="pProgress">If non-NULL, this will be updated with a (estimated) progress throughout the export. Ranges between 0.f - 1.f</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udPointCloud_Export")]
        public static extern udError ExportudPointCloud_Export(IntPtr pModel, string pExportFilename, IntPtr pFilter, ref float pProgress);
        
        /// <summary>
        /// Gets the default colour for a specific voxel in a given point cloud
        /// </summary>
        /// <param name="pModel">The point cloud to get a default colour for.</param>
        /// <param name="voxelID">The voxelID provided by picking or to voxel shaders</param>
        /// <param name="pColour">The address to write the colour of the given voxel to</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udPointCloud_GetNodeColour")]
        public static extern udError GetNodeColour(IntPtr pModel, udVoxelID voxelID, ref UInt32 pColour);

        /// <summary>
        /// Gets the default colour for a specific voxel in a given point cloud
        /// </summary>
        /// <param name="pModel">The point cloud to get a default colour for.</param>
        /// <param name="voxelID">The voxelID provided by picking or to voxel shaders</param>
        /// <param name="pColour">The address to write the colour of the given voxel to</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udPointCloud_GetNodeColour64")]
        public static extern udError GetNodeColour64(IntPtr pModel, udVoxelID pVoxelID, ref UInt64 pColour);
        
        /// <summary>
        /// Gets the pointer to the attribute offset on a specific voxel in a point cloud
        /// </summary>
        /// <param name="pModel">The point cloud to get an address for.</param>
        /// <param name="pVoxelID">The node provided by picking or to voxel shaders</param>
        /// <param name="attributeOffset">The attribute offset from udAttributeSet_GetOffsetOfNamedAttribute or udAttributeSet_GetOffsetOfStandardAttribute</param>
        /// <param name="ppAttributeAddress">The pointer will be updated with the address to the attribute</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udPointCloud_GetAttributeAddress")]
        public static extern udError GetAttributeAddress(IntPtr pModel, udVoxelID voxelID, uint attributeOffset, ref IntPtr ppAttributeAddress);
        
        /// <summary>
        /// Gets the streamer status for the model
        /// </summary>
        /// <param name="pModel">The point cloud to get the status of.</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udPointCloud_GetStreamingStatus")]
        public static extern udError GetStreamingStatus(IntPtr pModel);
        
        /// <summary>
        /// Gets the udAttributeSet of the model
        /// udAttributeSet_Destroy must be called on pAttributeSet 
        /// </summary>
        /// <param name="pModel">The point cloud to get original attributes of.</param>
        /// <param name="pAttributeSet">The attributeSet to be populated</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udPointCloud_GetSourceAttributes")]
        public static extern udError GetSourceAttributes(IntPtr pModel, IntPtr pAttributeSet);
    }
}