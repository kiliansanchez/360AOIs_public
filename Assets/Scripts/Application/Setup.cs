using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Currently doesen't do too much. Sets target framerate of application to 120Hz.
/// </summary>
public class Setup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 120;
        Application.runInBackground = true;
        VRManager.OnXRStatusUpdate.AddListener(SetFramerateOnVREnable);
    }

    /// <summary>
    /// Callback fro VR Managers XRStatusUpdate Event. Attempts to set framerate even after vr is enabled. currently
    /// this is impossible, because as soon as vr is enabled the unity vr plugin takes over fps management and locks it
    /// to refresh rate of vr headsets screens. With htc vive pro eye that is 90Hz.
    /// </summary>
    /// <param name="status"></param>
    private void SetFramerateOnVREnable(VRManager.VRStatus status)
    {
        if (status == VRManager.VRStatus.Active)
        {
            Application.targetFrameRate = 120;
        }
    }
}
