using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.Events;

public class VRManager : MonoBehaviour
{

    // reference to relevant Buttons to signal state changes
    public VRButton _vrButton;
    public UnityEngine.UI.Button _recButton;

    // reference to SRanipal framework and VR camera rig
    public GameObject _EyeFramework;
    public GameObject _XRRig;

    // bool to track status
    public static bool isXRactive { get; private set; } = false;

    public static UnityEvent<bool> XRStatusToggled = new();

    public IEnumerator StartXRCoroutine()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
            _vrButton.SetState(VRButton.State.Error);

            SetXrFlag(false);
        }
        else
        {
            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();

            _EyeFramework.SetActive(true);
            _EyeFramework.GetComponent<ViveSR.anipal.Eye.SRanipal_Eye_Framework>().StartFramework();

            _XRRig.SetActive(true);

            // set button states in UI
            if (ViveSR.anipal.Eye.SRanipal_Eye_Framework.Status == ViveSR.anipal.Eye.SRanipal_Eye_Framework.FrameworkStatus.WORKING)
            {
                _vrButton.SetState(VRButton.State.On);
                _recButton.interactable = true;
                SetXrFlag(true);
            }
            else
            {
                _vrButton.SetState(VRButton.State.Error);
                _recButton.interactable = false;
                SetXrFlag(true);
            }
            
        }
    }

    public void ToggleXR()
    {
        if (isXRactive)
        {
            StopXR();
        }
        else
        {
            StartXR();
        }
    }

    public void StartXR()
    {

        _vrButton.SetState(VRButton.State.Loading);
        StartCoroutine(StartXRCoroutine());
    }

    public void StopXR()
    {
        Debug.Log("Stopping XR...");

        // stop XR Plugins
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR stopped completely.");

        // stop eyetracking framework and deactivate gameobject with eye tracking script
        _EyeFramework.GetComponent<ViveSR.anipal.Eye.SRanipal_Eye_Framework>().StopFramework();
        _EyeFramework.SetActive(false);

        // deactivate VR camera rig object
        _XRRig.SetActive(false);

        // set UI button states
        _vrButton.SetState(VRButton.State.Off);
        _recButton.interactable = false;
        SetXrFlag(false);
    }

    private void SetXrFlag(bool value)
    {
        isXRactive = value;
        XRStatusToggled?.Invoke(isXRactive);
    }
}
