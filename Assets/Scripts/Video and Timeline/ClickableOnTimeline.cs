using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// This class serves as a base for all clickable children of the timeline. 
/// it makes sure that the hover state between the timeline and its children is properly handed-over.
/// For example if the user clicks on a keyframe and then drags the cursor, that should not move the playhead on the timeline
/// because the user wanted to interact with the keyframe, not the timeline. Vice versa if the user scrubs the playhead across
/// the timeline hovering over a keyframe should not trigger a click on the keyframe.
/// </summary>
public class ClickableOnTimeline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    
    public Timeline Timeline { get; protected set; }
    protected RectTransform _rect;
    public bool IsHovered { get; protected set; } = false;

    public bool IsClickedAndMouseStillDown { get; protected set; } = false;


    protected virtual void Start()
    {
        if (Timeline == null)
        {
            Timeline = GameObject.Find("Timeline").GetComponent<Timeline>();
        }     
        UnityEngine.Assertions.Assert.IsNotNull(Timeline);

        _rect = GetComponent<RectTransform>();
        UnityEngine.Assertions.Assert.IsNotNull(_rect);
    }

    /// <summary>
    /// After a clickable item on the timeline has been clicked, for as long as the mouse is still down, 
    /// the update function makes sure that the timeline never registeres as being "hovered", since the user
    /// is hovering and item on the timeline, not the timeline itself.
    /// </summary>
    protected virtual void Update()
    {
        if (IsClickedAndMouseStillDown && Timeline.IsHovered)
        {
            Timeline.HoverHandoverWithChildren(this.gameObject, false);
        }
    }

    /// <summary>
    /// When the cursor hover above an item on the timeline the item marks itself as hovered.
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
       
        if (eventData.pointerCurrentRaycast.gameObject.transform == this.transform)
        {
            IsHovered = true;
        }
    }

    /// <summary>
    /// If the cursor leaves the area of the clickable item but has previously beed clicked and the mouse is still pressed down, the item makes sure
    /// that the timeline still doesn't register as being hovered, because the user is still interacting with the 
    /// item itself, since the mouse is still pressed down (the user might e.g. be trying to drag an item across the timeline.). 
    /// If the  cursor leaves the area of the item and the item has not been clicked, it sets itself as not hovered and hands over
    /// the hover to the timeline.
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerExit(PointerEventData eventData)
    {

        if (IsClickedAndMouseStillDown)
        {
            if (Timeline.IsHovered)
            {
                Timeline.HoverHandoverWithChildren(this.gameObject, false);
            }

            return;
        }
        else
        {
            IsHovered = false;

            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (eventData.pointerCurrentRaycast.gameObject.transform == Timeline.transform)
                {
                    Timeline.HoverHandoverWithChildren(this.gameObject, true);
                }
            }      
        }   
    }

    /// <summary>
    /// If an item on the timeline has been clicked it marks itself as clicked and takes the hover away from the timeline.
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.transform == this.transform)
        {
            IsClickedAndMouseStillDown = true;
            Timeline.SetBlockEdgeSnapping(this.gameObject, true);
            if (Timeline.IsHovered)
            {
                Timeline.HoverHandoverWithChildren(this.gameObject, false);
            }


            //default behavior: set frame based on own position, update video and playhead accordingly
            Timeline.SetTargetFrame(_rect, true, true);

        }
    }

    /// <summary>
    /// On Pointer up the item registers itself as not clicked anymore, allows edge snapping of the timeline abd
    /// handsover hover to the timeline.
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (IsClickedAndMouseStillDown)
        {
            IsClickedAndMouseStillDown = false;
            Timeline.SetBlockEdgeSnapping(this.gameObject, false);
        }

        if (eventData.pointerCurrentRaycast.gameObject.transform == Timeline.transform)
        {
            Timeline.HoverHandoverWithChildren(this.gameObject, true);
        }
    }
}
