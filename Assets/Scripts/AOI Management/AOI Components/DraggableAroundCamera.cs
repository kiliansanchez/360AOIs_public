using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// This script is attached to AOIs and allows them to be dragged around to position them within the 360 degree video.
/// </summary>
public class DraggableAroundCamera : AOIComponent
{

    public static float DistanceToCamera = 10f;

    public bool IsTryingToMove { get; protected set; } = false;

    public bool IsTryingToMoveDueToBeingActivted { get; protected set; } = false;

    protected Vector3 _offset = Vector3.zero;

    public UnityEvent MovementCompleted = new();
    public UnityEvent MovementStarted = new();

    void Update()
    {

        if (IsTryingToMoveDueToBeingActivted && Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
        }

        //if object is trying to move and is currently active aoi: allow movement
        if (IsTryingToMove && transform.root.gameObject == AOIManager.ActiveAOI)
        {
            Move();
        }
    }

    /// <summary>
    /// Callback for when AOI is being clicked. If AOI is currently selected AOI start movement.
    /// </summary>
    public void OnMouseDown()
    {
        if (gameObject != AOIManager.ActiveAOI)
        {
            return;
        }

        // if the AOI is covered by e.g. its own right click menu, clicking
        // on the right click menu shouldnt start movement
        if (RightClickMenu.IsCursorOverUI())
        {
            return;
        }

        IsTryingToMove = true;
        MovementStarted?.Invoke();
    }

    /// <summary>
    /// callback for when mouse button is released.
    /// </summary>
    public void OnMouseUp()
    {
        Debug.Log("mouseup");
        if (IsTryingToMove)
        {
            MovementCompleted?.Invoke();
        }

        IsTryingToMoveDueToBeingActivted = false;
        IsTryingToMove = false;
        _offset = Vector3.zero;
    }

    /// <summary>
    /// When AOI is being activated (due to click on aoi, due to click on list entry or right after spawn)
    /// check if cursor is above AOI, if it is, immediately allow movement instead of having to click again.
    /// </summary>
    protected override void OnActivate()
    {

        Ray ray = EditorCamera.Camera.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit = new();

        var results = Physics.RaycastAll(ray.origin, ray.direction, 20f);

        if (results.Length == 1)
        {
            if (results[0].transform.gameObject == gameObject)
            {
                IsTryingToMoveDueToBeingActivted = true;
                OnMouseDown();
            }
        }
    }

    /// <summary>
    /// Calculates new AOI position based on cursor movement. AOI follows cursor around world-center. 
    /// </summary>
    protected virtual void Move()
    {

        // get mouse position and project it onto "ball" surface
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1);
        mousePosition = EditorCamera.Camera.ScreenToWorldPoint(mousePosition);
        var direction = mousePosition - EditorCamera.EditorCamera_GameObject.transform.position;
        mousePosition = direction.normalized * DistanceToCamera;



        // calculate offset between mouse position and object position, from then on out keep offset constant until movement stops
        if (_offset == Vector3.zero)
        {
            _offset = mousePosition - transform.position;
        }


        //move object in sync with mouse, keeping the same offset
        var temp = mousePosition - _offset;
        direction = temp - EditorCamera.EditorCamera_GameObject.transform.position;
        transform.position = direction.normalized * DistanceToCamera;


        transform.LookAt(EditorCamera.EditorCamera_GameObject.transform.position);

        //for quads gotta flip em
        if (AOIManager.FlipMesh)
        {
            transform.forward *= -1;
        }

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(DistanceToCamera, (transform.position - EditorCamera.EditorCamera_GameObject.transform.position).magnitude);
    }

    protected override void OnDeactivate()
    {

    }

}
