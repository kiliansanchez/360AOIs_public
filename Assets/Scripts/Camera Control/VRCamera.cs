using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * This class is attatched to the camera used by the VR headset and sets the cameras culling mask.
 */

public class VRCamera : MonoBehaviour
{
    private void Start()
    {
        
        if (VrSettingsMenu.AOIsVisibleInVr)
        {
            Camera.main.cullingMask = ~LayerMask.GetMask("EditorOnly");
        }
        else
        {
            Camera.main.cullingMask = ~LayerMask.GetMask("EditorOnly", "AOILayer");
        }

        VrSettingsMenu.AOIVisibilityToggled.AddListener(OnAOIVisibilityToggled);
    }

    void OnAOIVisibilityToggled(bool new_visibility)
    {
        if (new_visibility)
        {
            Camera.main.cullingMask = ~LayerMask.GetMask("EditorOnly");
        }
        else
        {
            Camera.main.cullingMask = ~LayerMask.GetMask("EditorOnly", "AOILayer");
        }
    }
}
