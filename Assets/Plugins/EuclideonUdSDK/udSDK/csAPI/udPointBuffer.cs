using System;
using System.Runtime.InteropServices;

namespace udSDK
{
  /// <summary>
  /// Stores a set of indices to be used with a **udPointBuffer**
  /// </summary>
  public struct udPointBufferIndices
  {
    public UInt32 count; //!< Total number of indices currently contained in this object
    public UInt32 capacity; //!< Total number of indices that can fit in this object
    // todo given the size stored in capacity, is this safe to do in c# as such? 
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
    public UInt32[] indices; //!< Array of indices, real length of the array is stored in capacity
  };
  
  /// <summary>
  /// Stores a set of points and their attributes that have positions as double (64bit float) values
  /// </summary>
  public struct udPointBufferF64
  {
    // double
    public IntPtr pPositions; //!< Flat array of XYZ positions in the format XYZXYZXYZXYZXYZXYZXYZ...
    public byte pAttributes; //!< Byte array of attribute data ordered as specified in `attributes`
    public udAttributeSet attributes; //!< Information on the attributes that are available in this point buffer
    public UInt32 positionStride; //!< Total bytes between the start of one position and the start of the next (currently always 24 (8 bytes per int64 * 3 int64))
    public UInt32 attributeStride; //!< Total number of bytes between the start of the attibutes of one point and the first byte of the next attribute
    public UInt32 pointCount; //!< How many points are currently contained in this buffer
    public UInt32 pointsAllocated; //!< Total number of points that can fit in this udPointBufferF64
    public UInt32 _reserved; //!< Reserved for internal use
  }
  
  /// <summary>
  /// Stores a set of points and their attributes that have positions as Int64 values
  /// </summary>
  public struct udPointBufferI64
  {
    // int64
    public IntPtr pPositions; //!< Flat array of XYZ positions in the format XYZXYZXYZXYZXYZXYZXYZ...
    public byte pAttributes; //!< Byte array of attribute data ordered as specified in `attributes`
    public udAttributeSet attributes; //!< Information on the attributes that are available in this point buffer
    public UInt32 positionStride; //!< Total bytes between the start of one position and the start of the next (currently always 24 (8 bytes per int64 * 3 int64))
    public UInt32 attributeStride; //!< Total number of bytes between the start of the attibutes of one point and the first byte of the next attribute
    public UInt32 pointCount; //!< How many points are currently contained in this buffer
    public UInt32 pointsAllocated; //!< Total number of points that can fit in this udPointBufferF64
    public UInt32 _reserved; //!< Reserved for internal use
  }

  /// <summary>
  /// Stores a set of points and their attributes that have positions as UInt64 values
  /// </summary>
  public struct udPointBufferU64
  {
    // uint64
    public IntPtr pPositions;  //!< Flat array of XYZ positions in the format XYZXYZXYZXYZXYZXYZXYZ...
    public byte pAttributes; //!< Byte array of attribute data ordered as specified in `attributes`
    public udAttributeSet attributes; //!< Information on the attributes that are available in this point buffer
    public UInt32 positionStride; //!< Total bytes between the start of one position and the start of the next (currently always 24 (8 bytes per int64 * 3 int64))
    public UInt32 attributeStride; //!< Total number of bytes between the start of the attibutes of one point and the first byte of the next attribute
    public UInt32 pointCount; //!< How many points are currently contained in this buffer
    public UInt32 pointsAllocated; //!< Total number of points that can fit in this udPointBufferF64
    public UInt32 _reserved; //!< Reserved for internal use
  };
  
  public class udPointBufferIndices_f
  {
    /// <summary>
    /// Create a point buffer indices object
    /// </summary>
    /// <param name="ppIndices">The pointer pointer of the udPointBufferIndices. This will allocate an instance of `udPointBufferIndices` into `ppIndices`</param>
    /// <param name="numIndices">The maximum number of indices that this object will contain (these are preallocated to avoid allocations later)</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udPointBufferIndices_Create")]
    public static extern udError Create(ref IntPtr ppIndices, UInt32 numIndices);
    
    /// <summary>
    /// Destroys the udPointBufferIndices.
    /// </summary>
    /// <param name="ppIndices">The pointer pointer of the udPointBufferIndices. This will deallocate any memory used.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udPointBufferIndices_Destroy")]
    public static extern udError Destroy(ref IntPtr ppIndices);
  }
  
  public class udPointBufferF64_f
  {
    /// <summary>
    /// Create a 64bit floating point, point buffer object
    /// </summary>
    /// <param name="ppBuffer">The pointer pointer of the udPointBufferF64. This will allocate an instance of `udPointBufferF64` into `ppBuffer`.</param>
    /// <param name="maxPoints">The maximum number of points that this buffer will contain (these are preallocated to avoid allocations later)</param>
    /// <param name="pAttributes">The pointer to the udAttributeSet containing information on the attributes that will be available in this point buffer; NULL will have no attributes. An internal copy is made of this attribute set.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udPointBufferIndices_Create")]
    public static extern udError Create(ref IntPtr ppBuffer, UInt32 maxPoints, IntPtr pAttributes);
    
    /// <summary>
    /// Destroys the udPointBufferF64.
    /// </summary>
    /// <param name="ppBuffer">The pointer pointer of the ppBuffer. This will deallocate any memory used.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udPointBufferIndices_Destroy")]
    public static extern udError Destroy(ref IntPtr ppBuffer);
  }
  
  public class udPointBufferI64_f
  {
    /// <summary>
    /// Create a 64bit integer, point buffer object
    /// </summary>
    /// <param name="ppBuffer">The pointer pointer of the udPointBufferI64. This will allocate an instance of `udPointBufferI64` into `ppBuffer`.</param>
    /// <param name="maxPoints">The maximum number of points that this buffer will contain (these are preallocated to avoid allocations later)</param>
    /// <param name="pAttributes">The pointer to the udAttributeSet containing information on the attributes that will be available in this point buffer; NULL will have no attributes. An internal copy is made of this attribute set.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udPointBufferIndices_Create")]
    public static extern udError Create(ref IntPtr ppBuffer, UInt32 maxPoints, IntPtr pAttributes);
    
    /// <summary>
    /// Destroys the udPointBufferI64.
    /// </summary>
    /// <param name="ppBuffer">The pointer pointer of the ppBuffer. This will deallocate any memory used.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udPointBufferIndices_Destroy")]
    public static extern udError Destroy(ref IntPtr ppBuffer);
  }
  
  public static class udPointBufferU64_f
  {
    /// <summary>
    /// Create a 64bit unsigned integer, point buffer object
    /// </summary>
    /// <param name="ppBuffer">The pointer pointer of the udPointBufferU64. This will allocate an instance of `udPointBufferU64` into `ppBuffer`.</param>
    /// <param name="maxPoints">The maximum number of points that this buffer will contain (these are preallocated to avoid allocations later)</param>
    /// <param name="pAttributes">The pointer to the udAttributeSet containing information on the attributes that will be available in this point buffer; NULL will have no attributes. An internal copy is made of this attribute set.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udPointBufferIndices_Create")]
    public static extern udError Create(ref IntPtr ppBuffer, UInt32 maxPoints, IntPtr pAttributes);
    
    /// <summary>
    /// Destroys the udPointBufferU64.
    /// </summary>
    /// <param name="ppBuffer">The pointer pointer of the ppBuffer. This will deallocate any memory used.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udPointBufferIndices_Destroy")]
    public static extern udError Destroy(ref IntPtr ppBuffer);
  }
}