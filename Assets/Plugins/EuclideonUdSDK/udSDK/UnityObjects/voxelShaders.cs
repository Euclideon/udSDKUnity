using System;
using System.Runtime.InteropServices;
using UnityEngine;

// This is a demonstration of creating a voxelshader callback in c# that is passed to the sdk.
// This is not in the master branch, because: 
//     It is not especially performant.
//     It will freeze the Unity editor if used during "edit" mode;
//         due to the way Unity terminates threads when "play" mode starts. 

// To make it work, you need to get the pointer from the below class, and pass that into your udRenderInstance

namespace udSDK
{
    public struct VoxelUserData_Intesity
    {
        public uint attributeOffset;
        public uint min;
        public uint max;
    }

    public class VoxelShader
    {
        public delegate uint voxelShaderDelegate(IntPtr pPointCloud, IntPtr pVoxelID, IntPtr pVoxelUserData);

        public static uint BlankShader(IntPtr pPointCloud, IntPtr pVoxelID, IntPtr pVoxelUserData)
        {
            return 0xFFFF00FF;
        }

        public static uint IntensityShader(IntPtr pPointCloud, IntPtr pVoxelID, IntPtr pVoxelUserData)
        {
            uint value;
            UInt64 color64 = 0;
            udError error;
            VoxelUserData_Intesity userData = (VoxelUserData_Intesity)Marshal.PtrToStructure(pVoxelUserData, typeof(VoxelUserData_Intesity));
            
            uint offset = userData.attributeOffset;
            
            uint address = 0;
            IntPtr ppAddress = IntPtr.Zero;
            
            error = udPointCloud_GetAttributeAddress(pPointCloud, pVoxelID, offset, ref ppAddress);
            
            if (error != udError.udE_Success)
            {
                Console.WriteLine("Get address failed : " + error.ToString());
            }
            
            unsafe
            {
                address = *((ushort*)ppAddress.ToPointer());
            }
            
            // prevent divide by zero crash 
            if (userData.max == 0)
                return 0xFF000000;
            
            // this step needs to happen as a float to capture the level of precision that we need for details 
            float fvalue = Pow((float)(address - userData.min) / (float)userData.max) * 255;
            
            // this clamps our channel into an acceptable range 
            uint channel = Clamp((uint)fvalue, 0, 255);
            
            // this turns one channel into 3
            value = (channel * 0x00010101);
            
            return 0xFF000000 | value; 
        }

        static uint Clamp(uint value, uint min, uint max)
        {
            if (value <= min)
                return min;
            if (value >= max)
                return max;
            return value; 
        }

        public static float Pow(float a)
        {
            return a * a;
        }
        
        public static IntPtr GetDelegatePointer(voxelShaderDelegate targetDelegate)
        {
            return Marshal.GetFunctionPointerForDelegate(targetDelegate);
        }

        // for convenience sake in this demonstration, I have just reimplemented udAttributeSet and udPointCloud here
        
        [DllImport(UDSDKLibrary.name)]
        private static extern udError udAttributeSet_GetOffsetOfStandardAttribute(ref udAttributeSet pAttributeSet, udStdAttribute attribute, ref uint pOffset);
        
        [DllImport(UDSDKLibrary.name)]
        private static extern udError udAttributeSet_GetOffsetOfNamedAttribute(ref udAttributeSet pAttributeSet, string pName, ref IntPtr pOffset);
        
        [DllImport(UDSDKLibrary.name)]
        private static extern udError udPointCloud_GetAttributeAddress(IntPtr pModel, IntPtr voxelID, uint attributeOffset, ref IntPtr ppAttributeAddress);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udPointCloud_GetNodeColour64(IntPtr pModel, IntPtr pVoxelID, ref System.UInt64 pColour);
    }
}