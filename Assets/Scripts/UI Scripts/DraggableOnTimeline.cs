using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/*
 * This class allows an object to be displayed on the timeline and then dragged along the timeline
 * using the mouse. (Used by the scrubber to scrub along the timeline)
 */

public class DraggableOnTimeline : ClickableOnTimeline
{

    public Canvas _canvas { get; protected set; }

    public bool IsBeingDragged { get; protected set; } = false;

    public delegate void MovementCompleted();
    public MovementCompleted MovementCompletedSubscribers;

    protected Vector2 _mouseOffset;

    protected override void Start()
    {
        base.Start();

        _rect = this.GetComponent<RectTransform>();
        UnityEngine.Assertions.Assert.IsNotNull(_rect);

        if (_canvas == null)
        {
            _canvas = GameObject.Find("UI Canvas").GetComponent<Canvas>();
        }
        UnityEngine.Assertions.Assert.IsNotNull(_canvas);

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        // check if user wants to move object along timeline
        if (MovementCondition())
        {
            IsBeingDragged = true;
            // if object is moved while video is playing, first pause video
            if (VideoManager.VideoPlayer.isPlaying)
            {
                VideoManager.interruptedPlayback = true;
                VideoManager.VideoPlayer.Pause();
            }

            var previous_position = transform.position;

            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Timeline.transform as RectTransform, Input.mousePosition, _canvas.worldCamera, out pos);
            pos = pos - _mouseOffset;
            Vector2 clampMin = new Vector2(0,0);
            Vector2 clampMax = new Vector2(Timeline.Width, 0);
            transform.position = new Vector3(Mathf.Clamp(Timeline.transform.TransformPoint(pos).x, Timeline.transform.TransformPoint(clampMin).x, Timeline.transform.TransformPoint(clampMax).x), transform.position.y, transform.position.z);

            if (previous_position != transform.position)
            {
                OnPositionChange();
            }

        }
        else if (IsBeingDragged)
        {
            IsBeingDragged = false;
            _mouseOffset = Vector2.zero;
            OnMovementCompleted();
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        
        Vector2 mousepos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Timeline.transform as RectTransform, Input.mousePosition, _canvas.worldCamera, out mousepos);
  
        _mouseOffset = mousepos - (Vector2)transform.localPosition;
        Debug.Log(_mouseOffset);
    }

    protected virtual void OnPositionChange() { }

    protected virtual void OnMovementCompleted() 
    {
        MovementCompletedSubscribers?.Invoke();

    }

    protected virtual bool MovementCondition()
    {
        return IsClickedAndMouseStillDown;
    }

    public virtual void SetAnchoredX(float new_x)
    {
        _rect.anchoredPosition = new Vector2(new_x, _rect.anchoredPosition.y);
    }

}
