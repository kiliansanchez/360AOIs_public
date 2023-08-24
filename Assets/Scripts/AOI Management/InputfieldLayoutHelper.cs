using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputfieldLayoutHelper : MonoBehaviour
{
    public TMPro.TMP_InputField InputField;
    public RectTransform TextfieldRect;
    public RectTransform _caretRect;
    public int LeftMargin = 8;
    public int RightMargin = 8;

    private void Start()
    {

        foreach (Transform child in InputField.transform)
        {
            if (child.name == "Caret")
            {
                _caretRect = child.GetComponent<RectTransform>();
                break;
            }
        }

    }

    //called from ui
    public void Reset()
    {
        // reset text rect
        SetLeft(TextfieldRect, LeftMargin);
        SetRight(TextfieldRect, RightMargin);

        //reset caret
        SetLeft(_caretRect, LeftMargin);
        SetRight(_caretRect, RightMargin);
    }

    public void SetLeft(RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public void SetRight(RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public void SetTop(RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public void SetBottom(RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }
}
