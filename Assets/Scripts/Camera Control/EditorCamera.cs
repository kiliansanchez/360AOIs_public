using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*

This script is attached to the editor camera and does two things
    a) gives a static reference to the EditorCamera to be used for e.g. position calculation etc.
    b) matches the editor camera rotation to the VR cameras rotation if flag is set by user.
 */

public class EditorCamera : MonoBehaviour
{

    public static GameObject GameObject;
    public static Camera Camera;

    // Start is called before the first frame update
    void Start()
    {
        GameObject = this.gameObject;
        Camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (VrSettingsMenu.TrackVRCamera)
        {
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }        
        }
    }
}
