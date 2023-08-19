using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This script is attached to the editor camera and does three things
///    a) gives a static reference to the EditorCamera to be used for e.g. position calculation etc.
///    b) matches the editor camera rotation to the VR cameras rotation if flag is set by user.
///    c) sets clear flags based on test-scene toggle of vrsettings menu
/// </summary>
/// 

public class EditorCamera : MonoBehaviour
{

    public static GameObject EditorCamera_GameObject;
    public static Camera Camera;

    // Start is called before the first frame update
    void Start()
    {
        EditorCamera_GameObject = this.gameObject;
        Camera = GetComponent<Camera>();
        VRManager.OnXRStatusUpdate.AddListener(OnVrStatusChange);
        VrSettingsMenu.OnIsPreVideoTestSceneEnabledToggled.AddListener(OnTestSceneToggled);
        EyeRecorder.OnRecordingToggled.AddListener(OnRecordingToggled);
    }

    /// <summary>
    /// Callback for VRManager VRStatusChange-Event. When VR is being enabled and TestScene is enabled AOIs and 360
    /// degree video are hidden from view.
    /// </summary>
    /// <param name="status"></param>
    void OnVrStatusChange(VRManager.VRStatus status)
    {

        if (!VrSettingsMenu.IsPreVideoTestSceneEnabled)
        {
            Camera.clearFlags = CameraClearFlags.Skybox;
            Camera.cullingMask = Physics.AllLayers;
            return;
        }

        if (status == VRManager.VRStatus.Active)
        {
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.cullingMask = ~LayerMask.GetMask("AOILayer");
        }
        else
        {
            Camera.clearFlags = CameraClearFlags.Skybox;
            Camera.cullingMask = Physics.AllLayers;
        }
    }
    
    /// <summary>
    /// Callback for when user is toggling testscene from VRSettings Menu.
    /// </summary>
    /// <param name="isTestSceneEnabled">bool whether or not testscene is enabled</param>
    void OnTestSceneToggled(bool isTestSceneEnabled)
    {
        if (isTestSceneEnabled && VRManager.CurrentVRStatus == VRManager.VRStatus.Active && !EyeRecorder.IsRecording)
        {
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.cullingMask = ~LayerMask.GetMask("AOILayer");
        }
        else
        {
            Camera.clearFlags = CameraClearFlags.Skybox;
            Camera.cullingMask = Physics.AllLayers;
        }
    }

    /// <summary>
    /// Callback for EyeRecorders recording event. When recording is started, make sure that skybox with 360degree video as well as
    /// AOIs are visible.
    /// </summary>
    /// <param name="is_recording">bool if recording has been turned on or off.</param>
    void OnRecordingToggled(bool is_recording)
    {
        if (is_recording)
        {
            Camera.clearFlags = CameraClearFlags.Skybox;
            Camera.cullingMask = Physics.AllLayers;
        }
    }

    /// <summary>
    /// Update function. If tracking of VR camera is enabled the editor camera updates its rotation every frame to match the
    /// VR Camera.
    /// </summary>
    void Update()
    {
        if (VrSettingsMenu.IsVRCameraTracked)
        {
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }        
        }
    }


}
