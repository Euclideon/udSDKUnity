using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vault;

/*
 * 
 */
[System.Serializable]
public class VDKCollider : MonoBehaviour
{
    [Tooltip("target object to follow")]
    public GameObject followTarget = null;
    [Tooltip("Determines whether to update the plane only when the target moves a threshold distance from the watcher point")]
    public bool threshholdFollow = false;
    [Tooltip("Distance from which to update the plane location when threshold follow is turned on")]
    public double followThreshold;
    [Tooltip("Position of the virtual watcher camera relative to the target")]
    public Vector3 watcherPos = new Vector3(0, 25, 0);
    //UDS Renderer components:
    private vdkRenderContext vRenderer;
    private vdkRenderView renderView;
    //Properties of the plane:
    [Tooltip("Width of the collision plane in metres")]
    public float width = 10; //width of the plane in metres
    [Tooltip("Height of the collision plane in metres")]
    public float height = 10; //height of the plane in meters
    [Tooltip("position of the near clipping plane along the z axis")]
    public float zNear = 0; //location of the near clipping plane 
    [Tooltip("position of the far clipping plane along the z axis")]
    public float zFar = 50; //location of the far plane 
    [Tooltip("width of the polygon sheet in number of vertices (this is also the resolution of the view)")]
    public int widthPix = 50;
    [Tooltip("Height of the polygon sheet in number of vertices (this is also the resolution of the view)")]
    public int heightPix = 50;
    [Tooltip("Turns on smoothing of collider surface")]
    public bool laplacianSmoothing = false;
    [Tooltip("Determines if the collider rotates with the body of the target object")]
    public LockRotationToBody lockRotationToBody;
    public Vector3 bodyLockOffset = new Vector3( 0, 0, 0);
    [System.Serializable]
    public class LockRotationToBody {
        public bool x = false;
        public bool y = false;
        public bool z = false;
    };


    public float[] depthBuffer;
    private Color32[] colourBuffer;
    void Awake()
    {
        SetRenderView();
        Update();
    }

    /*
     * converts the z buffer value to a world space displacement
     */
    float zBufferToDepth(float z)
    {
        return UDUtilities.zBufferToDepth(z, zNear, zFar);
    }

    /*
     * constructs a mesh of world size width x height in metres, each vertex's z dimension is set according to the contents of the z buffer
     * triangles are declared in a clockwise direction so face normals are back towards the generating camera
     */
    private void MakeSheetMesh(double width, double height, int numVertsX, int numVertsY, float[] depthMap = null)
    {
        Vector3[] verts;
        Vector3[] norms = new Vector3[numVertsX * numVertsY];
        Vector2[] uv = new Vector2[numVertsX * numVertsY];
        List<int> tris = new List<int>(numVertsX * numVertsY * 3);
        //initialise a mesh plane centred at origin 
        verts = new Vector3[numVertsX * numVertsY];
        double xSpacing = width / numVertsX;
        double ySpacing = height / numVertsY;
        int triCount = 0;
        for (int i = 0; i < numVertsY; i++)
        {
            for (int j = 0; j < numVertsX; j++)
            {
                float zPrime = depthMap == null ? 0f : depthMap[i * numVertsX + j];
                float depth = zBufferToDepth(zPrime);
                uv[i * numVertsX + j] = new Vector2(j / (float)numVertsX, i / (float)numVertsY);
                verts[numVertsX * i + j] = new Vector3((float)(j * xSpacing - width / 2), (float)(i * ySpacing - height / 2), -depth);
                if (((i + j) % 2 == 0))
                {
                    //make triangles at every interior even vert
                    if (i > 0 && j > 0)
                    {
                        tris.Add(numVertsX * i + j); //current vertex
                        tris.Add(numVertsX * i + j - 1); //vertex left
                        tris.Add(numVertsX * (i - 1) + j); //vertex above
                        triCount++;
                    }

                    if (i > 0 && j < numVertsX - 1)
                    {
                        tris.Add(numVertsX * i + j); //current vertex
                        tris.Add(numVertsX * (i - 1) + j); //vertex above
                        tris.Add(numVertsX * i + j + 1); //vertex Right
                        triCount++;
                    }

                    if (j < (numVertsX - 1) && i < (numVertsY - 1))
                    {
                        tris.Add(numVertsX * i + j); //current vertex
                        tris.Add(numVertsX * i + j + 1); //vertex Right
                        tris.Add(numVertsX * (i + 1) + j); //vertex below
                        triCount++;
                    }

                    if (i < numVertsY - 1 && j > 0)
                    {
                        tris.Add(numVertsX * i + j); //current vertex
                        tris.Add(numVertsX * (i + 1) + j); //vertex below
                        tris.Add(numVertsX * i + j - 1); //vertex left
                        triCount++;
                    }
                }
            }
        }

        if (laplacianSmoothing)
            verts = LaplacianSmooth(verts, numVertsX, numVertsY);

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.uv = uv;
        mesh.triangles = tris.ToArray();
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null)
            mf.mesh = mesh;

        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    /*
     * Currently unused, moves the mesh vertices towards and away from the camera with the sine of time
     */
    private void WarpMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = vertices[i] + new Vector3(0, 0, Mathf.Sin(Time.time));

        mesh.vertices = vertices;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    /*
     *Laplacian smoothing operation, each vertex is modified such that it is the average of its neighbours
     */
    private Vector3[] LaplacianSmooth(Vector3[] verts, int numVertsX, int numVertsY)
    {
        Vector3[] newVerts = new Vector3[numVertsX * numVertsY];
        for (int i = 0; i < numVertsY; i++)
        {
            for (int j = 0; j < numVertsX; j++)
            {
                float sumNeighbours = 0; 
                float numNeighbours = 0;
                if (i > 0 && j > 0)
                {
                    //node to the left and above
                    float neighbour = verts[(i - 1) * numVertsX + j - 1].z;
                    sumNeighbours += neighbour;
                    numNeighbours++;
                }

                if (i > 0)
                {
                   //node to the left 
                    float neighbour = verts[(i - 1) * numVertsX + j].z;
                    sumNeighbours += neighbour;
                    numNeighbours++;
                }

                if (j > 0)
                {
                   //node above
                    float neighbour = verts[(i) * numVertsX + j - 1].z;
                    sumNeighbours += neighbour;
                    numNeighbours++;
                }

                if (j < numVertsX - 1)
                {
                   //below
                    float neighbour = verts[numVertsX + j + 1].z;
                    sumNeighbours += neighbour;
                    numNeighbours++;
                }

                if (i < (numVertsY - 1))
                {
                    //right 
                    float neighbour = verts[(i + 1) * numVertsX + j].z;
                    sumNeighbours += neighbour;
                    numNeighbours++;
                }

                if (i > 0 && j < numVertsX - 1)
                {
                   //left and below
                    float neighbour = verts[(i - 1) * numVertsX + j + 1].z;
                    sumNeighbours += neighbour;
                    numNeighbours++;
                }

                if (j < (numVertsX - 1) && i < (numVertsY - 1))
                {
                    //right and below
                    float neighbour = verts[(i + 1) * numVertsX + j + 1].z;
                    sumNeighbours += neighbour;
                    numNeighbours++;
                }

                if (i < numVertsY - 1 && j > 0)
                {
                   //right and above
                    float neighbour = verts[(i + 1) * numVertsX + j - 1].z;
                    sumNeighbours += neighbour;
                    numNeighbours++;
                }

                Vector3 oldVert = verts[i * numVertsX + j];
                newVerts[i * numVertsX + j] = new Vector3(oldVert.x, oldVert.y, sumNeighbours / numNeighbours);
            }
        }

        return newVerts;
    }

    /*
     * creates the render view and sets the targets
     */
    void SetRenderView()
    {
        Debug.Log("creating plane render view");
        renderView = new vdkRenderView();
        if (GlobalVDKContext.isCreated == false)
            GlobalVDKContext.Login();

        renderView.Create(GlobalVDKContext.vContext, GlobalVDKContext.renderer, (uint)widthPix, (uint)heightPix);
        depthBuffer = new float[widthPix * heightPix];
        colourBuffer = null; 
        renderView.SetTargets(ref colourBuffer, 0, ref depthBuffer);
    }

    // Update is called once per frame
    void Update()
    {
        //code to follow a target around:
        //if we are following a target, we only update the corresponding mesh whne the object has moved a requisite distance,
        //this reduces the number of updates to the mesh required.
        Vector3 offset;
        if (followTarget != null)
        {
            Vector3 newRot = transform.rotation.eulerAngles;
            if (lockRotationToBody.x)
                newRot.x = followTarget.transform.eulerAngles.x + this.bodyLockOffset.x;

            if (lockRotationToBody.y)
                newRot.y = followTarget.transform.eulerAngles.y + this.bodyLockOffset.y;

            if (lockRotationToBody.z)
                newRot.z = followTarget.transform.eulerAngles.z +this.bodyLockOffset.z;

            transform.eulerAngles = newRot;
            offset = Matrix4x4.Rotate(transform.rotation) * new Vector4(watcherPos.x, watcherPos.y, watcherPos.z);
            bool thresholdTrigger = (this.transform.position - followTarget.transform.position).magnitude > followThreshold;
            if (!threshholdFollow || thresholdTrigger)
            {
                this.transform.position = followTarget.transform.position + offset;
                UpdateView();
            }
        }
        else
        {
            //we update the mesh every frame
            UpdateView();
        }
    }

    private void UpdateView()
    {
        if (renderView == null || renderView.pRenderView == IntPtr.Zero)
            SetRenderView();

        vdkRenderInstance[] modelArray = UDUtilities.getUDSInstances();
        Matrix4x4 watcherTrans = transform.localToWorldMatrix;
        double[] frontPlaneView = UDUtilities.GetUDMatrix(watcherTrans);
        renderView.SetMatrix(Vault.RenderViewMatrix.Camera, frontPlaneView);
        Matrix4x4 projection = Matrix4x4.Ortho(-width / 2, width / 2, height / 2, -height / 2, zNear, zFar);
        renderView.SetMatrix(Vault.RenderViewMatrix.Projection, UDUtilities.GetUDMatrix(projection));
        RenderOptions options = new RenderOptions();
        try
        {
            GlobalVDKContext.renderer.Render(renderView, modelArray, modelArray.Length, options);
        }
        catch (Exception e){
            Debug.Log("VDK dropped frame: " + e.ToString());
        }
        MakeSheetMesh(width, height, (int)widthPix, (int)heightPix, depthBuffer);
    }

    public void OnDestroy()
    {
        Debug.Log("destroying plane");
        renderView.Destroy();
    }
}

