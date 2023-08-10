using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scrubber : DraggableOnTimeline
{

    public RectTransform _currentFrameLabelWithBG;
    public TMPro.TextMeshProUGUI _currentFrameLabel;


    protected override void Update()
    {
        base.Update();

    }

    protected override void OnPositionChange()
    {
        Timeline.SetCurrentFrame(_rect, false, false);
        _currentFrameLabel.text = Timeline.CurrentFrame.ToString();

        _currentFrameLabelWithBG.anchoredPosition = new Vector2(this.GetComponent<RectTransform>().anchoredPosition.x, _currentFrameLabelWithBG.anchoredPosition.y);
    }

    protected override void OnMovementCompleted()
    {
        base.OnMovementCompleted();
        Timeline.SetCurrentFrame(_rect, false, true);
    }

    public override void SetAnchoredX(float new_x)
    {
        base.SetAnchoredX(new_x);
        _currentFrameLabelWithBG.anchoredPosition = new Vector2(new_x, _currentFrameLabelWithBG.anchoredPosition.y);
        _currentFrameLabel.text = Timeline.CurrentFrame.ToString();
    }

    protected override bool MovementCondition()
    {
        return Timeline.IsHovered && Input.GetMouseButton(0);
    }
}
