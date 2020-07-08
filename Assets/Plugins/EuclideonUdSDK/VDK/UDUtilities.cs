using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
namespace Vault
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
        public static vdkRenderInstance[] getUDSInstances()
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("UDSModel");
            int count = 0;
            vdkRenderInstance[] modelArray = new vdkRenderInstance[objects.Length];
            for (int i = 0; i < objects.Length; ++i)
            {
                UDSModel model = (UDSModel) objects[i].GetComponent("UDSModel");

                if (!model.isLoaded)
                    model.LoadModel();

                if (model.isLoaded)
                {
                    modelArray[count].pointCloud = model.udModel.pModel;
                    Transform localTransform = objects[i].transform;

                    modelArray[count].worldMatrix = UDUtilities.GetUDMatrix(
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
    public class VDKSessionThreadManager {
        bool logLicenseInformation = false;//this will print the license status every second to the log
        bool keepAlive = false;//Only necessary prior to vdk0.6 as this functionaity has been moved to library
        Thread keepAliveThread;
        Thread licenseLogThread;
        List<Thread> activeThreads = new List<Thread>();
        public VDKSessionThreadManager() {
            if(keepAlive)
            {
              keepAliveThread = new Thread(new ThreadStart(KeepAlive));
              keepAliveThread.Start();
              activeThreads.Add(keepAliveThread);
            }
            if (logLicenseInformation)
            {
                licenseLogThread = new Thread(new ThreadStart(LogLicenseStatus));
                licenseLogThread.Start();
                activeThreads.Add(licenseLogThread);
            }
        }

        /*
         * Polls the license server once every 30 seconds to keep the session alive
         */
        public void KeepAlive() {
            while (true)
            {
                try
                {
                    GlobalVDKContext.vContext.KeepAlive();
                }
                catch(System.Exception e)
                {
                    Debug.Log("keepalive failed: " + e.ToString());
                }
                Thread.Sleep(30000);
            }
        }

        /*
         *Logs the time until the render licens expires to the console every second
         */
        public void LogLicenseStatus() {
            while (true)
            {
                try
                {
                    vdkLicenseInfo info = new vdkLicenseInfo();
                    GlobalVDKContext.vContext.GetLicenseInfo(LicenseType.Render, ref info);
                    System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                    ulong cur_time = (ulong)(System.DateTime.UtcNow - epochStart).TotalSeconds;
                    UnityEngine.Debug.Log("Render License Expiry: " + (info.expiresTimestamp - cur_time).ToString());
                    Thread.Sleep(1000);
                }
                catch {
                    continue;
                }
            }
        }
        ~VDKSessionThreadManager() {
            foreach (Thread thread in activeThreads)
                thread.Abort();
        }
    }
}


