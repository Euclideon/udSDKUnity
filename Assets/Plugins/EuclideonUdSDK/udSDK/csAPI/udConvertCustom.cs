using System;
using System.Runtime.InteropServices;

//! udConvertCustomItem provides a way to convert proprietary or unsupported file formats to Unlimited Detail format

namespace udSDK
{
  /// <summary>
  /// Settings the custom converts need to be aware of that are set by the user
  /// </summary>
  public enum udConvertCustomItemFlags
  {
    udCCIF_None = 0, //!< No additional flags specified
    udCCIF_SkipErrorsWherePossible = 1, //!< If its possible to continue parsing, that is perferable to failing
    udCCIF_PolygonVerticesOnly = 2, //!< Do not rasterise the polygons, just use the vertices as points
    udCCIF_BakeLighting = 8, //!< Bake normals into color channel for polygons conversion
    udCCIF_ExportImages = 16, //!< Export images contained in e57 files
  };

  /// Note: not in the header, provided to get around lack of paramaterized function pointer in C#
  /// <summary>
  /// Open the file and provide information on the file (bounds, point count, etc.)
  /// </summary>
  public delegate udError ConvertCustomItem_Open(ref udConvertCustomItem pConvertInput, UInt32 everyNth, double pointResolution, udConvertCustomItemFlags flags);
  
  /// Note: not in the header, provided to get around lack of paramaterized function pointer in C#
  /// <summary>
  /// Provide position and attribute data to convert to UDS
  /// </summary>
  public delegate udError ConvertCustomItem_ReadPointsFloat(ref udConvertCustomItem pConvertInput);

  /// Note: not in the header, provided to get around lack of paramaterized function pointer in C#
  /// <summary>
  /// Cleanup all memory related to this custom convert item
  /// </summary>
  public delegate void ConvertCustomItem_Destroy(ref udConvertCustomItem pConvertInput);

  /// Note: not in the header, provided to get around lack of paramaterized function pointer in C#
  /// <summary>
  /// This function will be called when 
  /// </summary>
  public delegate void ConvertCustomItem_Close(ref udConvertCustomItem pConvertInput);

  /// <summary>
  /// Allows for conversion of custom data formats to UDS
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udConvertCustomItem
  {
    public ConvertCustomItem_Open pOpen; //!< Open the file and provide information on the file (bounds, point count, etc.)
    public ConvertCustomItem_ReadPointsFloat pReadPointsFloat; //!< Provide position and attribute data to convert to UDS
    public ConvertCustomItem_Destroy pDestroy; //!< Cleanup all memory related to this custom convert item
    public ConvertCustomItem_Close pClose; //!< This function will be called when 

    public IntPtr pData; //!< Private user data relevant to the specific geomtype, must be freed by the pClose function

    public string pName; //!< Filename or other identifier
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public double[] boundMin; //!< Optional (see boundsKnown) source space minimum values
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public double[] boundMax; //!< Optional (see boundsKnown) source space maximum values
    public double sourceResolution; //!< Source resolution (eg 0.01 if points are 1cm apart). 0 indicates unknown
    public Int64 pointCount; //!< Number of points coming, -1 if unknown
    public Int32 srid; //!< If non-zero, this input is considered to be within the given srid code (useful mainly as a default value for other files in the conversion)
    public udAttributeSet attributes; //!< Content of the input; this might not match the output
    public UInt32 boundsKnown; //!< If not 0, boundMin and boundMax are valid, if 0 they will be calculated later
    public UInt32 pointCountIsEstimate; //!< If not 0, the point count is an estimate and may be different
  };
  
  public static partial class udConvert_f
  {
    /// <summary>
    /// Adds a prefilled udConvertCustomItem to a udConvertContext
    /// </summary>
    /// <param name="pConvertContext">The convert context to add the item to</param>
    /// <param name="pCustomItem">The custom convert item to add</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_AddCustomItem")]
    public static extern udError AddCustomItem(IntPtr pConvertContext, udConvertCustomItem pCustomItem);
    
    /// <summary>
    /// Registers a format in the udConvertContext to allow users to just call udConvert_AddItem
    /// </summary>
    /// <param name="pConvertContext">The convert context to register the format with</param>
    /// <param name="pExtensionCheck">The callback used to determine if the format should be used, returns 1 when a match, 0 otherwise</param>
    /// <param name="pTryAddItem">The callback used when calling udConvert_AddItem needs to populate the pCustomItem parameter, returns udE_Success on success</param>
    /// <returns></returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConvert_AddCustomItemFormat")]
    public static extern udError AddCustomItemFormat(IntPtr pConvertContext, IntPtr pExtensionCheck, IntPtr pTryAddItem);
  }
}