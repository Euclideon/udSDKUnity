using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace udSDK
{
    [StructLayout(LayoutKind.Sequential)]
    public struct udVoxelID
    {
        public UInt64 index; 
        public IntPtr pTrav; 
        public IntPtr pRenderInfo; 
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct udPointCloudHeader
    {
        public double scaledRange; //!< The point cloud's range multiplied by the voxel size
        public double unitMeterScale; //!< The scale to apply to convert to/from metres (after scaledRange is applied to the unitCube)
        public uint totalLODLayers; //!< The total number of LOD layers in this octree
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

    public class udPointCloud
    {
        public IntPtr pModel = IntPtr.Zero;
        private udContext context;

        public void Load(udContext context, string modelLocation, ref udPointCloudHeader header)
        {
            if (context.pContext == IntPtr.Zero) 
                throw new Exception("Point cloud load failed: udContext is not initialised");

            udError error = udPointCloud_Load(context.pContext, ref pModel, modelLocation, ref header);
            if (error != udError.udE_Success)
                throw new Exception("udPointCloud.Load " +modelLocation + " failed: " + error.ToString());

            this.context = context;
        }

        public void Unload()
        {
            udError error = udPointCloud_Unload(ref pModel);
            if (error != udError.udE_Success)
                throw new Exception("udPointCloud.Unload failed.");
        }

        public void GetMetadata(ref string ppJSONMetadata)
        {
            udError error = udPointCloud_GetMetadata(pModel, ref ppJSONMetadata);
            if (error != udError.udE_Success)
                throw new Exception("udPointCloud.GetMetadata failed.");
        }

        public udError GetStreamingStatus()
        {
            return udPointCloud_GetStreamingStatus(this.pModel);
        }

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udPointCloud_Load(IntPtr pContext, ref IntPtr ppModel, string modelLocation, ref udPointCloudHeader header);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udPointCloud_Unload(ref IntPtr ppModel);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udPointCloud_GetMetadata(IntPtr pModel, ref string ppJSONMetadata);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udPointCloud_GetNodeColour(IntPtr pModel, udVoxelID voxelID, IntPtr pColour);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udPointCloud_GetAttributeAddress(IntPtr pModel, udVoxelID voxelID, uint attributeOffset, ref IntPtr ppAttributeAddress);
        
        [DllImport(UDSDKLibrary.name)]
        private static extern udError udPointCloud_GetStreamingStatus(IntPtr pModel);
    }
}