using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordButton : MonoBehaviour
{
    // Start is called before the first frame update

    public Sprite _offSprite;
    public Sprite _onSprite;

    private UnityEngine.UI.Button _recButton;
    private UnityEngine.UI.Image _recButtonImage;

    // reference to VRButton, so that it can be disabled when recording is running;
    public UnityEngine.UI.Button _vrButton;

    public static bool IsRecording = false;

    public enum State
    {
        Off,
        On
    }

    void Start()
    {
        _recButton = GetComponent<UnityEngine.UI.Button>();
        _recButtonImage = _recButton.GetComponent<UnityEngine.UI.Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && VRManager.isXRactive)
        {
            ToggleRecording();
        }
    }

    public void ToggleRecording()
    {
        if (IsRecording)
        {
            IsRecording = false;
            SetState(State.Off);
            _vrButton.interactable = true;
            EyeRecorder.Recorder.FinishRecording();

            VideoManager.VideoPlayer.Pause();
        }
        else
        {
            SetState(State.On);
            _vrButton.interactable = false;

            //VideoManager.VideoPlayer.Stop();
            Timeline.Instance.SetCurrentFrame(0, true, true);

            IsRecording = true;          
            
            EyeRecorder.Recorder.StartRecording();
            VideoManager.VideoPlayer.Play();
        }
    }

    public void SetState(State state)
    {
        switch (state)
        {
            case State.Off:
                _recButtonImage.sprite = _offSprite;
                break;

            case State.On:
                _recButtonImage.sprite = _onSprite;
                break;

            default:
                break;
        }
    }
}
