using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using udSDK;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using System;
using Microsoft.Win32;
using UnityEngine.Networking;

public class udProjectNodeUnity : MonoBehaviour
{
  public udProjectNodeType itemType;
  public string itemTypeString;
  public UDProjectNode projectNode;
  public udProjectGeometryType geometryType;
  public string URI;
  GameObject firstChild;
  GameObject nextSibling;

  // keeps track of all coordinates in the nodedata
  double[] positions = new double[3]; 

  private UDProjectUnity project; 

  Vector3 DoublesToVector3(double[] doubles)
  { 
    return new Vector3((float)doubles[0], (float)doubles[1], (float)doubles[2]);
  }

  double[] GetReorderedPosition(double[] positions, int i)
  {
    double[] position = new double[3];

    position[0] = -positions[3 * i];
    position[2] = -positions[3 * i + 1];
    position[1] = -positions[3 * i + 2];

    return position; 
  }

  IEnumerator LoadMediaImage(SpriteRenderer sprite)
  {
    string targetURI = URI;

    if (!URI.StartsWith("http") && !URI.StartsWith("www"))
      URI = "file://" + Application.dataPath + "/" + URI;  

    // warning : this syntax changes somewhat in later versions of Unity 
    using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(URI))
    {
      yield return uwr.SendWebRequest();

      if (uwr.isNetworkError || uwr.isHttpError)
      {
        Debug.Log(uwr.error);
      }
      else
      {
        // Get downloaded asset bundle
        var tex = DownloadHandlerTexture.GetContent(uwr);
        sprite.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
      }
    }

    yield return null;
  }

  GameObject PlaceChildSphere(double[] position, float size)
  {
    Vector3 positionVector = DoublesToVector3(position); 

    var sphereGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    var renderer = sphereGO.GetComponent<Renderer>();

    var shader = Shader.Find("udSDK/Demo/DepthOffsetSprite");
    if (shader == null)
      throw new Exception("Required shader is missing : udSDK/Demo/DepthOffsetSprite");
    renderer.material = new Material(shader);
    renderer.material.SetFloat("_ZOffset", 0 + size*0.01f);
    renderer.material.color = project.appearance.interestColor; 

    sphereGO.transform.parent = transform;
    sphereGO.transform.position = positionVector;
    sphereGO.transform.localScale = Vector3.one * size;

    return sphereGO; 
  }

  public void LoadTree(IntPtr pNode)
  {
    project = GetComponentInParent<UDProjectUnity>(); 
    projectNode = new UDProjectNode(pNode);

    if (projectNode.nodeData.pName != IntPtr.Zero)
      gameObject.name = Marshal.PtrToStringAnsi(projectNode.nodeData.pName);

    if (projectNode.nodeData.pURI != IntPtr.Zero)
      URI = Marshal.PtrToStringAnsi(projectNode.nodeData.pURI);

    if (projectNode.nodeData.pCoordinates != IntPtr.Zero)
    {
      if (!(projectNode.nodeData.geomCount == 0))
      {
        positions = new double[projectNode.nodeData.geomCount * 3];
        Marshal.Copy(projectNode.nodeData.pCoordinates, positions, 0, projectNode.nodeData.geomCount * 3);
      }
      else
      {
        positions = new double[3];
        Marshal.Copy(projectNode.nodeData.pCoordinates, positions, 0, 3);
      }
    }

    this.itemType = projectNode.nodeData.itemtype;
    this.geometryType = projectNode.nodeData.geomtype;

    switch (itemType)
    {
      case udProjectNodeType.udPNT_Custom://!<Need to check the itemtypeStr string manually.
        itemTypeString = new string(projectNode.nodeData.itemtypeStr);
        switch (itemTypeString) 
        {
          //these are the custom types currently supported by udSDK Client:
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
        if (geometryType != udProjectGeometryType.udPGT_LineString)
        {
          double[] pointPosition = GetReorderedPosition(positions, 0);
          pointPosition = project.CheckPosition(pointPosition); 
          var sphereGO = PlaceChildSphere(pointPosition, project.appearance.pointSize); 
          sphereGO.name = "Point of Interest"; 
        } 
        break;
      
      case udProjectNodeType.udPNT_Folder: //!<A folder of other nodes (“Folder”)
        break;
      
      case udProjectNodeType.udPNT_GTFS: //!< A General Transit Feed Specification object ("GTFS")
        break;
      
      case udProjectNodeType.udPNT_Media: //!<An Image, Movie, Audio file or other media object (“Media”)
        // only supporting images presently
        var icon = new GameObject(); 
        var sprite = icon.AddComponent<SpriteRenderer>();

        var shader = Shader.Find("udSDK/Demo/SpriteBillboard");
        if (shader == null)
          throw new Exception("Required shader is missing : udSDK/Demo/SpriteBillboard");
        sprite.material = new Material(shader);
        sprite.material.SetFloat("_ZOffset", 0.5f);

        Texture2D tex = Texture2D.blackTexture;
        sprite.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        
        icon.transform.parent = transform;

        double[] spritePosition = GetReorderedPosition(positions, 0);
        spritePosition = project.CheckPosition(spritePosition); 
        Vector3 positionVector = DoublesToVector3(spritePosition); 
        icon.transform.position = positionVector;

        icon.transform.localScale = Vector3.one * project.appearance.imageMediaSize;

        // load the image in a coroutine
        icon.name = "Media : " + URI;
        StartCoroutine(LoadMediaImage(sprite));
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
        // we check the position here, so that it sets any required offset values
        project.CheckPosition(GetReorderedPosition(positions, 0)); 
        break;

      case(udProjectGeometryType.udPGT_MultiPoint): //!<Array of udPGT_Point, pCoordinates is an array of 3D positions.
        if (!(projectNode.nodeData.geomCount==0)) 
        {
          //create a child object for each geometry object
          for(int i = 0; i < projectNode.nodeData.geomCount; i++)
          {
            double[] position = GetReorderedPosition(positions, i);
            position = project.CheckPosition(position); 
            var sphereGO = PlaceChildSphere(position, project.appearance.lineSize);
            sphereGO.name = "Point "+i; 
          }
        }
        break;

      case(udProjectGeometryType.udPGT_LineString): //!<pCoordinates is an array of 3D positions forming an open line
        LineRenderer lr = gameObject.AddComponent<LineRenderer>();

        var shader = Shader.Find("udSDK/Demo/DepthOffsetSprite");
        if (shader == null)
          throw new Exception("Required shader is missing : udSDK/Demo/DepthOffsetSprite");
        lr.material = new Material(shader);
        lr.material.SetFloat("_ZOffset", 0 + project.appearance.lineSize * 0.01f);
        lr.material.color = project.appearance.interestColor; 

        lr.useWorldSpace = false; // this gives us more tangible editor controls
        lr.positionCount = projectNode.nodeData.geomCount;
        
        Vector3[] verts = new Vector3[projectNode.nodeData.geomCount];
        if (!(projectNode.nodeData.geomCount==0)) 
        {
            //create a child object for each geometry object
            for(int i = 0; i < projectNode.nodeData.geomCount; i++)
            {
                double[] position = GetReorderedPosition(positions, i);
                position = project.CheckPosition(position); 
                var sphereGO = PlaceChildSphere(position, project.appearance.lineSize);
                verts[i] = sphereGO.transform.position; 
                sphereGO.name = "Point "+i; 
            }
        }

        lr.SetPositions(verts);
        lr.SetWidth(project.appearance.lineSize, project.appearance.lineSize);
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
}
