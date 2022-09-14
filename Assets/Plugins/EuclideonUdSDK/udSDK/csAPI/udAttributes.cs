using System;
using System.Runtime.InteropServices;

//! udAttributes.h provides an interface to attribute streams of Unlimited Detail models.

namespace udSDK
{
    /// <summary>
    /// A list of standard UDS attributes 
    /// </summary>
    public enum udStdAttribute
    {
        udSA_GPSTime, //!< udATI_float64 
        udSA_PrimitiveID, //!< udATI_uint32  
        udSA_ARGB, //!< udATI_color32 
        udSA_Normal, //!< udATI_normal32
        udSA_Red, //!< Legacy 16bit Red channel
        udSA_Green, //!< Legacy 16bit Green channel
        udSA_Blue, //!< Legacy 16bit Blue channel
        udSA_Intensity, //!< udATI_uint16  
        udSA_NIR, //!< udATI_uint16  
        udSA_ScanAngle, //!< udATI_uint16  
        udSA_PointSourceID, //!< udATI_uint16  
        udSA_Classification, //!< udATI_uint8   
        udSA_ReturnNumber, //!< udATI_uint8   
        udSA_NumberOfReturns, //!< udATI_uint8   
        udSA_ClassificationFlags, //!< udATI_uint8   
        udSA_ScannerChannel, //!< udATI_uint8   
        udSA_ScanDirection, //!< udATI_uint8   
        udSA_EdgeOfFlightLine, //!< udATI_uint8   
        udSA_ScanAngleRank, //!< udATI_uint8   
        udSA_LasUserData, //!< Specific LAS User data field (udATI_uint8)

        udSA_Count, //!< Count helper value to iterate this enum
        udSA_AllAttributes = udSA_Count, //!< Internal sentinal value used by some functions to indicate getting start of interleaved attributes
        udSA_First = 0, //!< Generally used to initialise an attribute value for use in loops
    };

    /// <summary>
    /// The standard UDS attributes provided as a bitfield. Note udStdAttributeContent enums are guaranteed to be 1 << associated udStdAttribute value 
    /// </summary>
    public enum udStdAttributeContent
    {
        udSAC_None = (0),
        udSAC_GPSTime = (1 << udStdAttribute.udSA_GPSTime),
        udSAC_PrimitiveID = (1 << udStdAttribute.udSA_PrimitiveID),
        udSAC_ARGB = (1 << udStdAttribute.udSA_ARGB),
        udSAC_Normal = (1 << udStdAttribute.udSA_Normal),
        udSAC_Red = (1 << udStdAttribute.udSA_Red),
        udSAC_Green = (1 << udStdAttribute.udSA_Green),
        udSAC_Blue = (1 << udStdAttribute.udSA_Blue),
        udSAC_Intensity = (1 << udStdAttribute.udSA_Intensity),
        udSAC_NIR = (1 << udStdAttribute.udSA_NIR),
        udSAC_ScanAngle = (1 << udStdAttribute.udSA_ScanAngle),
        udSAC_PointSourceID = (1 << udStdAttribute.udSA_PointSourceID),
        udSAC_Classification = (1 << udStdAttribute.udSA_Classification),
        udSAC_ReturnNumber = (1 << udStdAttribute.udSA_ReturnNumber),
        udSAC_NumberOfReturns = (1 << udStdAttribute.udSA_NumberOfReturns),
        udSAC_ClassificationFlags = (1 << udStdAttribute.udSA_ClassificationFlags),
        udSAC_ScannerChannel = (1 << udStdAttribute.udSA_ScannerChannel),
        udSAC_ScanDirection = (1 << udStdAttribute.udSA_ScanDirection),
        udSAC_EdgeOfFlightLine = (1 << udStdAttribute.udSA_EdgeOfFlightLine),
        udSAC_ScanAngleRank = (1 << udStdAttribute.udSA_ScanAngleRank),
        udSAC_LasUserData = (1 << udStdAttribute.udSA_LasUserData),

        udSAC_AllAttributes = (1 << udStdAttribute.udSA_AllAttributes) - 1,

        // these are not used, and result in errors otherwise
        // leaving here for the sake of parity 
        // udSAC_64BitAttributes = udSAC_GPSTime,
        // udSAC_32BitAttributes = udSAC_PrimitiveID + udSAC_ARGB + udSAC_Normal,
        // udSAC_16BitAttributes = udSAC_Intensity + udSAC_NIR + udSAC_ScanAngle + udSAC_PointSourceID,
    };

    /// <summary>
    /// These are the various options for how an attribute is calculated when merging voxels while generating LODs
    /// </summary>
    public enum udAttributeBlendType
    {
        udABT_Mean, //!< This blend type merges nearby voxels together and finds the mean value for the attribute on those nodes
        udABT_FirstChild, //!< This blend type selects the value from one of the nodes and uses that
        udABT_NoLOD, //!< This blend type has no information in LODs and is only stored in the highest detail level

        udABT_Count //!< Total number of blend types. Used internally but can be used as an iterator max when checking attribute blend modes.
    };

    /// <summary>
    /// These are the types that could be contained in attributes 
    /// </summary>
    public enum udAttributeTypeInfo
    {
        udATI_Invalid = 0,
        udATI_SizeMask = 0x000ff,  // Lower 8 bits define the size in bytes - currently the actual maximum is 32
        udATI_SizeShift = 0,
        udATI_ComponentCountMask = 0x0ff00,  // Next 8 bits define the number of components, component size is size/componentCount
        udATI_ComponentCountShift = 8,
        udATI_Signed = 0x10000,  // Set if type is signed (used in blend functions)
        udATI_Float = 0x20000,  // Set if floating point type (signed should always be set)
        udATI_Color = 0x40000,  // Set if type is de-quantized from a color
        udATI_Normal = 0x80000,  // Set if type is encoded normal (32 bit = 16:15:1)

        // Some keys to define standard types
        udATI_uint8 = 1,
        udATI_uint16 = 2,
        udATI_uint32 = 4,
        udATI_uint64 = 8,
        udATI_int8 = 1 | udATI_Signed,
        udATI_int16 = 2 | udATI_Signed,
        udATI_int32 = 4 | udATI_Signed,
        udATI_int64 = 8 | udATI_Signed,
        udATI_float32 = 4 | udATI_Signed | udATI_Float,
        udATI_float64 = 8 | udATI_Signed | udATI_Float,
        udATI_color32 = 4 | udATI_Color,
        udATI_normal32 = 4 | udATI_Normal,
        udATI_vec3f32 = 12 | 0x300 | udATI_Signed | udATI_Float
    };

    /// <summary>
    /// Describes the contents of an attribute stream including its size, type and how it gets blended in LOD layers 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct udAttributeDescriptor
    {
        public udAttributeTypeInfo typeInfo; //!< This contains information about the type
        public udAttributeBlendType blendType; //!< Which blend type this attribute is using
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] name; //!< Name of the attibute
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] prefix; //!< Value prefix for display
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] suffix; //!< Value suffix for display
    };

    /// <summary>
    /// Provides a set a attributes and includes an optimized lookup for standard types 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct udAttributeSet
    {
        public udStdAttributeContent content; //!< Which standard attributes are available (used to optimize lookups internally), they still appear in the descriptors
        public uint count; //!< How many udAttributeDescriptor objects are used in pDescriptors
        public uint allocated; //!< How many udAttributeDescriptor objects are allocated to be used in pDescriptors
        public IntPtr pDescriptors; //!< this contains the actual information on the attributes
    };

    public static class udAttributeSet_f
    {
        /// <summary>
        /// Creates a udAttributeSet
        /// </summary>
        /// <param name="pAttributeSet">The attribute set to allocate into</param>
        /// <param name="content">The standard attributes that will be created, provided as bitfields</param>
        /// <param name="additionalCustomAttributes">The count of additional attributes to generate, these will be added to the attribute set blank after the standard attributes</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udAttributeSet_Create")]

        public static extern udError Create(ref udAttributeSet pAttributeSet, udStdAttributeContent content, UInt32 additionalCustomAttributes);
        
        /// <summary>
        ///  Free the memory created by a call to udAttributeSet_Create
        /// </summary>
        /// <param name="pAttributeSet">The attribute set to free the resources of</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udAttributeSet_Destroy")]
        public static extern udError Destroy(ref udAttributeSet pAttributeSet);
        
        /// <summary>
        /// Gets the offset for a standard attribute so that further querying of that attribute can be performed
        /// </summary>
        /// <param name="pAttributeSet">The attribute set to get the offset for</param>
        /// <param name="attribute">The enum value of the attribute</param>
        /// <param name="pOffset"></param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udAttributeSet_GetOffsetOfStandardAttribute")]
        public static extern udError GetOffsetOfStandardAttribute(ref udAttributeSet pAttributeSet, udStdAttribute attribute, IntPtr pOffset);
        
        /// <summary>
        /// Gets the offset for a named attribute so that further querying of that attribute can be performed
        /// </summary>
        /// <param name="pAttributeSet">The attribute set to get the offset for</param>
        /// <param name="pName">The name of the attribute</param>
        /// <param name="pOffset">This pointer will be written to with the value of the offset if it is found</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udAttributeSet_GetOffsetOfNamedAttribute")]
        public static extern udError GetOffsetOfNamedAttribute(ref udAttributeSet pAttributeSet, string pName, IntPtr pOffset);
        
        /// <summary>
        /// Gets the descriptor of a named attribute stored in a udAttributeSet
        /// </summary>
        /// <param name="pAttributeSet">The attribute set to get the descriptor from</param>
        /// <param name="pName">The name of the attribute</param>
        /// <param name="pDescriptor">This pointer to be written to with the value of the descriptor if it is found</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udAttributeSet_GetDescriptorOfNamedAttribute")]
        public static extern udError GetDescriptorOfNamedAttribute(udAttributeSet pAttributeSet, string pName, ref udAttributeDescriptor pDescriptor);
        
        /// <summary>
        /// Gets the descriptor of a standard attribute 
        /// </summary>
        /// <param name="attribute">The standard attribute to return the descriptor of</param>
        /// <param name="pDescriptor">This pointer to be written to with the value of the descriptor if it is found</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udAttribute_GetDescriptorOfStandardAttribute")]
        public static extern udError GetDescriptorOfStandardAttribute(udStdAttribute attribute, ref udAttributeDescriptor pDescriptor);
    }
}