using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;

/// <summary>
/// This script inherits from the RaycastHitHandler and therefore its functiones are called by the GazeRaycaster.
/// It simply changed the color of the object it is attached to when the gaze of the person wearing the vr headset is over
/// the object. 
/// </summary>
public class ChangeColorOnDwell : RaycastHitHandler
{
    public Color RaycastEnterColor = Color.blue;
    public Color RaycastExitColor = Color.white;

    private Renderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// Change color on gaze-raycast enter.
    /// </summary>
    public override void OnRaycastEnter()
    {
        base.OnRaycastEnter();
        _renderer.material.color = RaycastEnterColor;
    }

    /// <summary>
    /// Change color on gaze-raycast exit.
    /// </summary>
    public override void OnRaycastExit()
    {
        base.OnRaycastEnter();
        _renderer.material.color = RaycastExitColor;
    }
}
