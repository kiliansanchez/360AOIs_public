using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Attached to the camera used by the VR headset. Sets the cameras culling mask and clear flags.
/// </summary>

public class VRCamera : MonoBehaviour
{
    private void Awake()
    {
        
        if (VrSettingsMenu.AreAOIsVisibleInVr)
        {
            Camera.main.cullingMask = ~LayerMask.GetMask("EditorOnly");
        }
        else
        {
            Camera.main.cullingMask = ~LayerMask.GetMask("EditorOnly", "AOILayer");
        }

        VrSettingsMenu.OnAOIVisibilityToggled.AddListener(OnAOIVisibilityToggled);
        VrSettingsMenu.OnIsPreVideoTestSceneEnabledToggled.AddListener(OnPreVideoStimulusToggled);
        EyeRecorder.OnRecordingToggled.AddListener(OnRecordingToggled);
        VRManager.OnXRStatusUpdate.AddListener(OnVRStatusChange);
    }

    /// <summary>
    /// Callback for VRManagers OnXRStatusUpdate-Event. When VR is being activated cheks whether or not to 
    /// display Video (Skybox) or Solid Color (for TestScene)
    /// </summary>
    /// <param name="status"></param>
    private void OnVRStatusChange(VRManager.VRStatus status)
    {
        if (status == VRManager.VRStatus.Active)
        {
            Camera.main.clearFlags = VrSettingsMenu.IsPreVideoTestSceneEnabled ? 
                CameraClearFlags.SolidColor : Camera.main.clearFlags = CameraClearFlags.Skybox;
        }
    }

    /// <summary>
    /// Callback for VRSettingsMenus OnAOIVisibilityToggled-Event. Changes whether or not AOIs are visible in VR.
    /// </summary>
    /// <param name="new_visibility"></param>
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

    /// <summary>
    /// Callback for Eyerecorders OnRecordingToggled-Event. Displays Skybox (Video) as soon as recording starts.
    /// </summary>
    /// <param name="is_recording"></param>
    void OnRecordingToggled(bool is_recording)
    {
        if (is_recording)
        {
            Camera.main.clearFlags = CameraClearFlags.Skybox;
        }
        else
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }
    }

    /// <summary>
    /// Callback for VRSettingsMenus OnIsPreVideoTestSceneEnabledToggled-Event. Changes Camera Clear Flags
    /// to display solid color instead of video when TestScene is enabled.
    /// </summary>
    /// <param name="isPreVideoTestSceneEnabled"></param>
    void OnPreVideoStimulusToggled(bool isPreVideoTestSceneEnabled)
    {
        if (isPreVideoTestSceneEnabled && !EyeRecorder.IsRecording)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }
        else
        {
            Camera.main.clearFlags = CameraClearFlags.Skybox;
        }
    }
}
