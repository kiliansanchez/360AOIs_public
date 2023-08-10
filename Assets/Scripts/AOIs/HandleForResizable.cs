using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * This script is attached to every AOI-Handle. It signals the parent object that a handle has been clicked
 * and resizing should start. Requires the parent to have the "Resizable" Component
 */

public class HandleForResizable : MonoBehaviour
{

    public Resizable Parent;

    // Start is called before the first frame update
    void Start()
    {
        transform.LookAt(EditorCamera.GameObject.transform.position);
    }

    public void OnMouseDown()
    {
        if (RightClickMenuManager.IsCursorOverUI())
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
