using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This is the base class for components that make up an area of interest.
/// The user can highlight and work with one AOI at a time, marking that one as active (done by the AOI Manager) 
/// This class subsribes to the event of changing the active AOI and declares the relevant methods (OnActivate, OnDeactivate) that need to be implemented by derived classes.
/// </summary>
public abstract class AOIComponent : MonoBehaviour
{


    protected virtual void Start()
    {
        // subscribe to activeaoi changes
        AOIManager.NewActiveAOI.AddListener(ActiveAOIchanged);
        OnActivate();
    }


    /// <summary>
    /// Callback for AOIManagers NewActiveAOI-Event. Called whenever user clicks on new AOI.
    /// </summary>
    /// <param name="newAoi">AOI that user has clicked on. New active AOI.</param>
    protected virtual void ActiveAOIchanged(GameObject newAoi)
    {
        if (this == null)
        {
            return;
        }

        if (newAoi != gameObject)
        {
            OnDeactivate();     
        }
        else if (newAoi == gameObject)
        {
            OnActivate();
        }
    }

    /// <summary>
    /// Method needs to be implemented by all AOI-Components. Specified how component reacts, when user clicks on AOI.
    /// </summary>
    protected abstract void OnActivate();

    /// <summary>
    /// Method needs to be implemented by all AOI-Components. Specified how component reacts, when user de-selects AOI.
    /// </summary>
    protected abstract void OnDeactivate();



    protected virtual void OnDestroy()
    {
        AOIManager.NewActiveAOI.RemoveListener(ActiveAOIchanged);
    }
}
