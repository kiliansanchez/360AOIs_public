using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class Timeline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public static Timeline Instance;

    public UnityEvent<long> OnCurrentFrameChange;

    private Scrubber _scrubber;
    public RectTransform _scrubberRect;
    
    public TMPro.TextMeshProUGUI _startFrameLabel;
    public TMPro.TextMeshProUGUI _endFrameLabel;

    public int FrameCount;// { get; private set; }
    public float Width /*{ get; private set; }*/;
    public float Height { get; private set; }

    public float ScrollScale { get; private set; } = 0.03f;
    public float ZoomFactor { get; private set; } = 1;

    public float StartFrame { get; private set; } = 0;
    public float VideoStartFrame { get; private set; } = -1;
    public float EndFrame { get; private set; }
    public float WidthPerFrame /*{ get; private set; }*/;
    public long CurrentFrame /*{ get; private set; }*/ = 0;
    public bool IsHovered { get; private set; } = false;
    public bool BlockedEdgeSnapping { get; private set; } = false;

    private const float TimeBetweenScreenChangeCalculations = 0.5f;
    private float _lastScreenChangeCalculationTime = 0;


    private void OnRectTransformDimensionsChange()
    {
        //if (Time.time - _lastScreenChangeCalculationTime < TimeBetweenScreenChangeCalculations)
        //    return;

        //_lastScreenChangeCalculationTime = Time.time;

        if (!VideoManager.IsInitialized)
        {
            return;
        }

        Debug.Log($"Window dimensions changed to {Screen.width}x{Screen.height}");

        var rectTransform = GetComponent<RectTransform>();
        Width = rectTransform.rect.width;
        Height = rectTransform.rect.height;
        ChangeTimelineScale(0);
    }


    void Awake()
    {
        Instance = this;
        _scrubber = _scrubberRect.GetComponent<Scrubber>();
        _lastScreenChangeCalculationTime = Time.time;
    }

    // Start is called before the first frame update
    void Start()
    {

        var rectTransform = GetComponent<RectTransform>();
        Width = rectTransform.rect.width;
        Height = rectTransform.rect.height;

        VideoManager.Instance.OnNewFrame.AddListener(OnNewFrameInVideoPlayer);
        VideoManager.Instance.OnFrameCountChange.AddListener(OnVideoPlayerNewFrameCount);
        VideoManager.Instance.VideoManagerInitialized.AddListener(OnVideoManagerInitialized);
    }

    void OnVideoManagerInitialized()
    {

        SetStartFrame(VideoStartFrame);
        SetEndFrame(VideoManager.Instance.MaxFrame);

        WidthPerFrame = Width / (EndFrame - StartFrame);

        GameObject.Find("Video Controls").GetComponent<CanvasGroup>().alpha = 1;
    }


    void OnNewFrameInVideoPlayer(long newframe)
    {

        if (IsHovered && Input.GetMouseButton(0))
        {
            return;
        }

        if (newframe != CurrentFrame)
        {
            SetCurrentFrame(newframe, true, false);
        }
    }

    void OnVideoPlayerNewFrameCount(ulong newframecount)
    {
        Debug.Log(newframecount);
        FrameCount = (int)(newframecount);
        Debug.LogWarning("Frame Count changed");
        SyncTimelineWithVideo();
        ChangeTimelineScale(0);
    }

    private void Update()
    {
        // if the videoplayer is playing and the timeline is zoomed, the scrubber can overshoot the timeline
        // during playback the timeline and scrubber need to account for this overshoot
        if ((_scrubberRect.anchoredPosition.x >= Width || _scrubberRect.anchoredPosition.x < -1) && !(CurrentFrame >= FrameCount) && VideoManager.VideoPlayer.isPlaying)
        {
            FixScrubberOvershoot();
        }

        // while hovering the timewheeling scrolling rescales the timeline
        else if (IsHovered && Input.mouseScrollDelta.y != 0 && !Input.GetMouseButton(0))
        {
            ChangeTimelineScale(Input.mouseScrollDelta.y * ScrollScale);
        }

    }


    public void FixScrubberOvershoot()
    {
        _scrubberRect.GetComponent<Scrubber>().SetAnchoredX(0);

        SetStartFrame(CurrentFrame);
        SetEndFrame(Mathf.Min(FrameCount, StartFrame + Width / WidthPerFrame));

        WidthPerFrame = Width / (EndFrame - StartFrame);
    }


    public void ChangeTimelineScale(float delta_zoom)
    {

        ZoomFactor = Mathf.Clamp(ZoomFactor + delta_zoom, 1, 10);

        if (ZoomFactor != 1f)
        {
            var calculated_start_frame = Mathf.Clamp(Mathf.Round(CurrentFrame * ZoomFactor - CurrentFrame), 0, Mathf.Max(0, CurrentFrame - 10));
            SetStartFrame(calculated_start_frame);

            if (StartFrame < VideoStartFrame)
            {
                SetStartFrame(VideoStartFrame);
            }

            var calculated_end_frame = Mathf.Clamp(Mathf.Round(FrameCount + (FrameCount - CurrentFrame) * (1 - ZoomFactor)), CurrentFrame + 10, FrameCount);
            SetEndFrame(calculated_end_frame);

            if (EndFrame > FrameCount)
            {
                SetEndFrame(FrameCount);
            }
        }
        else
        {
            SetStartFrame(VideoStartFrame);
            SetEndFrame(FrameCount);
        }

        WidthPerFrame = Width / (EndFrame - StartFrame);

        SetCurrentFrame(CurrentFrame, true, false);
    }

    public void SyncTimelineWithVideo()
    {
        if (VideoManager.VideoPlayer.frame != CurrentFrame)
        {
            // old way - syncing video to timeline
            // Debug.Log("Syncing + " + VideoManager.VideoPlayer.frame + " to " + CurrentFrame);
            // StartCoroutine(VideoManager.Instance.SeekFrameInVideo(CurrentFrame));

            Debug.Log("Syncing " + CurrentFrame + " to " + VideoManager.VideoPlayer.frame);
            SetCurrentFrame(VideoManager.VideoPlayer.frame, true, false);
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.transform == this.transform)
        {
            IsHovered = true;
        }
    
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        // if user intended to set scrubber to very end of timeline make scrubber snap to ends of timeline
        if (Input.GetMouseButton(0) && !BlockedEdgeSnapping)
        {
            if (transform.InverseTransformPoint(eventData.position).x < 0)
            {
                Debug.Log("Snap is setting to frame " + StartFrame);
                SetCurrentFrame((long)StartFrame, true, true);
  
            }
            else if (transform.InverseTransformPoint(eventData.position).x > Width)
            {
                Debug.Log("Snap is setting to frame " + EndFrame);
                SetCurrentFrame((long)EndFrame, true, true);
            }
        }

        IsHovered = false;
    }

    public void SetBlockEdgeSnapping(GameObject child, bool value)
    {
        if (child.transform.IsChildOf(transform))
        {
            BlockedEdgeSnapping = value;
        }
    }

    public void HoverHandoverWithChildren(GameObject child, bool hover)
    {
        if (child.transform.IsChildOf(this.transform))
        {
            IsHovered = hover;
        }
    }


    public void SetCurrentFrame(RectTransform childrect, bool updatescrubber, bool updatevideo)
    {
        if (!childrect.transform.IsChildOf(transform))
        {
            return;
        }

        var newframe = (long)Mathf.Round((childrect.anchoredPosition.x / WidthPerFrame) + StartFrame);

        SetCurrentFrame(newframe, updatescrubber, updatevideo);

    }

    public void SetCurrentFrame(long newframe, bool updatescrubber, bool updatevideo)
    {
        CurrentFrame = newframe;
       

        if (updatescrubber)
        {
            _scrubber.SetAnchoredX(CurrentFrame * WidthPerFrame - StartFrame * WidthPerFrame);
        }

        if (updatevideo)
        {
            // if video isnt insync with scrubber...
            if (CurrentFrame != VideoManager.Instance.CurrentFrame)
            {

                OnCurrentFrameChange?.Invoke(CurrentFrame);
                Debug.Log("Timeline is sending frame " + CurrentFrame);
                // ... sync video Frame to _currentframe based on scrubber
                //StartCoroutine(VideoManager.Instance.SeekFrameInVideo(CurrentFrame));
            }
        }
    }



    private void SetStartFrame(float frame)
    {
        if (frame < 0)
        {
            return;
        }

        if (frame < VideoStartFrame || frame > FrameCount)
        {
            Debug.LogWarning("Timeline Start Frame set out of bounds");
        }

        StartFrame = frame < VideoStartFrame ? VideoStartFrame : frame;
        _startFrameLabel.text = frame.ToString();
        _startFrameLabel.ForceMeshUpdate(true, true);
    }

    public void SetVideoStartFrame(float frame)
    {
        VideoStartFrame = frame;
        ChangeTimelineScale(0);
    }

    private void SetEndFrame(float frame)
    {

        if (frame < VideoStartFrame || frame > FrameCount)
        {
            Debug.LogError("Timeline End Frame set out of bounds");
        }

        EndFrame = frame;
        _endFrameLabel.text = frame.ToString();
    }
}
