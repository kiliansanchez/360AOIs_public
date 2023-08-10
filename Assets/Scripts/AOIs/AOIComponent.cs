using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* This is the base class for components that make up an area of interest. The user can highlight and work with one AOI at a time, marking that one as active (done by the AOI Manager) 
 This class subsribes to the event of changing the active AOI and specifies the relevant methods (OnActivate, OnDeactivate) that need to be implemented by derived classes */
public abstract class AOIComponent : MonoBehaviour
{


    // Start is called before the first frame update
    protected virtual void Start()
    {
        // subscribe to activeaoi changes
        AOIManager.NewActiveAoiSubscribers += ActiveAOIchanged;
        OnActivate();
    }


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


    protected abstract void OnActivate();

    protected abstract void OnDeactivate();



    protected virtual void OnDestroy()
    {
        AOIManager.NewActiveAoiSubscribers -= ActiveAOIchanged;
    }
}
