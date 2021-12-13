using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace udSDK 
{
    public class udQueryFilter
    {
        public IntPtr pQueryFilter = IntPtr.Zero;

        public udQueryFilter()
        {
            udError error = udQueryContext_Create(ref pQueryFilter);
            if (error != udError.udE_Success)
                throw new Exception("Query Creation Failed: " + error.ToString());
        }

        ~udQueryFilter()
        {
            udError error = udQueryContext_Destroy(ref pQueryFilter);
            if (error != udError.udE_Success)
                throw new Exception("Query Destruction Failed: " + error.ToString());
        }

        /*
         *Invert the result of a udQueryFilter
         * 
        Parameters

        inverted: True if the filter should be inverted, False is it should behave as default.

         */
        public void SetInverted(bool inverted)
        {
            udError error = udQueryContext_SetInverted(pQueryFilter, inverted);
            if (error != udError.udE_Success)
                throw new Exception("Query Inversion Failed: " + error.ToString());
        }

        /*
         *Set the udQueryFilter to find voxels within a box.
         * 
        Note:

            When inverted, this filter will return all points outside the box.

        Parameters:

        pQueryFilter: The udQueryFilter to configure.

        centrePoint: The world space {X,Y,Z} array for the center point.

        halfSize: The local space {X,Y,Z} half size of the box (the world space axis are defined by yawPitchRoll).

        yawPitchRoll: The rotation of the model (in radians). Applied in YPR order.

         */
        public void SetAsBox(double[] centrePoint, double[] halfSize, double[] yawPitchRoll)
        {
            udError error = udQueryContext_SetAsBox(pQueryFilter, centrePoint, halfSize, yawPitchRoll);
            if (error != udError.udE_Success)
                throw new Exception("Query SetAsBox Failed: " + error.ToString());
        }

        /*
         *Set the udQueryFilter to find voxels within a cylinder.
         * 
        Note

            When inverted, this filter will return all points outside the cylinder.
        Parameters

        pQueryFilter: The udQueryFilter to configure.

        centrePoint: The world space {X,Y,Z} array for the center point of the cylinder.

        radius: The radius of the cylinder (the world space axis are defined by yawPitchRoll) the circle exists in local axis XY extruded along Z.

        halfHeight: The half-height of the cylinder (the world space axis are defined by yawPitchRoll) the circle exists in local axis XY extruded along Z.

        yawPitchRoll: The rotation of the cylinder (in radians). Applied in YPR order.

         */
        public void SetAsCylinder(double[] centrePoint, double radius, double halfHeight, double[] yawPitchRoll)
        {
            udError error = udQueryContext_SetAsCylinder(pQueryFilter, centrePoint, radius, halfHeight, yawPitchRoll);
            if (error != udError.udE_Success)
                throw new Exception("Query SetAsCylinder Failed: " + error.ToString());
        }

        /*
         *Set the udQueryFilter to find voxels within a sphere.
         *
         * 
        Note:

            When inverted, this filter will return all points outside the sphere.

        Parameters:

        pQueryFilter: The udQueryFilter to configure.

        centrePoint: The world space {X,Y,Z} array for the center point.

        radius: The radius from the centerPoint to the edge of the sphere.

         */
        public void SetAsSphere(double[] centrePoint, double radius)
        {
            udError error = udQueryContext_SetAsSphere(pQueryFilter, centrePoint, radius);
            if (error != udError.udE_Success)
                throw new Exception("Query SetAsSphere Failed: " + error.ToString());
        }

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udQueryContext_Create(ref IntPtr ppQueryFilter);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udQueryContext_Destroy(ref IntPtr ppQueryFilter);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udQueryContext_SetInverted(IntPtr pQueryFilter, bool inverted);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udQueryContext_SetAsBox(IntPtr pQueryFilter, double[] centrePoint, double[] halfSize, double[] yawPitchRoll);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udQueryContext_SetAsCylinder(IntPtr pQueryFilter, double[] centrePoint, double radius, double halfHeight, double[] yawPitchRoll);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udQueryContext_SetAsSphere(IntPtr pQueryFilter, double[] centrePoint, double radius);
    }
}
