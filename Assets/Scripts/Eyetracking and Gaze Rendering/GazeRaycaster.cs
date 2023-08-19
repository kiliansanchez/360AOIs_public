using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;

/// <summary>
/// Creates a raycast from current eyedata gaze direction. If an object with the RaycastHitHandler component is hit by raycast
/// this script calls the OnRaycastEnter and OnRaycastExtit methods on entry and exit. This allows objects to react to beeing looked at by the participant.
/// </summary>
public class GazeRaycaster : MonoBehaviour
{
    public static GazeRaycaster Instance;
    public GameObject CurrentlyHitObject { get; private set; } = null;

    private void Awake()
    {
        Instance = this;
        VRManager.OnXRStatusUpdate.AddListener(ToggleGazeRaycastingVrStatusChange);
    }

    /// <summary>
    /// Callback for VRManagers XRStatusUpdate. Turns raycast on or off based on if VR is enabled.
    /// </summary>
    /// <param name="vrStatus">New status of VR.</param>
    private void ToggleGazeRaycastingVrStatusChange(VRManager.VRStatus vrStatus)
    {
        if (vrStatus == VRManager.VRStatus.Active)
        {       
            InvokeRepeating(nameof(GazeRaycast), 0, 0.004f);
        }
        else
        {
            CancelInvoke(nameof(GazeRaycast));
        }
    }


    /// <summary>
    /// Simple raycast based on gaze direction. Only interacts with first object hit. Can't handle overlapping objects yet.
    /// </summary>
    private void GazeRaycast()
    {
        Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
        SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, EyeRecorder.CurrentEyeData);
        Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);

        if (Physics.Raycast(Camera.main.transform.position, GazeDirectionCombined, out RaycastHit hit, 25f))
        {
            OnObjectHit(hit.collider.gameObject);
        }
        else
        {
            OnNothingHit();
        }
    }

    /// <summary>
    /// Handles what happens if no object got hit by raycast but previously an object was being hit. -> Calls OnRaycastExit.
    /// </summary>
    private void OnNothingHit()
    {
        if (CurrentlyHitObject == null)
        {
            return;
        }

        if (CurrentlyHitObject.TryGetComponent(out RaycastHitHandler hithandler_outgoing_object))
        {
            hithandler_outgoing_object.OnRaycastExit();
        }

        CurrentlyHitObject = null;
    }

    /// <summary>
    /// Handles what happens when an object is hit by raycast.
    /// </summary>
    /// <param name="new_hit"></param>
    private void OnObjectHit(GameObject new_hit)
    {

        if (CurrentlyHitObject == new_hit)
        {
            return;
        }

        // else
        if (CurrentlyHitObject != null)
        {
            if (CurrentlyHitObject.TryGetComponent(out RaycastHitHandler hithandler_outgoing_object))
            {
                hithandler_outgoing_object.OnRaycastExit();
            }
        }
       
        CurrentlyHitObject = new_hit;
        if (CurrentlyHitObject.TryGetComponent(out RaycastHitHandler hithandler_incoming_object))
        {
            hithandler_incoming_object.OnRaycastEnter();
        }

    }
}
