using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
namespace udSDK
{
    public static class UDUtilities
    {
        public static Matrix4x4 UDtoGL =
                        Matrix4x4.Scale(new Vector3(1, -1, 1)) *
                        Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0));

        /*
         * converts the z buffer value to a world space displacement
         */
        public static float zBufferToDepth(float z, float zNear, float zFar, bool ortho = true)
        {
            if (ortho)
                return (z * 0.5f + 0.5f) * (zFar - zNear) + zNear;
            else
                return (2 * zNear * zFar / (zNear - zFar)) / (z - (zFar + zNear) / (zFar - zNear));
        }
        /*
         *Converts matrix from Unity's left handed transformation convention ( y'=Ay) to
         * left handed system (y'=yA)
         */
        public static double[] GetUDMatrix(Matrix4x4 unityMat)
        {
            double[] udMat =
            {
                unityMat.m00,
                unityMat.m10,
                unityMat.m20,
                unityMat.m30,

                unityMat.m01,
                unityMat.m11,
                unityMat.m21,
                unityMat.m31,

                unityMat.m02,
                unityMat.m12,
                unityMat.m22,
                unityMat.m32,

                unityMat.m03,
                unityMat.m13,
                unityMat.m23,
                unityMat.m33
            };

         return udMat;
        }

        /*
         * attempts to load and returns all loaded UDS models in the scene
         */
        public static udRenderInstance[] getUDSInstances()
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("UDSModel");
            int count = 0;
            udRenderInstance[] modelArray = new udRenderInstance[objects.Length];
            for (int i = 0; i < objects.Length; ++i)
            {
                UDSModel model = (UDSModel) objects[i].GetComponent("UDSModel");

                if (!model.isLoaded)
                    model.LoadModel();

                if (model.isLoaded)
                {
                    modelArray[count].pointCloud = model.udModel.pModel;
                    Transform localTransform = objects[i].transform;

                    modelArray[count].matrix = UDUtilities.GetUDMatrix(
                            Matrix4x4.TRS(model.transform.position, model.transform.rotation, model.transform.localScale) *
                            model.modelToPivot
                        );
                    count++;
                }
            }
            return modelArray.Where(m => (m.pointCloud != System.IntPtr.Zero)).ToArray();
        }
    }

    /*
     *Class responsible for managing all threads related to VDK licensing
     */
    public class UDSessionThreadManager {
        bool logLicenseInformation = false;//this will print the license status every second to the log
        Thread licenseLogThread;
        List<Thread> activeThreads = new List<Thread>();
        public UDSessionThreadManager() {
            if (logLicenseInformation)
            {
                licenseLogThread = new Thread(new ThreadStart(LogLicenseStatus));
                licenseLogThread.Start();
                activeThreads.Add(licenseLogThread);
            }
        }

        /*
         *Logs the time until the license expires to the console every second
         */
        public void LogLicenseStatus() {
            while (true)
            {
                try
                {
                    UDSessionInfo info = GlobalUDContext.uContext.GetSessionInfo();

                    string message = string.Empty;
                    if (info.isOffline)
                        message += "Offline ";
                    else
                        message += "Online ";

                    message += "License Expiry: " + info.expiresTimestamp.ToString();

                    UnityEngine.Debug.Log(message);
                    Thread.Sleep(1000);
                }
                catch {
                    continue;
                }
            }
        }
        ~UDSessionThreadManager() {
            foreach (Thread thread in activeThreads)
                thread.Abort();
        }
    }
}


