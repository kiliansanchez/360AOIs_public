using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// Implements functionality of timeline. Allows timeline to be scrubbed and to zoom in on the timeline to be able to
/// move playhead with more precision to better place keyframes.
/// </summary>
public class Timeline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public static Timeline Instance { get; private set; }

    public UnityEvent<long> OnTargetFrameChange;

    private Playhead _playhead;
    public RectTransform PlayheadRect;
    
    public TMPro.TextMeshProUGUI StartFrameLabel;
    public TMPro.TextMeshProUGUI EndFrameLabel;

    public float Width { get; private set; }
    public float Height { get; private set; }

    public float ScrollScale { get; private set; } = 0.03f;
    public float ZoomFactor { get; private set; } = 1;

    public float StartFrame { get; private set; } = 0;
    public float VideoStartFrame { get; private set; } = -1;
    public float EndFrame { get; private set; }
    public float WidthPerFrame { get; private set; }
    public long TargetFrame { get; private set; } = 0;
    public bool IsHovered { get; private set; } = false;
    public bool BlockedEdgeSnapping { get; private set; } = false;

    //private const float TimeBetweenScreenChangeCalculations = 0.5f;
    //private float _lastScreenChangeCalculationTime = 0;


    private void OnRectTransformDimensionsChange()
    {
        //if (Time.time - _lastScreenChangeCalculationTime < TimeBetweenScreenChangeCalculations)
        //    return;

        //_lastScreenChangeCalculationTime = Time.time;

        if (!VideoManager.IsInitialized)
        {
            return;
        }

        //Debug.Log($"[Timeline] Window dimensions changed to {Screen.width}x{Screen.height}");

        var rectTransform = GetComponent<RectTransform>();
        Width = rectTransform.rect.width;
        Height = rectTransform.rect.height;
        ChangeTimelineScale(0);
    }


    void Awake()
    {
        Instance = this;
        _playhead = PlayheadRect.GetComponent<Playhead>();
        //_lastScreenChangeCalculationTime = Time.time;
    }

    // Start is called before the first frame update
    void Start()
    {

        var rectTransform = GetComponent<RectTransform>();
        Width = rectTransform.rect.width;
        Height = rectTransform.rect.height;

        VideoManager.Instance.OnNewFrame.AddListener(OnNewFrameInVideoPlayer);
        VideoManager.Instance.OnFrameCountChange.AddListener(OnVideoPlayerNewFrameCount);
        VideoManager.Instance.OnVideoManagerInitialized.AddListener(OnVideoManagerInitialized);

        EyeRecorder.OnRecordingToggled.AddListener(OnRecordingToggled);
    }

    /// <summary>
    /// Callback for Eyerecorders RecordingToggled Event. Resets video and scrubber to 0 when recording starts.
    /// </summary>
    /// <param name="is_recording">Bool; True if recording is running.</param>
    void OnRecordingToggled(bool is_recording)
    {
        if (is_recording)
        {
            Timeline.Instance.SetTargetFrame(0, true, true);
        }
    }

    /// <summary>
    /// Callback for VideoManagers VideoManagerInitialized Event. When videomanager is done loading
    /// the timeline can set the first and last frame.
    /// </summary>
    void OnVideoManagerInitialized()
    {

        SetStartFrame(VideoStartFrame);
        SetEndFrame(VideoManager.Instance.FrameCount);

        WidthPerFrame = Width / (EndFrame - StartFrame);

        GameObject.Find("Video Controls").GetComponent<CanvasGroup>().alpha = 1;
    }

    /// <summary>
    /// Callback for VideoManagers OnNewFrame event. If theres a new frame displayed by the video and the timeline isnt currently
    /// being scrubbed by user, update playhead to new frame.
    /// </summary>
    /// <param name="newframe">New frame in VideoPlayer</param>
    void OnNewFrameInVideoPlayer(long newframe)
    {

        if (IsHovered && Input.GetMouseButton(0))
        {
            return;
        }

        if (newframe != TargetFrame)
        {
            SetTargetFrame(newframe, true, false);
        }
    }

    /// <summary>
    /// Callback for VideoManagers OnNewFrameCount event.
    /// The VideoPlayer might only know what exactly the last frame of the video is when playback approaches the end of the video.
    /// If the FrameCount changes, the timeline updates itself using SyncTimelineWithVideo and ChangeTimelineScale.
    /// </summary>
    /// <param name="newframecount"></param>
    void OnVideoPlayerNewFrameCount(ulong newframecount)
    {
        SyncTimelineWithVideo();
        ChangeTimelineScale(0);
    }

    private void Update()
    {
        // if the videoplayer is playing and the timeline is zoomed, the playhead can overshoot the timeline
        // during playback the timeline and playhead need to account for this overshoot
        if ((PlayheadRect.anchoredPosition.x > Width || PlayheadRect.anchoredPosition.x < -1) && !(TargetFrame >= (int)VideoManager.Instance.FrameCount))
        {
            FixPlayheadOvershoot();
        }

        // while hovering the timewheeling scrolling rescales the timeline
        else if (IsHovered && Input.mouseScrollDelta.y != 0 && !Input.GetMouseButton(0))
        {
            ChangeTimelineScale(Input.mouseScrollDelta.y * ScrollScale);
        }

    }

    /// <summary>
    /// Resets playheads position back to lefthand side of the timeline. 
    /// </summary>
    public void FixPlayheadOvershoot()
    {
        PlayheadRect.GetComponent<Playhead>().SetAnchoredX(0);

        SetStartFrame(TargetFrame);
        SetEndFrame(Mathf.Min((int)VideoManager.Instance.FrameCount, StartFrame + Width / WidthPerFrame));

        WidthPerFrame = Width / (EndFrame - StartFrame);
    }

    /// <summary>
    /// Handles "zooming" of timeline, adjusting start and endframe of timeline as well as playhead position based on new
    /// timeline scale.
    /// </summary>
    /// <param name="delta_zoom"></param>
    public void ChangeTimelineScale(float delta_zoom)
    {

        ZoomFactor = Mathf.Clamp(ZoomFactor + delta_zoom, 1, 10);

        if (ZoomFactor != 1f)
        {
            var calculated_start_frame = Mathf.Clamp(Mathf.Round(TargetFrame * ZoomFactor - TargetFrame), 0, Mathf.Max(0, TargetFrame - 10));
            SetStartFrame(calculated_start_frame);

            if (StartFrame < VideoStartFrame)
            {
                SetStartFrame(VideoStartFrame);
            }

            var calculated_end_frame = Mathf.Clamp(Mathf.Round((int)VideoManager.Instance.FrameCount + ((int)VideoManager.Instance.FrameCount - TargetFrame) * (1 - ZoomFactor)), TargetFrame + 10, (int)VideoManager.Instance.FrameCount);
            SetEndFrame(calculated_end_frame);

            if (EndFrame > (int)VideoManager.Instance.FrameCount)
            {
                SetEndFrame((int)VideoManager.Instance.FrameCount);
            }
        }
        else
        {
            SetStartFrame(VideoStartFrame);
            SetEndFrame((int)VideoManager.Instance.FrameCount);
        }

        WidthPerFrame = Width / (EndFrame - StartFrame);

        SetTargetFrame(TargetFrame, true, false);
    }

    /// <summary>
    /// Can be used to sync timeline target frame and playhead position to current frame of video.
    /// </summary>
    public void SyncTimelineWithVideo()
    {
        if (VideoManager.VideoPlayer.frame != TargetFrame)
        {
            // old way - syncing video to timeline
            // Debug.Log("Syncing + " + VideoManager.VideoPlayer.frame + " to " + CurrentFrame);
            // StartCoroutine(VideoManager.Instance.SeekFrameInVideo(CurrentFrame));

            Debug.Log("Syncing " + TargetFrame + " to " + VideoManager.VideoPlayer.frame);
            SetTargetFrame(VideoManager.VideoPlayer.frame, true, false);
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.transform == this.transform)
        {
            IsHovered = true;
        }
    
    }

    /// <summary>
    /// Handles snapping, meaning when user exits timeline on left or right side of timeline the playhead should snap
    /// to the start/end frame of timeline.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {

        // if user intended to set playhead to very end of timeline make playhead snap to ends of timeline
        if (Input.GetMouseButton(0) && !BlockedEdgeSnapping)
        {
            if (transform.InverseTransformPoint(eventData.position).x < 0)
            {
                Debug.Log("[Timeline] Snap is setting to frame " + StartFrame);
                SetTargetFrame((long)StartFrame, true, true);
  
            }
            else if (transform.InverseTransformPoint(eventData.position).x > Width)
            {
                Debug.Log("[Timeline] Snap is setting to frame " + EndFrame);
                SetTargetFrame((long)EndFrame, true, true);
            }
        }

        IsHovered = false;
    }

    /// <summary>
    /// Edge snapping might have to be disabled when children are currently handling input events that happen ontop of timeline.
    /// </summary>
    /// <param name="child">child that wants to disable/enable edge snapping.</param>
    /// <param name="value">bool, if true edge snapping is disabled.</param>
    public void SetBlockEdgeSnapping(GameObject child, bool value)
    {
        if (child.transform.IsChildOf(transform))
        {
            BlockedEdgeSnapping = value;
        }
    }

    /// <summary>
    /// Allows children of timeline to signal if timeline is hovered or not.
    /// </summary>
    /// <param name="child">signaling child.</param>
    /// <param name="hover">bool; whether or not timeline is being hovered.</param>
    public void HoverHandoverWithChildren(GameObject child, bool hover)
    {
        if (child.transform.IsChildOf(this.transform))
        {
            IsHovered = hover;
        }
    }

    /// <summary>
    /// Used by children of timeline to set target frame based on childs position on timeline.
    /// </summary>
    /// <param name="childrect">Rect of child that wants to set target frame.</param>
    /// <param name="updateplayhead">bool; true if playhead needs to be updated.</param>
    /// <param name="updatevideo">bool; true if video needs to be updated.</param>
    public void SetTargetFrame(RectTransform childrect, bool updateplayhead, bool updatevideo)
    {
        if (!childrect.transform.IsChildOf(transform))
        {
            return;
        }

        var newframe = (long)Mathf.Round((childrect.anchoredPosition.x / WidthPerFrame) + StartFrame);

        SetTargetFrame(newframe, updateplayhead, updatevideo);

    }

    /// <summary>
    /// Used to update timeline to a new frame.
    /// </summary>
    /// <param name="newframe">New target frame for timeline.</param>
    /// <param name="updateplayhead">True if playhead needs to be updated.</param>
    /// <param name="updatevideo">True if video needs to be updated.</param>
    public void SetTargetFrame(long newframe, bool updateplayhead, bool updatevideo)
    {
        TargetFrame = newframe;
       

        if (updateplayhead)
        {
            _playhead.SetAnchoredX(TargetFrame * WidthPerFrame - StartFrame * WidthPerFrame);
        }

        if (updatevideo)
        {
            // if video isnt insync with playhead...
            if (TargetFrame != VideoManager.Instance.CurrentFrame)
            {

                OnTargetFrameChange?.Invoke(TargetFrame);
                Debug.Log("[Timeline] Timeline is sending frame " + TargetFrame);
                // ... sync video Frame to _currentframe based on playhead
                //StartCoroutine(VideoManager.Instance.SeekFrameInVideo(CurrentFrame));
            }
        }
    }



    /// <summary>
    /// Sets start frame of timeline and changes label at the left of timeline to display new start frame. 
    /// Note: the StartFrame is not the first frame in the video, but the first frame of the video that is represented on the timeline.
    /// </summary>
    /// <param name="frame"></param>
    private void SetStartFrame(float frame)
    {
        if (frame < 0)
        {
            return;
        }

        if (frame < VideoStartFrame || frame > (int)VideoManager.Instance.FrameCount)
        {
            Debug.LogWarning("[Timeline] Start Frame set out of bounds");
        }

        StartFrame = frame < VideoStartFrame ? VideoStartFrame : frame;
        StartFrameLabel.text = frame.ToString();
        StartFrameLabel.ForceMeshUpdate(true, true);
    }

    /// <summary>
    /// Called by VideoManager when it has accurate information about what the first frame of the video is.
    /// Should maybe be handled by an event for better decoupling.
    /// </summary>
    /// <param name="frame">Id of first frame in video</param>
    public void SetVideoStartFrame(float frame)
    {
        VideoStartFrame = frame;
        ChangeTimelineScale(0);
    }

    /// <summary>
    /// Sets end frame of timeline and changes label at the right of timeline to display new endframe. 
    /// Note: The EndFrame is not the last frame in the video, but the last frame of the video that is represented on the timeline.
    /// </summary>
    /// <param name="frame"></param>
    private void SetEndFrame(float frame)
    {

        if (frame < VideoStartFrame || frame > (int)VideoManager.Instance.FrameCount)
        {
            Debug.LogError("[Timeline] End Frame set out of bounds");
        }

        EndFrame = frame;
        EndFrameLabel.text = frame.ToString();
    }
}
