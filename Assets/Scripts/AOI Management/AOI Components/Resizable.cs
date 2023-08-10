using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;



/// <summary>
/// This script is attached to AOIs and is responsible for the creation of handles as well as resizing. 
/// Resizing happens when the user drags a handle of the currently selected AOI across the screen.
/// Something of note: Resizing is done by changing the vertices of the AOIs mesh. Moving the vertices doesn't
/// automatically recalculate the center (origin) of the game object, so this class manually recenters the objects origin
/// during and after resizing is completed.
/// </summary>
public class Resizable : AOIComponent
{

    // Prefab Handle for Resizing the Object
    public GameObject HandlePrefab;

    public bool IsTryingToResize { get; protected set; } = false;
    public Mesh Mesh { get; protected set; }

    private MeshCollider _collider;
    private Renderer _renderer;
    private Vector3[] _vertices;
    private List<HandleForResizable> _handles = new List<HandleForResizable>();

    public UnityEvent ResizingCompleted;
    public UnityEvent ResizingStarted;

    // Start is called before the first frame update
    protected override void Start()
    {

        // get mesh and its vertices, as well as collider and renderer
        Mesh = GetComponent<MeshFilter>().mesh;
        _collider = GetComponent<MeshCollider>();
        _renderer = GetComponent<Renderer>();
        _vertices = Mesh.vertices;

        // for every vertice create a handle and make it child of Resizable
        foreach (var vertex in _vertices)
        {
            GameObject newHandle = Instantiate(HandlePrefab, this.transform.position, Quaternion.identity);
            newHandle.transform.SetParent(this.transform, true);
            newHandle.transform.localPosition = vertex;

            newHandle.GetComponent<HandleForResizable>().Parent = this;
            newHandle.layer = LayerMask.NameToLayer("AOILayer");
            _handles.Add(newHandle.GetComponent<HandleForResizable>());
        }

        ShowHandles();

        base.Start();
    }


    void Update()
    {
        if (IsTryingToResize)
        {
            Resize();
        }
    }

    /// <summary>
    /// for debugging purposes
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, .1f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GetComponent<Renderer>().bounds.center, .1f);
    }


    /// <summary>
    /// Resizes the AOIs mesh by moving each vertex to the position of its corresponding handle.
    /// </summary>
    void Resize()
    {

        for (int i = 0; i < _vertices.Length; i++)
        {
            _vertices[i] = _handles[i].transform.localPosition;
        }

        //update mesh and collider so that mouse raycasts work properly
        Mesh.vertices = _vertices;
        Mesh.RecalculateBounds();

        _collider = GetComponent<MeshCollider>();
        _collider.sharedMesh = null;
        _collider.sharedMesh = Mesh;

        RecenterOrigin();

    }

    /// <summary>
    /// Called by the handle-gameobjects of the AOI. Allows handles to signal to AOI that resizing is happening and it should now start recalculating its mesh,
    /// or that resizing is now coming to an end.
    /// </summary>
    /// <param name="handle">The handle being dragged passes itself as parameter.</param>
    /// <param name="value">Bool to signal if movement of the handle is being started or stopped.</param>
    public void SetIsTryingToResize(HandleForResizable handle, bool value)
    {
        if (_handles.Contains(handle))
        {
            //if Object was resizing but now a Handle signals that resizing is completed, invoke event
            if (IsTryingToResize && !value)
            {
                ResizingCompleted?.Invoke();
            }

            //if Object was NOT resizing but now a Handle signals that resizing is starting, invoke event
            if (!IsTryingToResize && value)
            {
                ResizingStarted?.Invoke();
            }

            IsTryingToResize = value;
        }
    }

    /// <summary>
    /// Show handles at corners of AOI.
    /// </summary>
    public void ShowHandles()
    {

        foreach (var handle in _handles)
        {
            handle.gameObject.SetActive(true);
        }

    }

    /// <summary>
    /// Hide handles at corners of AOI.
    /// </summary>
    public void HideHandles()
    {
        foreach (var handle in _handles)
        {
            handle.gameObject.SetActive(false);
        }

    }


    /// <summary>
    ///  Recenters the origin of the object back to it's meshs center after changing vertices.
    ///  This way a nice and consitent centerpoint is kept for rotating and positioning the AOIs around the Editor Camera
    /// </summary>
    public void RecenterOrigin()
    {
        var offset = transform.position - _renderer.bounds.center;

        for (int i = 0; i < _vertices.Length; i++)
        {
            // important to change offset from world space to local space, since vertices are in local space
            _vertices[i] = _vertices[i] + transform.InverseTransformDirection(offset);

            // set local position of handles to vertice position (already local)
            _handles[i].transform.localPosition = _vertices[i];
        }

        transform.position = transform.position - offset;

        // recalculate mesh
        Mesh.vertices = _vertices;
        Mesh.RecalculateBounds();

        
        _collider.sharedMesh = null;
        _collider.sharedMesh = Mesh;


        // reset objects position from camera to be exactly 10 units away and turn towards camera
        var direction = transform.position - EditorCamera.EditorCamera_GameObject.transform.position;
        transform.position = direction.normalized * 10f;
        transform.LookAt(EditorCamera.EditorCamera_GameObject.transform.position);

        
        if (AOIManager.FlipMesh)
        {
            transform.forward *= -1;
        }
    }

    /// <summary>
    /// Called by Animation Component when AOIs are loaded from a csv file.
    /// </summary>
    /// <param name="newVertices">Vertices stored in keyframe data that need to be loaded into resizable component.</param>
    /// <returns></returns>
    public Vector3[] RecenterOriginBasedOnKeyframeData(Vector3[] newVertices)
    {
        Array.Copy(newVertices, 0, _vertices, 0, newVertices.Length);
        Mesh.vertices = _vertices;
        Mesh.RecalculateBounds();

        _collider = GetComponent<MeshCollider>();
        _collider.sharedMesh = null;
        _collider.sharedMesh = Mesh;

        RecenterOrigin();

        return Mesh.vertices;
    }

    protected override void OnActivate()
    {
        ShowHandles();
    }

    protected override void OnDeactivate()
    {
        HideHandles();
    }
}
