using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vault;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using System;
using Microsoft.Win32;

public class udProjectNodeUnity : MonoBehaviour
{
  public udProjectNodeType itemType;
  public string itemTypeString;
  public UDProjectNode projectNode;
  public udProjectGeometryType geometryType;
  public string URI;
  GameObject firstChild;
  GameObject nextSibling;
  public void LoadTree(IntPtr pNode)
  {

    this.projectNode = new UDProjectNode(pNode);
    if (projectNode.nodeData.pName != IntPtr.Zero)
      this.name = Marshal.PtrToStringAnsi(projectNode.nodeData.pName);

    if (projectNode.nodeData.pURI != IntPtr.Zero)
      this.URI = Marshal.PtrToStringAnsi(projectNode.nodeData.pURI);


    this.itemType = projectNode.nodeData.itemtype;
    this.geometryType = projectNode.nodeData.geomtype;
    switch (itemType)
    {
      case udProjectNodeType.udPNT_Custom://!<Need to check the itemtypeStr string manually.
        itemTypeString = new string(projectNode.nodeData.itemtypeStr);
        switch (itemTypeString) 
        {
          //these are the custom types currently supported by Vault Client:
          case "I3S":
            break;
          case ("Water"):
            break;
          case "ViewMap":
            break;
          case "Polygon":
            break;
          case "QFilter":
            break;
          case "Places":
            break;
          case "MHeight":
            break;
        }
        break;

      case udProjectNodeType.udPNT_PointCloud://!<A Euclideon Unlimited Detail Point Cloud file (“UDS”)
        UDSModel model = gameObject.AddComponent<UDSModel>();
        gameObject.tag = "UDSModel";
        model.path = this.URI;
        break;

      case udProjectNodeType.udPNT_PointOfInterest:
        var mf = gameObject.AddComponent<MeshFilter>();
        break;
      case udProjectNodeType.udPNT_Folder: //!<A folder of other nodes (“Folder”)
        break;
      case udProjectNodeType.udPNT_LiveFeed: //!<A Euclideon Vault live feed container (“IOT”)
        break;
      case udProjectNodeType.udPNT_Media: //!<An Image, Movie, Audio file or other media object (“Media”)
        break;
      case udProjectNodeType.udPNT_Viewpoint:
        break;
      case udProjectNodeType.udPNT_VisualisationSettings: //!<Visualisation settings (itensity, map height etc) (“VizSet”)
        break;
    }

    switch (geometryType)
    {
      case(udProjectGeometryType.udPGT_None): //!<There is no geometry associated with this node.
        break;

      case(udProjectGeometryType.udPGT_Point): //!<pCoordinates is a single 3D position
        {
          double[] position = new double[3];
          Marshal.Copy(projectNode.nodeData.pCoordinates, position, 0, 3);
          transform.position = new Vector3((float)position[0], (float)position[1], (float)position[2]);
          break;
        }

      case(udProjectGeometryType.udPGT_MultiPoint): //!<Array of udPGT_Point, pCoordinates is an array of 3D positions.
        if (!(projectNode.nodeData.geomCount==0)) 
        {
          //create a child object for each geometry object
          double[] positions = new double[projectNode.nodeData.geomCount * 3];
          Marshal.Copy(projectNode.nodeData.pCoordinates, positions, 0, projectNode.nodeData.geomCount * 3);
          for(int i = 0; i < projectNode.nodeData.geomCount; i++)
          {
            GameObject pointGO = new GameObject();
            double[] position = new double[3];
            position[0] = positions[3 * i];
            position[1] = positions[3 * i + 1];
            position[2] = positions[3 * i + 2];
            pointGO.transform.parent = transform;
            pointGO.transform.position = new Vector3((float)position[0], (float)position[1], (float)position[2]);
            pointGO.name = "Point " + i.ToString(); 
          }
        }
        break;

      case(udProjectGeometryType.udPGT_LineString): //!<pCoordinates is an array of 3D positions forming an open line
        LineRenderer lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = projectNode.nodeData.geomCount;
        Vector3[] verts = new Vector3[projectNode.nodeData.geomCount];
        if (!(projectNode.nodeData.geomCount==0)) 
        {
          //create a child object for each geometry object
          double[] positions = new double[projectNode.nodeData.geomCount * 3];
          Marshal.Copy(projectNode.nodeData.pCoordinates, positions, 0, projectNode.nodeData.geomCount * 3);
          for(int i = 0; i < projectNode.nodeData.geomCount; i++)
          {
            GameObject pointGO = new GameObject();
            double[] position = new double[3];
            position[0] = positions[3 * i];
            position[1] = positions[3 * i + 1];
            position[2] = positions[3 * i + 2];
            pointGO.transform.parent = transform;
            Vector3 posVec =new Vector3((float)position[0], (float)position[1], (float)position[2]);
            pointGO.transform.position = posVec;
            verts[i] = posVec;
            pointGO.name = "Point " + i.ToString(); 
          }
        }
        lr.SetPositions(verts);
        break;

      case(udProjectGeometryType.udPGT_MultiLineString): //!<Array of udPGT_LineString; pCoordinates is NULL and children will be present.
        break;

      case(udProjectGeometryType.udPGT_Polygon): //!<pCoordinates will be a closed linear ring (the outside), there MAY be children that are interior as pChildren udPGT_MultiLineString items, these should be counted as islands of the external ring.
        break;

      case(udProjectGeometryType.udPGT_MultiPolygon): //!<pCoordinates is null, children will be udPGT_Polygon (which still may have internal islands)
        break;

      case(udProjectGeometryType.udPGT_GeometryCollection): //!<Array of geometries; pCoordinates is NULL and children may be present of any type.
        break;
    }


    
    //create sibling
  if(projectNode.nodeData.pNextSibling != System.IntPtr.Zero)
    {
      nextSibling = new GameObject();
      nextSibling.transform.parent = transform.parent;
      var nextSiblingData = nextSibling.AddComponent<udProjectNodeUnity>();
      nextSiblingData.LoadTree(projectNode.nodeData.pNextSibling);
    }

    //create a child (if exists)
  if(projectNode.nodeData.pFirstChild != System.IntPtr.Zero)
    {
      firstChild = new GameObject();
      firstChild.transform.parent = transform;
      var firstChildData = firstChild.AddComponent<udProjectNodeUnity>();
      firstChildData.LoadTree(projectNode.nodeData.pFirstChild);
    }
    
  }

  void ProcessCustomType() 
  {
      
  }

    // Update is called once per frame
    void Update()
    {
        
    }
}
