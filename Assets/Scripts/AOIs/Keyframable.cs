using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * This script does a lot of things and should probably be refactored into multiple classes.
 *  a) Creates & Updates Keyframes for position, size and shape of AOI
 *     Does this by listening to DraggableAroundCameras and Resizables events
 *  b) Calculates position, size and shape for Frames between Keyframes using interpolation
 *  c) Animates object based on current frame of video (using the Timeline reference) by setting position, size and shape
 *     according to calculated frames
 *  
 */

public class Keyframable : AOIComponent
{

    public GameObject KeyframePrefab;

    public Timeline _timeline;

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
        if (_timeline == null)
        {
            _timeline = GameObject.Find("Timeline").GetComponent<Timeline>();        
        }

        UnityEngine.Assertions.Assert.IsNotNull(_timeline);

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

    // Start is called before the first frame update
    protected override void Start()
    {

        // check keyframe prefab is there
        UnityEngine.Assertions.Assert.IsNotNull(KeyframePrefab);


        SetupRightClickMenu(true);

        base.Start();
    }

    void SetupRightClickMenu(bool currentVisibility)
    {  
        if (TryGetComponent(out RightClickMenuManager menu))
        {
            string labelToAdd = currentVisibility ? "Set Invisible": "Set Visible";
            string labelToRemove = currentVisibility ? "Set Visible" : "Set Invisible";

            menu.RemoveItem(labelToRemove);
            menu.RemoveItem(labelToAdd);
            menu.AddItem(labelToAdd, ToggleKeyframeVisibilityFromRightClickMenu);
        }
        else
        {
            Debug.LogError("No right click  menu");
        }
    }

    void ToggleKeyframeVisibilityFromRightClickMenu()
    {

        GetComponent<RightClickMenuManager>().HideMenu();

        bool isCurrentFrameInKeyframes = IsFrameInKeyframes(_timeline.CurrentFrame);

        // before creating or updating a keyframe, first gotta find out the current visibility, either from
        // keyframes or interpolated frames

        var Frames = isCurrentFrameInKeyframes ? Keyframes : InterpolatedFrames;
        int index = Frames.FindIndex(item => item.VideoFrame == _timeline.CurrentFrame);
        bool currentVisibility = Frames[index].Visibility;


        if (!isCurrentFrameInKeyframes)
        {
            CreateKeyframe();
        }

        SetKeyframeVisibility(_timeline.CurrentFrame, !currentVisibility);         
        SetupRightClickMenu(!currentVisibility);
    }

    private void Update()
    {

        // CONSIDER USING FIXED UPDATE FOR ANIMATION!! 

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

    bool IsObjectInCorrectState()
    {

        bool isCurrentFrameInKeyframes = IsFrameInKeyframes(_timeline.CurrentFrame);
        bool isCurrentFrameInInterpolatedFrames = InterpolatedFrames.Any(item => item.VideoFrame == _timeline.CurrentFrame);

        if (!(isCurrentFrameInKeyframes || isCurrentFrameInInterpolatedFrames))
        {
            Debug.LogError("Missing Frame", this);
            CalculateInterpolatedFrames();
            return true;
        }

        var Frames = isCurrentFrameInKeyframes ? Keyframes : InterpolatedFrames;

        int i = Frames.FindIndex(item => item.VideoFrame == _timeline.CurrentFrame);

        bool vertices = true;
        if (_resizeComponent != null)
        {
            vertices = Enumerable.SequenceEqual(Frames[i].Vertices, _resizeComponent.Mesh.vertices);
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

    void SetObjectsStateToFrameData()
    {

        var frame = _timeline.CurrentFrame;

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
            _resizeComponent.RecenterOriginBasedOnKeyframeData(Frames[index].Vertices);
        }

        transform.SetPositionAndRotation(Frames[index].Position, Frames[index].Rotation);

        tag = Frames[index].Visibility ? "AOI" : "AOI_Invisible";

        SetupRightClickMenu(Frames[index].Visibility);     

    }


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
        var end = _timeline.FrameCount;

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

    void ObjectChangeStarted()
    {
        _objectIsBeingChanged = true;
    }

    void UpdateOrCreateKeyframeOnObjectChangeCompleted()
    {

        bool frameAlreadyExists = IsFrameInKeyframes(_timeline.CurrentFrame);

        if (frameAlreadyExists)
        {
            
            int index = GetKeyframeIndexByFrame(_timeline.CurrentFrame);
            UpdateKeyframe(index);
;       }
        else
        {
            Debug.Log("Creating New Keyframe");
            CreateKeyframe();
        }

        //InterpolatedFrames = new();
        CalculateInterpolatedFrames();

        _objectIsBeingChanged = false;
    }

    void CreateKeyframe()
    {
        FrameData newKeyframeData = new();

        newKeyframeData.UIrepresentation = Instantiate(KeyframePrefab);
        newKeyframeData.UIrepresentation.transform.SetParent(_timeline.transform, true);


        newKeyframeData.VideoFrame = _timeline.CurrentFrame;

        if (newKeyframeData.UIrepresentation.TryGetComponent(out DisplayAsFrameOnTimeline frameComponent))
        {
            frameComponent.Frame = newKeyframeData.VideoFrame;
            frameComponent.GetComponent<RectTransform>().anchoredPosition = new Vector2(_timeline.CurrentFrame * _timeline.WidthPerFrame - _timeline.StartFrame * _timeline.WidthPerFrame, 0);
        }


        if (newKeyframeData.UIrepresentation.TryGetComponent(out RightClickMenuManager rcmenu))
        {
            rcmenu.AddItem("Delete", DeleteKeyframeFromRightClickMenu);
        }

        Keyframes.Add(newKeyframeData);
        UpdateKeyframe(Keyframes.Count - 1);

        //Keyframes = Keyframes.OrderBy(item => item.VideoFrame).ToList();

        Debug.Log("Ney Keyframe created successfully");
    }

    void DeleteKeyframeFromRightClickMenu()
    {
        Destroy(RightClickMenuManager.LastClickedObjectWithMenu.GetComponent<RightClickMenuManager>().Menu);
        Destroy(RightClickMenuManager.LastClickedObjectWithMenu);
        int index = Keyframes.FindIndex(item => item.UIrepresentation == RightClickMenuManager.LastClickedObjectWithMenu);
        if (index == -1)
        {
            Debug.LogError("your code is buggy");
            return;
        }
        Keyframes.RemoveAt(index);    
        CalculateInterpolatedFrames();
    }

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

    
    public void LoadKeyframes(List<FrameData> loaded_keyframes)
    {
        _objectIsBeingChanged = true;
        Keyframes = loaded_keyframes;

        foreach (var item in Keyframes)
        {
            item.UIrepresentation = Instantiate(KeyframePrefab);
            item.UIrepresentation.transform.SetParent(_timeline.transform, true);
            item.UIrepresentation.GetComponent<UnityEngine.UI.Image>().color = item.Visibility ? new Color(1, .8f, 0) : new Color(0, 0, 1);

            if (item.UIrepresentation.TryGetComponent(out DisplayAsFrameOnTimeline frameComponent))
            {
                frameComponent.Frame = item.VideoFrame;
                frameComponent.GetComponent<RectTransform>().anchoredPosition = new Vector2(item.VideoFrame * _timeline.WidthPerFrame - _timeline.StartFrame * _timeline.WidthPerFrame, 0);
            }


            if (item.UIrepresentation.TryGetComponent(out RightClickMenuManager rcmenu))
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


    void HideKeyframes()
    {

        foreach (var keyframe in Keyframes)
        {
            keyframe.UIrepresentation.SetActive(false);
        }
    }

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

    bool IsFrameInKeyframes(long frame)
    {
        return Keyframes.Any(item => item.VideoFrame == frame);
    }

    int GetKeyframeIndexByFrame(long frame)
    {
        return Keyframes.FindIndex(item => item.VideoFrame == frame);
    }

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
