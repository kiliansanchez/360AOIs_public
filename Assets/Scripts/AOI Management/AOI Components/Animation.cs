using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// This script does a lot of things and should probably be refactored into multiple classes.
///  a) Creates & Updates Keyframes for position, size and shape of AOI
///     Does this by listening to DraggableAroundCameras and Resizables events
///  b) Calculates position, size and shape for Frames between Keyframes using interpolation
///  c) Animates object based on current frame of video by setting position, size and shape
///     according to calculated frames
/// </summary>
/// 
public class Animation : AOIComponent
{

    public GameObject KeyframePrefab;

    private bool _objectIsBeingChanged = false;

    protected Resizable _resizeComponent;
    protected DraggableAroundCamera _movementComponent;

    [System.Serializable]
    public class FrameData
    {
        public GameObject UIrepresentation;
        public long VideoFrame;

        public Vector3[] Vertices;

        public Vector3 Position;
        public Quaternion Rotation;

        public bool Visibility = true;
    }

    public List<FrameData> Keyframes {  get; private set; } = new();
    public List<FrameData> InterpolatedFrames { get; private set; } = new();


    private void Awake()
    {

        if (TryGetComponent(out DraggableAroundCamera movementComponent))
        {
            _movementComponent = movementComponent;
            _movementComponent.MovementStarted.AddListener(ObjectChangeStarted);
            _movementComponent.MovementCompleted.AddListener(UpdateOrCreateKeyframeOnObjectChangeCompleted);
        }

        if (TryGetComponent(out Resizable resizeComponent))
        {
            _resizeComponent = resizeComponent;
            _resizeComponent.ResizingStarted.AddListener(ObjectChangeStarted);
            _resizeComponent.ResizingCompleted.AddListener(UpdateOrCreateKeyframeOnObjectChangeCompleted);
        }
    }

    protected override void Start()
    {

        // check keyframe prefab is there
        UnityEngine.Assertions.Assert.IsNotNull(KeyframePrefab);

        SetupRightClickMenu(true);

        if (Keyframes.Count == 0)
        {
            CreateKeyframe();
        }

        base.Start();
    }

    /// <summary>
    /// Adds visibility toggle option to right click menu of AOI
    /// </summary>
    /// <param name="currentVisibility"> Information whether or not AOI is currently visible to eyetracking raycast </param>
    void SetupRightClickMenu(bool currentVisibility)
    {  
        if (TryGetComponent(out RightClickMenu menu))
        {
            string labelToAdd = currentVisibility ? "Set Invisible": "Set Visible";
            string labelToRemove = currentVisibility ? "Set Visible" : "Set Invisible";

            menu.RemoveItem(labelToRemove);
            menu.RemoveItem(labelToAdd);
            menu.AddItem(labelToAdd, ToggleKeyframeVisibilityFromRightClickMenu);
        }
        else
        {
            Debug.LogError("[Animation] No right click menu");
        }
    }


    /// <summary>
    /// Callback for AOI-Visibility Button in right-click menu of AOI.
    /// </summary>
    void ToggleKeyframeVisibilityFromRightClickMenu()
    {

        GetComponent<RightClickMenu>().HideMenu();

        bool isCurrentFrameInKeyframes = IsFrameInKeyframes(Timeline.Instance.TargetFrame);

        // before creating or updating a keyframe, first gotta find out the current visibility, either from
        // keyframes or interpolated frames

        var Frames = isCurrentFrameInKeyframes ? Keyframes : InterpolatedFrames;
        int index = Frames.FindIndex(item => item.VideoFrame == Timeline.Instance.TargetFrame);
        bool currentVisibility = Frames[index].Visibility;


        if (!isCurrentFrameInKeyframes)
        {
            CreateKeyframe();
        }

        SetKeyframeVisibility(Timeline.Instance.TargetFrame, !currentVisibility);         
        SetupRightClickMenu(!currentVisibility);
    }

    private void Update()
    {

        // CONSIDER USING FIXED UPDATE FOR ANIMATION SINCE UPDATE ONLY RUNS AT 90FPS WHEN PRO EYE IS CONNECTED 

        if (_objectIsBeingChanged)
        {
            return;
        }  

        if (IsObjectInCorrectState())
        {
            return;
        }

        SetObjectsStateToFrameData();
        
    }


    /// <summary>
    /// Checks if objects state already matches correct state for current frame.
    /// Uses Timelines TargetFrame rather than VideoPlayers current frame, because current frame jumps around due to scrubbing implementation.
    /// </summary>
    /// <returns> Returns true of object is already in correct state, false if not. </returns>
    bool IsObjectInCorrectState()
    {

        bool isCurrentFrameInKeyframes = IsFrameInKeyframes(Timeline.Instance.TargetFrame);
        bool isCurrentFrameInInterpolatedFrames = InterpolatedFrames.Any(item => item.VideoFrame == Timeline.Instance.TargetFrame);

        if (!(isCurrentFrameInKeyframes || isCurrentFrameInInterpolatedFrames))
        {
            CalculateInterpolatedFrames();
            return true;
        }

        var Frames = isCurrentFrameInKeyframes ? Keyframes : InterpolatedFrames;

        int i = Frames.FindIndex(item => item.VideoFrame == Timeline.Instance.TargetFrame);

        bool vertices = true;
        if (_resizeComponent != null)
        {
            //vertices = Enumerable.SequenceEqual(Frames[i].Vertices, _resizeComponent.Mesh.vertices);

            for (int index = 0; index < Frames[i].Vertices.Length; index++)
            {
                vertices = vertices && (Frames[i].Vertices[index] == _resizeComponent.Mesh.vertices[index]);
            }

        }

        bool pos = Frames[i].Position == transform.position;
        bool rot = Frames[i].Rotation == transform.rotation;

        bool visibility;
        if (Frames[i].Visibility)
        {
            visibility = CompareTag("AOI");
        }
        else
        {
            visibility = CompareTag("AOI_Invisible");
        }

        return vertices && pos && rot && visibility;
    }


    /// <summary>
    /// Sets AOIs state to state defined in Keyframe or InterpolatedFrame.
    /// </summary>
    void SetObjectsStateToFrameData()
    {

        var frame = Timeline.Instance.TargetFrame;

        bool isCurrentFrameInKeyframes = IsFrameInKeyframes(frame);
        bool isCurrentFrameInInterpolatedFrames = InterpolatedFrames.Any(item => item.VideoFrame == frame);

        if (!(isCurrentFrameInKeyframes || isCurrentFrameInInterpolatedFrames))
        {
            Debug.LogError("Missing Frame at " + frame + ", should be impossible, logic error in code");
            return;
        }

        var Frames = isCurrentFrameInKeyframes ? Keyframes : InterpolatedFrames;
 
        int index = Frames.FindIndex(item => item.VideoFrame == frame);

        
        if (_resizeComponent != null)
        {
            // since re-centering the origin of the AOI after every resize introduces a rounding error, the keyframe/interpolated framedata for the vertices
            // is reassigned to the rounding error, so that when checking for equality between object state and frame data vertices dont break condition;
            var vertices_with_rounding_error = _resizeComponent.RecenterOriginBasedOnKeyframeData(Frames[index].Vertices);
            Array.Copy(vertices_with_rounding_error, 0, Frames[index].Vertices, 0, vertices_with_rounding_error.Length);
        }

        transform.SetPositionAndRotation(Frames[index].Position, Frames[index].Rotation);

        gameObject.tag = Frames[index].Visibility ? "AOI" : "AOI_Invisible";

        SetupRightClickMenu(Frames[index].Visibility);

    }


    /// <summary>
    /// Calculates Position, Rotation and Vertex positions for all frames based on Keyframes.
    /// </summary>
    void CalculateInterpolatedFrames()
    {
       
        if (Keyframes.Count == 0)
        {
            return;
        }

        Keyframes = Keyframes.OrderBy(item => item.VideoFrame).ToList();
        InterpolatedFrames = new();

        //var start = Keyframes[0].VideoFrame;
        //var end = Keyframes[^1].VideoFrame;

        var start = 0;
        var end = (int)VideoManager.Instance.FrameCount;

        for (long frame = start; frame <= end; frame++)
        {

            if (IsFrameInKeyframes(frame))
            {
                continue;
            }

            //get list of all frames in keyframedata
            var frames = Keyframes.Select(a => a.VideoFrame);

            //check if theres two frames to animate between
            long closestKeyframeFrame = frames.Aggregate((x, y) => Mathf.Abs(x - frame) < Mathf.Abs(y - frame) ? x : y);
            int index = GetKeyframeIndexByFrame(closestKeyframeFrame);

            FrameData a, b; //closest keyframe left and right of current frame

            FrameData interpolatedFrame = new();
            interpolatedFrame.VideoFrame = frame;

            if (closestKeyframeFrame > frame)
            {
                a = Keyframes.ElementAtOrDefault(index - 1);
                b = Keyframes.ElementAtOrDefault(index);
            }
            else
            {
                a = Keyframes.ElementAtOrDefault(index);
                b = Keyframes.ElementAtOrDefault(index + 1);
            }

            // only existing keyframe to the left, not to the right
            if (a != default && b == default)
            {
                interpolatedFrame.Visibility = a.Visibility;

                if (_resizeComponent != null)
                {
                    Vector3[] interpolated_vertices = new Vector3[a.Vertices.Length];

                    for (int i = 0; i < a.Vertices.Length; i++)
                    {
                        interpolated_vertices[i] = a.Vertices[i];
                    }

                    interpolatedFrame.Vertices = interpolated_vertices;
                }

                interpolatedFrame.Position = a.Position;
                interpolatedFrame.Rotation = a.Rotation;
            }

            // only exisitng keyframe to the right, not to the left
            else if (a == default && b != default)
            {
                interpolatedFrame.Visibility = b.Visibility;

                if (_resizeComponent != null)
                {
                    Vector3[] interpolated_vertices = new Vector3[b.Vertices.Length];

                    for (int i = 0; i < b.Vertices.Length; i++)
                    {
                        interpolated_vertices[i] = b.Vertices[i];
                    }

                    interpolatedFrame.Vertices = interpolated_vertices;
                }

                interpolatedFrame.Position = b.Position;
                interpolatedFrame.Rotation = b.Rotation;
            }

            //existing keyframe to either side to interpolate between
            else
            {
                var frame_a = a.VideoFrame;
                var frame_b = b.VideoFrame;

                float t = (frame + 1 - frame_a) / ((float)(frame_b - frame_a));

                interpolatedFrame.Visibility = a.Visibility;


                if (_resizeComponent != null)
                {
                    Vector3[] interpolated_vertices = new Vector3[a.Vertices.Length];

                    for (int i = 0; i < a.Vertices.Length; i++)
                    {
                        interpolated_vertices[i] = Vector3.Lerp(a.Vertices[i], b.Vertices[i], t);
                    }

                    interpolatedFrame.Vertices = interpolated_vertices;
                }

                interpolatedFrame.Position = Vector3.Lerp(a.Position, b.Position, t);
                interpolatedFrame.Rotation = Quaternion.Lerp(a.Rotation, b.Rotation, t);
            } 

            InterpolatedFrames.Add(interpolatedFrame);
        }
    
    }

    /// <summary>
    /// little bit unncessary, but here just in case in the future some other things need to happen on start of object change.
    /// </summary>
    void ObjectChangeStarted()
    {
        _objectIsBeingChanged = true;
    }



    /// <summary>
    /// Callback for resize and movement events. Creates or updates keyframe after user has completed object change.
    /// </summary>
    void UpdateOrCreateKeyframeOnObjectChangeCompleted()
    {

        bool frameAlreadyExists = IsFrameInKeyframes(Timeline.Instance.TargetFrame);

        if (frameAlreadyExists)
        {
            
            int index = GetKeyframeIndexByFrame(Timeline.Instance.TargetFrame);
            UpdateKeyframe(index);
;       }
        else
        {
            Debug.Log("Creating New Keyframe");
            CreateKeyframe();
        }

        //InterpolatedFrames = new();
        //CalculateInterpolatedFrames();

        _objectIsBeingChanged = false;
    }


    /// <summary>
    /// Creates new Keyframe at current frame of timeline.
    /// Creates both UI representation as well as keyframe data
    /// </summary>
    void CreateKeyframe()
    {
        FrameData newKeyframeData = new();

        newKeyframeData.UIrepresentation = Instantiate(KeyframePrefab);
        newKeyframeData.UIrepresentation.transform.SetParent(Timeline.Instance.transform, true);
        newKeyframeData.UIrepresentation.transform.localScale = Vector2.one;

        newKeyframeData.VideoFrame = Timeline.Instance.TargetFrame;

        if (newKeyframeData.UIrepresentation.TryGetComponent(out DisplayAsFrameOnTimeline frameComponent))
        {
            frameComponent.Frame = newKeyframeData.VideoFrame;
            frameComponent.GetComponent<RectTransform>().anchoredPosition = new Vector2(Timeline.Instance.TargetFrame * Timeline.Instance.WidthPerFrame - Timeline.Instance.StartFrame * Timeline.Instance.WidthPerFrame, 0);
        }


        if (newKeyframeData.UIrepresentation.TryGetComponent(out RightClickMenu rcmenu))
        {
            rcmenu.AddItem("Delete", DeleteKeyframeFromRightClickMenu);
        }


        Keyframes.Add(newKeyframeData);

        UpdateKeyframe(Keyframes.Count - 1);

        SetKeyframeVisibility(newKeyframeData.VideoFrame, gameObject.CompareTag("AOI"));

        Debug.Log("New Keyframe created successfully");
    }



    /// <summary>
    /// Callback for delete option in right click menu of keyframe right click menu
    /// </summary>
    void DeleteKeyframeFromRightClickMenu()
    {       
        int index = Keyframes.FindIndex(item => item.UIrepresentation == RightClickMenu.LastClickedObjectWithMenu);
        if (index == -1)
        {
            Debug.LogError("Keyframe to delete doesn't exist");
            return;
        }
        else
        {
            Keyframes.RemoveAt(index);
        }
        
        //Destroy(RightClickMenu.LastClickedObjectWithMenu.GetComponent<RightClickMenu>().Menu);
        Destroy(RightClickMenu.LastClickedObjectWithMenu);
        CalculateInterpolatedFrames();
    }



    /// <summary>
    /// Updates Keyframe at index i with objects current Position, Rotation and Vertex Information
    /// </summary>
    /// <param name="i"> Index of keyframe in list to be updated. </param>
    void UpdateKeyframe(int i /*index*/)
    {

        //if object is resizable get vertices, otherwise just get position and rotation;
        if (_resizeComponent != null)
        {
            Keyframes[i].Vertices = _resizeComponent.Mesh.vertices;
        }

        Keyframes[i].Position = transform.position;
        Keyframes[i].Rotation = transform.rotation;


        CalculateInterpolatedFrames();
    }


    /// <summary>
    /// Called by SaveAndLoadAOIs class when loading AOIs from csv.
    /// Stores passed list as keyframes, creates UI representations for all keyframes and calls CalculateInterpolatedFrames.
    /// </summary>
    /// <param name="loaded_keyframes"> List of loaded Keyframes that were stored in csv. </param>
    public void LoadKeyframes(List<FrameData> loaded_keyframes)
    {
        _objectIsBeingChanged = true;
        Keyframes = loaded_keyframes;

        foreach (var item in Keyframes)
        {
            item.UIrepresentation = Instantiate(KeyframePrefab);
            item.UIrepresentation.transform.SetParent(Timeline.Instance.transform, true);
            item.UIrepresentation.transform.localScale = Vector2.one;
            item.UIrepresentation.GetComponent<UnityEngine.UI.Image>().color = item.Visibility ? new Color(1, .8f, 0) : new Color(0, 0, 1);

            if (item.UIrepresentation.TryGetComponent(out DisplayAsFrameOnTimeline frameComponent))
            {
                frameComponent.Frame = item.VideoFrame;
                frameComponent.GetComponent<RectTransform>().anchoredPosition = new Vector2(item.VideoFrame * Timeline.Instance.WidthPerFrame - Timeline.Instance.StartFrame * Timeline.Instance.WidthPerFrame, 0);
            }


            if (item.UIrepresentation.TryGetComponent(out RightClickMenu rcmenu))
            {
                rcmenu.AddItem("Delete", DeleteKeyframeFromRightClickMenu);
            }

        }

        CalculateInterpolatedFrames();

        _objectIsBeingChanged = false;
    }


    protected override void OnDestroy()
    {
        foreach (var keyframe in Keyframes)
        {
            Destroy(keyframe.UIrepresentation);
        }

        //AOIManager.NewActiveAoiSubscribers -= ActiveAOIchanged;
        base.OnDestroy();
    }




    /// <summary>
    /// Hides objects keyframes from timeline
    /// </summary>
    void HideKeyframes()
    {

        foreach (var keyframe in Keyframes)
        {
            keyframe.UIrepresentation.SetActive(false);
        }
    }


    /// <summary>
    /// Displays objects keyframes on timeline
    /// </summary>
    void ShowKeyframes()
    {
        foreach (var keyframe in Keyframes)
        {
            keyframe.UIrepresentation.SetActive(true);
        }
    }

    protected override void OnActivate()
    {
        ShowKeyframes();
    }

    protected override void OnDeactivate()
    {
        HideKeyframes();
    }

    /// <summary>
    /// Helper function checks if specified videoframe has keyframe.
    /// </summary>
    /// <param name="frame">Videoframe to check for keyframe.</param>
    /// <returns></returns>
    bool IsFrameInKeyframes(long frame)
    {
        return Keyframes.Any(item => item.VideoFrame == frame);
    }

    /// <summary>
    /// Helper function that for a given videoframe returns the index of the keyframe. Returns -1 if no keyframe exists for videoframe.
    /// </summary>
    /// <param name="frame">Videoframe for which to get index of keyframe.</param>
    /// <returns></returns>
    int GetKeyframeIndexByFrame(long frame)
    {
        return Keyframes.FindIndex(item => item.VideoFrame == frame);
    }


    /// <summary>
    /// Method used to change AOI at keyframe from visible to invisible. 
    /// Changes AOI Tag as well as keyframe color based on new visibility.
    /// </summary>
    /// <param name="frame">Position of keyframe to change visibility of. </param>
    /// <param name="newvisibility">Visibility to change keyframe to.</param>
    void SetKeyframeVisibility(long frame, bool newvisibility)
    {
        if (!IsFrameInKeyframes(frame))
        {
            Debug.LogError("No keyframe for specified frame");
            return;
        }

        Keyframes[GetKeyframeIndexByFrame(frame)].Visibility = newvisibility;
        Keyframes[GetKeyframeIndexByFrame(frame)].UIrepresentation.GetComponent<UnityEngine.UI.Image>().color = newvisibility ? new Color(1, .8f, 0) : new Color(0, 0, 1);

        CalculateInterpolatedFrames();
    }
}
