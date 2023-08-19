using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;

/// <summary>
/// This script is derived from RaycastHitHandler. Its functiones are called by the GazeRaycaster when hit by a raycast.
/// It simply changes the color of the object it is attached to while the gaze is over the object. 
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
