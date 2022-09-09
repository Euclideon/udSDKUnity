using System;
using System.Runtime.InteropServices;

namespace udSDK
{
  // Note: these are raised to the top of the namespace to allow "using" keyword in place of typedef
  using udGeometryDouble2 = udMathDouble2;
  using udGeometryDouble3 = udMathDouble3;
  using udGeometryDouble4 = udMathDouble4;
  using udGeometryDouble4x4 = udMathDouble4x4;
  
  // Note: not in the header, provided as interface to get around lack of union function in C#
  /// <summary>
  /// Identifies data which can be used by udGeometry.
  /// </summary>
  public interface udGeometryData {}
  
  /// <summary>
  /// The currently supported geometry types
  /// </summary>
  public enum udGeometryType
  {
    udGT_Inverse, //!< An inversion filter; flips the udGeometryTestResult of the child udGeometry node
    udGT_CircleXY, //!< A 2D Circle with an infinite Z value
    udGT_RectangleXY, //!< A 2D Rectangle with an infinite Z value
    udGT_PolygonXY, //!< A 2D Polygon with rotation (quaternion) to define the up of the polygon
    udGT_PolygonPerspective, //!< A 2D polygon with a perspective projection to the screen plane
    udGT_Cylinder, //!< @deprecated A radius out of a line which caps immediately at the end of the line
    udGT_Capsule, //!< A line with a radius from the line; forming hemispherical caps at the end of the line
    udGT_Sphere, //!< A radius from a point
    udGT_HalfSpace, //!< A binary space partition allowing 1 side of a plane
    udGT_AABB, //!< An axis aligned box; Use with caution. OBB while less performant correctly handles transforms
    udGT_OBB, //!< An oriented bounding box using half size and orientation
    udGT_CSG, //!< A constructed solid geometry that uses a udGeometryCSGOperation to join to child udGeometry nodes

    udGT_Count //!< Count helper value to iterate this enum
  };
  
  // note: udGeometryDouble* definitions moved to top of namespace to allow "using" keyword in place of typedef
  
  /// <summary>
  /// The geometric representation of a Node in a Unlimited Detail Model.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometryVoxelNode
  {
    public udGeometryDouble3 minPos; //!< The Bottom, Left, Front corner of the voxel (closest to the origin)
    public double childSize; //!< The half size of the voxel (which is the same size as this voxels children)
  };
  
  /// <summary>
  /// The geometric representation of a Circle.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometryCircleXY : udGeometryData
  {
    public udGeometryDouble2 centre; //!< The centre of the circle
    public double radius; //!< The radius of the circle
  };

  /// <summary>
  /// The geometric representation of a Rectangle.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometryRectangleXY : udGeometryData
  {
    public udGeometryDouble2 minPoint; //!< The lowest point of the rectangle
    public udGeometryDouble2 maxPoint; //!<The highest point of the rectangle
  };

  /// <summary>
  /// The geometric representation of a Polygon.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometryPolygonXYZ : udGeometryData
  {
    public UInt32 pointCount; //!< THe number of points defining the polygon
    public udGeometryDouble3[] pointsList; //!< The list of points defining the polygon
    public udGeometryDouble4 rotationQuat; //!< The rotation of the polygon
  };

  /// <summary>
  /// The geometric representation of a Polygon with a perspective projection.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometryPolygonPerspective : udGeometryData
  {
    public UInt32 pointCount; //!< The number of points defining the polygon
    public udGeometryDouble3[] pointsList; //!< The list of points defining the polygon
    public udGeometryDouble4 rotationQuat; //!< The rotation of the polygon
    public udGeometryDouble4x4 worldToScreen; //!< The matrix to project from World space to Screen space
    public udGeometryDouble4x4 projectionMatrix; //!< The matrix to project the points of the polygon
    public udGeometryDouble4x4 cameraMatrix; //!< The camera matrix
    public udGeometryDouble3 normRight; //!< The normal on the right of the plane
    public udGeometryDouble3 normLeft; //!< The normal on the left of the plane
    public udGeometryDouble3 normTop; //!< The normal on the top of the plane
    public udGeometryDouble3 normBottom; //!< The normal on the bottom of the plane
    public double nearPlane; //!< the near plane distance
    public double farPlane; //!< The far plane distance
  };

  /// <summary>
  /// The geometric representation of a cylinder. @deprecated This object has edge cases that do not resolve correctly and is highly non-performant
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometryCylinder : udGeometryData
  {
    public udGeometryDouble3 point1; //!< The point at one end of the line
    public udGeometryDouble3 point2; //!< The point at the other end of the line
    public double radius; //!< The radius around the line

    // Derived values
    public udGeometryDouble3 axisVector; //!< The vector of the line
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public udGeometryDouble4[] planes; //!< The two planes for the caps
  };

  /// <summary>
  /// Stores the properties of a geometric capsule 
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometryCapsule : udGeometryData
  {
    public udGeometryDouble3 point1; //!< One end of the line
    public udGeometryDouble3 point2; //!< The other end of the line
    public double radius; //!< The radius around the line

    // Derived values
    public udGeometryDouble3 axisVector; //!< The vector of the line
    public double length; //!< The length of the line
  };

  /// <summary>
  /// Stores the properties of a geometric sphere
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometrySphere : udGeometryData
  {
    public udGeometryDouble3 center; //!< The center of the sphere
    public double radius; //!< The radius of the sphere
  };

  /// <summary>
  /// Stores the properties of a geometric half space
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometryHalfSpace : udGeometryData
  {
    public udGeometryDouble4 plane; //!< The parameters to define the plane (normal XYZ and offset from origin)
  };

  /// <summary>
  /// Stores the properties of a geometric axis aligned bounding box
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometryAABB : udGeometryData
  {
    public udGeometryDouble3 center; //!< The point at the center of the AABB
    public udGeometryDouble3 extents; //!< The half space size of the AABB
  };

  /// <summary>
  /// Stores the properties of a geometric axis aligned bounding box (extending AABB)
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometryOBB : udGeometryData
  {
    public udGeometryDouble3 center; //!< The point at the center of the AABB
    public udGeometryDouble3 extents; //!< The half space size of the AABB
    public udGeometryDouble4x4 rotationMatrix; //!< The transform that represents the rotation
  };

  /// <summary>
  /// The Constructive Solid Geometry operations
  /// </summary>
  public enum udGeometryCSGOperation
  {
    udGCSGO_Union = 0,   //!< A union CSG operation; any point matching the one or the other geometry (OR operation)
    udGCSGO_Difference,  //!< A subtractive CSG operation; any point in the first geometry but not matching the second geometry (XOR operation)
    udGCSGO_Intersection //!< An intersection CSG operation; any point that matches both geometries (AND operation)
  };
  
  /// <summary>
  /// Stores the properties of an inversed udGeometry node
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometryInverse : udGeometryData
  {
    public udGeometry pInverseOf; //!< The inverse geometry
    public int owns; //!< If non-zero pInverseOf is owned by this need (and will need to be cleaned up)
  };

  /// <summary>
  /// Stores the properties of a CSG udGeometry node
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometryCSG : udGeometryData
  {
    public udGeometry pFirst; //!< The first geometry
    public udGeometry pSecond; //!< The second geometry
    public udGeometryCSGOperation operation; //!< The operation applied to the 2 gemetries
    public int owns; //!< non zero if it owns both children
  };
  
  /// <summary>
  /// The results of a geometry test
  /// </summary>
  public enum udGeometryTestResult
  {
    udGTR_CompletelyOutside = 0, //!< The node is totally outside of the geometry (and no further tests are required)
    udGTR_CompletelyInside  = 1, //!< The node is totally inside of the geometry (and no further tests are required)
    udGTR_PartiallyOverlap  = 2 //!< The node is overlapping the boundary of the geoetry and further tests will be required to place the voxel inside or outside 
  };
  
  /// <summary>
  /// The function that will be called for each node until the node is either inside or outside of the geometry
  /// </summary>
  public delegate udError udGeometryOverlapFunc(udGeometry pGeometry, udGeometryVoxelNode pVoxel);
    
  /// <summary>
  /// This sets up pFilterOut by doing the pMatrix transform on pFilterIn (which isn't modified)- pFilterOut needs to be deinited after this
  /// </summary>
  public delegate udError udGeometryTransform(udGeometry pFilterIn, udGeometry pFilterOut, udGeometryDouble4x4 pMatrix);
  
  /// <summary>
  /// This sets up pFilterOut by doing the pMatrix transform on pFilterIn (which isn't modified)- pFilterOut needs to be deinited after this
  /// </summary>
  public delegate void udGeometryDeinit(ref udGeometry pGeometry);
  
  /// <summary>
  /// Stores the information required for all udGeometry shapes. @warning This struct will change drastically as udSDK shifts to programable query filters
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udGeometry
  {
    public udGeometryOverlapFunc pTestFunc; //!< The function to call to test the geometry
    public udGeometryTransform pTransformFunc; //!< The function to transform this geometry using a linear matrix
    public udGeometryDeinit pDeinitFunc; //!< The function that is called when this is cleaned up

    public udGeometryData data; //!< The geometry used

    public udGeometryType type; //!< The type of the geometry for internal verification
  };

  public static partial class udGeometry_f
  {
    /// <summary>
    /// Helper to initialise a geometry that simply inverts another primitive (CompletelyOutside <-> CompletelyInside).
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="pSource">The Geometry to inverse.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitInverse")]
    public static extern void InitInverse(udGeometry pGeometry, udGeometry pSource);
    
    /// <summary>
    /// Helper to initialise a 2D circle extended to infinity in Z (elevation).
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="centre">THe centre fo the Circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitCircleXY")]
    public static extern void InitCircleXY(udGeometry pGeometry, udGeometryDouble2 centre, double radius);

    /// <summary>
    /// Helper to initialise a 2D rectangle extended to infinity in Z (elevation).
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="point1">The first point to define a rectangle.</param>
    /// <param name="point2">The second point to define a rectangle.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitRectangleXY")]
    public static extern void InitRectangleXY(udGeometry pGeometry, udGeometryDouble2 point1, udGeometryDouble2 point2);

    /// <summary>
    /// Helper to initialise a Polygon shape extened to infinity along a direction.
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="pXYCoords">The list of 2D positions defining the polygon.</param>
    /// <param name="count">The number of points in pXYCoords list.</param>
    /// <param name="rotationQuat">The rotation quaternion between Up and the direction of the polygon.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitPolygonXY")]
    public static extern void InitPolygonXY(udGeometry pGeometry, udGeometryDouble3 pXYCoords, UInt32 count, udGeometryDouble4 rotationQuat);

    /// <summary>
    /// Helper to initialise a Polygon shape extended to infinity with a perspective projection.
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="pXYCoords">The list of 2D positions defining the polygon.</param>
    /// <param name="count">The number of points in pXYCoords list.</param>
    /// <param name="projectionMatrix">The projection matrix of model to world.</param>
    /// <param name="cameraMatrix">The projection matrix of world to screen.</param>
    /// <param name="nearPlaneOffset">The offset off the near plane to detect if voxel is visible by the camera.</param>
    /// <param name="farPlaneOffset">The offset off the far plane to detect if voxel is visible by the camera.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitPolygonPerspective")]
    public static extern void InitPolygonPerspective(udGeometry pGeometry, udGeometryDouble2 pXYCoords, UInt32 count, udGeometryDouble4x4 projectionMatrix, udGeometryDouble4x4 cameraMatrix, double nearPlaneOffset, double farPlaneOffset);
    
    /// <summary>
    /// Helper to initialise a CSG shape.
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="pGeometry1">First Geometry.</param>
    /// <param name="pGeometry2">Second Geometry.</param>
    /// <param name="function">The function to apply to these 2 geometries (Union, Difference, Intersection).</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitCSG")]
    public static extern void InitCSG(udGeometry pGeometry, udGeometry pGeometry1, udGeometry pGeometry2, udGeometryCSGOperation function);
    
    /// <summary>
    /// Helper to initialise a plane extended to infinity in the direction of its normal.
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="point">The first point to define the space.</param>
    /// <param name="normal">The normal vector of the defined space.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitHalfSpace")]
    public static extern void InitHalfSpace(udGeometry pGeometry, udGeometryDouble3 point, udGeometryDouble3 normal);
    
    /// <summary>
    /// Helper to initialise a Cylinder.
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="point1">The First point to define a cylinder.</param>
    /// <param name="point2">The Second point to define a cylinder.</param>
    /// <param name="radius">The radius of the cylinder.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitCylinderFromEndPoints")]
    public static extern void InitCylinderFromEndPoints(udGeometry pGeometry, udGeometryDouble3 point1, udGeometryDouble3 point2, double radius);
    
    /// <summary>
    /// Helper to initialise a Cylinder from.
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="centre">The centre of the Cylinder.</param>
    /// <param name="radius">The radius of the Cylinder.</param>
    /// <param name="halfHeight">Half the height of the Cylinder.</param>
    /// <param name="yawPitchRoll">The Rotation if the Cylinder.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitCylinderFromCenterAndHeight")]
    public static extern void InitCylinderFromCenterAndHeight(udGeometry pGeometry, udGeometryDouble3 centre, double radius, double halfHeight, udGeometryDouble3 yawPitchRoll);
    
    /// <summary>
    /// Helper to initialise a Capsule.
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="point1">First point to define a capsule at this position.</param>
    /// <param name="point2">Second point to define a capsule.</param>
    /// <param name="radius">Radius of the capsule.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitCapsule")]
    public static extern void InitCapsule(udGeometry pGeometry, udGeometryDouble3 point1, udGeometryDouble3 point2, double radius);
    
    /// <summary>
    /// Helper to initialise a Sphere.
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="center">The center of the Sphere.</param>
    /// <param name="radius">The radius of the sphere.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitSphere")]
    public static extern void InitSphere(udGeometry pGeometry, udGeometryDouble3 center, double radius);
    
    /// <summary>
    /// Helper to initialise an Axis Aligned Box using the min and max point.
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="point1">First point to define a box starting at this position.</param>
    /// <param name="point2">Last point to define a box ending at this position.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitAABBFromMinMax")]
    public static extern void InitAABBFromMinMax(udGeometry pGeometry, udGeometryDouble3 point1, udGeometryDouble3 point2);
    
    /// <summary>
    /// Helper to initialise an Axis Aligned Box using center and extents.
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="centre">The centre of the Axis Aligned Box.</param>
    /// <param name="extents">The dimension of the Axis Aligned Box.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitAABBFromCentreExtents")]
    public static extern void InitAABBFromCentreExtents(udGeometry pGeometry, udGeometryDouble3 centre, udGeometryDouble3 extents);

    /// <summary>
    /// Helper to initialise an arbitrarily aligned three dimensional box, centered at centerPoint, rotated about the center.
    /// </summary>
    /// <param name="pGeometry">The preallocated udGeometry to init.</param>
    /// <param name="centerPoint">The centre of the box.</param>
    /// <param name="extents">The distances from the center to the sides (half the size of the dimensions of the box).</param>
    /// <param name="rotations">The rotations of the box in radians about x,y,z.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_InitOBB")]
    public static extern void InitOBB(udGeometry pGeometry, udGeometryDouble3 centerPoint, udGeometryDouble3 extents, udGeometryDouble3 rotations);
    
    /// <summary>
    /// This cleans up internal allocations
    /// </summary>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_Deinit")]
    public static extern void Deinit(udGeometry pGeometry);

    /// <summary>
    /// Helper to create a pointer to an allocated udGeometry struct. This is a conveneince for wrapping libraries that do not need or have a concept of the underlying object.
    /// It is NOT recommended to use this function in applications where creating a udGeometry struct directly is possible (either using an allocation or on stack).
    /// </summary>
    /// <param name="ppGeometry">Pointer to memory location with which to return the allocated struct.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_Create")]
    public static extern void Create(ref udGeometry ppGeometry);
    
    /// <summary>
    /// Free the struct allocated by udGeometry_Create
    /// udGeometry_DeInit should be called on the struct first before calling this function.
    /// Calling this function on a udGeometry struct that was not allocated using udGeometry_Create may result in a crash
    /// </summary>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeometry_Destroy")]
    public static extern void Destroy(ref udGeometry ppGeometry);
  }
}