using System;
using System.Runtime.InteropServices;

namespace udSDK
{
    public class udServerAPI
    {
        public IntPtr pQueryResult = IntPtr.Zero;

        ~udServerAPI()
        {
            ReleaseResult();
        }

        /// <summary>
        /// Queries provided API on the specified Euclideon udServer.
        /// </summary>
        /// <param name="context">The context to execute the query with.</param>
        /// <param name="APIAddress">The API address to query, this is the part of the address *after* `/api/`. The rest of the address is constructed from the context provided.</param>
        /// <param name="JSONToPost">The JSON text to POST to the API address.</param>
        /// <returns>A string in which the result data is to be stored.</returns>
        public string Query(udContext context, string APIAddress, string JSONToPost)
        {
            udError error = udServerAPI_Query(context.pContext, APIAddress, JSONToPost, ref pQueryResult);

            if (error != udSDK.udError.udE_Success)
                throw new Exception("Failed to query the server API with error:" + error.ToString());

            string result = Marshal.PtrToStringAnsi(pQueryResult);
            ReleaseResult();
            return result;
        }

        /// <summary>
        /// Destroys the query result when its no longer needed.
        /// </summary>
        public void ReleaseResult()
        {
            udServerAPI_ReleaseResult(ref pQueryResult);
        }

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udServerAPI_Query(IntPtr pContext, string pAPIAddress, string pJSON, ref IntPtr ppResult);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udServerAPI_ReleaseResult(ref IntPtr ppResult);
     }
}