using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This script is attached to every AOI-Handle. It signals the parent object that a handle has been clicked
/// and resizing should start. Requires the parent to have the "Resizable" Component
/// </summary>
/// 
public class HandleForResizable : MonoBehaviour
{

    public Resizable Parent;

    void Start()
    {
        transform.LookAt(EditorCamera.EditorCamera_GameObject.transform.position);
    }

    public void OnMouseDown()
    {
        if (RightClickMenu.IsCursorOverUI())
        {
            return;
        }

        Parent.SetIsTryingToResize(this, true);
    }

    public void OnMouseUp()
    {      
        Parent.RecenterOrigin();
        Parent.SetIsTryingToResize(this, false);
    }

}
