using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.Events;

/// <summary>
/// Handles activation and deactivation of Unitys XR Framework and SRanipal Eye Framework.
/// </summary>
public class VRManager : MonoBehaviour
{
    public enum VRStatus
    {
        Inactive,
        Loading,
        Error,
        Active
    } 

    public static UnityEvent<VRStatus> OnXRStatusUpdate { get; private set; } = new();
    public static VRStatus CurrentVRStatus { get; private set; }

    /// <summary>
    /// tiny helper function to set vr status and invoke event at same time.
    /// could've beed done using auto prperties though
    /// </summary>
    /// <param name="new_status"></param>
    private void SetCurrentVRStatusAndNotify(VRStatus new_status)
    {
        CurrentVRStatus = new_status;
        OnXRStatusUpdate?.Invoke(CurrentVRStatus);
    }

    // reference to SRanipal framework and VR camera rig
    public GameObject _EyeFramework;
    public GameObject _XRRig;

    /// <summary>
    /// Coroutine to attempt to start unity XRFramework and SRanipal Framework.
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartXRCoroutine()
    {
        Debug.Log("[VRManager] Initializing XR...");

        SetCurrentVRStatusAndNotify(VRStatus.Loading);

        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("[VRManager] Initializing XR Failed. Check Editor or Player log for details.");

            SetCurrentVRStatusAndNotify(VRStatus.Error);

        }
        else
        {
            Debug.Log("[VRManager] Starting XR...");

            XRGeneralSettings.Instance.Manager.StartSubsystems();

            _EyeFramework.SetActive(true);
            _EyeFramework.GetComponent<ViveSR.anipal.Eye.SRanipal_Eye_Framework>().StartFramework();
            _XRRig.SetActive(true);

            if (ViveSR.anipal.Eye.SRanipal_Eye_Framework.Status == ViveSR.anipal.Eye.SRanipal_Eye_Framework.FrameworkStatus.WORKING)
            {
                SetCurrentVRStatusAndNotify(VRStatus.Active);
            }
            else
            {
                Debug.Log("[VRManager] SRanipal Eyeframework Error");
                SetCurrentVRStatusAndNotify(VRStatus.Error);
            }
            
        }
    }

    
    /// <summary>
    /// function called by VRButton in UI
    /// </summary>
    public void ToggleXR()
    {
        if (CurrentVRStatus == VRStatus.Active)
        {
            StopXR();
        }
        else
        {
            StartXR();
        }
    }

    /// <summary>
    /// Wrapper for coroutine. 
    /// </summary>
    public void StartXR()
    {
        StartCoroutine(StartXRCoroutine());
    }

    /// <summary>
    /// Stops Unity XR Framework as well as SRanipal Framework
    /// </summary>
    public void StopXR()
    {
        Debug.Log("[VRManager] Stopping XR...");

        // stop XR Plugins
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("[VRManager] XR stopped completely.");

        // stop eyetracking framework and deactivate gameobject with eye tracking script
        _EyeFramework.GetComponent<ViveSR.anipal.Eye.SRanipal_Eye_Framework>().StopFramework();
        _EyeFramework.SetActive(false);

        // deactivate VR camera rig object
        _XRRig.SetActive(false);

        SetCurrentVRStatusAndNotify(VRStatus.Inactive);

    }

}
