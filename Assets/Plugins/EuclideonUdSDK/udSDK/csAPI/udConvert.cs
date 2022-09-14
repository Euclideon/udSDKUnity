using System;
using System.Runtime.InteropServices;

//! The **udConvertContext** object provides an interface to create a Euclideon Unlimited Detail model from a number of supported pointcloud formats.
//! Once instantiated, the **udConvertContext** object can be populated with input files and various conversion settings, before initiating the conversion process.

namespace udSDK
{
  /// <summary>
  /// Provides a copy of a subset of the convert state
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udConvertInfo
  {
    public string pOutputName; //!< The output filename
    public string pTempFilesPrefix; //!< The file prefix for temp files

    public string pMetadata; //!< The metadata that will be added to this model (in JSON format)

    public udAttributeSet attributes; //!< The attributes in this model

    public Int32 ignoredAttributesLength; //!< The length of the ignored attributes list
    public IntPtr[] ppIgnoredAttributes; //!< The list of ignored attributes

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public double[] globalOffset; //!< This amount is added to every point during conversion. Useful for moving the origin of the entire scene to geolocate

    public double minPointResolution; //!< The native resolution of the highest resolution file
    public double maxPointResolution; //!< The native resolution of the lowest resolution file
    public UInt32  skipErrorsWherePossible; //!< If not 0 it will continue processing other files if a file is detected as corrupt or incorrect

    public UInt32 everyNth; //!< If this value is >1, only every Nth point is included in the model. e.g. 4 means only every 4th point will be included, skipping 3/4 of the points
    public UInt32 polygonVerticesOnly; //!< If not 0 it will skip rasterization of polygons in favour of just processing the vertices
    public UInt32 retainPrimitives; //!< If not 0 rasterised primitives such as triangles/lines/etc are retained to be rendered at finer resolution if required at runtime
    public UInt32 bakeLighting; //!< if not 0 bake the normals into the colour channel in the output UDS file
    public UInt32 exportOtherEmbeddedAssets; //!< if not 0 export images contained in e57 files

    public UInt32 overrideResolution; //!< Set to not 0 to stop the resolution from being recalculated
    public double pointResolution; //!< The scale to be used in the conversion (either calculated or overriden)

    public UInt32 overrideSRID; //!< Set to not 0 to prevent the SRID being recalculated
    public int srid; //!< The geospatial reference ID (either calculated or overriden)
    public readonly string pWKT; //!< The geospatial WKT string

    public UInt64 totalPointsRead; //!< How many points have been read in this model
    public UInt64 totalItems; //!< How many items are in the list

    // These are quick stats while converting
    public UInt64 currentInputItem; //!< The index of the item that is currently being read
    public UInt64 outputFileSize; //!< Size of the result UDS file
    public UInt64 sourcePointCount; //!< Number of points added (may include duplicates or out of range points)
    public UInt64 uniquePointCount; //!< Number of unique points in the final model
    public UInt64 discardedPointCount; //!< Number of duplicate or ignored out of range points
    public UInt64 outputPointCount; //!< Number of points written to UDS (can be used for progress)
    public UInt64 peakDiskUsage; //!< Peak amount of disk space used including both temp files and the actual output file
    public UInt64 peakTempFileUsage; //!< Peak amount of disk space that contained temp files
    public UInt32 peakTempFileCount; //!< Peak number of temporary files written
  };

  /// <summary>
  /// Provides a copy of a subset of a convert item state
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udConvertItemInfo
  {
    public string pFilename; //!< Name of the input file
    public Int64 pointsCount; //!< This might be an estimate, -1 is no estimate is available
    public UInt64 pointsRead; //!< Once conversation begins, this will give an indication of progress
    public double estimatedResolution; //!< The estimated scale of the item
    public int srid; //!< The calculated geospatial reference ID of the item
  };

  public static partial class udConvert_f
  {
    /// <summary>
    /// Create a udConvertContext to convert models to the Euclideon file format.
    /// </summary>
    /// <param name="pContext">The context to be used to create the convert context.</param>
    /// <param name="ppConvertContext">The pointer pointer of the udConvertContext. This will allocate an instance of `udConvertContext` into `ppConvertContext`.</param>
    /// <returns>A udError value based on the result of the convert context creation.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_CreateContext")]
    public static extern udError CreateContext(IntPtr pContext, ref IntPtr ppConvertContext);

    
    /// <summary>
    /// Destroys the instance of `ppConvertContext`.
    /// </summary>
    /// <param name="ppConvertContext">The pointer pointer of the udConvertContext. This will deallocate the instance of `udConvertContext`.</param>
    /// <returns>A udError value based on the result of the convert context destruction.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_DestroyContext")]
    public static extern udError DestroyContext(ref IntPtr ppConvertContext);
    
    /// <summary>
    /// Sets the filename of the output UDS.
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the output filename.</param>
    /// <param name="pFilename">The filename to set for the output.</param>
    /// <returns>A udError value based on the result of setting the output filename.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetOutputFilename")]
    public static extern udError SetOutputFilename(IntPtr pConvertContext, string pFilename);

    /// <summary>
    /// Sets the temporary output directory for the conversion.
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the temporary directory.</param>
    /// <param name="pFolder">The folder path to set for the temporary directory.</param>
    /// <returns>A udError value based on the result of setting the temporary directory.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetTempDirectory")]
    public static extern udError SetTempDirectory(IntPtr pConvertContext, string pFolder);
    
    // NOTE: the override argument below has the underscore because it's a keyword in C#
    /// <summary>
    /// Sets the point resolution for the conversion.
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the point resolution.</param>
    /// <param name="_override">A boolean value (0 is false) to indicate whether to override the point resolution or use the auto-detected value.</param>
    /// <param name="pointResolutionMeters">The point resolution in meters.</param>
    /// <returns>A udError value based on the result of setting the point resolution.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetPointResolution")]
    public static extern udError SetPointResolution(IntPtr pConvertContext, UInt32 _override, double pointResolutionMeters);
    
    /// <summary>
    /// Flags an attribute to be ignored for the conversion.
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the ignored attribute.</param>
    /// <param name="pAttributeName">The name of the attribute to be ignored.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_IgnoreAttribute")]
    public static extern udError IgnoreAttribute(IntPtr pConvertContext, string pAttributeName);
    
    /// <summary>
    /// Includes an attribute in the conversion if the attribute has previously been ignored.
    /// </summary>
    /// <param name="pConvertContext">The convert context to restore attribute.</param>
    /// <param name="pAttributeName">The name of the attribute to be restored.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_RestoreAttribute")]
    public static extern udError RestoreAttribute(IntPtr pConvertContext, string pAttributeName);
    
    /// <summary>
    /// Sets the prefix of the attribute, used when displaying values to users.
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the attribute prefix.</param>
    /// <param name="pAttributeName">The name of the attribute to set the prefix for.</param>
    /// <param name="pPrefix">The prefix to use for the attribute.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetAttributePrefix")]
    public static extern udError SetAttributePrefix(IntPtr pConvertContext, string pAttributeName, string pPrefix);
    
    /// <summary>
    /// Sets the suffix of the attribute, used when displaying values to users.
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the attribute suffix.</param>
    /// <param name="pAttributeName">The name of the attribute to set the suffix for.</param>
    /// <param name="pSuffix">The suffix to use for the attribute.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetAttributeSuffix")]
    public static extern udError SetAttributeSuffix(IntPtr pConvertContext, string pAttributeName, string pSuffix);
    
    // NOTE: the override argument below has the underscore because it's a keyword in C#
    /// <summary>
    /// Sets the SRID for the conversion.
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the SRID and WKT using SRID.</param>
    /// <param name="_override">A boolean value (0 is false) to indicate whether to override the SRID or use the auto-detected value.</param>
    /// <param name="srid">The SRID value to use.</param>
    /// <returns>A udError value based on the result of setting the SRID & WKT.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetSRID")]
    public static extern udError SetSRID(IntPtr pConvertContext, UInt32 _override, int srid);
    
    /// <summary>
    /// Sets the WKT for the conversion.
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the SRID and WKT using WKT.</param>
    /// <param name="pWKT">The WKT string to use.</param>
    /// <returns>A udError value based on the result of setting the SRID & WKT.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetWKT")]
    public static extern udError SetWKT(IntPtr pConvertContext, string pWKT);
    
    /// <summary>
    /// This function adds the supplied global offset to each point in the model.
    /// </summary>
    /// <param name="pConvertContext">The convert context to set the offset within.</param>
    /// <param name="globalOffset">An array of 3 Doubles representing the desired offset in X, Y and then Z.</param>
    /// <returns>A udError value based on the result of setting the global offset.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetGlobalOffset")]
    public static extern udError SetGlobalOffset(IntPtr pConvertContext, double[] globalOffset);
    
    /// <summary>
    /// This function sets the convert context up to attempt to skip errors where it can.
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the skip errors where possible option.</param>
    /// <param name="ignoreParseErrorsWherePossible">A boolean value (0 is false) to indicate whether to skip errors where possible.</param>
    /// <returns>A udError value based on the result of setting the skip errors where possible option.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetSkipErrorsWherePossible")]
    public static extern udError SetSkipErrorsWherePossible(IntPtr pConvertContext, UInt32 ignoreParseErrorsWherePossible); 
    
    /// <summary>
    /// `EveryNth` lets the importers know to only include every *_n_*th point. If this is set to 0 or 1, every point will be included.
    /// The first (0th) point is always included regardless of this value.
    ///       Example:
    ///         Setting this to `50` would:
    ///           1. Include the first point(point 0)
    ///           2. Skip 49 points
    ///           3. Include the 50th point
    ///           4. Skip another 49 points
    ///           5. Include the 100th point
    ///           n. ...and so on skipping 49 points and including the 50th until all points from this input are processed.
    ///           The next input would then reset the count and include the 0th, skipping 49 etc.as before.
    /// </summary>
    /// <param name="pConvertContext">The convert context to set the everyNth param on.</param>
    /// <param name="everyNth">How many _n_th points to include. Alternatively, how many (n - 1) points to skip for every point included in the export. _See the example below for a bit more context on what this number means_.</param>
    /// <returns>A udError value based on the result of setting the every Nth option.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetEveryNth")]
    public static extern udError SetEveryNth(IntPtr pConvertContext, UInt32 everyNth);
    
    /// <summary>
    /// This function sets the convert context up to skip rasterization of the polygons, leaving only the vertices.
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the polygonVerticesOnly param on.</param>
    /// <param name="polygonVerticesOnly">A boolean value (0 is false) to indicate whether to skip rasterization of the polygons being converted, leaving only the vertices.</param>
    /// <returns>A udError value based on the result of setting the polygon vertices only option.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetPolygonVerticesOnly")]
    public static extern udError SetPolygonVerticesOnly(IntPtr pConvertContext, UInt32 polygonVerticesOnly);
    
    /// <summary>
    /// This function sets the convert context up to retain rasterised primitives such as lines/triangles to be rendered at finer resolutions at runtime.
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the polygonVerticesOnly param on.</param>
    /// <param name="retainPrimitives">A boolean value (0 is false) to indicate whether to retain the primitives in the output UDS file.</param>
    /// <returns>A udError value based on the result of setting the retainPrimitives option.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetRetainPrimitives")]
    public static extern udError SetRetainPrimitives(IntPtr pConvertContext, UInt32 retainPrimitives);
    
    /// <summary>
    /// This function sets the convert context up to set the udCIF_BakeLightning flag allowing the read point function to bake normals into the colour channel.
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the polygonVerticesOnly param on.</param>
    /// <param name="bakeLighting">A boolean value (0 is false) to indicate whether to bake the normals into the colour channel in the output UDS file.</param>
    /// <returns>A udError value based on the result of setting the retainPrimitives option.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetBakeLighting")]
    public static extern udError SetBakeLighting(IntPtr pConvertContext, UInt32 bakeLighting);
    
    /// <summary>
    /// This function sets the convert context up to set the udCIF_ExportImages flag allowing the open function to export images to png or jpg files.
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the polygonVerticesOnly param on.</param>
    /// <param name="exportImages">A boolean value (0 is false) to indicate whether to export or not images contained in e57 files.</param>
    /// <returns>A udError value based on the result of setting the exportImages option.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetExportOtherEmbeddedAssets")]
    public static extern udError SetExportOtherEmbeddedAssets(IntPtr pConvertContext, UInt32 exportImages);
    
    /// <summary>
    /// This adds a metadata key to the output UDS file. There are no restrictions on the key.
    /// There are a number of 'standard' keys that are recommended to support.
    /// - `Author`: The name of the company that owns or captured the data
    /// - `Comment`: A miscellaneous information section
    /// - `Copyright`: The copyright information
    /// - `License`: The general license information
    /// </summary>
    /// <param name="pConvertContext">The convert context to use to set the metadata key.</param>
    /// <param name="pMetadataKey">The name of the key.This is parsed as a JSON address.</param>
    /// <param name="pMetadataValue">The contents of the key, settings this as `NULL` will remove the key from the system (if it exists). This value is handled internal as a string (won't be parsed as JSON).</param>
    /// <returns>A udError value based on the result of setting the metadata key and value.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetMetadata")]
    public static extern udError SetMetadata(IntPtr pConvertContext, string pMetadataKey, string pMetadataValue);

    /// <summary>
    /// This adds an item to be converted in the convert context.
    /// </summary>
    /// <param name="pConvertContext">The convert context to add the item to.</param>
    /// <param name="pFilename">The file to add to the convert context.</param>
    /// <returns>A udError value based on the result of adding the item.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_AddItem")]
    public static extern udError AddItem(IntPtr pConvertContext, string pFilename);

    /// <summary>
    /// This removes an item to be converted from the convert context.
    /// </summary>
    /// <param name="pConvertContext">The convert context to remove the item from.</param>
    /// <param name="index">The index of the item to remove from the convert context.</param>
    /// <returns>A udError value based on the result of removing the item.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_RemoveItem")]
    public static extern udError RemoveItem(IntPtr pConvertContext, UInt64 index);
    
    /// <summary>
    /// This specifies the projection of the source data.
    /// </summary>
    /// <param name="pConvertContext">The convert context to set the input source projection on.</param>
    /// <param name="index">The index of the item to set the source project on.</param>
    /// <param name="srid">The SRID to use for the specified item.</param>
    /// <returns>A udError value based on the result of setting the source projection.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetInputSourceProjection")]
    public static extern udError SetInputSourceProjection(IntPtr pConvertContext, UInt64 index, int srid);
    
    /// <summary>
    /// This provides a way to get the information of the convert context.
    /// </summary>
    /// <param name="pConvertContext">The convert context to retrieve the information from.</param>
    /// <param name="ppInfo">The pointer pointer of the udConvertInfo. This will be managed by the convert context and does not need to be deallocated.</param>
    /// <returns>A udError value based on the result of getting the information of the convert context.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_GetInfo")]
    public static extern udError GetInfo(IntPtr pConvertContext, IntPtr ppInfo);
    
    /// <summary>
    /// This provides a way to get the information of a specific item in the convert context.
    /// </summary>
    /// <param name="pConvertContext">The convert context to retrieve the item information from.</param>
    /// <param name="index">The index of the item to retrieve the information for from the convert context.</param>
    /// <param name="pInfo">The pointer of the udConvertItemInfo. The will be populated by the convert context from an internal representation.</param>
    /// <returns>A udError value based on the result of getting the information of the specified item.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_GetItemInfo")]
    public static extern udError GetItemInfo(IntPtr pConvertContext, UInt64 index, ref udConvertItemInfo pInfo);
    
    /// <summary>
    /// This begins the conversion process for the provided convert context.
    /// </summary>
    /// <param name="pConvertContext">The convert context on which to start the conversion.</param>
    /// <returns>A udError value based on the result of starting the conversion.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_DoConvert")]
    public static extern udError DoConvert(IntPtr pConvertContext);
    
    /// <summary>
    /// This cancels the running conversion for the provided convert context.
    /// </summary>
    /// <param name="pConvertContext">The convert context on which to cancel the conversion.</param>
    /// <returns>A udError value based on the result of cancelling the conversion.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_Cancel")]
    public static extern udError Cancel(IntPtr pConvertContext);
    
    /// <summary>
    /// This resets the statis for the provided convert context, for example to re-run a previously completed conversion.
    /// </summary>
    /// <param name="pConvertContext">The convert context on which to reset the status.</param>
    /// <returns>A udError value based on the result of resetting the status.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_Reset")]
    public static extern udError Reset(IntPtr pConvertContext);
    
    /// <summary>
    /// This generates a preview of the provided convert context.
    /// </summary>
    /// <param name="pConvertContext">The convert context to generate the preview for.</param>
    /// <param name="ppCloud">The pointer pointer of the udPointCloud. This will allocate an instance of `udPointCloud` into `ppCloud`.</param>
    /// <returns>A udError value based on the result of genearting the preview.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_GeneratePreview")]
    public static extern udError GeneratePreview(IntPtr pConvertContext, IntPtr ppCloud);
    
    /// <summary>
    /// Callback invoked on pointbuffers after being read in during the convert process to conditionally modify points based on buffer contents.
    /// </summary>
    /// <param name="pConvertInput">The convert info associated with the current item being processed at the time the callback is invoked.</param>
    /// <param name="pBuffer">The point buffer containing the points currently being read in prior to any processing done by convert (e.g. reprojection).</param>
    /// <param name="pUserData">Pointer to a struct containing user data used by this function- this may be freed by pCleanUpUserData on completion of processing of convert inputs if necessary.</param>
    /// <returns>A udError to indicate the success of the postprocessing - returning anything other than udE_Success will cause the conversion to fail.</returns>
    public delegate udError PostProcessCallback(IntPtr pConvertInput, udPointBufferF64 pBuffer, IntPtr pUserData); 
    
    /// <summary>
    /// Postprocessing to perform on points as they are read in.
    /// </summary>
    /// <param name="pContext">The convert context.</param>
    /// <param name="callback">Takes the convertInput, a point buffer, a pointer to user data (which must point to memory that is valid for the duration of the convert process); returns udError.
    /// This can be used to modify the points and their attributes as well as modify the contents of the userData Structure.</param>
    /// <param name="pUserData">A pointer to any data used by the callback.</param>
    /// <param name="pCleanUpUserData">A function called with pUserData as the argument once the input has finished processing.</param>
    /// <returns>A udError value based on the result of setting the callback.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_SetPostProcessCallback")]
    public static extern udError SetPostProcessCallback(IntPtr pContext, PostProcessCallback callback, IntPtr pUserData, IntPtr pCleanUpUserData);
    
    /// <summary>
    /// Forces the produced UDS to include the specified attribute despite not being present in any input file. This is useful when these attributes are calculated using a postprocess callback.
    /// </summary>
    /// <param name="pContext">The convert context.</param>
    /// <param name="pAttribute">Descriptor of the attribute to be added. This is copied by the function.</param>
    /// <returns>A udError value based on the result of setting the forced attribute.
    /// udE_CountExceeded if the number of attributes exceeds the limit in a UDS.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_AddOutputAttribute")]
    public static extern udError AddOutputAttribute(IntPtr pContext, udAttributeDescriptor pAttribute);
    
    /// <summary>
    /// Removes the forced attribute at the index specified from the list.
    /// </summary>
    /// <param name="pContext">The convert context.</param>
    /// <param name="index">The index of from the array of forced attributes to remove.</param>
    /// <returns>A udError value based on the result of removing the forced attribute.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_RemoveOutputAttribute")]
    public static extern udError RemoveOutputAttribute(IntPtr pContext, UInt32 index);
  }
}