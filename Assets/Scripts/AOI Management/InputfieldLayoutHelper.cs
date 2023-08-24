using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to the textmeshpro object of the list entries in the AOI list. Helps with fixing the random jumps the text fields make when editing.
/// </summary>

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

    /// <summary>
    /// Callback for TextField (TMP) OnDeselect and OnEndEdit Callbacks. Resets position of label (TMP) and Caret after editing is complete.
    /// </summary>
    public void Reset()
    {
        // reset text rect
        SetLeft(TextfieldRect, LeftMargin);
        SetRight(TextfieldRect, RightMargin);

        //reset caret
        SetLeft(_caretRect, LeftMargin);
        SetRight(_caretRect, RightMargin);
    }

    /// <summary>
    /// Helper to set "left" property off RectTransform similar to how you would do it in the UnityEditor.
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="left"></param>
    public void SetLeft(RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    /// <summary>
    /// Helper to set "right" property off RectTransform similar to how you would do it in the UnityEditor.
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="right"></param>
    public void SetRight(RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    /// <summary>
    /// Helper to set "top" property off RectTransform similar to how you would do it in the UnityEditor.
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="top"></param>
    public void SetTop(RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    /// <summary>
    /// Helper to set "bottom" property off RectTransform similar to how you would do it in the UnityEditor.
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="bottom"></param>
    public void SetBottom(RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }
}
