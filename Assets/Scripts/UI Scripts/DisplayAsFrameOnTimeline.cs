using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/*
 * This classed is used for any UI element that should be displayed on a certain frame in the timeline
 * for example a keyframe for frame 47 always needs to be positioned where frame 47 is in the timeline, regardless
 * of scale/zoom level of the timeline. This class handles that.
 */

public class DisplayAsFrameOnTimeline : MonoBehaviour
{
    public Timeline Timeline { get; protected set; }
    public long Frame;
    public RectTransform Rect { get; protected set; }
    public CanvasGroup CanvasGroup { get; protected set; }
    public bool IsDraggable { get; protected set; }

    // Start is called before the first frame update
    protected virtual void Start()
    {

        if (Timeline == null)
        {
            Timeline = GameObject.Find("Timeline").GetComponent<Timeline>();
            UnityEngine.Assertions.Assert.IsNotNull(Timeline);
        }

        Rect = GetComponent<RectTransform>();
        UnityEngine.Assertions.Assert.IsNotNull(Rect);

        CanvasGroup = GetComponent<CanvasGroup>();
        UnityEngine.Assertions.Assert.IsNotNull(CanvasGroup);

        //if object should only display something but not be clickable or draggable, disable raycast targets
        if (GetComponent<ClickableOnTimeline>() == null && GetComponent<DraggableOnTimeline>() == null)
        {
            Debug.LogWarning("Object on Timeline without \"ClickableOnTimeline\" class. Disabling raycast target");

            if (TryGetComponent(out UnityEngine.UI.Image iamge))
            {
                iamge.raycastTarget = false;
            }

            if (TryGetComponent(out TMPro.TextMeshProUGUI texmeshpro))
            {
                texmeshpro.raycastTarget = false;
            }

        }

        //if object should be displayed on timeline based on frame, but frame can be changed by dragging, register with draggable component to update frame on completed movement
        if (TryGetComponent(out DraggableOnTimeline dragcomponent))
        {
            IsDraggable = true;
            dragcomponent.MovementCompletedSubscribers += RecalculateFrameBasedOnPosition;
        } 
        else { IsDraggable = false; }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        

        if (IsDraggable)
        {
            if (GetComponent<DraggableOnTimeline>().IsBeingDragged)
            {
                return;
            }
        }

        Rect.anchoredPosition = new Vector2(Frame * Timeline.WidthPerFrame - Timeline.StartFrame * Timeline.WidthPerFrame, Rect.anchoredPosition.y);
        

        if (Rect.anchoredPosition.x > Timeline.Width || Rect.anchoredPosition.x < 0)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    void Hide()
    {
        CanvasGroup.alpha = 0f;
        CanvasGroup.blocksRaycasts = false;
    }

    void Show()
    {
        CanvasGroup.alpha = 1f;
        CanvasGroup.blocksRaycasts = true;
    }

    void RecalculateFrameBasedOnPosition()
    {
        Frame = (long)Mathf.Round((Rect.anchoredPosition.x / Timeline.WidthPerFrame) + Timeline.StartFrame);
    }
}
