using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
 * This script is attached to AOIs and allows them to be dragged around to position them within the 360 degree video
 */
public class DraggableAroundCamera : AOIComponent
{

    public static float DistanceToCamera = 10f;

    public bool _flipMesh = false;

    public bool IsTryingToMove { get; protected set; } = false;

    public bool IsTryingToMoveDueToBeingActivted = false;

    protected Vector3 _offset = Vector3.zero;

    public UnityEvent MovementCompleted;
    public UnityEvent MovementStarted;

    void Update()
    {

        if (IsTryingToMoveDueToBeingActivted && Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
        }

        //check first if object itself has intention to move
        if (IsTryingToMove && transform.root.gameObject == AOIManager.ActiveAOI)
        {
            Move();
        }
    }

    public void OnMouseDown()
    {
        if (gameObject != AOIManager.ActiveAOI)
        {
            return;
        }

        // if the AOI is covered by e.g. its own right click menu, clicking
        // on the right click menu shouldnt start movement
        if (RightClickMenuManager.IsCursorOverUI())
        {
            return;
        }

        IsTryingToMove = true;
        MovementStarted?.Invoke();
    }

    public void OnMouseUp()
    {
        
        if (IsTryingToMove)
        {
            MovementCompleted?.Invoke();
        }

        IsTryingToMove = false;
        _offset = Vector3.zero;
    }

    protected override void OnActivate()
    {

        // when AOI is being activated (due to click or due to click of list entry)
        // check if cursoer is above AOI, if it is, immediately allow movement

        Ray ray = EditorCamera.Camera.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit = new();

        var results = Physics.RaycastAll(ray.origin, ray.direction, 20f);

        if (results.Length == 1)
        {
            Debug.Log("only thing clicked: " + results[0].transform.gameObject);
            if (results[0].transform.gameObject == gameObject)
            {
                IsTryingToMoveDueToBeingActivted = true;
                OnMouseDown();
            }
        }
    }

    protected virtual void Move()
    {

        // get mouse position and project it onto "ball" surface
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1);
        mousePosition = EditorCamera.Camera.ScreenToWorldPoint(mousePosition);
        var direction = mousePosition - EditorCamera.GameObject.transform.position;
        mousePosition = direction.normalized * DistanceToCamera;



        // calculate offset between mouse position and object position, from then on out keep offset constant until movement stops
        if (_offset == Vector3.zero)
        {
            _offset = mousePosition - transform.position;
        }


        //move object in sync with mouse, keeping the same offset
        var temp = mousePosition - _offset;
        direction = temp - EditorCamera.GameObject.transform.position;
        transform.position = direction.normalized * DistanceToCamera;


        transform.LookAt(EditorCamera.GameObject.transform.position);

        //for quads gotta flip em
        if (_flipMesh)
        {
            transform.forward *= -1;
        }

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(DistanceToCamera, (transform.position - EditorCamera.GameObject.transform.position).magnitude);
    }

    protected override void OnDeactivate()
    {

    }

}
