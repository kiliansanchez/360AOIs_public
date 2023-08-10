using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class that defines OnRaycastEnter and OnRaycastExit for interaction with GazeRaycaster.
/// Should probably be an interface in the future.
/// </summary>
public abstract class RaycastHitHandler: MonoBehaviour
{

    public bool IsCurrentlyHitByRaycast { get; private set; } = false;

    /// <summary>
    /// Called by GazeRaycaster when Dwell on object is started.
    /// </summary>
    public virtual void OnRaycastEnter()
    {
        IsCurrentlyHitByRaycast = true;
    }

    /// <summary>
    /// Called by GazeRaycaster when Dwell on object has ended.
    /// </summary>
    public virtual void OnRaycastExit()
    {
        IsCurrentlyHitByRaycast = false;
    }

}
