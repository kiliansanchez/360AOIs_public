using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class VrSettingsMenu : MonoBehaviour
{

    public GameObject menu;

    public Toggle CameraTrackingToggle;
    public Toggle AOIVisibilityToggle;
    public Toggle GazeVisibilityToggle;

    public TMPro.TMP_InputField FixationDurationThresholdInput;
    public TMPro.TMP_InputField FixationDispersionThresholdInput;
    public TMPro.TMP_InputField FixationVelocityThresholdInput;
    public TMPro.TMP_InputField DwellToleranceInput;


    public GameObject EyeFramework;

    public static bool TrackVRCamera;
    public static bool AOIsVisibleInVr;
    public static bool GazeVisibleInVr;

    public static UnityEvent<bool> AOIVisibilityToggled = new();


    private void Start()
    {
        TrackVRCamera = CameraTrackingToggle.isOn;
        AOIsVisibleInVr = AOIVisibilityToggle.isOn;
        GazeVisibleInVr = GazeVisibilityToggle.isOn;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) && VRManager.isXRactive)
        {
            LaunchCalibration();
        }

        if (Input.GetKeyDown(KeyCode.K) && VRManager.isXRactive)
        {
            GazeVisibilityToggle.isOn = !GazeVisibilityToggle.isOn;
            ToggleGazeVisibilityInVr();
        }
    }

    public void ToggleVisibility()
    {
        menu.SetActive(!menu.activeSelf);
    }

    public void ToggleVRCameraTracking()
    {
        TrackVRCamera = CameraTrackingToggle.isOn;
    }

    public void ToggleAOIVisibilityInVr()
    {
        AOIsVisibleInVr = AOIVisibilityToggle.isOn;

        AOIVisibilityToggled?.Invoke(AOIsVisibleInVr);
    }

    public void ToggleGazeVisibilityInVr()
    {
        GazeVisibleInVr = GazeVisibilityToggle.isOn;
        EyeFramework.layer = GazeVisibleInVr ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("EditorOnly");
    }

    public void OnNewFixationDurationThresholdEntered()
    {
        bool success = int.TryParse(FixationDurationThresholdInput.text, out EventDetection.FixationDurationThresholdInMs);
        if (!success)
        {
            
        }
        else
        {
            Debug.Log("Fixation Duration Threshold changed to: " + EventDetection.FixationDurationThresholdInMs + "ms");
            EventDetectionTester.RunTest();
        }
    }

    public void OnNewFixationDispersionThresholdEntered()
    {
        bool success = int.TryParse(FixationDispersionThresholdInput.text, out EventDetection.FixationDispersionThresholdInDegrees);
        if (!success)
        {
            //FixationThresholdInMs = 150;
        }
        else
        {
            Debug.Log("Fixation Dispersion Threshold changed to: " + EventDetection.FixationDispersionThresholdInDegrees + "deg");
        }
    }

    public void OnNewFixationVelocityThresholdEntered()
    {
        bool success = int.TryParse(FixationVelocityThresholdInput.text, out EventDetection.FixationVelocityThresholdInDegPerSecond);
        if (!success)
        {
            
        }
        else
        {
            Debug.Log("Fixation Velocity Threshold changed to: " + EventDetection.FixationVelocityThresholdInDegPerSecond + "deg/s");
        }
    }

    public void OnNewDwellToleranceEntered()
    {
        bool success = int.TryParse(DwellToleranceInput.text, out EventDetection.DwellToleranceInMs);
        if (!success)
        {
            //FixationThresholdInMs = 150;
        }
        else
        {
            Debug.Log("Dwell Tolerance changed to: " + EventDetection.DwellToleranceInMs + "ms");
        }

    }

    public void LaunchCalibration()
    {
        ViveSR.anipal.Eye.SRanipal_Eye_v2.LaunchEyeCalibration();
    }
}
