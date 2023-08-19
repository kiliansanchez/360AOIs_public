using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Attatched to UI GameObject "TopBar". Handles all user input related to the VR Settings Menu and invokes
/// events to notify other Scripts of setting-changes.
/// </summary>
public class VrSettingsMenu : MonoBehaviour
{

    public GameObject menu;

    public Toggle CameraTrackingToggle;
    public Toggle PreVideoTestSceneToggle;
    public Toggle AOIVisibilityToggle;
    public Toggle GazeVisibilityToggle;

    public TMPro.TMP_InputField FixationDurationThresholdInput;
    public TMPro.TMP_InputField FixationDispersionThresholdInput;
    public TMPro.TMP_InputField FixationVelocityThresholdInput;
    public TMPro.TMP_InputField DwellToleranceInput;


    public GameObject EyeFramework;

    public static bool IsVRCameraTracked;
    public static bool IsPreVideoTestSceneEnabled;
    public static bool AreAOIsVisibleInVr;
    public static bool IsGazeVisibleInVr;

    public static UnityEvent<bool> OnAOIVisibilityToggled = new();
    public static UnityEvent<bool> OnIsPreVideoTestSceneEnabledToggled = new();


    private void Start()
    {
        IsVRCameraTracked = CameraTrackingToggle.isOn;
        IsPreVideoTestSceneEnabled = PreVideoTestSceneToggle.isOn;
        AreAOIsVisibleInVr = AOIVisibilityToggle.isOn;
        IsGazeVisibleInVr = GazeVisibilityToggle.isOn;

        FixationDurationThresholdInput.text = EventDetection.FixationDurationThresholdInMs.ToString();
        FixationDispersionThresholdInput.text = EventDetection.FixationDispersionThresholdInDegrees.ToString();
        FixationVelocityThresholdInput.text = EventDetection.FixationVelocityThresholdInDegPerSecond.ToString();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) && VRManager.CurrentVRStatus == VRManager.VRStatus.Active)
        {
            LaunchCalibration();
        }

        if (Input.GetKeyDown(KeyCode.K) && VRManager.CurrentVRStatus == VRManager.VRStatus.Active)
        {
            GazeVisibilityToggle.isOn = !GazeVisibilityToggle.isOn;
            ToggleGazeVisibilityInVr();
        }
    }

    /// <summary>
    /// Show or hide settings menu
    /// </summary>
    public void ToggleVisibility()
    {
        menu.SetActive(!menu.activeSelf);
    }

    /// <summary>
    /// Called from UI.
    /// Sets static property whenever toggle is used. Property is read by EditorCamera.
    /// </summary>
    public void ToggleVRCameraTracking()
    {
        IsVRCameraTracked = CameraTrackingToggle.isOn;
    }

    /// <summary>
    /// Called from UI. Toggles whether or not test scene is displaye before video.
    /// </summary>
    public void TogglePreVideoTestScene()
    {
        IsPreVideoTestSceneEnabled = PreVideoTestSceneToggle.isOn;
        OnIsPreVideoTestSceneEnabledToggled?.Invoke(IsPreVideoTestSceneEnabled);
    }

    /// <summary>
    /// Called from UI. Toggles whether or not AOIs are visible in vr.
    /// </summary>
    public void ToggleAOIVisibilityInVr()
    {
        AreAOIsVisibleInVr = AOIVisibilityToggle.isOn;

        OnAOIVisibilityToggled?.Invoke(AreAOIsVisibleInVr);
    }

    /// <summary>
    /// Called from UI. Toggles whether or not gaze is visible in vr.
    /// </summary>
    public void ToggleGazeVisibilityInVr()
    {
        IsGazeVisibleInVr = GazeVisibilityToggle.isOn;
        EyeFramework.layer = IsGazeVisibleInVr ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("EditorOnly");
    }

    /// <summary>
    /// Called from UI. Sets minimum fixation duration threshold.
    /// </summary>
    public void OnNewFixationDurationThresholdEntered()
    {
        bool success = int.TryParse(FixationDurationThresholdInput.text, out EventDetection.FixationDurationThresholdInMs);
        if (!success)
        {
            
        }
        else
        {
            Debug.Log("[VrSettingsMenu] Fixation Duration Threshold changed to: " + EventDetection.FixationDurationThresholdInMs + "ms");
            EventDetectionTester.RunTest();
        }
    }

    /// <summary>
    /// Called from UI. Sets fixation dispersion threshold.
    /// </summary>
    public void OnNewFixationDispersionThresholdEntered()
    {
        bool success = float.TryParse(FixationDispersionThresholdInput.text, out EventDetection.FixationDispersionThresholdInDegrees);
        if (!success)
        {
            //FixationThresholdInMs = 150;
        }
        else
        {
            Debug.Log("[VrSettingsMenu] Fixation Dispersion Threshold changed to: " + EventDetection.FixationDispersionThresholdInDegrees + "deg");
        }
    }

    /// <summary>
    /// Called from UI. Sets fixation velocity threshold.
    /// </summary>
    public void OnNewFixationVelocityThresholdEntered()
    {
        bool success = float.TryParse(FixationVelocityThresholdInput.text, out EventDetection.FixationVelocityThresholdInDegPerSecond);
        if (!success)
        {
            
        }
        else
        {
            Debug.Log("[VrSettingsMenu] Fixation Velocity Threshold changed to: " + EventDetection.FixationVelocityThresholdInDegPerSecond + "deg/s");
        }
    }

    /// <summary>
    /// Called from UI. Sets Time between dwells threshold.
    /// </summary>
    public void OnNewDwellToleranceEntered()
    {
        bool success = int.TryParse(DwellToleranceInput.text, out EventDetection.DwellToleranceInMs);
        if (!success)
        {
            //FixationThresholdInMs = 150;
        }
        else
        {
            Debug.Log("[VrSettingsMenu] Dwell Tolerance changed to: " + EventDetection.DwellToleranceInMs + "ms");
        }

    }

    /// <summary>
    /// Called from UI. Launches SRanipal Calibration.
    /// </summary>
    public void LaunchCalibration()
    {
        ViveSR.anipal.Eye.SRanipal_Eye_v2.LaunchEyeCalibration();
    }
}
