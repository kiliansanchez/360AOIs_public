using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/*
 * this class serves as a base for all clickable children of the timeline. 
 * it makes sure that the hover state between the timeline and its children is properly hand-over 
 */

public class ClickableOnTimeline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    
    public Timeline Timeline { get; protected set; }
    protected RectTransform _rect;
    public bool IsHovered { get; protected set; } = false;

    public bool IsClickedAndMouseStillDown { get; protected set; } = false;


    // Start is called before the first frame update
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

    protected virtual void Update()
    {
        if (IsClickedAndMouseStillDown && Timeline.IsHovered)
        {
            Timeline.HoverHandoverWithChildren(this.gameObject, false);
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
       
        if (eventData.pointerCurrentRaycast.gameObject.transform == this.transform)
        {
            IsHovered = true;
        }
    }

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


            //default behavior: set frame based on owm position, update video and scrubber accordingly
            Timeline.SetCurrentFrame(_rect, true, true);

        }
    }

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
