using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/*

DraggableAlongXYPlane is attached to every AOI handle and allows the handle to be dragged along the XY plane of its
parent (the AOI)

 */
public class DraggableAlongXYPlane : MonoBehaviour
{

    public bool _flipMesh = false;

    public bool IsTryingToMove { get; protected set; } = false;
    protected Vector3 _offset = Vector3.zero;

    public UnityEvent MovementCompleted;
    public UnityEvent MovementStarted;

    void Update()
    {
        //check first if object itself has intention to move
        if (IsTryingToMove)
        {
            Move();
        }
    }

    public void OnMouseDown()
    {
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

    private void OnDisable()
    {
        OnMouseUp();
    }

    protected void Move()
    {

        var plane = new Plane(transform.root.position.normalized, transform.root.position);

        // create a ray from the mousePosition
        var ray = EditorCamera.Camera.ScreenPointToRay(Input.mousePosition);

        // plane.Raycast returns the distance from the ray start to the hit point
        if (plane.Raycast(ray, out float distance))
        {
            // some point of the plane was hit - get its coordinates
            var hitPoint = ray.GetPoint(distance);

            // calculate offset between hitPoint position and object position, from then on out keep offset constant until movement stops
            if (_offset == Vector3.zero)
            {
                _offset =  transform.position - hitPoint;
            }

            transform.position = hitPoint + _offset;
        }
        

        //for quads gotta flip em
        if (_flipMesh)
        {
            transform.forward *= -1;
        }
    }
}
