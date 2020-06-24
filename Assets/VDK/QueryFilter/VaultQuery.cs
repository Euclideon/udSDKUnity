using System;
using System.Runtime.InteropServices;

namespace Vault {
    public class vdkQueryFilter
    {
        public IntPtr pQueryFilter = IntPtr.Zero;
        public vdkQueryFilter()
        {
            vdkError error = vdkQueryFilter_Create(ref pQueryFilter);
            if (error != vdkError.vE_Success)
                throw new Exception("Query Creation Failed: " + error.ToString());
        }

        ~vdkQueryFilter()
        {
            vdkError error = vdkQueryFilter_Destroy(ref pQueryFilter);
            if (error != vdkError.vE_Success)
                throw new Exception("Query Destruction Failed: " + error.ToString());
        }

        /*
         *Invert the result of a vdkQueryFilter
         * 
        Parameters

        inverted: True if the filter should be inverted, False is it should behave as default.

         */
        public void SetInverted(bool inverted)
        {
            
            vdkError error = vdkQueryFilter_SetInverted(pQueryFilter, inverted);
            if (error != vdkError.vE_Success)
                throw new Exception("Query Inversion Failed: " + error.ToString());
        }

        /*
         *Set the vdkQueryFilter to find voxels within a box.
         * 
        Note:

            When inverted, this filter will return all points outside the box.

        Parameters:

        pQueryFilter: The vdkQueryFilter to configure.

        centrePoint: The world space {X,Y,Z} array for the center point.

        halfSize: The local space {X,Y,Z} half size of the box (the world space axis are defined by yawPitchRoll).

        yawPitchRoll: The rotation of the model (in radians). Applied in YPR order.

         */
        public void SetAsBox(double[] centrePoint, double[] halfSize, double[] yawPitchRoll)
        {
            
            vdkError error = vdkQueryFilter_SetAsBox(pQueryFilter, centrePoint, halfSize, yawPitchRoll);
            if (error != vdkError.vE_Success)
                throw new Exception("Query SetAsBox Failed: " + error.ToString());
        }

        /*
         *Set the vdkQueryFilter to find voxels within a cylinder.
         * 
        Note

            When inverted, this filter will return all points outside the cylinder.
        Parameters

        pQueryFilter: The vdkQueryFilter to configure.

        centrePoint: The world space {X,Y,Z} array for the center point of the cylinder.

        radius: The radius of the cylinder (the world space axis are defined by yawPitchRoll) the circle exists in local axis XY extruded along Z.

        halfHeight: The half-height of the cylinder (the world space axis are defined by yawPitchRoll) the circle exists in local axis XY extruded along Z.

        yawPitchRoll: The rotation of the cylinder (in radians). Applied in YPR order.

         */
        public void SetAsCylinder(double[] centrePoint, double radius, double halfHeight, double[] yawPitchRoll)
        {
            
            vdkError error = vdkQueryFilter_SetAsCylinder(pQueryFilter, centrePoint, radius, halfHeight, yawPitchRoll);
            if (error != vdkError.vE_Success)
                throw new Exception("Query SetAsCylinder Failed: " + error.ToString());
        }

        /*
         *Set the vdkQueryFilter to find voxels within a sphere.
         *
         * 
        Note:

            When inverted, this filter will return all points outside the sphere.

        Parameters:

        pQueryFilter: The vdkQueryFilter to configure.

        centrePoint: The world space {X,Y,Z} array for the center point.

        radius: The radius from the centerPoint to the edge of the sphere.

         */
        public void SetAsSphere(double[] centrePoint, double radius)
        {
            
            vdkError error = vdkQueryFilter_SetAsSphere(pQueryFilter, centrePoint, radius);
            if (error != vdkError.vE_Success)
                throw new Exception("Query SetAsSphere Failed: " + error.ToString());
        }
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkQueryFilter_Create(ref IntPtr ppQueryFilter);
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkQueryFilter_Destroy(ref IntPtr ppQueryFilter);
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkQueryFilter_SetInverted(IntPtr pQueryFilter, bool inverted);
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkQueryFilter_SetAsBox(IntPtr pQueryFilter, double[] centrePoint, double[] halfSize, double[] yawPitchRoll);
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkQueryFilter_SetAsCylinder(IntPtr pQueryFilter, double[] centrePoint, double radius, double halfHeight, double[] yawPitchRoll);
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkQueryFilter_SetAsSphere(IntPtr pQueryFilter, double[] centrePoint, double radius);
    }
}
