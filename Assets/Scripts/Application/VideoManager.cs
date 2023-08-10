using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.Events;

public class VideoManager : MonoBehaviour
{

    public static VideoManager Instance;

    public UnityEvent<long> OnNewFrame = new();
    public UnityEvent<ulong> OnFrameCountChange = new();
    public UnityEvent VideoManagerInitialized = new();

    public RenderTexture TargetTexture;
    public Material SkyboxMaterial;

    public static VideoPlayer VideoPlayer { get; private set; }
    public static bool IsInitialized { get; private set; } = false;

    private static bool _isSeeking = false;

    // if video was playing but is paused by other class, it sets interruptedPlayback to true;
    public static bool interruptedPlayback = false;


    public long CurrentFrame; // { get; private set; }
    public ulong MaxFrame;


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
        Timeline.Instance.OnCurrentFrameChange.AddListener(OnNewFrameInTimeline);
        VideoLoader.Instance.OnVideoLoaded.AddListener(OnVideoLoaded);
    }

    
    void OnVideoPlayerError(VideoPlayer source, string message)
    {
        Debug.Log(message);
    }

    public void OnVideoLoaded(string path)
    {
        VideoPlayer.url = path;
        VideoPlayer.Prepare();    
    }

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

        MaxFrame = VideoPlayer.frameCount;
        OnFrameCountChange?.Invoke(MaxFrame);

        VideoManagerInitialized?.Invoke();
        IsInitialized = true;
    }

    IEnumerator PlayUntilFrameOne()
    {
        VideoPlayer.Play();
        yield return new WaitUntil(() => VideoPlayer.frame >= 0);
        VideoPlayer.Pause();
    }

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

        if (MaxFrame != VideoPlayer.frameCount)
        {
            MaxFrame = VideoPlayer.frameCount;
            OnFrameCountChange?.Invoke(MaxFrame);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayPause();
        }

        if (!VideoPlayer.isPlaying && Input.GetKeyDown(KeyCode.RightArrow) && !_isSeeking && CurrentFrame < (long)MaxFrame)
        {
            StartCoroutine(SeekFrameInVideo(CurrentFrame + 1));
        }

        if (SkyboxMaterial.mainTexture == null)
        {
            SkyboxMaterial.mainTexture = TargetTexture;
        }
    }

    private void OnNewFrameReady(VideoPlayer source, long frameIdx)
    {

        if (Timeline.Instance.VideoStartFrame == -1)
        {
            Timeline.Instance.SetVideoStartFrame(frameIdx);
        }

        if (frameIdx == Timeline.Instance.CurrentFrame && !VideoPlayer.isPlaying)
        {
            Debug.Log("Frame ID: " + frameIdx);
            Debug.Log("Setting Texture manually");
            Graphics.Blit(source.texture, TargetTexture);
            //_videoTexture = source.texture;
        }
    }

    void SeekCompleted(VideoPlayer source)
    {
        Debug.Log("Seeking completed to: " + VideoPlayer.frame);
        _isSeeking = false;
    }

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

    // Toggle the play/pause state of the video when the Play/Pause button is clicked
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

}
