using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace udSDK
{
    public class udConvertContext
    {
        public IntPtr pConvertContext;

        public void Create(udContext context)
        {
            udError error = udConvert_CreateContext(context.pContext, ref pConvertContext);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udConvertContext.Create failed.");
        }

        public void Destroy()
        {
            udError error = udConvert_DestroyContext(ref pConvertContext);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udConvertContext.Destroy failed.");
        }

        // Adds the given filename as an input item for the convert 
        public void AddItem(string fileName)
        {
            udError error = udConvert_AddItem(pConvertContext, fileName);
            Debug.Log(error);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udConvertContext.AddItem failed.");
        }
        
        // Adds the given filename as the ouput path
        public void SetOutputFilename(string fileName)
        {
            udError error = udConvert_SetOutputFilename(pConvertContext, fileName);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udConvertContext.SetOutputFilename failed.");
        }
        
        public void SetPointResolution(bool overrideDefaultResolution, float pointResolutionMeters)
        {
            udError error = udConvert_SetPointResolution(pConvertContext, overrideDefaultResolution, pointResolutionMeters);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udConvertContext.SetPointResolution failed.");
        }

        public void DoConvert()
        {
            udError error = udConvert_DoConvert(pConvertContext);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udConvertContext.DoConvert failed.");
        }

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udConvert_CreateContext(IntPtr pContext, ref IntPtr ppConvertContext);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udConvert_DestroyContext(ref IntPtr ppConvertContext);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udConvert_AddItem(IntPtr pConvertContext, string fileName);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udConvert_SetOutputFilename(IntPtr pConvertContext, string fileName);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udConvert_DoConvert(IntPtr pConvertContext);
        
        [DllImport(UDSDKLibrary.name)]
        private static extern udError udConvert_SetPointResolution(IntPtr pConvertContext, bool overrideDefaultResolution, float pointResolutionMeters);
    }
}