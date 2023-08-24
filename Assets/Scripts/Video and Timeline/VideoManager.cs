using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// This class handles the videoplayer component and its events.
/// </summary>
public class VideoManager : MonoBehaviour
{

    public static VideoManager Instance;

    public Slider VolumeSlider;
    public UnityEvent<long> OnNewFrame { get; private set; } = new();
    public UnityEvent<ulong> OnFrameCountChange { get; private set; } = new();
    public UnityEvent OnVideoManagerInitialized { get; private set; } = new();

    public RenderTexture TargetTexture;
    public Material SkyboxMaterial;

    public static VideoPlayer VideoPlayer { get; private set; }
    public static bool IsInitialized { get; private set; } = false;

    private static bool _isSeeking = false;


    public long CurrentFrame { get; private set; }
    public ulong FrameCount { get; private set; }


    private void Awake()
    {
        Instance = this;
        // Get a reference to the VideoPlayer component on the same GameObject
        VideoPlayer = GetComponent<VideoPlayer>();

        //UnityEngine.Assertions.Assert.AreEqual(TargetTexture, VideoPlayer.targetTexture);

        VideoPlayer.seekCompleted += SeekCompleted;
        VideoPlayer.sendFrameReadyEvents = true;
        VideoPlayer.frameReady += OnNewFrameReady;

        VideoPlayer.errorReceived += OnVideoPlayerError;
        VideoPlayer.prepareCompleted += OnVideoPlayerPrepareCompleted;
    }

    void Start()
    {   
        Timeline.Instance.OnTargetFrameChange.AddListener(OnNewFrameInTimeline);
        VideoLoader.Instance.OnVideoLoaded.AddListener(OnVideoLoaded);
        EyeRecorder.OnRecordingToggled.AddListener(OnRecordingToggled);
    }

    /// <summary>
    /// Callback for VideoPlayers errorReceived event.
    /// If VideoPlayer component encounters and error its being logged.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="message"></param>
    void OnVideoPlayerError(VideoPlayer source, string message)
    {
        Debug.LogError("[VideoManager] " + message);
    }

    /// <summary>
    /// Callback for when video has been loaded by videoloader. Starts preparing video player component with
    /// selected video.
    /// </summary>
    /// <param name="path"></param>
    public void OnVideoLoaded(string path)
    {
        VideoPlayer.url = path;
        VideoPlayer.Prepare();    
    }

    /// <summary>
    /// Callback for videoplayer components prepareCompleted event. When preparation is completed, resolution of video is 
    /// read. Then a new render texture with correct resoltion is created and assigned to skybox material.
    /// </summary>
    /// <param name="source"></param>
    void OnVideoPlayerPrepareCompleted(VideoPlayer source)
    {

        var video_width = VideoPlayer.texture.width;
        var video_height = VideoPlayer.texture.height;

        TargetTexture = new RenderTexture(video_width, video_height, 0);

        SkyboxMaterial.mainTexture = TargetTexture;
        VideoPlayer.targetTexture = TargetTexture;


        //VideoPlayer.Play();
        //VideoPlayer.Pause();

        StartCoroutine(PlayUntilFrameOne());

        FrameCount = VideoPlayer.frameCount;
        OnFrameCountChange?.Invoke(FrameCount);

        OnVideoManagerInitialized?.Invoke();
        IsInitialized = true;
    }

    /// <summary>
    /// Plays the video until first valid frame right after being loaded. Needed to find first frame of video, because not all videos
    /// start with frame 0.
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayUntilFrameOne()
    {
        VideoPlayer.Play();
        yield return new WaitUntil(() => VideoPlayer.frame >= 0);
        VideoPlayer.Pause();
    }

    /// <summary>
    /// Callback for timelines NewFrameInTimeline event. When timeline is targeting a new frame the video manager starts seeking
    /// that frame. 
    /// </summary>
    /// <param name="newframe"></param>
    void OnNewFrameInTimeline(long newframe)
    {
        if (newframe != CurrentFrame)
        {
            StartCoroutine(SeekFrameInVideo(newframe));                  
        }
    }

    private void Update()
    {

        if (!IsInitialized)
        {
            return;
        }

        if (CurrentFrame != VideoPlayer.frame)
        {
            CurrentFrame = VideoPlayer.frame;

            if (!_isSeeking)
            {
                OnNewFrame?.Invoke(CurrentFrame);
            }       
        }

        if (FrameCount != VideoPlayer.frameCount)
        {
            FrameCount = VideoPlayer.frameCount;
            OnFrameCountChange?.Invoke(FrameCount);
        }

        if (Input.GetKeyDown(KeyCode.Space) && Listable.IsListEntryBeingEdited == false)
        {
            PlayPause();
        }

        if (!VideoPlayer.isPlaying && Input.GetKeyDown(KeyCode.RightArrow) && !_isSeeking && CurrentFrame < (long)FrameCount)
        {
            StartCoroutine(SeekFrameInVideo(CurrentFrame + 1));
        }

        if (SkyboxMaterial.mainTexture == null)
        {
            SkyboxMaterial.mainTexture = TargetTexture;
        }
    }

    /// <summary>
    /// Callback for videoplayer newframeready event. 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="frameIdx"></param>
    private void OnNewFrameReady(VideoPlayer source, long frameIdx)
    {

        if (Timeline.Instance.VideoStartFrame == -1)
        {
            Timeline.Instance.SetVideoStartFrame(frameIdx);
        }

        if (frameIdx == Timeline.Instance.TargetFrame && !VideoPlayer.isPlaying)
        {
            Debug.Log("Frame ID: " + frameIdx);
            Debug.Log("Setting Texture manually");
            Graphics.Blit(source.texture, TargetTexture);
            //_videoTexture = source.texture;
        }
    }

    /// <summary>
    /// Sets _isSeeking to false when seeking is completed.
    /// </summary>
    /// <param name="source"></param>
    void SeekCompleted(VideoPlayer source)
    {
        Debug.Log("Seeking completed to: " + VideoPlayer.frame);
        _isSeeking = false;
    }

    /// <summary>
    /// Function to seek frame in video. Stutters towards frame by seeking to a frame before the target frame and then
    /// stepping forward. This is done because seeking to a frame doesn't guarantee that the frame will be displayed in the video-texture.
    /// Stuttering towards the frame makes the video display the corret frame 99% of times.
    /// </summary>
    /// <param name="frame">target frame</param>
    /// <returns></returns>
    public IEnumerator SeekFrameInVideo(long frame)
    {

        if (VideoPlayer.isPlaying)
        {
            VideoPlayer.Pause();
        }
        
        if (_isSeeking)
        {
            Debug.Log("Waiting for previous seek to complete");
            yield return new WaitUntil(() => !_isSeeking);
        }

        if (frame == VideoPlayer.frame + 1)
        {
            VideoPlayer.StepForward();
        }
        else if (frame == 0)
        {
            _isSeeking = true;
            VideoPlayer.frame = frame;
            Debug.Log("Seeking");
        }
        else
        {
            _isSeeking = true;
            VideoPlayer.frame = frame-1;
            Debug.Log("Seeking");
            yield return new WaitUntil(() => !_isSeeking);

            _isSeeking = true;
            yield return new WaitForSeconds(0.200f);
            VideoPlayer.StepForward();
            _isSeeking = false;
        }

    }


    /// <summary>
    /// Toggle the play/pause state of the video when the Play/Pause button is clicked
    /// </summary>
    public void PlayPause()
    {
        // Check if the video is currently playing
        if (VideoPlayer.isPlaying)
        {
            // If the video is playing, pause it
            VideoPlayer.Pause();
            //_timeline.SyncTimelineWithVideo();
        }
        else
        {
            // If the video is not playing, play it
            VideoPlayer.Play();
        }
    }

    /// <summary>
    /// Callback for EyeRecorders RecordingToggled event. When recording starts start video playback.
    /// When recording ends stop video playback.
    /// </summary>
    /// <param name="is_recording"></param>
    private void OnRecordingToggled(bool is_recording)
    {
        if (is_recording)
        {
            VideoPlayer.Play();
        }
        else
        {
            VideoPlayer.Pause();
        }
    }

    /// <summary>
    /// Callback for volume slider in UI. Changes volume based on sliders value.
    /// </summary>
    public void OnVolumeSliderValueChanged()
    {
        VideoPlayer.SetDirectAudioVolume(0, VolumeSlider.value);
    }
}
