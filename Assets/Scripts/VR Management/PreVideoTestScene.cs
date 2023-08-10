using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles display of testscene after vr is enabled but before recording starts. 
/// </summary>
public class PreVideoTestScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        EyeRecorder.OnRecordingToggled.AddListener(OnRecordingToggled);
        VRManager.OnXRStatusUpdate.AddListener(OnVrStatusChange);
        VrSettingsMenu.OnIsPreVideoTestSceneEnabledToggled.AddListener(OnPreVideoTestSceneToggled);
    }

    /// <summary>
    /// Callback for VrSettingsMenus OnIsPreVideoTestSceneEnabledToggled-Event. Makes Spheres visible when
    /// Toggle is set to active and VR is enabled.
    /// </summary>
    /// <param name="isEnabled">Bool whether or not user wants to user testscene.</param>
    private void OnPreVideoTestSceneToggled(bool isEnabled)
    {
        if (isEnabled && VRManager.CurrentVRStatus == VRManager.VRStatus.Active && !EyeRecorder.IsRecording)
        {
            transform.localScale = Vector3.one;
        }
        else
        {
            transform.localScale = Vector3.zero;
        }
    }

    /// <summary>
    /// Callback for VRManagers OnXRStatusUpdate-Event. Makes Spheres visible when
    /// VR is being enabled and user has option toggled.
    /// </summary>
    /// <param name="status">New Status of VR Framework.</param>
    private void OnVrStatusChange(VRManager.VRStatus status)
    {

        if (!VrSettingsMenu.IsPreVideoTestSceneEnabled)
        {
            transform.localScale = Vector3.zero;
            return;
        }

        if (status == VRManager.VRStatus.Active)
        {
            transform.localScale = Vector3.one;
        }
        else
        {
            transform.localScale = Vector3.zero;
        }
    }

    /// <summary>
    /// Callback for Eyerecorders OnRecordingToggled Event. Hides Spheres when recording has been started.
    /// </summary>
    /// <param name="is_recording">Bool whether recording has been started or turned off.</param>
    private void OnRecordingToggled(bool is_recording)
    {
        if (is_recording)
        {
            //gameObject.SetActive(false);
            transform.localScale = Vector3.zero;
        }
    }

}
