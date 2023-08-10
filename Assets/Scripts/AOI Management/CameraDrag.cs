using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// This script is attached to the Editor Camera and allows the User to change the portion of the 360 degree video being displayed, by
/// holding down right click and dragging. The camere then rotates based on the cursor movement.
/// </summary>
public class CameraDrag : MonoBehaviour
{
    private GameObject _canvas;

    public float Speed = 3.5f;
    private float _X;
    private float _Y;

    private void Start()
    {
        _canvas = GameObject.Find("UI Canvas");
        UnityEngine.Assertions.Assert.IsNotNull(_canvas);
    }

    /// <summary>
    /// Since this is a very simple script all functionality is implemented in update function.
    /// </summary>
    void Update()
    {
        if (Input.GetMouseButton(1) && !RightClickMenu.IsCursorOverUI())
        {

            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);

            // Set the position of the PointerEventData to the current mouse position
            pointerEventData.position = Input.mousePosition;

            // Create a new List to store the results of the raycast
            List<RaycastResult> raycastResults = new List<RaycastResult>();

            // Get a reference to the GraphicRaycaster component on the Canvas using the GameObject.GetComponent method
            GraphicRaycaster graphicRaycaster = _canvas.GetComponent<GraphicRaycaster>();


            // Cast a ray using the PointerEventData and store the results in the List
            graphicRaycaster.Raycast(pointerEventData, raycastResults);

            // Check if the raycast hit any UI elements
            if (raycastResults.Count == 0)
            {
                transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * Speed, -Input.GetAxis("Mouse X") * Speed, 0));
                _X = transform.rotation.eulerAngles.x;
                _Y = transform.rotation.eulerAngles.y;
                transform.rotation = Quaternion.Euler(_X, _Y, 0);
            }
                
        }
    }

}
