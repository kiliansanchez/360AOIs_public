using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attatched to VR Button. Manages interactability as well as sprite changes for button.
/// </summary>
public class VRButton : MonoBehaviour
{

    public Sprite _loadingSprite;
    public Sprite _onSprite;
    public Sprite _errorSprite;
    public Sprite _offSprite;


    private UnityEngine.UI.Button _vrButton;
    private UnityEngine.UI.Image _vrButtonImage;


    // Start is called before the first frame update
    void Start()
    {
        _vrButton = GetComponent<UnityEngine.UI.Button>();
        _vrButtonImage = _vrButton.GetComponent<UnityEngine.UI.Image>();

        VRManager.OnXRStatusUpdate.AddListener(UpdateImageBasedOnVrManagerStatus);
        EyeRecorder.OnRecordingToggled.AddListener(UpdateInteractabilityBasedOnRecordingStatus);
    }

    /// <summary>
    /// Callback for Eyerecorders OnRecordingToggled-Event. Makes button uninteractable while recording is running.
    /// </summary>
    /// <param name="status">Whether or not recording is running.</param>
    private void UpdateInteractabilityBasedOnRecordingStatus(bool status)
    {
        if (status)
        {
            _vrButton.interactable = false;
        }
        else
        {
            _vrButton.interactable = true;
        }
    }

    /// <summary>
    /// Callback for VRManagers OnXRStatusUpdate Event. Changes sprite color based on status of VR-Framework.
    /// </summary>
    /// <param name="status">New status of VR-Framework</param>
    private void UpdateImageBasedOnVrManagerStatus(VRManager.VRStatus status)
    {
        switch (status)
        {
            case VRManager.VRStatus.Inactive:
                _vrButtonImage.sprite = _offSprite;
                break;
            case VRManager.VRStatus.Loading:
                _vrButtonImage.sprite = _loadingSprite;
                break;
            case VRManager.VRStatus.Active:
                _vrButtonImage.sprite = _onSprite;
                break;
            case VRManager.VRStatus.Error:
                _vrButtonImage.sprite = _errorSprite;
                break;
            default:
                break;
        }
    }
}
