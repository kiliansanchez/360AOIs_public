using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements the playhead. Inherits DraggableOnTimeline.
/// </summary>
public class Playhead : DraggableOnTimeline
{

    public RectTransform CurrentFrameLabelWithBG;
    public TMPro.TextMeshProUGUI CurrentFrameLabel;


    protected override void Update()
    {
        base.Update();

    }

    /// <summary>
    /// Called when user changes position of playhead by dragging it along the timeline. Sets the targetframe of the timeline
    /// based on the new position of the playhead. 
    /// </summary>
    protected override void OnPositionChange()
    {
        Timeline.SetTargetFrame(_rect, false, false);
        CurrentFrameLabel.text = Timeline.TargetFrame.ToString();

        CurrentFrameLabelWithBG.anchoredPosition = new Vector2(this.GetComponent<RectTransform>().anchoredPosition.x, CurrentFrameLabelWithBG.anchoredPosition.y);
    }

    /// <summary>
    /// called when user releases mouseclick after having moved the playhead. Tells timeline to update targetframe based on
    /// playhead position as well as updating the video based on new target frame.
    /// </summary>
    protected override void OnMovementCompleted()
    {
        base.OnMovementCompleted();
        Timeline.SetTargetFrame(_rect, false, true);
    }

    /// <summary>
    /// Allos the timeline to change to position of the playhead during video playback.
    /// </summary>
    /// <param name="new_x"></param>
    public override void SetAnchoredX(float new_x)
    {
        base.SetAnchoredX(new_x);
        CurrentFrameLabelWithBG.anchoredPosition = new Vector2(new_x, CurrentFrameLabelWithBG.anchoredPosition.y);
        CurrentFrameLabel.text = Timeline.TargetFrame.ToString();
    }

    protected override bool MovementCondition()
    {

        if (!VideoManager.IsInitialized)
        {
            return false;
        }

        return Timeline.IsHovered && Input.GetMouseButton(0);
    }
}
