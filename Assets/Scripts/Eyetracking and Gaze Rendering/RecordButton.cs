using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script attached to record button at the top right corner of UI. Handles sprite changes based on events.
/// </summary>
public class RecordButton : MonoBehaviour
{

    public Sprite _offSprite;
    public Sprite _onSprite;

    private UnityEngine.UI.Button _recButton;
    private UnityEngine.UI.Image _recButtonImage;


    void Start()
    {
        _recButton = GetComponent<UnityEngine.UI.Button>();
        _recButtonImage = _recButton.GetComponent<UnityEngine.UI.Image>();

        VRManager.OnXRStatusUpdate.AddListener(SetInteractabilityBasedOnVrStatus);
        EyeRecorder.OnRecordingToggled.AddListener(SetImageOnRecordingToggle);
    }

    /// <summary>
    /// Callback fro VRManagers XRStatusUpdate. Makes button interactable when VR is successfully enabled. 
    /// </summary>
    /// <param name="new_status">New status of VR.</param>
    void SetInteractabilityBasedOnVrStatus(VRManager.VRStatus new_status)
    {
        if (new_status == VRManager.VRStatus.Active)
        {
            _recButton.interactable = true;
        }
        else
        {
            _recButton.interactable = false;
        }
    }

    /// <summary>
    /// Callback for EyeRecorders RecordingToggled Event. Changes sprite based on whether recording is on or off.
    /// </summary>
    /// <param name="state"></param>
    public void SetImageOnRecordingToggle(bool state)
    {

        if (state)
        {
            _recButtonImage.sprite = _onSprite;
        }
        else
        {
            _recButtonImage.sprite = _offSprite;
        }     
    }
}
